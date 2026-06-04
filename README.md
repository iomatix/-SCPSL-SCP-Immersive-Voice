# SCP Immersive Voice

**SCP Immersive Voice** is an enterprise-grade real-time Digital Signal Processing (DSP) and proximity voice chat framework for **SCP: Secret Laboratory** powered by **LabAPI**.

By bypassing high-overhead frame-by-frame reinstantiations, this framework runs a stateful, thread-safe, and completely zero-allocation processing pipeline. Every SCP is equipped with an organic, character-accurate acoustic model that dynamically reacts to their physical abilities, health states, and environmental conditions in real time.

---

## 🚀 Key Innovations in Version 0.9.0

### 🧵 Thread-Safe Pipeline Isolation
VoIP packets in SCP:SL are processed asynchronously across worker threads, while game logic fires on the Unity main thread. Version 0.9.0 introduces an isolated `PipelineContainer` synchronization block utilizing high-speed cached reflection field injections. This fully eliminates `CollectionWasModifiedException` crashes and guarantees that changing roles or states will never cause audio drops or gurgling artifacts.

### ⏱️ Asynchronous Watchdog Lifespans
Transient combat states (such as SCP-939 lunging or SCP-096 prying a gate) are protected by internal high-precision structural watchdogs. Even under extreme server tick rate degradation or dropped network event frames, the system automatically falls back to baseline states (like `IdleWhisper` or `Calm`), preventing frozen or corrupted voices.

### 🎛️ In-Place Phase Continuity
Instead of destroying and rebuilding audio filters during state shifts (which causes massive DC offset steps and destructive bursts of digital white noise), the 0.9.0 synchronization loop mutates coefficient scalars *in-place*. Buffers, Biquad delay lines, and Feedback Network registers (`Reverb`, `WetDecay`, `Echo`) maintain perfect mathematical phase continuity.

---

## ✨ System Features

### 🎤 Native Proximity Proxy Routing
Disables standard global sub-channels to force custom positional 3D audio proxying, with automated safeguards for radio-based entities (SCP-079).

### 🧠 Dynamic Modular Maszyny Stanów
Fully decoupled event handlers manage individual SCP gameplay triggers independently, eliminating monolithic architectural bloat.

### 🧬 Real-Time Production Profiling
Includes a band-limited condition-gated diagnostics engine to monitor Signal-to-Noise Ratio (SNR), RMS energy shifts, and digital clipping thresholds without draining CPU cycles during live gameplay.

---

## 🎚️ Production-Grade DSP Effects Matrix

All modular components are explicitly engineered for float-native operations directly inside the raw PCM stream:

* **NoiseGateEffect** — Studio-grade follower with independent Attack/Hold/Release exponential RC filters.
* **PitchShiftEffect** — Crossfading time-domain circular buffer with Hann window modulation.
* **FormantShiftEffect & Drift** — Cascaded 4-Band Biquad Resonator Matrix with dynamic LFO center-frequency smearing.
* **SubharmonicGrowlEffect** — Phase-locked sub-octave frequency divider producing heavy chest-rattling low-end.
* **GutturalResonanceEffect** — Feedback comb-filtering coupled with an asymmetric waveshaper replicating laryngeal rasp.
* **WhisperFilterEffect** — Amplitude-modulated pink noise matrix combined with spectral bandpass tracking.
* **FleshCrackle / DryCrackle** — Stochastic impulse generators driving rapid high-frequency granular transients.
* **StoneCrack / StoneGrind** — Peak-amplitude triggered physical-modeling fracture models.
* **PocketDimensionEchoEffect** — Chaotic non-Euclidean phase-inversion feedback matrix.
* **WetDecay & Reverb** — 4x4 Feedback Delay Networks utilizing Householder unitary matrix diffusion.
* **Bitcrush & SampleRateReducer** — Mid-tread quantization and clock-divider aliasing modules.
* **GlitchBurst & StaticNoise** — Micro-buffer capture looping arrays mimicking hardware underruns and RF interference.

---

## 🧠 Dynamic State Architecture

### 🩸 SCP‑096 — Hysterical Despair System
Tracks stress parameters to scale acoustic instability:
- **Calm / Crying** — Deep FormantDrift laryngeal tremolo layered with high-intensity unvoiced sobbing.
- **Trying Not to Cry** — Extreme vocal chord constriction using rigid saturation.
- **Enraging / Enraged** — Shifting fundamental frequencies into an ear-piercing scream backed by sub-harmonic low-end blocks.

### 🐾 SCP‑939 — Biomorphic Camouflage System
Balances predatory intent with eerie mimicry:
- **Idle Whisper / Focused** — Pristine bandpass-filtered whisper matrices.
- **Mimicking** — High human fundamental replication leaking micro-LFO drift and slimy throat articulation.
- **Attacking / Lunging** — Complete unmasking into visceral laryngeal gravel and sub-octave roars.

### 🫀 SCP‑3114 — Skeletal Anatomy System
Manages mechanical bone friction layered with organic tissues:
- **Undisguised** — High-tension calcium snapping and ligaments cracking.
- **Disguised** — Psychoacoustic human mimicry leaking trace structural full-flesh crawls.
- **Strangling** — Peak larynx friction waveshaping saturation to maximize voice presence.

---

## 🔧 Configuration

All SCP presets are fully configurable via the plugin config file:

- Enable/disable proximity voice 
- Per‑role voice presets 
- Per‑effect intensity 
- Forbidden proximity roles 
- Dynamic preset providers 

---

## 📝 Changelog — Version 0.9.0

- **Architecture Overhaul:** Disassembled monolithic handlers into clean, decoupled, single-responsibility modules.
- **Thread Safety:** Implemented `ConcurrentDictionary` and explicit pipeline lock synchronization.
- **Phase Continuity:** Upgraded reconciliation logic using cached reflection injections.
- **Watchdog Implementation:** Added high-precision time limits to combat states.
- **Audio Tuning:** Rewrote 100% of default and dynamic presets for SCP-096, SCP-106, SCP-939, and SCP-3114.
- **Profiler Optimization:** Replaced active performance loops with boolean-gated conditional switches.

---

## 🔗 Download & Support

* **Download:** [GitHub Releases](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)
* **Support:** [Buy Me A Coffee](https://buymeacoffee.com/iomatix)