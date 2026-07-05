# SCP Immersive Voice — Core Real-Time DSP Effects Matrix

This directory contains the production-grade, float-native Digital Signal Processing (DSP) effects matrix for the `SCP_Immersive_Voice` subsystem. These components handle high-frequency voice streaming packets natively, transforming human speech into anomalous, non-human, and cybernetic acoustic profiles in real time.

---

## 🏗️ Architectural Optimization Manifest

Following a comprehensive low-level systems refactor, every engine module conforms to an elite suite of high-performance real-time audio standards:

* **In-Place Allocationless Processing** — All modules inherit from `IAudioEffect` and manipulate the incoming raw PCM sample array natively via `void Process(float[] pcm, int length)`. Memory footprints remain perfectly flat.
* **Stack-Local Register Isolation** — Volatile heap properties, LCG counters, and value-type filter structures are mirrored onto stack registers immediately prior to entering processing loops. This eliminates L1/L2 cache line pointer chasing (**Field Thrashing**), granting the JIT compiler room for loop unrolling.
* **Loop Invariant Code Motion (LICM)** — Constant gain stages, wet/dry blend matrices, and static conditional boundaries are computed once outside the loop sweep block to preserve CPU cycles.
* **Float-Native SIMD Execution** — All standard 64-bit mathematical calculations (`double` precision routines like `Math.Sin`, `Math.Cos`, `Math.Exp`, `Math.Pow`, `Math.Tanh`) have been stripped from hot-paths. They are replaced by 32-bit float-native alternatives (`Mathf`) or custom 3rd-order algebraic polynomial approximations.
* **Branch Prediction Optimization** — High-overhead dynamic loops (`while` expressions) used for delay lines or phase wrapping are replaced by hardware-friendly, single-branch conditional offset assignments.

---

## 🎛️ Reference Manual

### 1. Core Vocal Tract & Timbre Modifiers

---

#### `NoiseGateEffect.cs`

* **Unit / Scale:** Decibels (**-96.0f** to **0.0f**)
* **Technical Implementation:** Energy-based **RMS Envelope Tracking** follower using squared input power evaluations rather than raw voltage peaks. Features independent Attack (2 ms), Hold (120 ms), and Release (200 ms) stages. Integrates seamlessly with `MathExtensions.DbToLinear()`.
* **Acoustic Objective:** Eliminates background room floor noise and amplifier hiss. Protects vocal sibilants and quieter consonants without gate chattering or stuttering during zero-crossings.

---

#### `PitchShiftEffect.cs`

* **Unit / Scale:** Frequency scaling factor (**0.25f** to **4.0f**)
* **Technical Implementation:** Dual-head crossfading time-domain delay network using a power-of-two circular buffer. Features a branch-optimized **180° phase offset**, constant-power Hann window weighting, and an aggressively inlined **4-point Cubic Hermite Spline** interpolation block.
* **Acoustic Objective:** Shifts vocal pitch instantly without changing time duration, preventing metallic aliasing, phase fractures, or zipper noise.

---

#### `FormantShiftEffect.cs`

* **Unit / Scale:** Spectral warp ratio (**0.5f** to **2.0f**)
* **Technical Implementation:** Cascaded 4-band **Biquad Resonator Matrix** mapped directly to human vocal tract resonance points:
* **F1 (500 Hz):** Throat resonance
* **F2 (1500 Hz):** Mouth cavity resonance
* **F3 (2500 Hz):** Nasal / Palate resonance
* **F4 (3500 Hz):** Vocal presence resonance


* **Acoustic Objective:** Rescales the acoustic cavity envelope independently of the fundamental pitch. Lower settings simulate massive chest cavities (monstrous entities); higher settings create tight, pinched, goblin-like vocal profiles.

---

#### `FormantDriftEffect.cs`

* **Unit / Scale:** Modulation depth (**0.0f** to **1.5f**)
* **Technical Implementation:** Bounded non-linear coefficient morphing driven by a low-frequency oscillator with an integrated ultra-fast bitwise LCG jitter engine. Core features a 3rd-order algebraic tanh approximation for laryngeal soft clipping.
* **Acoustic Objective:** Simulates involuntary organic throat muscle contractions, introducing subtle timbre drift to eliminate digital static flatness.

---

#### `LaryngealAsymmetryEffect.cs`

* **Unit / Scale:** Asymmetry intensity (**0.0f** to **1.0f**)
* **Technical Implementation:** Splits the voice data into two parallel, highly asymmetrical vocal-tract paths. Introduces a sub-millisecond phase drift (**0.15 ms to 1.45 ms**) driven by an accurate **5.8 Hz circular tissue LFO** and sub-sample linear interpolation.
* **Acoustic Objective:** Generates a predatory, complex comb-filtering effect that sits firmly in the **Uncanny Valley**, essential for SCP-939 voice mimicry deception.

---

#### `DemonicOctaverEffect.cs`

* **Unit / Scale:** Wet mix intensity (**0.0f** to **1.0f**)
* **Technical Implementation:** Dual-head time-domain crossfading delay network operating with bitwise wrapping boundaries (`BufferSize = 8192`). Phase registers move at exactly **0.5x speed** to generate a clean sub-octave layer (**-12 semitones**).
* **Acoustic Objective:** Blends a massive, constant-power sub-bass layer underneath the dry signal, adding monumental cinematic weight to rogue containment breaches.

---

#### `VocalShriekShifterEffect.cs`

* **Unit / Scale:** Multi-tap mix intensity (**0.0f** to **1.0f**)
* **Technical Implementation:** Multi-head phase-dislocation transposer tracking three distinct parallel layers:
* **Layer 1:** +14 Semitones Falsetto (2.2449x read speed)
* **Layer 2:** +12 Semitones Upper Octave (2.0x read speed)
* **Layer 3:** +24 Semitones Double Octave (4.0x read speed)


* Driven by a high-speed bitwise **XORShift PRNG phase-instability matrix** and a soft-limiter knee block.
* **Acoustic Objective:** Synthesizes aggressive, tearing, non-periodic falsetto high-fry screech layers for terrifying creature screams.

---

#### `SubharmonicGrowlEffect.cs`

* **Unit / Scale:** Wet mix intensity (**0.0f** to **1.5f**)
* **Technical Implementation:** Phase-locked time-domain frequency divider using a **Zero-Crossing Flip-Flop** state machine. Signals pass through a low fundamental analysis filter (**130 Hz low-pass**), a subharmonic reconstruction filter (**75 Hz low-pass**), and a polynomial soft-clipping growl clipper.
* **Acoustic Objective:** Generates a perfect, tracking subharmonic layer ($f_0 / 2$) to add chest-rattling low-end to monstrous entities.

---

#### `GutturalResonanceEffect.cs`

* **Unit / Scale:** Rasp texture intensity (**0.0f** to **1.5f**)
* **Technical Implementation:** Modulated non-linear feedback comb filter coupled with a **1st-order High-Pass DC Blocker**. Uses an envelope-driven chaotic LFO to modulate fractional delay lines inside a 1024-sample array via linear interpolation.
* **Acoustic Objective:** Replicates ventricular fold (false vocal cord) vibrations, adding a distinct biological rasping texture to creature vocals.

---

### 2. Biological & Material Textures

---

#### `WhisperFilterEffect.cs`

* **Unit / Scale:** Whisper intensity (**0.0f** to **1.0f**)
* **Technical Implementation:** Completely de-voices the input stream by tracking the vocal amplitude envelope and applying it to a multi-pole **Voss-McCartney Pink Noise generator**. Signals filter through an articulation bandpass filter (**2800 Hz**) and an air turbulence filter (**4500 Hz**) before blending with a **0.18x conversational speech residue**.
* **Acoustic Objective:** Synthesizes natural, unvoiced breath whispers while protecting speech intelligibility and word recognition.

---

#### `PredatoryCamouflageEffect.cs`

* **Unit / Scale:** Camouflage intensity (**0.0f** to **1.0f**)
* **Technical Implementation:** Specialized multi-band biological turbulence engine running three parallel biquad bandpass filters driven by an absolute envelope follower:
* **Deep Throat Friction:** Centered at 850 Hz ($Q = 0.7$)
* **Wet Mouth Tissue / Saliva:** Centered at 1800 Hz ($Q = 0.8$)
* **Airflow Turbulence:** Centered at 4200 Hz ($Q = 0.6$)


* **Acoustic Objective:** Designed exclusively for SCP-939. Generates a predatory breathing camouflage layer while maintaining high communication readability.

---

#### `BreathNoiseEffect.cs`

* **Unit / Scale:** Airflow velocity (**0.0f** to **1.0f**)
* **Technical Implementation:** High-speed LCG white noise generator modulated by an asymmetric envelope lag network. Low-pass filters shape the output to model air friction.
* **Acoustic Objective:** Introduces realistic inhalation/exhalation layers and frantic hyperventilation noises driven by vocal effort.

---

#### `WetOrganicEffect.cs`

* **Unit / Scale:** Fluid tissue saturation (**0.0f** to **1.5f**)
* **Technical Implementation:** Sub-millisecond fractional micro-delay loop coupled with a **3200 Hz Biquad High-Pass Filter**. Features an ultra-fast local LCG that acts as a stochastic saliva bubble/pop generator, driving bidirectional dynamic triggers.
* **Acoustic Objective:** Simulates the sound of moist, living vocal tract tissue, creating realistic saliva and fluid mechanics.

---

#### `FleshCrackleEffect.cs`

* **Unit / Scale:** Tissue split density (**0.0f** to **1.5f**)
* **Technical Implementation:** Stochastic impulse engine running a fast bitwise LCG generator. Higher vocal envelope thresholds exponentially scale the probability of generating a sharp bidirectional transient spike, which then excites a wet biological trace resonator (**1600 Hz bandpass, $Q = 4.5$**).
* **Acoustic Objective:** Replicates the sound of tearing flesh, shifting muscle tissue, or fluid-coated cellular snapping.

---

#### `DeathRattleEffect.cs`

* **Unit / Scale:** Agonal choking intensity (**0.0f** to **1.0f**)
* **Technical Implementation:** Low-frequency sub-audio bubbling oscillator (**14 Hz to 40 Hz**) that modulates sample amplitude alongside a short 256-sample feedback comb delay line.
* **Acoustic Objective:** Emulates necrotic breathing patterns and agonal choking.

---

#### `DryCrackleEffect.cs`

* **Unit / Scale:** Granular bone friction density (**0.0f** to **1.5f**)
* **Technical Implementation:** High-frequency, sparse stochastic impulse generator. Randomly timed bipolar clicks are passed through a stack-allocated **Biquad High-Pass Filter nastrojony na 4000 Hz** ($Q = 1.0$).
* **Acoustic Objective:** Replicates dry bone friction, clicking ligaments, or fracturing skeletal joints (essential for entities like SCP-3114).

---

#### `StoneCrackEffect.cs`

* **Unit / Scale:** Fault line fracture intensity (**0.0f** to **2.0f**)
* **Technical Implementation:** Macro-fracture structural engine that models long physical concrete failure cascades (**60 ms to 200 ms**). Spaced impulse streams (**25 ms to 115 ms time-gaps**) excite a dual biquad resonator matrix:
* **Deep Concrete Mass:** Centered at 220 Hz ($Q = 38.0$) for a heavy thud.
* **Surface Cleave Crack:** Centered at 1100 Hz ($Q = 16.0$) for mineral crispness.


* Features a **Vocal Shredder Cross-Modulation** step that actively uses crack energy to cancel out the phase of the dry human voice.
* **Acoustic Objective:** Replicates the sound of structural concrete cracking and material failure, designed for SCP-173.

---

#### `StoneGrindEffect.cs`

* **Unit / Scale:** Mineral friction floor (**0.0f** to **2.0f**)
* **Technical Implementation:** Physical **Stick-Slip Macro-Modeling engine** that replaces continuous thermal white noise with a discrete, interlocking mineral crystal ridge shear matrix. Contains a **Sub-bass Tectonic Rumble filter** (110 Hz bandpass) and an **Abrasive Aggregate Scratch filter** (750 Hz bandpass) combined with asymmetric ring modulation driven by a **4.2 Hz surface macro-fault LFO**.
* **Acoustic Objective:** Replicates heavy stone grinding and concrete friction, stripping human vocal identity and replacing it with raw mineral grinding textures (designed for SCP-173).

---

#### `ChirpEffect.cs`

* **Unit / Scale:** Syrinx modulation factor (**0.0f** to **1.0f**)
* **Technical Implementation:** High-speed frequency modulation (FM) synthesizer that executes rapid down-sweeps through a series of high-Q tracking resonators.
* **Acoustic Objective:** Generates bird-like chirping, sharp avian clicks, and frantic alien communication layers.

---

### 3. Nonlinearity, Saturation & System Degradation

---

#### `DistortionEffect.cs`

* **Unit / Scale:** Input saturation drive (**0.0f** to **1.0f**)
* **Technical Implementation:** Analog-modeled **Asymmetric Polynomial Triode Waveshaper**. Shapes positive and negative signal peaks differently to harvest warm, even harmonics:
* **Positive Peaks:** Soft exponential curve ($x / (1 + x)$).
* **Negative Valleys:** Tight, compressed response ($x / (1 - 0.4x)$).


* Paired with an integrated **1st-order DC Blocker** and a 1-pole Low-Pass filter (**4500 Hz high-frequency roll-off**).
* **Acoustic Objective:** Emulates vacuum tube overdrive to introduce warm, biological vocal strain without digital aliasing fuzz.

---

#### `BitcrushEffect.cs`

* **Unit / Scale:** Quantization depth distortion (**0.0f** to **1.0f**)
* **Technical Implementation:** Non-linear mid-tread amplitude step quantizer that aggressively reduces bit depth down to **2.5 bits**. Includes a safety DC blocking filter.
* **Acoustic Objective:** Generates harsh digital distortion, truncation noise, and square-step quantization.

---

#### `SiliconRingModulationEffect.cs`

* **Unit / Scale:** Cybernetic mix intensity (**0.0f** to **1.0f**)
* **Technical Implementation:** Inharmonic balanced ring intermodulator. Multiplies the vocal input by a jagged, transistor-style pseudo-square carrier wave that shifts dynamically based on vocal load (**58 Hz to 92 Hz**). Output feeds into a short **144-sample steel enclosure comb buffer**.
* **Acoustic Objective:** Breaks down human harmonic intervals, adding a cold, metallic, machine-like quality (designed for mainframe entities like SCP-079).

---

#### `ScreechModulatorEffect.cs`

* **Unit / Scale:** Screech depth intensity (**0.0f** to **1.0f**)
* **Technical Implementation:** Envelope-driven inharmonic sideband modulator. Raw inputs pass through a pre-cleaning high-pass filter (1400 Hz) into a high-frequency carrier loop that sweeps from **1300 Hz up to 2100 Hz**. The output then excites a high-Q peaking filter centered at **3150 Hz** (the human ear's peak pain threshold).
* **Acoustic Objective:** Generates piercing, glass-shattering shrieks that create immediate discomfort for the listener (ideal for SCP-096).

---

#### `DigitalDataBurstEffect.cs`

* **Unit / Scale:** Data transmission amount (**0.0f** to **1.0f**)
* **Technical Implementation:** High-frequency telemetry burst engine. Uses a fast LCG random step trigger to generate asymmetric step-quantized square wave sweeps (**5000 Hz down to 2200 Hz**), which excite a 2nd-order IIR **mainframe bandpass resonator centered at 5800 Hz**.
* **Acoustic Objective:** Synthesizes digital data bursts and packet telemetry chirps, ideal for synthetic or robotic entities.

---

#### `SampleRateReducerEffect.cs`

* **Unit / Scale:** Downsampling factor (**0.0f** to **1.0f**)
* **Technical Implementation:** Frame-independent clock-divider **Sample-and-Hold downsampler**. Uses a **1.2 Hz thermal clock-drift LFO** and a bitwise LCG phase jitter matrix to modulate step rates. The signal then feeds a lossy **2200 Hz DAC reconstruction filter** and a polynomial soft-clipping stage.
* **Acoustic Objective:** Introduces bright digital aliasing and authentic low-bandwidth downsampling artifacts, emulating vintage hardware components.

---

#### `GlitchBurstEffect.cs`

* **Unit / Scale:** Buffer fracture instability (**0.0f** to **1.5f**)
* **Technical Implementation:** Buffer fracture engine that triggers random lockups (**15 ms to 65 ms durations**). Features a **Sample-and-Hold Stutter Quantizer** that freezes micro-grains of audio, blends them with LCG-corrupted white noise, and routes them through a **Digital Foldback Distortion** waveshaper.
* **Acoustic Objective:** Simulates critical processing underruns, digital frame fractures, and memory dropouts (ideal for SCP-079).

---

#### `StaticNoiseEffect.cs`

* **Unit / Scale:** Noise amplitude floor (**0.0f** to **1.0f**)
* **Technical Implementation:** Multi-layered RF electromagnetic noise generator. An ultra-fast local LCG generates raw white noise, which is filtered by two independent biquad networks:
* **Intercom Bandpass:** Centered at 1800 Hz ($Q = 0.6$) modulated by a **0.4 Hz swell LFO**.
* **Electrical Fizz Highpass:** Centered at 5500 Hz ($Q = 1.0$) modulated by a **7.5 Hz crackle LFO**.


* Output passes through a polynomial analog circuit overdrive shaper.
* **Acoustic Objective:** Generates realistic radio frequency interference and signal corruption.

---

#### `TremoloEffect.cs`

* **Unit / Scale:** Amplitude modulation depth (**0.1f** to **20.0f Hz**)
* **Technical Implementation:** Double-phase amplitude modulator. Combines a stable float-native LFO step increment with a **high-frequency circular micro-jitter phase offset** to prevent mechanical repetition. Uses a 3rd-order polynomial tanh approximation for soft clipping.
* **Acoustic Objective:** Introduces an unstable, trembling quality to vocal volume, ideal for zombies (SCP-049-2), tracking radio anomalies, or modeling spatial distortion.

---

### 4. Acoustic Space & Environmental Filters

---

#### `LowPassEffect.cs`

* **Unit / Scale:** Cutoff Frequency (**Hertz**)
* **Technical Implementation:** Production-grade **2nd-order Butterworth Low-Pass Filter** structured as a Direct Form I Biquad. Pre-calculates stable coefficients and features a strict Nyquist guard cap ($sr \times 0.45$).
* **Acoustic Objective:** Attenuates high frequencies to simulate acoustic muffling, distance occlusion, or thick barriers (such as SCP-049's mask).

---

#### `HighPassEffect.cs`

* **Unit / Scale:** Cutoff Frequency (**Hertz**)
* **Technical Implementation:** production-grade **2nd-order Butterworth High-Pass Filter** structured as a Direct Form I Biquad with a safety clamp entry boundary (20 Hz minimum).
* **Acoustic Objective:** Removes muddy low-end frequencies, proximity bass spikes, and structural rumble. Emulates the thin, restricted sound profiles of radio intercoms or handheld transceivers.

---

#### `ReverbEffect.cs`

* **Unit / Scale:** Room decay wet blend (**0.0f** to **1.0f**)
* **Technical Implementation:** Highly optimized **Schroeder-Moorer Reverb Matrix**. Combines four parallel **Low-Pass Feedback Comb Filters (LBCF)** using prime-like delay times (29.7ms, 37.1ms, 41.3ms, 45.7ms) with two cascaded **Late-Decay Diffusion All-Pass Filters (APF)** (5.2ms and 1.7ms delay profiles).
* **Acoustic Objective:** Synthesizes dense acoustic spaces, modeling containment chambers, underground tunnel systems, and massive metal vaults.

---

#### `PocketDimensionEchoEffect.cs`

* **Unit / Scale:** Spatial dislocation factor (**0.0f** to **1.5f**)
* **Technical Implementation:** Non-Euclidean delay engine. Combines a 32768-sample ring buffer with parallel linear phase accumulators (**0.35 Hz time LFO and 0.12 Hz feedback LFO**) to dynamically modulate delay times. Features an internal **inline All-Pass phase shifter** (-0.65 feedback coefficient) for chaotic phase inversion.
* **Acoustic Objective:** Generates impossible, disorienting, creeping echo responses. Designed specifically to reflect the reality-bending nature of SCP-106's Pocket Dimension.

---

#### `WetDecayEffect.cs`

* **Unit / Scale:** Slime coating absorption (**0.0f** to **1.5f**)
* **Technical Implementation:** Recursive viscous delay loop using a 1024-sample buffer. Includes an **800 Hz high-frequency dampened absorption filter** coupled with a stateful low-frequency fluid bubble resonator (centered at 320 Hz, $Q = 5.5$) driven by a stochastic local LCG trigger engine.
* **Acoustic Objective:** Models damp, claustrophobic environments, adding liquid-coated acoustic reflections and bubbling squelches.

---

## 🛠️ Verification & Pipeline Integration

To bind an effect from this matrix into a live player session thread, instantiate the module inside the player's audio pipeline and call `Process()` during each buffer cycle:

```csharp
// Example integration patch for SCP-096 voice state engine
public class Scp096AudioProcessor
{
    private readonly IAudioEffect _octaver;
    private readonly IAudioEffect _screech;
    private readonly float[] _processingBuffer;

    public Scp096AudioProcessor(float sampleRate)
    {
        // Allocation occurs once on initialization
        _octaver = new DemonicOctaverEffect(mix: 0.70f, sampleRate);
        _screech = new ScreechModulatorEffect(amount: 0.85f, sampleRate);
        _processingBuffer = new float[512];
    }

    public void OnVoiceDataReceived(float[] incomingPcm)
    {
        // Hot-path execution loop remains completely allocation-free
        int sampleCount = incomingPcm.Length;
        Array.Copy(incomingPcm, _processingBuffer, sampleCount);

        _octaver.Process(_processingBuffer, sampleCount);
        _screech.Process(_processingBuffer, sampleCount);

        Array.Copy(_processingBuffer, incomingPcm, sampleCount);
    }
}

```