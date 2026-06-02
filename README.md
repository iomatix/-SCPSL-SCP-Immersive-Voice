# SCP Immersive Voice

**SCP Immersive Voice** is a full audio‑enhancement framework for **SCP: Secret Laboratory**.  
It brings proximity voice chat to SCPs and adds **high‑quality, role‑accurate voice processing** using a custom DSP pipeline.

Every SCP receives a unique, handcrafted audio profile designed to match its lore, personality, and gameplay behavior.  
This includes pitch shifting, formant shaping, distortion, spectral filtering, reverb, noise layers, glitch effects, and fully dynamic voice states for SCPs with evolving emotional or physical conditions.

The result is a far more immersive, atmospheric, and expressive SCP experience — **without sacrificing clarity or performance**.

---

## ✨ Features

### 🎤 Proximity Voice Chat for SCPs
SCPs can communicate using proximity voice, with configurable distance and role‑based restrictions.

### 🔊 Advanced DSP Audio Pipeline
Each voice message is decoded, processed through a modular chain of audio effects, and re‑encoded in real time.

Included effects:

- Pitch shifting  
- Formant shifting  
- Distortion  
- Low‑pass / high‑pass filtering  
- Reverb  
- Breath noise synthesis  
- Whisper filtering  
- Stone crack & grind layers  
- Wet/organic modulation  
- Flesh crackle  
- Formant drift  
- Bitcrush  
- Sample‑rate reduction  
- Glitch bursts  
- Static noise  
- Chirp effects (Flamingo variants)

All effects are **custom‑built** for this plugin — no reused assets, no generic filters.

---

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
- Static noise  

### 🧟 SCP‑049‑2 — Zombie
- Guttural resonance  
- Dry crackle  
- Subharmonic growl  

### 🦩 Flamingo Variants
- Comedic pitch  
- Light distortion  
- Chirp effect  

---

## ⚙️ Performance

The DSP pipeline is optimized for real‑time processing:

- All effects are lightweight and stream‑safe  
- No allocations during processing  
- No reflection or dynamic dispatch  
- Uses Opus decode → DSP → encode pipeline  
- Designed to scale for large servers  

Performance testing guidelines are included below.

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

---

## 📜 License

Free to use, modify, and deploy on any SCP:SL server.  
Attribution appreciated but not required.

---

## 📥 Download

[![Download Latest Release](https://img.shields.io/badge/Download-Latest%20Release-blue?style=for-the-badge)](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)  
[![GitHub Downloads](https://img.shields.io/github/downloads/iomatix/-SCPSL-SCP-Immersive-Voice/latest/total?sort=date&style=for-the-badge)](https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/releases/latest)

---

## ❤️ Supporting Development

My mods are **always free to use**.  
If you appreciate my work, you can support me by [buying me a coffee](https://buymeacoffee.com/iomatix).

---

## 👥 Contributors

<a href="https://github.com/iomatix/-SCPSL-SCP-Immersive-Voice/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=iomatix/-SCPSL-SCP-Immersive-Voice" />
</a>
