using LabApi.Features.Wrappers;
using SCP_Immersive_Voice.AudioProcessing;
using SCP_Immersive_Voice.AudioProcessing.Effects;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using SCP_Immersive_Voice.AudioProcessing.Utils;
using SCP_Immersive_Voice.Presets;
using System;
using System.Collections.Generic;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

namespace SCP_Immersive_Voice.Managers
{
    public class VoiceSession : IDisposable
    {
        #region Operational Properties
        public int SessionId { get; set; }
        public Player PlayerInstance { get; set; }
        public AudioEffectPipeline Pipeline { get; } = new();
        public Dictionary<string, IAudioEffect> ActiveNodes { get; } = new(32);
        public ScpVoicePreset LastAppliedPreset { get; set; }
        public DateTime LastPacketReceivedTime { get; set; } = DateTime.MinValue;
        public object SyncLock { get; } = new();
        public SpatializationDebouncer SpatialDebouncer { get; } = new(350f);
        #endregion

        #region Thread-Isolated Codec Resources
        public OpusDecoder SessionDecoder { get; } = new();
        public OpusEncoder SessionEncoder { get; } = new(OpusApplicationType.Voip);
        #endregion

        #region Pre-Allocated Reusable Heap Graph Buffers
        private readonly List<IAudioEffect> _reusableEffectsList = new(32);
        private readonly Dictionary<string, IAudioEffect> _temporaryMap = new(32);
        #endregion

        #region Hardware-Matched Fixed Buffer Ring
        private const int BufferRingSize = 32;
        private float[][] _bufferRing;
        private int _ringIndex;

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

            // 1. Dynamics & Gate
            float gateThreshold = preset.UseNoiseGate ? preset.NoiseGateThreshold : -52f;
            UpdateOrRegisterSlot("Noise Gate", true, (gateThreshold, sampleRate), static s => new NoiseGateEffect(s.gateThreshold, s.sampleRate), gateThreshold);

            // 2. Biomorphic & Pitch Processing
            UpdateOrRegisterSlot("Vocal Shriek", preset.VocalShriek > 0f, (preset.VocalShriek, sampleRate), static s => new VocalShriekShifterEffect(s.VocalShriek, s.sampleRate), preset.VocalShriek);
            UpdateOrRegisterSlot("Pitch Shift", Math.Abs(preset.Pitch - 1f) > 0.01f, (preset.Pitch, sampleRate), static s => new PitchShiftEffect(s.Pitch, s.sampleRate, 40f), preset.Pitch);
            UpdateOrRegisterSlot("Formant Shift", Math.Abs(preset.Formant - 1f) > 0.01f, (preset.Formant, sampleRate), static s => new FormantShiftEffect(s.Formant, s.sampleRate), preset.Formant);
            UpdateOrRegisterSlot("Formant Drift", preset.FormantDrift > 0f, preset.FormantDrift, static s => new FormantDriftEffect(s), preset.FormantDrift);
            UpdateOrRegisterSlot("Laryngeal Asymmetry", preset.LaryngealAsymmetry > 0f, (preset.LaryngealAsymmetry, sampleRate), static s => new LaryngealAsymmetryEffect(s.LaryngealAsymmetry, s.sampleRate), preset.LaryngealAsymmetry);
            UpdateOrRegisterSlot("Death Rattle", preset.DeathRattle > 0f, (preset.DeathRattle, sampleRate), static s => new DeathRattleEffect(s.DeathRattle, s.sampleRate), preset.DeathRattle);
            UpdateOrRegisterSlot("Subharmonic Growl", preset.Subharmonic > 0f, (preset.Subharmonic, sampleRate), static s => new SubharmonicGrowlEffect(s.Subharmonic, s.sampleRate), preset.Subharmonic);
            UpdateOrRegisterSlot("Demonic Octaver", preset.DemonicOctaverMix > 0f, (preset.DemonicOctaverMix, sampleRate), static s => new DemonicOctaverEffect(s.DemonicOctaverMix, s.sampleRate), preset.DemonicOctaverMix);
            UpdateOrRegisterSlot("Guttural Resonance", preset.Guttural > 0f, (preset.Guttural, sampleRate), static s => new GutturalResonanceEffect(s.Guttural, s.sampleRate), preset.Guttural);

            // 3. Saturation & Modulation Models
            UpdateOrRegisterSlot("Distortion", preset.Distortion > 0f, (preset.Distortion, sampleRate), static s => new DistortionEffect(s.Distortion, s.sampleRate), preset.Distortion);
            UpdateOrRegisterSlot("Silicon Ring Modulator", preset.SiliconModulation > 0f, (preset.SiliconModulation, sampleRate), static s => new SiliconRingModulatorEffect(s.SiliconModulation, s.sampleRate), preset.SiliconModulation);
            UpdateOrRegisterSlot("Screech Modulator", preset.ScreechModulation > 0f, (preset.ScreechModulation, sampleRate), static s => new ScreechModulatorEffect(s.ScreechModulation, s.sampleRate), preset.ScreechModulation);
            UpdateOrRegisterSlot("Bitcrush", preset.Bitcrush > 0f, preset.Bitcrush, static s => new BitcrushEffect(s), preset.Bitcrush);
            UpdateOrRegisterSlot("Sample Rate Reducer", preset.SampleRateReduce > 0f, (preset.SampleRateReduce, sampleRate), static s => new SampleRateReducerEffect(s.SampleRateReduce, s.sampleRate), preset.SampleRateReduce);
            UpdateOrRegisterSlot("Tremolo", preset.Tremolo > 0f, preset.Tremolo, static s => new TremoloEffect(s), preset.Tremolo);
            UpdateOrRegisterSlot("Glitch Burst", preset.Glitch > 0f, (preset.Glitch, sampleRate), static s => new GlitchBurstEffect(s.Glitch, s.sampleRate), preset.Glitch);
            UpdateOrRegisterSlot("Predatory Camouflage", preset.PredatoryCamouflage > 0f, (preset.PredatoryCamouflage, sampleRate), static s => new PredatoryCamouflageEffect(s.PredatoryCamouflage, s.sampleRate), preset.PredatoryCamouflage);

            // 4. Acoustic Respiration & Synthesis Layer
            UpdateOrRegisterSlot("Whisper Filter", preset.WhisperAmount > 0f, (preset.WhisperAmount, sampleRate), static s => new WhisperFilterEffect(s.WhisperAmount, s.sampleRate), preset.WhisperAmount);
            UpdateOrRegisterSlot("Breath Noise", preset.BreathNoise > 0f, (preset.BreathNoise, sampleRate), static s => new BreathNoiseEffect(s.BreathNoise, s.sampleRate), preset.BreathNoise);
            UpdateOrRegisterSlot("Static Noise", preset.StaticNoise > 0f, (preset.StaticNoise, sampleRate), static s => new StaticNoiseEffect(s.StaticNoise, s.sampleRate), preset.StaticNoise);

            // 5. Environmental Textures
            UpdateOrRegisterSlot("Dry Crackle", preset.DryCrackle > 0f, (preset.DryCrackle, sampleRate), static s => new DryCrackleEffect(s.DryCrackle, s.sampleRate), preset.DryCrackle);
            UpdateOrRegisterSlot("Flesh Crackle", preset.FleshCrackle > 0f, (preset.FleshCrackle, sampleRate), static s => new FleshCrackleEffect(s.FleshCrackle, s.sampleRate), preset.FleshCrackle);
            UpdateOrRegisterSlot("Stone Crack", preset.StoneCrack > 0f, (preset.StoneCrack, sampleRate), static s => new StoneCrackEffect(s.StoneCrack, s.sampleRate), preset.StoneCrack);
            UpdateOrRegisterSlot("Stone Grind", preset.StoneGrind > 0f, (preset.StoneGrind, sampleRate), static s => new StoneGrindEffect(s.StoneGrind, s.sampleRate), preset.StoneGrind);

            // 6. Digital & Aero-Acoustic Signals
            UpdateOrRegisterSlot("Chirp", preset.Chirp > 0f, (preset.Chirp, sampleRate), static s => new ChirpEffect(s.Chirp, s.sampleRate), preset.Chirp);
            UpdateOrRegisterSlot("Digital Data Burst", preset.DataBurst > 0f, (preset.DataBurst, sampleRate), static s => new DigitalDataBurstEffect(s.DataBurst, s.sampleRate), preset.DataBurst);
            UpdateOrRegisterSlot("Wet Organic", preset.WetOrganic > 0f, (preset.WetOrganic, sampleRate), static s => new WetOrganicEffect(s.WetOrganic, s.sampleRate), preset.WetOrganic);

            // 7. Space & Time-Domain FX
            UpdateOrRegisterSlot("Low-Pass Filter", preset.LowPass > 0f, (preset.LowPass, sampleRate), static s => new LowPassEffect(s.LowPass, s.sampleRate), preset.LowPass);
            UpdateOrRegisterSlot("High-Pass Filter", preset.HighPass > 0f, (preset.HighPass, sampleRate), static s => new HighPassEffect(s.HighPass, s.sampleRate), preset.HighPass);
            UpdateOrRegisterSlot("Wet Decay", preset.WetDecay > 0f, (preset.WetDecay, sampleRate), static s => new WetDecayEffect(s.WetDecay, s.sampleRate), preset.WetDecay);
            UpdateOrRegisterSlot("Pocket Dimension Echo", preset.PocketEcho > 0f, (preset.PocketEcho, sampleRate), static s => new PocketDimensionEchoEffect(s.PocketEcho, s.sampleRate), preset.PocketEcho);
            UpdateOrRegisterSlot("Reverb", preset.Reverb > 0f, (preset.Reverb, sampleRate), static s => new ReverbEffect(s.Reverb, s.sampleRate), preset.Reverb);

            Pipeline.UpdateEffects(_reusableEffectsList);

            ActiveNodes.Clear();
            foreach (var kvp in _temporaryMap)
            {
                ActiveNodes[kvp.Key] = kvp.Value;
            }
        }

        private void UpdateOrRegisterSlot<TEffect, TState>(
            string effectName,
            bool isActive,
            TState state,
            Func<TState, TEffect> factory,
            float runtimeValue) where TEffect : class, IAudioEffect
        {
            if (!isActive) return;

            if (ActiveNodes.TryGetValue(effectName, out var existingInstance))
            {
                if (existingInstance is IAdjustableAudioEffect adjustable)
                {
                    adjustable.AdjustParameter(runtimeValue);
                }

                _temporaryMap[effectName] = existingInstance;
                _reusableEffectsList.Add(existingInstance);
            }
            else
            {
                var newInstance = factory(state);
                _temporaryMap[newInstance.Name] = newInstance;
                _reusableEffectsList.Add(newInstance);
            }
        }
        #endregion

        public void Dispose()
        {
            SessionDecoder?.Dispose();
            SessionEncoder?.Dispose();
        }
    }
}