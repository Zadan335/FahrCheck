# Mixed Audi traffic fleet — 7 September 2026

- `Logs/audi-traffic-build.log`: Unity generated five Audi R8 traffic cars with distinct red, blue, silver, black and green paint, while retaining six coupe traffic cars for variety. All cars passed individual tyre-contact checks and the Intel macOS build completed successfully.
- `Logs/audi-traffic-player.log`: all 52 standalone runtime assertions passed with exit 0, including rolling traffic wheels, stationary parked wheels, rules, weather, collisions, mission completion and the actual wheel-driven left turn.
- The detailed Audi is limited to five traffic actors to control renderer and triangle cost. The scene/build check covered 4,819 enabled renderer/material assertions.

---

# Audi R8 and grounded traffic — 7 September 2026

- `Logs/traffic-grounding-build.log`: Unity 2022.3.62f3 compiled and regenerated Lindenallee, passed 66 prototype checks, 29 Academy checks and 4,037 asset/material assertions, then built `Builds/Mac/FahrCheck-R8.app`; exit 0. Executable verified as Intel x86_64.
- Removed the coupe's fixed visual ground offset. Generation raycasts the actual road/ground surface and aligns each tyre individually, including the source model's unequal axle heights. All 11 traffic/parked cars have four checked tyre contacts. Wheels rotate from actual displacement; stationary cars remain still and route teleports do not cause a spin jump. This is visual wheel motion on scripted traffic, not simulated NPC suspension.
- `Logs/traffic-grounding-player.log`: all 52 standalone runtime assertions passed, exit 0, including moving/stationary traffic wheels, imported human animation, collisions and the actual wheel-driven left turn. Automated player ran headlessly under Rosetta.
- Chase render inspected at `Logs/traffic-chase.png`. Interactive appearance and driving feel still need user testing. Prior manual-validation limitations below continue to apply.
- Scene remains `Assets/FahrCheck/Scenes/Lindenallee.unity`. Audi provenance and attribution are recorded in Asset-sources.md and the app's third-party notices.

---

# Continued-driving foul assessment and junction detail — 7 September 2026

- `Logs/foul-summary-build.log`: Unity regenerated the scene, passed 66 prototype checks, 29 Academy checks and 4,841 asset/material assertions, then rebuilt the Intel x86_64 app successfully.
- `Logs/foul-summary-player.log`: all 53 standalone runtime assertions passed. Tests confirm that rule violations display a notice while driving continues, duplicate offences are suppressed, and reaching the destination with an offence produces a failed result containing the accumulated fine list.
- Added low planters, hedges and open guard rails along the previously empty priority side road. Their height and setback preserve the junction sight triangle. Collisions and mission-boundary exits still end an attempt immediately.

---

# Fuel-station visual refinement — 7 September 2026

- `Logs/fuel-district-build.log`: Unity passed 66 prototype checks, 29 Academy checks and 4,964 asset/material assertions, then rebuilt the Intel app successfully.
- Both stations now include detailed pump islands, displays, keypads, hoses, nozzles, bollards, illuminated canopies, branded shops and two-building background rows. Taller architecture remains recessed behind the junction sight triangle.

---

# Screenshot-targeted streetscape and pedestrians — 8 September 2026

- `Logs/streetscape-pedestrians-build.log`: Unity regenerated the screenshot gaps with 24 additional buildings, six extended side-road pavements, eight trees and eight animated sidewalk pedestrians. It passed 66 prototype checks, 37 Academy scene checks and 5,549 asset/material assertions, then rebuilt the Intel app successfully.
- `Logs/streetscape-pedestrians-player.log`: all 53 standalone runtime assertions passed with the ambient walkers active, including rules, collisions and the physical left-turn route.
- Buildings fill the signal-junction arms, the west side of the priority junction and both sides of the final road. Pedestrian paths remain on pavements and avoid vehicle lanes.

---

# Two-kilometre Autobahn mission — 8 September 2026

- `Logs/autobahn-final-build.log`: Unity generated a separate 2 km Autobahn with three lanes per direction, median barrier, 25 traffic cars, reflectors, signs and irregular mountain ridges; the Intel app built successfully after 69 prototype, 68 Academy and 6,981 asset/material checks.
- `Logs/autobahn-player.log`: all 56 standalone assertions passed, including Autobahn selection/right-lane spawn, no posted-limit state, 2 km completion and all existing city missions. `Logs/autobahn-final.png` was visually inspected after replacing the initial spherical mountain placeholders.
- The geometric ridge placeholders were replaced by continuous high-resolution mountain surfaces along both sides of the complete route, with meadow, exposed rock and snow selected by altitude and slope. A Mac-player screenshot then exposed that Unity's Terrain shader was absent while the forest still rendered at terrain height. The release implementation uses regular 33,153-vertex meshes and the project-owned `FahrCheck/Mountain Terrain` Built-in Pipeline shader, so mountain geometry and trees share the same portable runtime path. Checks cover both surfaces, all three vertex-blended layers, forested foothills, removal of the old wedges and absence of Terrain components; `Logs/autobahn-mountains.ppm` records the driver's view for visual review.
- The signal-junction zebra was removed. The dedicated z=245 zebra now uses five thin painted/paint-height stripes; side-road markings remain split around the main carriageway.

---

# Flat crossings, lane traffic and collision avoidance — 8 September 2026

- `Logs/traffic-safety-roads-build.log`: Unity passed 69 rule/scene checks, 40 Academy checks and 5,674 asset/material assertions, then rebuilt the Intel app successfully.
- Raised sidewalk strips no longer cross vehicle lanes. Three flat painted zebra crossings and centre/edge markings were added to the side roads. Ambient pedestrians now face their actual direction of travel.
- Three vehicles now travel in the player's lane. Both traffic directions decelerate for a player vehicle blocking their lane and ignore obstacles in the opposing lane.
- `Logs/traffic-safety-roads-player.log`: all 54 standalone runtime assertions passed, including a physical same-lane auto-braking check, collisions, rules and the wheel-driven left turn.

---

# Corner fuel stations — 7 September 2026

- `Logs/fuel-stations-build.log`: Unity regenerated the course, passed 66 prototype checks, 29 Academy checks and 4,877 asset/material assertions, then rebuilt the Intel Mac app successfully.
- Added two original-geometry fuel stations with shops, glazed fronts, pump canopies, pumps, forecourts and price pylons. Both are north of their side roads and set back beyond the junction sight triangle; no external station assets were introduced.
- The prior `Logs/foul-summary-player.log` 53-test runtime pass remains applicable to the unchanged driving/rule code. This visual-only station revision was compiled, generated, rendered and build-checked in Unity; it did not repeat the full standalone runtime suite.

---

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
