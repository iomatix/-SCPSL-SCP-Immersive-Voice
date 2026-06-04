# SCP Immersive Voice - DSP Effects Matrix

This directory contains the core real-time, float-native Digital Signal Processing (DSP) effects for the `SCP_Immersive_Voice` subsystem. 

## Architectural Principles
Every effect implemented in this pipeline adheres to the strict **AAA Game Audio Standards**:
* **In-Place Processing:** Every effect implements `IAudioEffect` and operates directly on the native float PCM buffer (`void Process(float[] pcm, int length)`) to avoid redundant memory copies.
* **Zero Allocations:** No heap allocations (`new`) are permitted inside the critical audio processing loops. All state variables, filters, and rings are persistent and stack/heap-allocated only during instantiation.
* **Persistent Statefulness (Cached-DSP):** Effects that rely on time-domain or history data (delays, filters, envelopes) maintain their state context *per player* inside a persistent cache. They are never instantiated per-frame, eliminating phase fractures, zipper noise, and click artifacts.

---

## Effects Reference Manual

### 1. Core & Timbre Modifiers
These effects alter the fundamental biological or structural properties of the creature's vocal tract and source signal.

#### `NoiseGateEffect`
* **Unit/Scale:** Decibels (`dB`) ranging from `-96.0f` to `0.0f`.
* **Technical Implementation:** Studio-grade stateful noise gate utilizing an RMS/Absolute envelope follower with exponential RC filter smoothing. Implements dedicated, independent **Attack** (2ms), **Hold** (100ms), and **Release** (200ms) time multipliers.
* **Acoustic Objective:** Acts as the primary pipeline guard. It completely silences background noise and breathing artifacts generated when the Auto Gain Control (AGC) expands a player's silent frames, preventing tail noise from corrupting downstream effects.

#### `PitchShiftEffect`
* **Unit/Scale:** Frequency multiplier ratio (`0.25f` to `4.0f`).
* **Technical Implementation:** Delay-Line Crossfading Pitch Shifter (Doppler/Rotary method). Uses a power-of-two sized circular buffer with dynamic bitwise masking (`&`) wrapping. Features dual read heads operating $180^\circ$ out of phase, modulated by a continuous Hann window for constant-power crossfading. Sample extraction uses 4-point **Cubic Hermite Spline** interpolation.
* **Acoustic Objective:** Alters vocal pitch natively in the time-domain *without* changing the playback speed or duration of the speech package, completely eliminating linear aliasing and metallic comb-filtering.

#### `FormantShiftEffect`
* **Unit/Scale:** Formant scale ratio (`0.5f` to `2.0f`).
* **Technical Implementation:** Cascaded 4-Band Biquad Resonator Matrix. It passes the signal sequentially through four custom-tuned peaking EQ structures representing the main human vocal tract resonances ($F_1 = 500\text{ Hz}$, $F_2 = 1500\text{ Hz}$, $F_3 = 2500\text{ Hz}$, $F_4 = 3500\text{ Hz}$).
* **Acoustic Objective:** Shifts the spectral envelope (timbre/resonance) independently of the fundamental pitch ($f_0$). Lowering formants simulates a massive gargantuan throat and rib cage (e.g., titan voices), while raising them shrinks the creature profile (e.g., goblin/gremlin effects).

#### `FormantDriftEffect`
* **Unit/Scale:** Modulation depth (`0.0f` to `1.0f`).
* **Technical Implementation:** Low-Frequency Oscillator (LFO) driven center-frequency modulator operating on the biquad formant matrices.
* **Acoustic Objective:** Introduces organic, dynamic instability to the creature’s throat resonance, preventing a static digital timbre and emulating a loss of muscular or psychological vocal control.

#### `SubharmonicGrowlEffect`
* **Unit/Scale:** Wet mix intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** Phase-locked subharmonic frequency divider. Tracks the dominant pitch fundamental and synthesizes a sub-octave ($f_0 / 2$) signal using half-wave rectification or zero-crossing sync combined with a steep low-pass filter.
* **Acoustic Objective:** Generates pristine, chest-rattling cinematic low-end frequencies that natively tracking the player's voice pitch—perfect for demonic growls and monstrous roars.

#### `GutturalResonanceEffect`
* **Unit/Scale:** Texture intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** Short-delay feedback comb-filtering combined with an asymmetric waveshaper to emulate vocal cord rasp and ventricular fold (false vocal cord) vibration.
* **Acoustic Objective:** Adds a biological, chropowaty, predator-like growling texture directly inside the larynx structure of the voice.

---

### 2. Biological & Material Textures
These modules generate synthetic acoustic layers layered directly over the speech stream, dynamically driven by an amplitude envelope follower.

#### `WhisperFilterEffect`
* **Unit/Scale:** Whisper substitution intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** Amplitude-modulated pink noise combined with a spectral voice tracking bandpass filter matrix that strips out harmonic voiced chords.
* **Acoustic Objective:** Converts speech into an eerie, completely unvoiced, breathy szept, keeping articulation and intelligibility high while stripping out human vocal chord vibration.

#### `BreathNoiseEffect`
* **Unit/Scale:** Airflow intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** Low-pass filtered white noise shaped by an envelope follower with asymmetrical lag (fast attack, slow release).
* **Acoustic Objective:** Synthesizes realistic, predatory hyperventilation and air-flow rushing under the creature's voice during active speech.

#### `FleshCrackleEffect`
* **Unit/Scale:** Frequency/Density of micro-bursts (`0.0f` to `1.0f`).
* **Technical Implementation:** Stochastic impulse generator driven exponentially by the vocal energy level, generating rapid, high-frequency granular transients.
* **Acoustic Objective:** Emulates the horrific, wet sound of shifting tissues, flexing wet muscles, and squelching biology (e.g., shifting skin or flesh-monsters).

#### `DryCrackleEffect`
* **Unit/Scale:** Granular density (`0.0f` to `1.0f`).
* **Technical Implementation:** High-Q sparse impulse generator producing erratic, sharp, low-energy transient crackles.
* **Acoustic Objective:** Simulates necrotic, dried, mummified bone friction, or cracking ligaments (e.g., decayed undead).

#### `StoneCrackEffect`
* **Unit/Scale:** Fracture probability (`0.0f` to `1.0f`).
* **Technical Implementation:** Peak-amplitude triggered physical-modeling transient generator that injects steep, high-energy acoustic fracture models.
* **Acoustic Objective:** Simulates brittle structural cracking under extreme vocal strain (e.g., living concrete fracturing).

#### `StoneGrindEffect`
* **Unit/Scale:** Friction noise floor (`0.0f` to `1.0f`).
* **Technical Implementation:** Low-frequency bandpass-filtered granular texture generator that tracks vocal amplitude.
* **Acoustic Objective:** Simulates the heavy friction, mass, and grinding of tectonic material or sliding concrete blocks.

#### `ChirpEffect`
* **Unit/Scale:** Chirp rate / Resonance (`0.0f` to `1.0f`).
* **Technical Implementation:** High-frequency sinusoidal micro-sweeps triggered by voice transjents, modeling avian syrinx physics.
* **Acoustic Objective:** Injects erratic bird-like anomalies or clicks directly into the vocal delivery.

---

### 3. Nonlinearity & Degradation
These processors distort the voice using analog-modeled saturation algorithms or simulated digital errors.

#### `DistortionEffect`
* **Unit/Scale:** Saturation drive (`0.0f` to `1.0f`).
* **Technical Implementation:** Polynomial soft-clipping waveshaper coupled with an auto-centering DC-blocking high-pass filter. 
* **Acoustic Objective:** Adds warm, aggressive analog grit or intense, tearing vocal strain without generating harsh, aliased square-wave clipping.

#### `BitcrushEffect`
* **Unit/Scale:** Down-quantization factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Mid-tread amplitude quantization loop that reduces sample depth down to arbitrary bit boundaries (e.g., 2-bit to 8-bit).
* **Acoustic Objective:** Introduces cold digital quantization noise, replicating legacy hardware or degraded AD/DA converters.

#### `SampleRateReducerEffect`
* **Unit/Scale:** Downsampling factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Sample-and-hold step decorator that drops the effective sampling rate below the Nyquist limit.
* **Acoustic Objective:** Forces severe, bright digital **aliasing** mirroring low-bandwidth synthetic audio chips.

#### `GlitchBurstEffect`
* **Unit/Scale:** Instability factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Micro-buffer capture array that randomly halts read pointer progress, looping small grains of audio or dropping frames completely based on a internal random distribution.
* **Acoustic Objective:** Replicates malfunctioning tech, hardware core freezing, or buffer underrun errors.

#### `StaticNoiseEffect`
* **Unit/Scale:** Interference amplitude (`0.0f` to `1.0f`).
* **Technical Implementation:** Continuous analog RF interference emulator generating multi-band filtered white noise intermixed with micro-burst discharge models.
* **Acoustic Objective:** Simulates environment radio interference or bad signal connections.

#### `TremoloEffect`
* **Unit/Scale:** Modulation depth (`0.0f` to `1.0f`).
* **Technical Implementation:** Low-Frequency Oscillator (LFO) sinus or triangle wave modulating the global amplitude envelope.
* **Acoustic Objective:** Creates an unstable, trembling vocal delivery—essential for mimicking panic, crying, or intense emotional fear.

---

### 4. Acoustic Space & Filters
These modules define the physical boundaries, dampening environments, and environmental acoustic space where the sound is heard.

#### `LowPassEffect`
* **Unit/Scale:** Cutoff frequency in Hertz (`Hz`).
* **Technical Implementation:** 2nd-order critically-damped Biquad/Butterworth Low-Pass filter with a strict $12\text{ dB/octave}$ roll-off slope.
* **Acoustic Objective:** Eliminates high-frequency content. Replicates acoustic muffling caused by heavy clothing, leather masks, skin thickness, or transmission through dense physical boundaries like concrete walls.

#### `HighPassEffect`
* **Unit/Scale:** Cutoff frequency in Hertz (`Hz`).
* **Technical Implementation:** 2nd-order Biquad High-Pass filter designed to remove low frequencies.
* **Acoustic Objective:** Strips out muddy rumbling and excessive proximity bass-boost from proximity mics, or replicates tiny, thin intercom speakers when set to aggressive thresholds.

#### `ReverbEffect`
* **Unit/Scale:** Decay time / Mix (`0.0f` to `1.0f`).
* **Technical Implementation:** 4x4 Feedback Delay Network (FDN) utilizing a unitary householder matrix for optimal echo density diffusion, combined with low-pass absorption dampening.
* **Acoustic Objective:** Replicates the natural acoustic environment of huge containment chambers, echoing underground concrete tunnels, and metal vaults.

#### `PocketDimensionEchoEffect`
* **Unit/Scale:** Spatial dislocation (`0.0f` to `1.0f`).
* **Technical Implementation:** Dual-channel all-pass nested delay line matrix with chaotic phase-inversion feedback and cross-channel delay modulation.
* **Acoustic Objective:** Creates an unnatural, non-Euclidean, physically impossible space—ideal for capturing the detached, terrifying echoes of the Pocket Dimension.

#### `WetDecayEffect`
* **Unit/Scale:** Absorption factor (`0.0f` to `1.0f`).
* **Technical Implementation:** High-frequency dampening delay line loop modeling extreme acoustic absorption by liquid-coated walls.
* **Acoustic Objective:** Creates a claustrophobic, damp, dripping, and suffocating echo character (e.g., flooded or slime-covered rooms).