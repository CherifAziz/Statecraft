# Statecraft foundation

The current milestone contains only the playable flow `Boot -> World Map -> Leader`.

- `Core/Runtime`: application bootstrap and screen navigation.
- `Data/Runtime`: country, leader and catalog ScriptableObjects.
- `UI/Runtime/Themes`: visual tokens, semantic Leader Screen art slots and their country-agnostic applicator.
- `UI/Runtime/Screens`: UI Toolkit screens.
- `UI/Runtime/Components`: small reusable UI elements.
- `Resources/GameData`: generated demo assets for France and Tunisia.
- `Resources/UI`: shared responsive stylesheet.
- `Editor`: idempotent demo-content generator.

The existing sample scene stays deliberately free of UI wiring. A runtime bootstrap creates the UI document, so pressing Play from the configured scene is sufficient.

The optional Leader Screen art slots are `leaderBackgroundArtwork`, `leaderForegroundOverlay`, `emblem`, `portraitFrame` and `surfaceTexture`. Portraits remain leader data. Skill artwork is resolved by skill ID from the theme's `leaderSkillArtworks` bindings, then passed to reusable `LeaderSkillCard` instances through `LeaderSkillCardData`.

Leader background motion is opt-in per `CountryTheme`. `leaderBackgroundFxEnabled` is the master switch; `leaderParallaxStrength`, `leaderDriftStrength` and `leaderLightBreathingStrength` independently control the three subtle effects. A disabled theme, or a theme without background artwork, remains completely static.
