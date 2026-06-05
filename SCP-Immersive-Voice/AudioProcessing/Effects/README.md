# SCP Immersive Voice - DSP Effects Matrix

This directory contains the core real-time, float-native Digital Signal Processing (DSP) effects for the `SCP_Immersive_Voice` subsystem. 

## Architectural Principles
Every effect implemented in this pipeline adheres to strict **Game Audio Standards**:
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
* **Technical Implementation:** Delay-Line Crossfading Pitch Shifter (Doppler/Rotary method). Uses a power-of-two sized circular buffer with dynamic bitwise masking (`&`) wrapping. Features dual read heads operating 180° out of phase, modulated by a continuous Hann window for constant-power crossfading. Sample extraction uses 4-point **Cubic Hermite Spline** interpolation.
* **Acoustic Objective:** Alters vocal pitch natively in the time-domain *without* changing the playback speed or duration of the speech package, completely eliminating linear aliasing and metallic comb-filtering.

#### `FormantShiftEffect`
* **Unit/Scale:** Formant scale ratio (`0.5f` to `2.0f`).
* **Technical Implementation:** Cascaded 4-Band Biquad Resonator Matrix. It passes the signal sequentially through four custom-tuned peaking EQ structures representing the main human vocal tract resonances ($F_1 = 500\text{ Hz}$, $F_2 = 1500\text{ Hz}$, $F_3 = 2500\text{ Hz}$, $F_4 = 3500\text{ Hz}$).
* **Acoustic Objective:** Shifts the spectral envelope (timbre/resonance) independently of the fundamental pitch ($f_0$). Lowering formants simulates a massive gargantuan throat and rib cage (e.g., titan voices), while raising them shrinks the creature profile (e.g., goblin/gremlin effects).

#### `FormantDriftEffect`
* **Unit/Scale:** Modulation depth (`0.0f` to `1.0f`).
* **Technical Implementation:** Low-Frequency Oscillator (LFO) driven center-frequency modulator operating on the biquad formant matrices.
* **Acoustic Objective:** Introduces organic, dynamic instability to the creature’s throat resonance, preventing a static digital timbre and emulating a loss of muscular or psychological vocal control.

### `LaryngealAsymmetryEffect`
* **Unit/Scale**: Uncanny asymmetry intensity ($0.0\text{f}$ to $1.0\text{f}$).
* **Technical Implementation**: Splits the vocal stream into two parallel asymmetrical vocal tract paths, introducing a dynamic, sub-millisecond phase and delay drift ($0.15\text{ ms} - 1.45\text{ ms}$) modulated by an organic slow-frequency tissue LFO ($5.8\text{ Hz}$).
* **Acoustic Objective**: Simulates biological asymmetry in throat muscles, laryngeal walls, and glottis geometry. This breaks natural vocal symmetry, creating a subtle but highly unsettling "Uncanny Valley" comb-filtering effect—essential for deceptive human mimicry (SCP-939).
 
#### `DemonicOctaverEffect`
* **Unit/Scale:** Wet mix intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** Dual-head time-domain crossfading delay network using a power-of-two allocation scheme with fast bitwise wrapping. Features a sub-sample phase interpolator configured to drive a constant-power sub-octave layer (-12 semitones) without execution overhead.
* **Acoustic Objective:** Stacks a massive, clean, cinematic sub-bass layer beneath the voice fundamental to deliver profound demonic weight and nieludzką depth without modifying the original speech rate.

#### `VocalShriekShifterEffect`
* **Unit/Scale:** Multi-tap mix factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Multi-head time-domain phase dislocation transposer that executes a +14 semitone falsetto base shift while simultaneously stacking parallel +12 and +24 semitone higher harmonic channels. Utilizes sub-sample linear interpolation and an integrated XORShift pseudo-random noise matrix to inject granular phase instability (jitter).
* **Acoustic Objective:** Violently transposes standard microphone signals at the very entrance of the pipeline into an unhinged, tearing falsetto/shriek layer to synthesize flawless High Fry Screams.

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
* **Technical Implementation:** Voice-envelope-driven colored-noise synthesis using articulation-focused spectral shaping. Speech energy is tracked in real time and transferred onto a filtered unvoiced noise source through a band-limited whisper reconstruction stage. Constant-power dry/wet mixing preserves articulation while progressively suppressing vocal fold dominance.
* **Acoustic Objective:** Converts speech into a natural unvoiced whisper while preserving intelligibility and conversational clarity. Designed to remove the perception of vocal cord vibration without introducing radio-static artifacts or excessive synthetic noise.
* **Design Notes:**
  * General-purpose whisper synthesizer.
  * Not intended to emulate animalistic or creature vocalizations.
  * Optimized for real-time multiplayer voice transmission.
  * Maintains speech intelligibility significantly better than direct noise substitution techniques.

#### `PredatoryCamouflageEffect`

* **Unit/Scale:** Predatory camouflage intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** Voice-reactive turbulent airflow synthesis driven by a speech envelope follower. Multi-band biological resonance modeling combines throat friction, wet tissue turbulence, and high-frequency respiratory airflow textures. Synthetic excitation is generated from colored noise and dynamically shaped by vocal energy to preserve articulation while masking natural human phonation.
* **Acoustic Objective:** Transforms speech into a predatory biological vocal camouflage layer. Unlike a conventional whisper synthesizer, the effect preserves recognizable speech structure while introducing organic respiratory turbulence, throat friction, and animalistic vocal leakage. The result resembles a large living creature attempting to mimic speech through an unfamiliar vocal anatomy rather than a human producing a whispered voice.
* **Design Notes:**
  * Intended exclusively for SCP-939 vocal states.
  * Operates as a biological texture generator rather than a speech replacement system.
  * Designed to coexist with FormantShift, BreathNoise, WetOrganic, DeathRattle, and Guttural processing stages.
  * Preserves intelligibility significantly better than full unvoiced speech substitution.
  * Avoids radio-static artifacts commonly associated with naïve whisper synthesis approaches.
  * Maintains continuous internal state to ensure stable real-time voice transmission and natural acoustic continuity between processing frames.

#### `BreathNoiseEffect`
* **Unit/Scale:** Airflow intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** Low-pass filtered white noise shaped by an envelope follower with asymmetrical lag (fast attack, slow release).
* **Acoustic Objective:** Synthesizes realistic, predatory hyperventilation and air-flow rushing under the creature's voice during active speech.

#### `WetOrganicEffect`
* **Unit/Scale:** Fluid saturation factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Stateful parallel recursive feedback delay-line network combined with an ultra-low frequency (ULF) chaotic offset modulator to continuously warp resonance phase lengths.
* **Acoustic Objective:** Injects a visceral, saliva-choked, or blood-saturated texture directly over the audio buffer, modeling cellular breakdown, wet mucosal throat accumulation, or necrotic liquefaction.

#### `FleshCrackleEffect`
* **Unit/Scale:** Frequency/Density of micro-bursts (`0.0f` to `1.0f`).
* **Technical Implementation:** Stochastic impulse generator driven exponentially by the vocal energy level, generating rapid, high-frequency granular transients.
* **Acoustic Objective:** Emulates the horrific, wet sound of shifting tissues, flexing wet muscles, and squelching biology (e.g., shifting skin or flesh-monsters).

#### `DeathRattleEffect`
* **Unit/Scale:** Viscous choking intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** A voice-envelope driven stochastic LCG bubbling oscillator (operating in the sub-audio 14 Hz - 40 Hz band) modulating both the amplitude of the signal and the delay path of a 256-sample feedback comb-filter (0.5 ms - 2.2 ms delay bounds).
* **Acoustic Objective:** Models the acoustic profile of sound waves scattering through pooled liquids and necrotic fluids inside a reanimated trachea. Replaces majestic low-end doubling with a visceral, choking, fluid-filled gasping struggle for breath—essential for reanimated corpses.

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
* **Unit/Scale:** Avian syrinx penetration factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Stochastic triggering of exponential FM down-sweeps processed through a dual-band high-Q modal concrete resonator. Features real-time voice-envelope coupling to mimic biological vocal exertion.
* **Acoustic Objective:** Reproduces organic, high-frequency avian resonance and shrill, piercing shrieks. Primarily utilized for biological mimicry and creature-based vocalizations where surgical high-end precision is required.

---

### 3. Nonlinearity & Degradation
These processors distort the voice using analog-modeled saturation algorithms or simulated digital errors.

#### `DistortionEffect`
* **Unit/Scale:** Saturation drive (`0.0f` to `1.0f`).
* **Technical Implementation:** Polynomial soft-clipping waveshaper coupled with an auto-centering DC-blocking high-pass filter. 
* **Acoustic Objective:** Adds warm, aggressive analog grit or intense, tearing vocal strain without generating harsh, aliased square-wave clipping.

#### `BitcrushEffect`
* **Unit/Scale:** Quantization depth factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Mid-tread amplitude quantization loop that utilizes exponential mapping to reduce bit depth (from 16-bit down to a hard 2.5-bit limit), completely bypassing analog TPDF dithering to force harsh pixelated step boundaries. Paired with a stateful 1st-order recursive DC blocker filter and rational soft-clipping.
* **Acoustic Objective:** Introduces sterile, hard-edge digital aliasing and aggressive step-quantization noise, stripping all warmth from the audio signal to enforce a "pure binary" aesthetic.

#### `SiliconRingModulationEffect`
* **Unit/Scale:** Demodulation and resonance matrix mix (`0.0f` to `1.0f`).
* **Technical Implementation:** An inharmonic low-frequency pseudo-square carrier oscillator dynamically modulated by vocal root-mean-square (RMS) envelopes, coupled with a fixed 144-sample short-feedback delay line matrix acting as a physical comb filter.
* **Acoustic Objective:** Drastically dismantles organic human harmonic structures to produce an aggressive, cold, inharmonic metallic clang. Replicates the acoustic fingerprint of an evil AI entity speaking through the vibrating steel frame of an uninsulated server room cabinet.

#### `ScreechModulatorEffect`
* **Unit/Scale:** Screech intensity depth (`0.0f` to `1.0f`).
* **Technical Implementation:** Asymmetric high-frequency envelope-driven ring modulator operating a floating carrier sweep network (1300Hz to 2100Hz), wired sequentially into a 2nd-order peaking resonator filter hard-tuned to the extreme human ear vulnerability threshold center of 3150Hz.
* **Acoustic Objective:** Generates piercing, non-harmonic sideband distortion arrays and agonizing glass-shattering shrieks that translate screaming patterns into extreme physiological discomfort.

#### `DigitalDataBurstEffect`
* **Unit/Scale:** Cybernetic modulation intensity (`0.0f` to `1.0f`).
* **Technical Implementation:** A high-frequency asynchronous impulse engine that drives a metallic Biquad resonator (centered at 5800Hz) using asymmetric binary square-wave modulation. It utilizes a stochastic LCG-based trigger cascade to simulate rapid packet loss and data-bus overflows.
* **Acoustic Objective:** Generates pristine, cold, and calculated digital bursts (chirps) without analog noise floors. Used to emulate non-organic AI mainframe diagnostics, data-stream processing, and high-frequency binary communication patterns for digital entities like SCP-079.

#### `SampleRateReducerEffect`
* **Unit/Scale:** Downsampling factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Sample-and-hold step decorator that drops the effective sampling rate below the Nyquist limit.
* **Acoustic Objective:** Forces severe, bright digital **aliasing** mirroring low-bandwidth synthetic audio chips.

#### `GlitchBurstEffect`
* **Unit/Scale:** Instability factor (`0.0f` to `1.0f`).
* **Technical Implementation:** Micro-buffer capture array that randomly halts read pointer progress, looping small grains of audio or dropping frames completely based on an internal random distribution.
* **Acoustic Objective:** Replicates malfunctioning tech, hardware core freezing, or buffer underrun errors.

#### `StaticNoiseEffect`
* **Unit/Scale:** Interference amplitude (`0.0f` to `1.0f`).
* **Technical Implementation:** Continuous analog RF interference emulator generating multi-band filtered white noise intermixed with micro-burst discharge models.
* **Acoustic Objective:** Simulates environmental radio interference or bad signal connections.

#### `TremoloEffect`
* **Unit/Scale:** Modulation depth (`0.0f` to `1.0f`).
* **Technical Implementation:** Low-Frequency Oscillator (LFO) sinus or triangle wave modulating the global amplitude envelope. Fully synchronized and optimized for in-place scalar overrides via reflection field injection.
* **Acoustic Objective:** Creates an unstable, trembling vocal delivery—essential for mimicking panic, crying, or intense emotional fear (e.g., SCP-096 transitional states).

---

### 4. Acoustic Space & Filters
These modules define the physical boundaries, dampening environments, and environmental acoustic space where the sound is heard.

#### `LowPassEffect`
* **Unit/Scale:** Cutoff frequency in Hertz (`Hz`).
* **Technical Implementation:** 2nd-order critically-damped Biquad/Butterworth Low-Pass filter with a strict 12 dB/octave roll-off slope.
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