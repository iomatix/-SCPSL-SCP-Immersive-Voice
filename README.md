# SCP Immersive Voice
[![Download Latest Release](https://img.shields.io/badge/Download-Latest%20Release-blue?style=for-the-badge)](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)
[![GitHub Downloads](https://img.shields.io/github/downloads/iomatix/-SCPSL-SCP-Immersive-Voice/latest/total?sort=date&style=for-the-badge)](https://github.com/iomatix/-SCPSL-SCP-575-NPC/releases/latest)


**SCP Immersive Voice** is an enterprise-grade real-time Digital Signal Processing (DSP) and proximity voice chat framework for **SCP: Secret Laboratory** powered by **[LabAPI](https://github.com/northwood-studios/LabAPI)** and **[AudioManagerAPI](https://github.com/iomatix/-SCPSL-AudioManagerAPI/tree/main/AudioManagerAPI)**.

The core real-time, float-native Digital Signal Processing (DSP) effects for the `SCP_Immersive_Voice` subsystem are implemented **within** this repository.

## Architectural Principles

Every effect implemented in this pipeline adheres to strict **AAA Game Audio Standards**:
* **In-Place Processing:** Every effect implements `IAudioEffect` and operates directly on the native float PCM buffer (`void Process(float[] pcm, int length)`) to avoid redundant memory copies.
* **Zero Allocations:** No heap allocations (`new`) are permitted inside the critical audio processing loops. All state variables, filters, and rings are persistent and stack/heap-allocated only during instantiation.
* **Persistent Statefulness (Cached-DSP):** Effects that rely on time-domain or history data (delays, filters, envelopes) maintain their state context *per player* inside a persistent cache. They are never instantiated per-frame, eliminating phase fractures, zipper noise, and click artifacts.

By bypassing high-overhead frame-by-frame reinstantiations, this framework runs a stateful, thread-safe, and completely zero-allocation processing pipeline. Every SCP is equipped with an organic, character-accurate acoustic model that dynamically reacts to their physical abilities, health states, and environmental conditions in real time.

## **Effects Documentation** - [README.md](SCP-Immersive-Voice/AudioProcessing/Effects/README.md)


## 📝 Changelog — Version 1.0.0 (The Foundation Update)

This is the most significant milestone in the project's history. Version 1.0.0 is not just a collection of new effects; it is a total reconstruction of the `SCP Immersive Voice` engine. Every SCP has been re-calibrated, and the audio processing architecture has been rewritten from the ground up to provide studio-grade fidelity with minimal CPU overhead.

### 🏗️ Architecture & Performance
- **Full Modularity:** The monolithic handler has been decomposed into independent, lightweight modules (`Scp096AudioHandler`, `Scp939AudioHandler`, etc.). Each SCP now manages its state in total isolation.
- **Zero-Allocation Pipeline:** The entire DSP pipeline now operates in "zero-heap-allocation" mode. By eliminating redundant memory allocations, we guarantee zero jitter and perfect audio continuity, even under heavy server load.
- **Thread-Safety 2.0:** Pipeline synchronization has been migrated to the `PipelineContainer` level. Atomic operations and `ConcurrentDictionary` usage eliminate race conditions, ending the era of "audio gurgling" or state-lock issues common in older versions.

### 🔊 Comprehensive Roster Rework
- **Unified Acoustic Matrix:** All SCP roles have been re-tuned based on our internal "Audio Engineer’s Bible." Every preset—from 049 to 3114—now utilizes improved organic resonance algorithms rather than static filters.
- **State Transition Smoothing:** We implemented advanced transition smoothing across all states. Digital "clicks" and abrupt volume jumps during emotional transitions or attack triggers have been eliminated.
- **State Watchdogs:** Every SCP state is now guarded by a high-precision temporal watchdog. If the game engine fails to fire a completion event (e.g., during tick rate spikes), the system automatically and seamlessly reverts to the baseline state.

### 🧪 DSP Innovations
- **Uncanny Valley Generator (`LaryngealAsymmetryEffect`):** Introduced for SCP-939; generates unnatural, biology-inspired asymmetry, perfect for deceptive mimicry.
- **Airy Whisper Engine:** A complete whisper synthesis overhaul. Replacing radio-like noise with filtered aerodynamic airflow, achieving crystal-clear speech intelligibility.
- **Real-time Diagnostics:** Integrated an internal floating-point trace engine that detects `NaN` (Not a Number) spikes in real-time, instantly resetting affected modules to protect the global audio stream.

### 🔧 Stability & Reliability
- Resolved all persistent issues regarding SCPs becoming "stuck" in combat states.
- Optimized CPU usage via condition-gated gating (DSP nodes are only computed if the player is actively transmitting).
- Fixed all event-leak vulnerabilities that occurred during hot-reloads.

---

## 🚀 The Road Ahead
Version 1.0.0 serves as the solid, professional-grade foundation I have been working toward. While this build covers the entire roster with absolute precision, the architecture is now flexible enough to support even more complex acoustic phenomena. I am looking forward to exploring further physical modeling and even deeper integration with the game's environmental audio in future updates.

*This project has been a solo journey of technical obsession—thank you for following along and for your interest in the high-fidelity future of SCP: Secret Laboratory voice chat.*

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
