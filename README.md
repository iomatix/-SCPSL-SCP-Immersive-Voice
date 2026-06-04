> **Version 0.6.0 — Full DSP Rewrite + AudioManagerAPI Integration**

# SCP Immersive Voice

**SCP Immersive Voice** is a full audio‑enhancement framework for **SCP: Secret Laboratory**.  
It brings proximity voice chat to SCPs and adds **high‑quality, role‑accurate voice processing** using a custom DSP pipeline.

Every SCP receives a unique, handcrafted audio profile designed to match its lore, personality, and gameplay behavior.  
This includes pitch shifting, formant shaping, distortion, spectral filtering, reverb, noise layers, glitch effects, and fully dynamic voice states for SCPs with evolving emotional or physical conditions.

The result is a far more immersive, atmospheric, and expressive SCP experience — **without sacrificing clarity or performance**.

---

## ✨ Features

### 🎤 Proximity Voice Chat for SCPs
SCPs communicate using proximity voice, with configurable distance and role‑based restrictions.

### 🔊 Advanced DSP Audio Pipeline
Each voice message is decoded, processed through a modular chain of audio effects, and re‑encoded in real time using AudioManagerAPI.

### 🔊 DSP Effects (0.6.0)

All effects are fully float‑native, zero‑allocation and optimized for real‑time
processing through AudioManagerAPI.

Included effects:

- **PitchShiftEffect** — high‑quality, zero‑alloc pitch shifting  
- **FormantShiftEffect** — tilt‑EQ formant shaping with drift  
- **FormantDriftEffect** — organic throat drift modulation  
- **DistortionEffect** — analog‑style soft‑knee saturation  
- **GutturalResonanceEffect** — deep throat resonance  
- **WetDecayEffect** — moist, decaying smear  
- **WetOrganicEffect** — subtle slimy modulation  
- **WhisperFilterEffect** — breathy whisper shaping  
- **DryCrackleEffect / FleshCrackleEffect** — transient crackle engines  
- **GlitchBurstEffect** — digital fracture bursts  
- **BitcrushEffect** — TPDF dithering + DC blocker  
- **PocketDimensionEchoEffect** — extradimensional unstable echo  
- **ReverbEffect** — nonlinear diffusion reverb  
- **HighPassEffect / LowPassEffect** — standardized one‑pole filters  
- **ChirpEffect** — FM‑modulated chirps (SCP‑079 + flamingo variants)  
- **StaticNoiseEffect** — radio‑style noise layer  
- **BreathNoiseEffect** — procedural breath synthesis

All effects are **custom‑built** for this plugin — no reused assets, no generic filters.

---



## 🧩 AudioManagerAPI Integration (New in 0.6.0)

SCP Immersive Voice now uses the **SCPSL-AudioManagerAPI** for all audio
streaming, decoding, encoding and DSP routing.

This provides:

- Zero‑allocation audio pipeline  
- Float‑native PCM processing  
- Real‑time Opus decode → DSP → encode  
- Stable speaker instances  
- High‑performance audio caching  
- Unified API for all voice effects  

Repository: [-SCPSL-AudioManagerAPI](https://github.com/iomatix/-SCPSL-AudioManagerAPI)

## 🧠 Dynamic Voice Systems

Several SCPs have **state‑driven voice behavior**, reacting to in‑game events via LabAPI.

### 🩸 SCP‑096 — Emotional State Voice System
- Calm  
- Crying  
- Trying Not to Cry  
- Enraging  
- Enraged  
- Charging  

Each state has its own voice preset, automatically applied.

### 🐾 SCP‑939 — Whisper & Mimicry System
- Idle Whisper  
- Mimicking  
- Focused  
- Attacking  
- Amnestic Cloud  

Includes breath noise, whisper filtering, and dynamic transitions.

### 🫀 SCP‑3114 — Flesh Voice System
- Undisguised  
- Disguising  
- Disguised (clean human voice)  
- Revealing  
- Strangling  

Uses organic wet layers, flesh crackle, formant drift, and subtle modulation.

---

## 🧬 Unique SCP Voice Profiles

### 🪨 SCP‑173 — Stone Entity
- Stone crack  
- Stone grind  
- Heavy distortion  
- Zero human characteristics  

### 🕳️ SCP‑106 — Pocket Dimension Horror
- Wet decay  
- Pocket dimension echo  
- Low‑formant, decayed voice  

### 🤖 SCP‑079 — Old AI
- Bitcrush  
- Sample‑rate reduction  
- Glitch bursts  
- ChirpEffect (FM‑modulated interference)
- StaticNoiseEffect (radio noise)

### 🧟 SCP‑049‑2 — Zombie
- Guttural resonance  
- Dry crackle  
- Subharmonic growl  

### 🦩 Flamingo Variants
- Comedic pitch  
- Light distortion  
- ChirpEffect (FM wobble + jitter)

---

## ⚙️ Performance (0.6.0)

The entire DSP pipeline is now powered by AudioManagerAPI and is fully
zero‑allocation during audio processing.

Performance features:

- Float‑native PCM pipeline  
- Zero allocations inside Process()  
- Persistent buffers for all effects  
- Stable speaker instances  
- Optimized Opus decode → DSP → encode loop  
- Designed for large servers with many simultaneous SCPs

---

## 🔧 Configuration

All SCP presets are fully configurable via the plugin config file:

- Enable/disable proximity voice  
- Per‑role voice presets  
- Per‑effect intensity  
- Forbidden proximity roles  
- Distance settings  
- Dynamic preset providers  

---

## 🔗 Compatibility

- Works with all major server frameworks  
- Uses LabAPI for event‑driven state transitions  
- Fully compatible with Remote Admin and custom events  
- Does not modify game assets or client files  

This plugin requires the [SCPSL‑AudioManagerAPI](https://github.com/iomatix/-SCPSL-AudioManagerAPI)
for all audio routing and real‑time DSP processing.


---

## 📜 License

Free to use, modify, and deploy on any SCP:SL server.  
Attribution appreciated but not required.

---

## 📥 Download

[![Download Latest Release](https://img.shields.io/badge/Download-Latest%20Release-blue?style=for-the-badge)](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)  
[![GitHub Downloads](https://img.shields.io/github/downloads/iomatix/-SCPSL-SCP-Immersive-Voice/latest/total?sort=date&style=for-the-badge)](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)

## 📝 Changelog — Version 0.6.0

- Full DSP rewrite (zero‑alloc, float‑native)
- Integration with SCPSL‑AudioManagerAPI
- New and improved DSP effects
- Standardized filters (HPF/LPF)
- Stable feedback engines (Echo/Reverb)
- Improved formant and pitch processing
- New ChirpEffect for SCP‑079 and Flamingos
- Updated SCP voice profiles
- Major performance improvements

---

## ❤️ Supporting Development

My mods are **always free to use**.  
If you appreciate my work, you can support me by [buying me a coffee](https://buymeacoffee.com/iomatix).

---

## 👥 Contributors

<a href="https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=iomatix/-SCPSL-SCP-Immersive-Voice" />
</a>
