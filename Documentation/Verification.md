# Current asset upgrade verification — 7 September 2026

- `Logs/realism-final-build.log`: Unity 2022.3.62f3 compiled, generated Lindenallee, passed the existing 66 + 29 checks, plus 3,819 asset/material assertions, and built `Builds/Mac/FahrCheck-Realism.app`; exit 0.
- New checks validate imported wheel attachment, a valid Humanoid avatar, looping idle/walk clip bindings, root-motion ownership, texture/normal maps, the reusable facade prefab and all enabled renderer materials.
- Chase, pedestrian and cockpit camera renders were inspected. They do not capture the IMGUI overlay or establish interactive performance.
- The executable is Intel x86_64. Unity version and scene GUID are unchanged. Asset sources and limitations: [Asset-sources.md](Asset-sources.md).
- This pass fixes the missing built-in Animation module, incompatible imported car shaders, prefab wheel reparenting and persistent looping clip settings.
- Artifact hashes: `Logs/realism-artifact-hashes.json`.

- `Logs/realism-player.log`: 43 standalone runtime assertions passed before the user requested manual testing. The background test was stopped before full-suite completion; no complete standalone pass is claimed for this revision. Imported walking blend and moving skeleton assertions passed.
- `FahrCheck-Realism.app` was opened normally without automation flags for user testing. The user tested the build before authorizing the asset upgrade for version tracking.

---

# Current Academy verification — 7 September 2026

- `Logs/academy-final-build.log`: Unity 2022.3.62f3 generated the course, passed 66 existing plus 29 Academy rule/scene checks, and built the Intel x86_64 app; exit 0.
- `Logs/academy-runtime.log`: 43 Editor runtime checks passed, including lesson completion, STOP, physical collision and a wheel-driven left turn to the destination. Rule scenarios also use position injection; the isolated left-turn test disables NPC traffic.
- Unity street and articulated-pedestrian renders were inspected. STOP lettering was corrected before the final build.
- `Logs/academy-player.log`: the final standalone headless run completed all 43 runtime checks before the stop request reached it. The background test exited; the manual Academy process runs without automation flags.
- New build: `Builds/Mac/FahrCheck-Academy.app`. The older app is retained. The Academy executable was launched directly without automation flags for the user’s test after macOS bundle opening returned error -600.
- Human keyboard feel, complete manual routes, UI readability, native Intel hardware and distribution signing remain manual validation items.
- Four lessons share one town. The 22-sign guide and fine assessments cover the documented subset, not all driving-law cases. See [scope and sources](German-rules-and-fines.md).

---

## Historical verification below

# Verification record — 6 September 2026

## Original setup diagnosis

- `Packages/manifest.json` and Unity's resolved package list both contained `com.unity.modules.vehicles` 1.0.0. `WheelCollider` was therefore available in the inspected project; the old missing-Vehicles error was no longer the blocker.
- Unity's original Editor log showed assembly loading and execution of `PrototypeChecks.Run`, with no `error CS` compiler diagnostics. The custom Editor assembly was running.
- There was no `Assets/FahrCheck/Scenes/Lindenallee.unity` or generated asset directory. The rule checks failed on absent car/session/camera/HUD/signals and absent Build Settings registration. An empty backup scene was repeatedly loaded.
- `PrototypeBuilder.CreatePrototype` had a bare `return` while `EditorApplication.isPlayingOrWillChangePlaymode`, before any message. The log showed Play-mode reloads. This is a confirmed silent failure path and the strongest explanation for the reported no-op; the log does not record the menu click or prove its precise state at that click. Cancelling either existing save/rebuild dialog was also silent.
- Directory creation and `File.Exists` originally depended on the process working directory, and scene-save success was not checked. These were reliability defects; no evidence establishes them as the original trigger.

The repair reports every blocker/cancellation, displays progress and exceptions in Open Setup, uses AssetDatabase paths, checks scene saves, preserves existing scene/material identifiers, keeps Build Settings entries, and backs up the previous generated scene. Fresh-domain compiler errors still prevent Unity from loading custom Editor UI; this unavoidable limitation is documented in README.

## Executed in Unity

- Installed Intel Editor: `/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity`.
- Original generator foundation: 35 rule and scene checks passed; Lindenallee created and saved.
- Initial runtime integration: 27 checks passed, including WheelCollider acceleration, service/space braking, reverse, steering, cameras, indicator, pause/resume, retry, speed and signal failures, no retroactive red failure, yielding, pedestrian priority, turn indication, completion and retry after success. `Logs/runtime-editor.log`; exit 0.
- Expanded Editor integration: 35 checks passed, including selectable left route, both destination gates, additional traffic references, rain emission, fog visibility and clear-weather restoration. `Logs/runtime-expanded-editor.log`; exit 0.
- Final Unity compilation, scene generation and Intel Mac build: **66 rule and scene checks passed**, including four opposing signal heads and outward-facing car geometry. `Logs/build-final-mac.log`; exit 0.
- Expanded standalone integration before the last geometry/sky polish: **35 checks passed**, `Logs/runtime-expanded-player.log`; exit 0. The exact final app subsequently passed the same 35 checks, `Logs/runtime-final-player.log`, with exit 0 and no runtime errors.
- The executable was inspected with `file`: Mach-O 64-bit executable **x86_64**. `ProjectVersion.txt` remains 2022.3.62f3 and GraphicsSettings has no custom render pipeline.
- Unity-rendered chase and overview previews were visually inspected (`Logs/chase.png`, `Logs/course.png`). Player rain/fog/clear captures are produced by the integration run. These camera renders do not capture the IMGUI overlay.

The controls checks drive the real wheel physics through the same control-processing path used by keyboard input. Rule integration scenarios deliberately reposition the car to exercise known boundaries and outcomes. These are actual Unity Play Mode checks, not a parser or a claim that a human drove the entire route.

## Resolved verification issues

- An early test waited for a FixedUpdate after a failure had correctly frozen time. Failure tests now use unscaled waits.
- One early batch generation exited with a native MonoManager/Selection shutdown crash after all checks passed. The generator no longer changes Editor selection in batch mode; subsequent generation, runtime and build processes exited normally.
- The first standalone test allowed physical keyboard/retry input during automation and later used a destroyed Rigidbody after a scene reload. Automated runs now isolate keyboard and GUI input, run in the background and have a watchdog; normal play retains its controls.
- Visual renders exposed inward-facing custom car-body geometry; faces were corrected and an outward-volume check added. Extra foliage is now attached to existing trees.
- Unity emitted non-blocking licensing-token and USB service diagnostics on some starts. Build/compilation results and runtime outcomes are assessed separately from those platform messages.

## Manual validation still required

1. Drive each complete route by hand. Tune steering feel and stopping distances, especially in rain.
2. Verify keyboard input and UI readability at the user's preferred resolution; inspect both camera views, rain and fog in motion.
3. Check interactive Setup cancellation, overwrite confirmation and backup workflow with a deliberately edited copy.
4. Test the Intel executable on native Intel Mac hardware. This host reports arm64 / Apple M5; Intel Editor and player run via Rosetta.
5. Distribution signing/notarization and Gatekeeper behaviour on another Mac were not tested. This is a local development build.

The implemented legal scope and deliberate exceptions are in [Rules.md](Rules.md). At that earlier revision: collisions, all possible roads/turn directions at arbitrary intersections, yellow stopping decisions and full driving-test assessment are not claimed.

## Final handoff

`Builds/Mac/FahrCheck.app` is the final Intel x86_64 development build (about 135 MB). It launched and completed all 35 standalone integration checks under Rosetta. After the user requested control, it was reopened through macOS without automation flags for normal manual play; no further automated runs were started.

Final scene: `Assets/FahrCheck/Scenes/Lindenallee.unity`, enabled first in Build Settings. Its GUID remains `50704e669f98a4cfca83565ee5ba46d2`. Artifact SHA-256 values are in `Logs/final-artifact-hashes.json`.
