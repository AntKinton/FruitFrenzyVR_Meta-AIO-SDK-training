# Fruit Frenzy VR

** Unity 6.2 VR Arcade Game with Meta AIO SDK Integration**

![Unity Version](https://img.shields.io/badge/Unity-6000.0.27f2-black?style=for-the-badge&logo=unity)
![Platform](https://img.shields.io/badge/Platform-Meta_Quest_2%2B-0467DF?style=for-the-badge&logo=meta)
![SDK](https://img.shields.io/badge/AIO--Meta--SDK-v85.0.0-7149FF?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![Version](https://img.shields.io/badge/Version-v1.0.0--training-blue?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Training_Alpha_Development-orange?style=for-the-badge)

## Fork Objectives: FruitFrenzyVR_AIO-SDK-training

This fork serves as a technical sandbox for mastering the **Meta All-In-One SDK** within the Unity ecosystem. 

### Version Migration & Training Scope
The project tracks the transition from the original base versions to the current training environment:

* **Base Versions (Source):**
    * **Unity:** 6000.0.47f1
    * **Meta AIO SDK:** v76.0.1
* **Training Versions (Current):**
    * **Unity:** 6000.0.27f2
    * **Meta AIO SDK:** v85.0.0 (Currently Stable)

### Key Areas of Focus
* **SDK Implementation:** Deep dive into the v85.0.0 features, including the Interaction SDK and OVRCameraRig.
* **Version Stability:** Evaluating performance and API changes between the base 6000.0.x versions.
* **Build Pipeline:** Optimization for standalone Quest deployment using Unity 6's latest rendering features.
* **Linux Development:** Full Linux development support with custom Meta XR SDK patches.

### Platform Support
- **Development**: Linux, Windows, macOS ✅
- **Target**: Meta Quest 2+ ✅
- **Build Pipeline**: Android deployment ✅
- **Simulator**: Windows/macOS only (Linux not supported)

Fruit Frenzy VR is a fast-paced, arcade-style virtual reality game developed in Unity. Built for the Meta Quest 2, players slice flying fruit in an immersive, low-poly environment using hand-tracking powered by Meta's All-in-One SDK. Inspired by the simplicity and addictiveness of Fruit Ninja, this game was designed and developed in a 2-week sprint for UCR CS135.

## Project Structure

```
FruitFrenzyVR_Meta-AIO-SDK-training/
├── 📁 Assets/                     # Main Unity Assets
│   ├── 🎨 Art/                    # 3D Models & Materials
│   │   ├── Bomb.fbx               # Bomb 3D model
│   │   ├── Fruit/                 # Fruit models directory
│   │   └── Materials/             # Shader materials
│   ├── 🔊 Audio/                  # Sound Effects & Music
│   │   ├── explosion-312361.mp3  # Explosion sounds
│   │   ├── mixkit-sword-cutting-flesh-2788.wav  # Slicing sounds
│   │   └── whoosh-motion-243505.mp3  # Movement sounds
│   ├── 🎮 Scripts/                # C# Game Logic
│   │   ├── AudioManager.cs        # Audio system management
│   │   ├── Fruit.cs               # Fruit behavior & slicing
│   │   ├── Katana.cs             # Weapon mechanics
│   │   ├── GameManger.cs         # Game state & scoring
│   │   └── ClaudeFruitSpawner.cs # Advanced spawning system
│   ├── 🏗️ Prefabs/               # Prefabricated Objects
│   │   ├── Fruit/                # Fruit prefabs
│   │   ├── GameManager.prefab    # Game controller
│   │   └── Explosion.prefab      # Particle effects
│   ├── 🌍 Scenes/                # Unity Scenes
│   │   ├── TestScene.unity       # Main game scene
│   │   └── Environment/          # Environment scenes
│   ├── 🥽 MetaXR/                # Meta SDK Integration
│   ├── 📦 Packages/               # Unity Package Manager
│   │   ├── manifest.json         # Package dependencies
│   │   └── packages-lock.json    # Package version lock
│   └── 🎯 XR/                    # XR Configuration
│       ├── Settings/              # XR-specific settings
│       └── Loaders/              # XR loader configurations
├── 📁 doc/                       # Documentation
│   ├── INITIAL-STRUCTURE.md      # Project architecture analysis
│   ├── MIGRATION.md              # SDK migration guide
│   ├── LINUX.md                  # Linux development guide
│   ├── HELP.md                   # Support & troubleshooting
│   └── EXAMPLES.md              # Code examples & patterns
├── 📄 README.md                  # Project documentation
├── 📄 LICENSE                   # MIT License
└── 📄 ProjectSettings/           # Unity project configuration
```

## Features

- Hand-tracked fruit slicing using Meta’s SDK
- Low-poly stylized 3D art optimized for VR
- Custom-built particle effects and slicing logic
- In-game scoring system
- Built and deployed on Meta Quest 2
Check out the [DEMO](https://drive.google.com/file/d/1oAES8PxBRCcuWLv7wenGbycEZS4SkD8Q/view)! 
## Visual Effects

Fruit Frenzy VR includes custom-designed particle effects for immersive gameplay feedback:

<p align="center">
  <img src="media/gifs/BombExplosion.gif" alt="Bomb Explosion" width="250"/>
  <img src="media/gifs/CannonSmoke.gif" alt="Cannon Smoke" width="250"/>
  <img src="media/gifs/WindwakerWind.gif" alt="Wind Waker Wind" width="250"/>
</p>

## What is Unity?

[Unity](https://unity.com/) is a cross-platform game engine widely used for developing both 2D and 3D interactive experiences. It offers a powerful real-time editor, a comprehensive suite of tools, and strong support for XR development.

### Why Unity?

- VR-ready: Native support for Meta Quest and other major VR platforms  
- Real-time development: Rapid iteration using Unity Editor and Play Mode  
- Asset flexibility: Easy integration of 3D models, sound, and effects  
- Large community: Extensive documentation, plugins, and support  
- Cross-platform: Build once, deploy to PC, mobile, and VR

## Installation

### Prerequisites
- **Unity Editor**: 6000.0.27f2 or later
- **Meta XR SDK**: v85.0.0 (included)
- **Development OS**: Linux, Windows, or macOS
- **Target Device**: Meta Quest 2+ with Developer Mode

### Setup Instructions
```bash
# Clone the repository
git clone https://github.com/AntKinton/FruitFrenzyVR_Meta-AIO-SDK-training.git
cd FruitFrenzyVR_Meta-AIO-SDK-training
```

1. Open the folder in **Unity 6000.0.27f2** or later
2. Unity will automatically resolve packages and apply Linux patches
3. Connect your Meta Quest 2 via USB (enable Developer Mode)
4. Configure XR Plugin Management:
   - **Linux**: Enable Meta XR Plugin, disable OpenXR
   - **Windows/macOS**: Enable both Meta XR Plugin and OpenXR
5. Build and run to deploy the game to the headset

### Linux Development Notes
- Meta XR Simulator is not supported on Linux (expected behavior)
- All core VR development features work perfectly
- Use Quest 2 device for testing VR functionality
- See [doc/LINUX.md](doc/LINUX.md) for detailed Linux setup guide

## Build

Use Unity’s Build Settings to target **Android** (Meta Quest 2), and click **Build and Run**.  
Ensure that Developer Mode is enabled on your headset and all required XR plugins are configured.

## Asset Credits

While all scripts and particle effects were developed in-house, we used the following asset packs:

- `Low Poly Environment Starter Pack` by `MysticForge` – [link](https://assetstore.unity.com/packages/3d/environments/low-poly-environment-starter-pack-228606)
- `Low Poly Samurai Warrior Weapons Pack` by `Poly Ronin` – [link](https://assetstore.unity.com/packages/3d/props/weapons/low-poly-samurai-warrior-weapons-pack-310630)
- `Customizable skybox` by `Key Mouse` – [link](https://assetstore.unity.com/packages/2d/textures-materials/sky/customizable-skybox-174576)
