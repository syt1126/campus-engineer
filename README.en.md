# Campus Engineer

**A first-person STEAM learning prototype set in a cozy Hong Kong school.**

[简体中文](README.md) · [Play guide](docs/PLAY_GUIDE.md) · [Architecture](docs/ARCHITECTURE.md) · [Roadmap](docs/ROADMAP.md)

An empty classroom still has its fan running. Explore the room, inspect occupancy, temperature and light sensors, change control rules, and test whether your proposed solution actually works.

The project explores an education product for primary and secondary school settings. It is an individual, AI-assisted development prototype, with some AI-generated original illustrations. School pilots, learning-outcome evidence and willingness-to-pay validation remain future work.

![Actual classroom render](docs/media/classroom-gameplay.png)

## Current prototype

- Unity 2022.3 LTS / C#, Windows-first, version 0.3.
- Simplified Chinese, Traditional Chinese and English: 199 localization entries.
- Three engineering challenges: occupancy gating, light hysteresis, and testing an all-off counterexample against service requirements.
- Local experiment traces, evidence records, saves and result export.
- A retained original red-panda/raccoon companion with connected sculpt geometry and animated responses.
- Seven optional discoveries: paper-plane flight, eight-note xylophone, watering a flower, wind chimes and three hidden stars.
- First-person movement and an alternative fixed-viewpoint click mode.

![Actual companion animation](docs/media/companion-gameplay.gif)

Screenshots and the animation above were recaptured from this public source edition during its Unity integration run. Menu artwork and the icon are original AI-generated illustrations, not screenshots of the 3D game.

## Windows preview

[Download the development preview](https://github.com/syt1126/campus-engineer/blob/main/downloads/CampusEngineer-Windows-v0.3-public.zip), extract the whole folder, and run `CampusEngineer.exe`. Unity is not required to play the build.

## Run from source

Open this repository as a project in Unity 2022.3 LTS (original editor: `2022.3.46f1c1`). Open `Assets/Campus/Scenes/SmartClassroom.unity` and press Play. The empty edit-time scene is intentional: the game constructs its scene at runtime.

Asset Store source models are excluded. The original environment, companion, lessons and interactive activities remain included. Optional props are skipped if absent; see [asset setup](docs/ASSETS.md).

Use **Campus Engineer → Build Windows prototype** to produce a Windows build. Keep the complete build directory together.

Controls: WASD/mouse to move/look, E to interact, Esc to pause, Tab for evidence, R for hints, J for discoveries. Fixed-viewpoint click mode is available in the pause menu.

## Verification and limits

The original local build passed 46 Unity integration checks and a final Windows UI smoke test. The public edition separately passed 46 Unity integration checks and 15 simulation assertions; see [VALIDATION](docs/VALIDATION.md). GitHub Actions checks the standalone C# simulation and publication hygiene; it does not build Unity or validate rendering.

Hints are currently offline presets. Python snippets illustrate rule cards; there is no arbitrary Python execution or online LLM backend. Steam publication, target-student usability and other-device performance have not been validated.

## Source and assets

This is a public source showcase, without a general MIT/Apache reuse grant for original project code or artwork. Third-party CC0 content keeps its own terms. Read [LICENSE](LICENSE.md), [ASSETS](docs/ASSETS.md) and [CONTRIBUTING](CONTRIBUTING.md).
