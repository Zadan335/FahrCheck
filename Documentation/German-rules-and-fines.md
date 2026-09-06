# German rules, signs and fine model

Research date: 7 September 2026. This is the design reference for a passenger-car driving game, not an exhaustive legal assessment engine. The in-game reference library currently explains 22 common sign types and links to the official complete catalogue; only the explicitly labelled mission rules are assessed.

## Authoritative sources

- [StVO complete text and annexes](https://www.gesetze-im-internet.de/stvo_2013/): road conduct and signs. Annex 1 covers warnings, Annex 2 mandatory/prohibitory signs, Annex 3 information/priority signs.
- [BKatV offence catalogue](https://www.gesetze-im-internet.de/bkatv_2013/anlage.html): standard fine rates and case variants.
- [BKatV speeding table](https://www.gesetze-im-internet.de/bkatv_2013/anhang.html): the implementation uses Table 1(c), urban passenger cars.
- [FeV Annex 13](https://www.gesetze-im-internet.de/fev_2010/anlage_13.html): points associated with listed offences.
- [StVG §4](https://www.gesetze-im-internet.de/stvg/__4.html): points system and administrative stages.
- [FeV Annex 12](https://www.gesetze-im-internet.de/fev_2010/anlage_12.html): serious/less serious probationary offences.
- [FeV Annex 7](https://www.gesetze-im-internet.de/fev_2010/anlage_7.html): driving examination requirements.

A game failure, an examination failure, an administrative fine, a driving ban, and licence withdrawal are different outcomes. This game does not simulate an official examiner or convert its mission score into a licence decision. Points shown on a result are an illustrative offence assessment, not a persistent official driver record.

## Implemented standard rates

| Detected scenario | EUR | Source |
| --- | ---: | --- |
| Missing turn indication | 10 | BKat 29 |
| Materially obstructing priority traffic | 25 | BKat 33; scenario assumption disclosed |
| Not allowing a vehicle through when turning | 40 | BKat 39 |
| Pedestrian-crossing priority | 80 | BKat 113; one point under FeV Annex 13 |
| STOP not obeyed | 10 | BKat 136.1 |
| Stop-line crossing | 10 | BKat 154 |

The red-light reference evaluator distinguishes up to one second (EUR 90) from more than one second (EUR 200, one-month ban), BKat 132/132.3. **The playable strict detector currently stops at the stop line and assesses the stop-line case, not automatically a full protected-area red-light offence.** It cannot fairly infer a more serious offence that the player was prevented from committing.

Urban passenger-car speeding bands (excess km/h → EUR): ≤10 → 30; ≤15 → 50; ≤20 → 70; ≤25 → 115; ≤30 → 180; ≤40 → 260; ≤50 → 400; ≤60 → 560; ≤70 → 700; above 70 → 800. Source: BKatV Appendix Table 1(c). Points/bans follow the applicable band. The game uses its exact simulated speed and a numerical epsilon; it does not mimic a particular measuring device or police measurement deduction.

Rates assume the standard negligent case. Administrative fees, expenses, repetition, intent, endangerment, injury, damages, concurrent offences and individual legal adjustments are not inferred. The strict game ends at the first confirmed rule failure, so amounts are not stacked into an invented invoice. Collision contact itself produces a safety failure with no automatic fine: it is insufficient to establish fault, damage or criminal liability.

## Signs and coverage

The 22 reference cards cover 274, 278, 282, 206, 205, 306, 307, 301, 267, 250, 209, 220, 276, 283, 286, 350, 101, 136, 123, 330.1, 310 and 314. Each card explains its purpose; assessed signs are visibly distinguished from reference-only entries.

The course physically contains posted speeds, sign 282, STOP in the dedicated lesson, a pedestrian-crossing sign and signals. Sign 282 at z=215 ends the posted restriction: **urban 50 km/h still applies**. Neither 278 nor 282 means every road becomes unlimited. Special vehicle, road and weather rules still matter. The urban game intentionally has no unrestricted Autobahn section.

Not every sign is placed on the playable road. Placing contradictory prohibitions and priorities at one junction would produce an incoherent lesson. The official catalogue links cover the full legal sign system; the game does not claim to enforce all its entries.

## Broader licence knowledge

The sources above cover substantially more than this game: lane discipline, following distance, overtaking, railway crossings, bus stops, parking/stopping, lights, emergency corridors, load/passenger security, mobile-device use, impairment, accident duties, vehicle/driver documentation and probationary rules. These need their own observable scenarios before they can be scored fairly. They are not silently treated as implemented.

Practical observation, mirror checks, hazard anticipation, weather-appropriate speed, communication and safe positioning cannot be replaced by the current simple boundary detectors. Collision fault, personal injury, intoxication, offences requiring intent and licence consequences require evidence this game does not collect.
