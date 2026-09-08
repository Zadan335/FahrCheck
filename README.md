# FahrCheck — German driving challenge for Mac

Unity **2022.3.62f3**, Intel Editor, **Built-in Render Pipeline**. Original geometry plus freely licensed CC0 models and photographed textures; no purchases required. The Vehicles built-in package is enabled in `Packages/manifest.json` for `WheelCollider`.

## Play

1. Open this project with Unity 2022.3.62f3.
2. Choose **FahrCheck → Open Setup → Open and Play**, or open `Assets/FahrCheck/Scenes/Lindenallee.unity` and press Play.
3. Select one of five missions, including the new **Autobahn** level, then click **Start driving** (or press Enter). Focus the Game view for keyboard controls.
4. City Exam offers **LEFT** and **RIGHT** destinations. Autobahn is a separate 2 km, six-lane divided route with no posted maximum and a 130 km/h advisory speed.

For standalone play, open **Builds/Mac/FahrCheck-R8.app**. Older builds are retained; use the R8 app for the new player car.

| Controls | Action |
| --- | --- |
| W / Up | Accelerate |
| S / Down | Service brake (does not select reverse) |
| A / D or Left / Right | Steer |
| Space | Brake |
| X held | Reverse; brakes forward movement first |
| Q / E | Toggle left / right indicator |
| C | Chase / cockpit camera |
| Escape | Pause / resume |
| F | Cycle clear / rain / fog (switches automatic weather off) |
| R | Retry (also after success or failure) |

City limits are 50 → 30 → 50 km/h. Violations display while driving continues and produce a failed assessment with a list at the finish. A legal signal entry is never invalidated by a later signal change. Wrong-route/off-course outcomes are explicitly labelled mission boundaries.

The final turn requires Q for left or E for right at least one second before turning; this is a disclosed course approximation of advance indication. See [implemented rules, sources and exceptions](Documentation/Rules.md), and the [fine model and sign reference](Documentation/German-rules-and-fines.md). Supported failures show illustrative German standard fines; collisions are safety failures without invented legal liability.

## Graphics, traffic and weather

Cars have shaped bodies, transparent windscreens and side glass, mirrors, trim, wheels and working player headlights. The streets use original procedural asphalt grain, trees, curb detail, buildings, four moving oncoming cars, six parked cars and the priority-junction car. Both final-turn exits have finish gates. The Academy update adds articulated walking pedestrians, physical actor collisions, pitched roofs, street furniture, sign geometry, a mission-selection interface, route minimap and subtle image grading. These are original stylized assets, not photorealistic licensed models.

Weather automatically cycles every 60 seconds of mission time: clear → rain → fog. **F** chooses a preset manually. Rain produces local falling droplets, wet-road gloss and reduced tyre grip; fog reduces sight distance. Headlights brighten in poor weather. These are gameplay approximations, not a calibrated vehicle or weather simulation. Pause freezes physics, traffic, precipitation and the automatic weather clock together.

## Setup and preservation

**FahrCheck → Open Setup** provides Create Course, status/error details, Open Lindenallee, Open and Play, and rule/scene checks. The original **1. Create or rebuild prototype** menu remains supported. Play mode, compilation and import blockers are explained instead of silently ignored. If scripts cannot compile on a fresh domain, Unity cannot register any custom editor window: inspect the first red Console compiler error.

Rebuilding asks for confirmation and copies the previous scene to `Assets/FahrCheck/Preserved/`. Scene saves and existing material updates retain their `.meta` identifiers. Asset folders are created through AssetDatabase, independently of shell working directory. Existing Build Settings entries are retained and Lindenallee is enabled first. Batch regeneration only accepts a scene matching the last generated SHA-256 fingerprint in Library; edited/unrecognised scenes require interactive confirmation.

## Verification and build

Use the installed executable below with the Editor closed for this project; never open the same project in two Editor processes.

```sh
UNITY='/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity'
"$UNITY" -batchmode -quit -projectPath "$PWD" -executeMethod FahrCheck.Editor.BuildVerification.Foundation -logFile "$PWD/Logs/checks.log"
"$UNITY" -batchmode -projectPath "$PWD" -executeMethod FahrCheck.Editor.BuildVerification.PlayChecks -fahrcheckSmoke -logFile "$PWD/Logs/runtime-editor.log"
"$UNITY" -batchmode -quit -projectPath "$PWD" -executeMethod FahrCheck.Editor.BuildVerification.BuildMac -logFile "$PWD/Logs/build-mac.log"
```

The runtime suite intentionally has no `-quit`: it enters Play Mode, exercises actual WheelCollider physics, then exits with its result. It supplies automated controls through the same vehicle control path, and uses position injection for rule scenarios. It is not a substitute for a human driving the entire route. The build target is Intel x64 macOS with Mono scripting and a windowed player at `Builds/Mac/FahrCheck-R8.app`. The development build can run the same suite with `-fahrcheckSmoke`. Automated checks disable human controls and exit on completion; omit that flag for normal play.

```sh
Builds/Mac/FahrCheck-R8.app/Contents/MacOS/FahrCheck -batchmode -nographics -fahrcheckSmoke -logFile "$PWD/Logs/academy-player.log"
```

The command above is a headless automated test, not normal play. Tests do not save mission progress. Normal play requires no command-line flags. The in-game guide contains 22 sign references and official catalogue links; only marked mission rules are enforced.

See [verification record](Documentation/Verification.md) for checks actually executed and remaining manual acceptance work.

## Code

- `Editor/PrototypeBuilder.cs`: original geometry, persistent materials, scene generation.
- `Editor/SetupWindow.cs`: visible setup workflow and blockers.
- `Editor/PrototypeChecks.cs`, `Editor/BuildVerification.cs`: rules, scene checks and Mac build entrypoints.
- `Scripts/RuntimeVerification.cs`: opt-in runtime integration suite.
- `Scripts/CarController.cs`, `DrivingCamera.cs`, `DrivingHud.cs`: driving, camera and interface.
- `Scripts/RuleLogic.cs`, `EncounterRules.cs`: modular rule decisions.
- `Scripts/CourseSession.cs`, `CourseEncounters.cs`, `TrafficSignal.cs`: mission state and scenario presentation.

All paths above are relative to `Assets/FahrCheck/` unless otherwise stated.

## Realism asset upgrade

The course now uses an imported coupe with separate animated wheels, a skinned humanoid with idle/walk clips, photographed road/pavement/brick materials, reusable European window modules, gutters and individual-leaf trees. [Asset sources, licences and limitations](Documentation/Asset-sources.md) document exactly what was acquired and adapted. All imports remain compatible with the Built-in pipeline.

The player now uses the owner-supplied Audi R8 2017 by artbyzia (CC BY 4.0), with independently grouped wheels and an interior. Existing traffic keeps the lighter coupe. Attribution and adaptation details are in [Asset-sources.md](Documentation/Asset-sources.md).
