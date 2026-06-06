# SCP Immersive Voice

**SCP Immersive Voice** is an enterprise‑grade real‑time Digital Signal Processing (DSP) and proximity voice chat framework for **SCP: Secret Laboratory**, powered by [LabAPI](https://github.com/northwood-studios/LabAPI) and [AudioManagerAPI](https://github.com/iomatix/-SCPSL-AudioManagerAPI/tree/main/AudioManagerAPI).

All core real‑time, float‑native DSP effects for the `SCP_Immersive_Voice` subsystem are implemented directly within this repository.

[![Download Latest Release](https://img.shields.io/badge/Download-Latest%20Release-blue?style=for-the-badge)](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)  
[![GitHub Downloads](https://img.shields.io/github/downloads/iomatix/-SCPSL-SCP-Immersive-Voice/latest/total?sort=date&style=for-the-badge)](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)

---

## Architectural principles

Every effect in the pipeline follows strict **AAA game audio standards**:

- **In‑place processing** — Each effect implements `IAudioEffect` and operates directly on the native float PCM buffer (`void Process(float[] pcm, int length)`), avoiding redundant memory copies.
- **Zero allocations** — No heap allocations (`new`) are performed inside critical audio loops. All state, filters, and buffers are created once and reused.
- **Persistent statefulness (cached DSP)** — Time‑domain and history‑based effects (delays, filters, envelopes) maintain per‑player state in a persistent cache, preventing phase fractures, zipper noise, and clicks.

This design yields a stateful, thread‑safe, zero‑allocation processing pipeline. Each SCP is modeled with an organic, character‑accurate acoustic profile that reacts in real time to abilities, health, and environment.

---

## Effects documentation

Detailed documentation for all effects is available in:

- [`SCP-Immersive-Voice/AudioProcessing/Effects/README.md`](SCP-Immersive-Voice/AudioProcessing/Effects/README.md)

---

## Changelog — Version 1.0.0 (The Foundation Update)

Version 1.0.0 is a complete reconstruction of the **SCP Immersive Voice** engine. All SCPs have been recalibrated and the audio architecture has been redesigned for studio‑grade fidelity with minimal CPU overhead.

### Architecture & performance

- **Full modularity** — The former monolithic handler has been split into independent modules (`Scp096AudioHandler`, `Scp939AudioHandler`, etc.), each managing its own state.
- **Zero‑allocation pipeline** — The DSP pipeline now runs in a fully zero‑heap‑allocation mode, ensuring stable performance even under heavy load.
- **Thread‑safety 2.0** — Synchronization has been moved to the `PipelineContainer` level. Atomic operations and `ConcurrentDictionary` usage eliminate race conditions and audio “gurgling” or state‑lock issues.

### Comprehensive roster rework

- **Unified acoustic matrix** — All SCP roles have been retuned using improved organic resonance algorithms instead of static filters.
- **State transition smoothing** — Advanced smoothing removes digital clicks and abrupt volume jumps during emotional or combat transitions.
- **State watchdogs** — High‑precision temporal watchdogs automatically revert SCPs to baseline states if completion events are missed.

### DSP innovations

- **Uncanny Valley Generator (`LaryngealAsymmetryEffect`)** — New asymmetry engine for SCP‑939, producing unsettling, biology‑inspired vocal behavior ideal for mimicry.
- **Airy Whisper Engine** — Whisper synthesis overhaul replacing radio‑like noise with filtered aerodynamic airflow for clear intelligibility.
- **Real‑time diagnostics** — Internal floating‑point trace engine detects `NaN` spikes and resets affected modules to protect the global audio stream.

### Stability & reliability

- Resolved all known issues with SCPs becoming stuck in combat states.
- Optimized CPU usage via condition‑gated DSP (nodes are processed only while the player is transmitting).
- Fixed event‑leak problems occurring during hot‑reloads.

---

## The road ahead

Version 1.0.0 provides a solid, production‑grade foundation for future work, including more advanced physical modeling and deeper integration with environmental audio systems.

---

## Key innovations in Version 0.9.0

### Thread‑safe pipeline isolation

VoIP packets are processed asynchronously on worker threads, while game logic runs on the Unity main thread. Version 0.9.0 introduces an isolated `PipelineContainer` synchronization block using high‑speed cached reflection field injection, eliminating `CollectionWasModifiedException` crashes and preventing audio drops during role or state changes.

### Asynchronous watchdog lifespans

Transient combat states (e.g., SCP‑939 lunges, SCP‑096 prying gates) are guarded by high‑precision watchdogs. Even under severe tick‑rate degradation or dropped events, the system automatically falls back to safe baseline states such as `IdleWhisper` or `Calm`.

### In‑place phase continuity

Instead of destroying and recreating filters during state changes, the synchronization loop mutates coefficients in place. Buffers, Biquad delay lines, and feedback registers (`Reverb`, `WetDecay`, `Echo`) maintain phase continuity and avoid DC jumps and white‑noise bursts.

---

## System features

### Native proximity proxy routing

Standard global sub‑channels are disabled to enforce custom positional 3D audio proxying, with automatic safeguards for radio‑based entities such as SCP‑079.

### Dynamic modular state machines

Decoupled event handlers manage each SCP’s gameplay triggers independently, removing monolithic architectural bloat.

### Real‑time production profiling

A band‑limited, condition‑gated diagnostics engine monitors SNR, RMS energy shifts, and clipping thresholds without impacting runtime performance.

---

## Production‑grade DSP effects matrix

All components are engineered for float‑native operation directly on the PCM stream:

- **NoiseGateEffect** — Studio‑grade follower with independent Attack/Hold/Release RC filters  
- **PitchShiftEffect** — Time‑domain circular buffer with Hann‑windowed crossfading  
- **FormantShiftEffect & Drift** — 4‑band Biquad resonator matrix with LFO‑driven center‑frequency smearing  
- **SubharmonicGrowlEffect** — Phase‑locked sub‑octave generator for heavy low‑end  
- **GutturalResonanceEffect** — Feedback comb filtering with asymmetric waveshaping for laryngeal rasp  
- **WhisperFilterEffect** — Amplitude‑modulated pink‑noise matrix with spectral bandpass tracking  
- **FleshCrackle / DryCrackle** — Stochastic impulse generators for granular high‑frequency transients  
- **StoneCrack / StoneGrind** — Peak‑triggered physical‑model fracture and grinding models  
- **PocketDimensionEchoEffect** — Chaotic non‑Euclidean phase‑inversion feedback matrix  
- **WetDecay & Reverb** — 4×4 Feedback Delay Networks with Householder diffusion  
- **Bitcrush & SampleRateReducer** — Mid‑tread quantization and clock‑divider aliasing  
- **GlitchBurst & StaticNoise** — Micro‑buffer capture and looping for hardware‑style underruns and RF interference  

---

## Dynamic state architecture

### SCP‑096 — Hysterical Despair System

- **Calm / Crying** — Deep formant drift and laryngeal tremolo layered with intense unvoiced sobbing  
- **Trying Not to Cry** — Strong vocal‑cord constriction with rigid saturation  
- **Enraging / Enraged** — Rising fundamentals into piercing screams backed by subharmonic low‑end  

### SCP‑939 — Biomorphic Camouflage System

- **Idle Whisper / Focused** — Clean, band‑limited whisper matrices  
- **Mimicking** — Human‑like fundamentals with micro‑LFO drift and wet throat articulation  
- **Attacking / Lunging** — Fully exposed gravel, growls, and sub‑octave roars  

### SCP‑3114 — Skeletal Anatomy System

- **Undisguised** — High‑tension calcium snapping and ligament cracking  
- **Disguised** — Human mimicry with subtle full‑flesh structural noise  
- **Strangling** — Peak larynx friction and saturation for maximum presence  

---

## Configuration

All SCP presets are configurable via the plugin configuration file:

- Enable/disable proximity voice  
- Per‑role voice presets  
- Per‑effect intensity  
- Forbidden proximity roles  
- Dynamic preset providers  

---

## Changelog — Version 0.9.0

- Architecture overhaul into clean, decoupled, single‑responsibility modules  
- Thread‑safe pipeline synchronization using `ConcurrentDictionary` and explicit locks  
- Phase‑continuity improvements via cached reflection‑based reconciliation  
- High‑precision watchdogs for combat states  
- Complete preset retuning for SCP‑096, SCP‑106, SCP‑939, and SCP‑3114  
- Profiler optimization with boolean‑gated diagnostic switches  

---

## Download & support

- **Download:** [GitHub Releases](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)  
- **Support:** [Buy Me A Coffee](https://buymeacoffee.com/iomatix)
