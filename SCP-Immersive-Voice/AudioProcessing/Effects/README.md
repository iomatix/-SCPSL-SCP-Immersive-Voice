# SCP Immersive Voice — DSP Effects Matrix

This directory contains the core real‑time, float‑native Digital Signal Processing (DSP) effects for the `SCP_Immersive_Voice` subsystem.

---

## Architectural Principles

All DSP modules follow strict **Game Audio Engineering Standards**:

- **In‑Place Processing** — Every effect implements `IAudioEffect` and operates directly on the native float PCM buffer (`void Process(float[] pcm, int length)`), avoiding redundant memory copies.
- **Zero Allocations** — No heap allocations occur inside real‑time audio loops. All buffers, filters, and state variables are created once and reused.
- **Persistent Statefulness (Cached DSP)** — Time‑domain and history‑based effects maintain per‑player state, preventing phase fractures, zipper noise, and transient artifacts.

---

## Effects Reference Manual

# 1. Core & Timbre Modifiers

These effects alter the biological or structural properties of the vocal tract and the source signal.

---

### `NoiseGateEffect`
- **Unit / Scale:** Decibels (`-96.0f` to `0.0f`)
- **Technical Implementation:** Stateful RMS/absolute envelope follower with exponential RC smoothing. Independent Attack (2 ms), Hold (100 ms), and Release (200 ms) stages.
- **Acoustic Objective:** Eliminates background noise amplified by AGC, preventing tail‑noise contamination of downstream DSP stages.

---

### `PitchShiftEffect`
- **Unit / Scale:** Frequency multiplier (`0.25f` to `4.0f`)
- **Technical Implementation:** Delay‑line crossfading pitch shifter using a power‑of‑two circular buffer, dual read heads (180° offset), Hann‑windowed constant‑power crossfading, and 4‑point Cubic Hermite interpolation.
- **Acoustic Objective:** Shifts pitch in the time domain without altering speech duration, avoiding metallic aliasing and comb‑filtering artifacts.

---

### `FormantShiftEffect`
- **Unit / Scale:** Formant ratio (`0.5f` to `2.0f`)
- **Technical Implementation:** Cascaded 4‑band Biquad resonator matrix tuned to human vocal tract resonances (500 Hz, 1500 Hz, 2500 Hz, 3500 Hz).
- **Acoustic Objective:** Alters spectral envelope independently of pitch. Lower ratios simulate massive chest cavities; higher ratios create small, sharp vocal profiles.

---

### `FormantDriftEffect`
- **Unit / Scale:** Modulation depth (`0.0f` to `1.0f`)
- **Technical Implementation:** LFO‑driven center‑frequency modulation applied to formant resonators.
- **Acoustic Objective:** Adds organic instability and prevents static digital timbre.

---

### `LaryngealAsymmetryEffect`
- **Unit / Scale:** Asymmetry intensity (`0.0f` to `1.0f`)
- **Technical Implementation:** Splits the vocal stream into two asymmetrical vocal‑tract paths. Introduces dynamic sub‑millisecond phase/delay drift (0.15–1.45 ms) modulated by a 5.8 Hz tissue‑LFO.
- **Acoustic Objective:** Simulates biological asymmetry in throat muscles and glottal geometry, producing an uncanny comb‑filtering effect essential for SCP‑939 mimicry.

---

### `DemonicOctaverEffect`
- **Unit / Scale:** Wet mix (`0.0f` to `1.0f`)
- **Technical Implementation:** Dual‑head crossfading delay network generating a clean sub‑octave layer (‑12 semitones) with minimal overhead.
- **Acoustic Objective:** Adds deep, cinematic low‑end weight without altering speech rate.

---

### `VocalShriekShifterEffect`
- **Unit / Scale:** Multi‑tap mix (`0.0f` to `1.0f`)
- **Technical Implementation:** Multi‑head phase‑dislocation transposer (+14, +12, +24 semitone layers) with XORShift‑driven granular jitter.
- **Acoustic Objective:** Produces aggressive falsetto/shriek textures suitable for high‑intensity creature screams.

---

### `SubharmonicGrowlEffect`
- **Unit / Scale:** Wet mix (`0.0f` to `1.0f`)
- **Technical Implementation:** Phase‑locked subharmonic generator producing a clean `f0 / 2` signal via rectification and steep low‑pass filtering.
- **Acoustic Objective:** Adds chest‑rattling low‑end ideal for monstrous growls.

---

### `GutturalResonanceEffect`
- **Unit / Scale:** Texture intensity (`0.0f` to `1.0f`)
- **Technical Implementation:** Short‑delay comb filtering combined with asymmetric waveshaping.
- **Acoustic Objective:** Adds biological rasp and false‑vocal‑fold vibration.

---

# 2. Biological & Material Textures

These modules generate synthetic acoustic layers driven by amplitude envelopes.

---

### `WhisperFilterEffect`
- **Unit / Scale:** Whisper intensity (`0.0f` to `1.0f`)
- **Technical Implementation:** Envelope‑driven colored‑noise synthesis with articulation‑preserving spectral shaping.
- **Acoustic Objective:** Converts voiced speech into natural whispering while maintaining intelligibility.
- **Design Notes:**  
  - General‑purpose whisper synthesis  
  - No radio‑static artifacts  
  - Optimized for real‑time multiplayer  

---

### `PredatoryCamouflageEffect`
- **Unit / Scale:** Camouflage intensity (`0.0f` to `1.0f`)
- **Technical Implementation:** Multi‑band biological turbulence model combining throat friction, wet tissue noise, and airflow textures.
- **Acoustic Objective:** Produces SCP‑939‑style predatory mimicry with preserved articulation.
- **Design Notes:**  
  - SCP‑939 exclusive  
  - Coexists with formant, breath, wet, and guttural layers  
  - Maintains intelligibility  

---

### `BreathNoiseEffect`
- **Unit / Scale:** Airflow intensity (`0.0f` to `1.0f`)
- **Technical Implementation:** Low‑pass filtered noise shaped by asymmetric envelope lag.
- **Acoustic Objective:** Adds hyperventilation and airflow textures.

---

### `WetOrganicEffect`
- **Unit / Scale:** Fluid saturation (`0.0f` to `1.0f`)
- **Technical Implementation:** Recursive delay‑line network with ULF chaotic modulation.
- **Acoustic Objective:** Simulates saliva, blood, or necrotic fluid accumulation.

---

### `FleshCrackleEffect`
- **Unit / Scale:** Burst density (`0.0f` to `1.0f`)
- **Technical Implementation:** Stochastic impulse generator producing granular transients.
- **Acoustic Objective:** Emulates shifting wet tissue and muscle movement.

---

### `DeathRattleEffect`
- **Unit / Scale:** Choking intensity (`0.0f` to `1.0f`)
- **Technical Implementation:** Sub‑audio bubbling oscillator (14–40 Hz) modulating amplitude and a 256‑sample comb delay.
- **Acoustic Objective:** Models fluid‑filled necrotic breathing.

---

### `DryCrackleEffect`
- **Unit / Scale:** Granular density (`0.0f` to `1.0f`)
- **Technical Implementation:** Sparse impulse generator with sharp transients.
- **Acoustic Objective:** Simulates dried bone friction and ligament cracking.

---

### `StoneCrackEffect`
- **Unit / Scale:** Fracture probability (`0.0f` to `1.0f`)
- **Technical Implementation:** Peak‑triggered transient generator.
- **Acoustic Objective:** Simulates brittle structural cracking.

---

### `StoneGrindEffect`
- **Unit / Scale:** Friction floor (`0.0f` to `1.0f`)
- **Technical Implementation:** Low‑frequency granular texture tracking vocal amplitude.
- **Acoustic Objective:** Emulates heavy stone or concrete grinding.

---

### `ChirpEffect`
- **Unit / Scale:** Syrinx factor (`0.0f` to `1.0f`)
- **Technical Implementation:** Stochastic FM down‑sweeps through high‑Q resonators.
- **Acoustic Objective:** Produces avian‑like shrill chirps and biological shrieks.

---

# 3. Nonlinearity & Degradation

These processors introduce analog‑style saturation or digital degradation.

---

### `DistortionEffect`
- **Unit / Scale:** Drive (`0.0f` to `1.0f`)
- **Technical Implementation:** Polynomial soft‑clipper with DC‑blocking.
- **Acoustic Objective:** Adds warm or aggressive grit without harsh aliasing.

---

### `BitcrushEffect`
- **Unit / Scale:** Quantization depth (`0.0f` to `1.0f`)
- **Technical Implementation:** Mid‑tread quantizer reducing bit depth to ~2.5 bits, paired with DC blocking.
- **Acoustic Objective:** Produces harsh digital aliasing and step quantization.

---

### `SiliconRingModulationEffect`
- **Unit / Scale:** Mix (`0.0f` to `1.0f`)
- **Technical Implementation:** Inharmonic pseudo‑square carrier with a 144‑sample comb matrix.
- **Acoustic Objective:** Creates cold, metallic, AI‑like inharmonicity.

---

### `ScreechModulatorEffect`
- **Unit / Scale:** Screech depth (`0.0f` to `1.0f`)
- **Technical Implementation:** High‑frequency ring modulation (1300–2100 Hz) into a 3150 Hz resonator.
- **Acoustic Objective:** Generates piercing, glass‑shattering shrieks.

---

### `DigitalDataBurstEffect`
- **Unit / Scale:** Modulation intensity (`0.0f` to `1.0f`)
- **Technical Implementation:** High‑frequency impulse engine driving a 5800 Hz resonator with LCG‑based triggers.
- **Acoustic Objective:** Produces clean digital chirps for AI‑style communication.

---

### `SampleRateReducerEffect`
- **Unit / Scale:** Downsampling factor (`0.0f` to `1.0f`)
- **Technical Implementation:** Sample‑and‑hold downsampler.
- **Acoustic Objective:** Introduces bright aliasing and low‑bandwidth artifacts.

---

### `GlitchBurstEffect`
- **Unit / Scale:** Instability (`0.0f` to `1.0f`)
- **Technical Implementation:** Micro‑buffer capture with random pointer halts and frame drops.
- **Acoustic Objective:** Emulates hardware underruns and digital malfunction.

---

### `StaticNoiseEffect`
- **Unit / Scale:** Noise amplitude (`0.0f` to `1.0f`)
- **Technical Implementation:** Multi‑band RF noise generator with burst discharges.
- **Acoustic Objective:** Simulates radio interference and signal corruption.

---

### `TremoloEffect`
- **Unit / Scale:** Modulation depth (`0.0f` to `1.0f`)
- **Technical Implementation:** LFO‑driven amplitude modulation.
- **Acoustic Objective:** Produces trembling, unstable vocal delivery.

---

# 4. Acoustic Space & Filters

These modules define environmental acoustics and spatial coloration.

---

### `LowPassEffect`
- **Unit / Scale:** Cutoff frequency (Hz)
- **Technical Implementation:** 2nd‑order Butterworth low‑pass filter (12 dB/oct).
- **Acoustic Objective:** Simulates muffling through dense materials or heavy coverings.

---

### `HighPassEffect`
- **Unit / Scale:** Cutoff frequency (Hz)
- **Technical Implementation:** 2nd‑order high‑pass filter.
- **Acoustic Objective:** Removes rumble and proximity bass; simulates thin intercom speakers.

---

### `ReverbEffect`
- **Unit / Scale:** Decay / Mix (`0.0f` to `1.0f`)
- **Technical Implementation:** 4×4 Feedback Delay Network with Householder diffusion and absorption filtering.
- **Acoustic Objective:** Models large chambers, tunnels, and metallic vaults.

---

### `PocketDimensionEchoEffect`
- **Unit / Scale:** Spatial dislocation (`0.0f` to `1.0f`)
- **Technical Implementation:** Nested all‑pass delay matrix with chaotic phase inversion.
- **Acoustic Objective:** Produces impossible, non‑Euclidean echo spaces.

---

### `WetDecayEffect`
- **Unit / Scale:** Absorption factor (`0.0f` to `1.0f`)
- **Technical Implementation:** High‑frequency dampened delay loop.
- **Acoustic Objective:** Creates damp, claustrophobic, liquid‑coated acoustic environments.
