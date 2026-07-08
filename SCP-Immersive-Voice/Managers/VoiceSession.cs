using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.AudioProcessing;
using SCP_Immersive_Voice.AudioProcessing.Effects;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using SCP_Immersive_Voice.AudioProcessing.Utils;
using SCP_Immersive_Voice.Presets;
using System;
using System.Collections.Generic;
using VoiceChat;

namespace SCP_Immersive_Voice.Managers
{
    public class VoiceSession
    {
        #region Operational Properties
        public int SessionId { get; set; }
        public Player PlayerInstance { get; set; }
        public AudioEffectPipeline Pipeline { get; } = new();
        public Dictionary<string, IAudioEffect> ActiveNodes { get; } = new(32);
        public ScpVoicePreset LastAppliedPreset { get; set; }
        public DateTime LastPacketReceivedTime { get; set; } = DateTime.MinValue;
        public object SyncLock { get; } = new();

        /// <summary>
        /// Encapsulates spatial updates inside a high-precision hardware tick boundary layer.
        /// </summary>
        public SpatializationDebouncer SpatialDebouncer { get; } = new(350f);
        #endregion

        #region Pre-Allocated Reusable Heap Graph Buffers
        private readonly List<IAudioEffect> _reusableEffectsList = new(32);
        private readonly Dictionary<string, IAudioEffect> _temporaryMap = new(32);
        #endregion

        #region Hardware-Matched Fixed Buffer Ring
        private const int BufferRingSize = 32;
        private float[][] _bufferRing;
        private int _ringIndex;

        /// <summary>
        /// Resolves a strictly isolated, hardware-aligned buffer. 
        /// Guarantees total thread safety and zero mixer desynchronization for concurrent talkers.
        /// </summary>
        public float[] GetNextFixedBuffer()
        {
            int maxSize = VoiceChatSettings.PacketSizePerChannel > 0 ? VoiceChatSettings.PacketSizePerChannel : 960;

            if (_bufferRing is null)
            {
                _bufferRing = new float[BufferRingSize][];
                for (int i = 0; i < BufferRingSize; i++)
                    _bufferRing[i] = new float[maxSize];
            }

            float[] buf = _bufferRing[_ringIndex];
            _ringIndex = (_ringIndex + 1) % BufferRingSize;

            // Defensively clear the entire array buffer to guarantee no ghost echoes or digital cross-talk bleed
            Array.Clear(buf, 0, buf.Length);
            return buf;
        }
        #endregion

        #region Zero-Allocation Graph Synchronization Matrix
        public void SynchronizePipelineGraph(ScpVoicePreset preset)
        {
            if (preset is null) return;

            float sampleRate = VoiceChatSettings.SampleRate > 0 ? (float)VoiceChatSettings.SampleRate : 48000f;

            _reusableEffectsList.Clear();
            _temporaryMap.Clear();

            // SOFTENED NOISE FLOOR: Shifted default gate threshold from -45f to -52f to accommodate quiet vocal patterns
            float gateThreshold = preset.UseNoiseGate ? preset.NoiseGateThreshold : -52f;
            UpdateOrRegisterSlot("NoiseGate", () => new NoiseGateEffect(gateThreshold, sampleRate), gateThreshold);

            if (preset.VocalShriek > 0f)
                UpdateOrRegisterSlot("VocalShriek", () => new VocalShriekShifterEffect(preset.VocalShriek, sampleRate), preset.VocalShriek);

            if (Math.Abs(preset.Pitch - 1f) > 0.01f)
                UpdateOrRegisterSlot("PitchShift", () => new PitchShiftEffect(preset.Pitch, sampleRate, 40f), preset.Pitch);

            if (Math.Abs(preset.Formant - 1f) > 0.01f)
                UpdateOrRegisterSlot("FormantShift", () => new FormantShiftEffect(preset.Formant, sampleRate), preset.Formant);

            if (preset.FormantDrift > 0f)
                UpdateOrRegisterSlot("FormantDrift", () => new FormantDriftEffect(preset.FormantDrift), preset.FormantDrift);

            if (preset.LaryngealAsymmetry > 0f)
                UpdateOrRegisterSlot("LaryngealAsymmetry", () => new LaryngealAsymmetryEffect(preset.LaryngealAsymmetry, sampleRate), preset.LaryngealAsymmetry);

            if (preset.DeathRattle > 0f)
                UpdateOrRegisterSlot("DeathRattle", () => new DeathRattleEffect(preset.DeathRattle, sampleRate), preset.DeathRattle);

            if (preset.Subharmonic > 0f)
                UpdateOrRegisterSlot("Subharmonic", () => new SubharmonicGrowlEffect(preset.Subharmonic, sampleRate), preset.Subharmonic);

            if (preset.DemonicOctaverMix > 0f)
                UpdateOrRegisterSlot("Octaver", () => new DemonicOctaverEffect(preset.DemonicOctaverMix, sampleRate), preset.DemonicOctaverMix);

            if (preset.Guttural > 0f)
                UpdateOrRegisterSlot("Guttural", () => new GutturalResonanceEffect(preset.Guttural, sampleRate), preset.Guttural);

            if (preset.Distortion > 0f)
                UpdateOrRegisterSlot("Distortion", () => new DistortionEffect(preset.Distortion, sampleRate), preset.Distortion);

            if (preset.SiliconModulation > 0f)
                UpdateOrRegisterSlot("SiliconModulation", () => new SiliconRingModulatorEffect(preset.SiliconModulation, sampleRate), preset.SiliconModulation);

            if (preset.ScreechModulation > 0f)
                UpdateOrRegisterSlot("ScreechModulation", () => new ScreechModulatorEffect(preset.ScreechModulation, sampleRate), preset.ScreechModulation);

            if (preset.Bitcrush > 0f)
                UpdateOrRegisterSlot("Bitcrush", () => new BitcrushEffect(preset.Bitcrush), preset.Bitcrush);

            if (preset.SampleRateReduce > 0f)
                UpdateOrRegisterSlot("SampleRateReduce", () => new SampleRateReducerEffect(preset.SampleRateReduce, sampleRate), preset.SampleRateReduce);

            if (preset.Tremolo > 0f)
                UpdateOrRegisterSlot("Tremolo", () => new TremoloEffect(preset.Tremolo), preset.Tremolo);

            if (preset.Glitch > 0f)
                UpdateOrRegisterSlot("Glitch", () => new GlitchBurstEffect(preset.Glitch, sampleRate), preset.Glitch);

            if (preset.PredatoryCamouflage > 0f)
                UpdateOrRegisterSlot("PredatoryCamouflage", () => new PredatoryCamouflageEffect(preset.PredatoryCamouflage, sampleRate), preset.PredatoryCamouflage);

            if (preset.WhisperAmount > 0f)
                UpdateOrRegisterSlot("Whisper", () => new WhisperFilterEffect(preset.WhisperAmount, sampleRate), preset.WhisperAmount);

            if (preset.BreathNoise > 0f)
                UpdateOrRegisterSlot("Breath", () => new BreathNoiseEffect(preset.BreathNoise, sampleRate), preset.BreathNoise);

            if (preset.StaticNoise > 0f)
                UpdateOrRegisterSlot("Static", () => new StaticNoiseEffect(preset.StaticNoise, sampleRate), preset.StaticNoise);

            if (preset.DryCrackle > 0f)
                UpdateOrRegisterSlot("DryCrackle", () => new DryCrackleEffect(preset.DryCrackle, sampleRate), preset.DryCrackle);

            if (preset.FleshCrackle > 0f)
                UpdateOrRegisterSlot("FleshCrackle", () => new FleshCrackleEffect(preset.FleshCrackle, sampleRate), preset.FleshCrackle);

            if (preset.StoneCrack > 0f)
                UpdateOrRegisterSlot("StoneCrack", () => new StoneCrackEffect(preset.StoneCrack, sampleRate), preset.StoneCrack);

            if (preset.StoneGrind > 0f)
                UpdateOrRegisterSlot("StoneGrind", () => new StoneGrindEffect(preset.StoneGrind, sampleRate), preset.StoneGrind);

            if (preset.Chirp > 0f)
                UpdateOrRegisterSlot("Chirp", () => new ChirpEffect(preset.Chirp, sampleRate), preset.Chirp);

            if (preset.DataBurst > 0f)
                UpdateOrRegisterSlot("DataBurst", () => new DigitalDataBurstEffect(preset.DataBurst, sampleRate), preset.DataBurst);

            if (preset.WetOrganic > 0f)
                UpdateOrRegisterSlot("WetOrganic", () => new WetOrganicEffect(preset.WetOrganic, sampleRate), preset.WetOrganic);

            if (preset.LowPass > 0f)
                UpdateOrRegisterSlot("LowPass", () => new LowPassEffect(preset.LowPass, sampleRate), preset.LowPass);

            if (preset.HighPass > 0f)
                UpdateOrRegisterSlot("HighPass", () => new HighPassEffect(preset.HighPass, sampleRate), preset.HighPass);

            if (preset.WetDecay > 0f)
                UpdateOrRegisterSlot("WetDecay", () => new WetDecayEffect(preset.WetDecay, sampleRate), preset.WetDecay);

            if (preset.PocketEcho > 0f)
                UpdateOrRegisterSlot("PocketEcho", () => new PocketDimensionEchoEffect(preset.PocketEcho, sampleRate), preset.PocketEcho);

            if (preset.Reverb > 0f)
                UpdateOrRegisterSlot("Reverb", () => new ReverbEffect(preset.Reverb, sampleRate), preset.Reverb);

            Pipeline.UpdateEffects(_reusableEffectsList);

            ActiveNodes.Clear();
            foreach (var kvp in _temporaryMap)
            {
                ActiveNodes[kvp.Key] = kvp.Value;
            }
        }

        private void UpdateOrRegisterSlot(string key, Func<IAudioEffect> factory, float runtimeValue)
        {
            if (ActiveNodes.TryGetValue(key, out var existingInstance))
            {
                if (existingInstance is IAdjustableAudioEffect adjustable)
                {
                    adjustable.AdjustParameter(runtimeValue);
                }

                _temporaryMap[key] = existingInstance;
                _reusableEffectsList.Add(existingInstance);
            }
            else
            {
                var newInstance = factory();
                _temporaryMap[key] = newInstance;
                _reusableEffectsList.Add(newInstance);
            }
        }
        #endregion
    }
}