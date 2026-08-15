# Statecraft foundation

The current milestone contains only the playable flow `Boot -> World Map -> Leader`.

- `Core/Runtime`: application bootstrap and screen navigation.
- `Data/Runtime`: country, leader and catalog ScriptableObjects.
- `UI/Runtime/Themes`: visual tokens and their country-agnostic applicator.
- `UI/Runtime/Screens`: UI Toolkit screens.
- `UI/Runtime/Components`: small reusable UI elements.
- `Resources/GameData`: generated demo assets for France and Tunisia.
- `Resources/UI`: shared responsive stylesheet.
- `Editor`: idempotent demo-content generator.

The existing sample scene stays deliberately free of UI wiring. A runtime bootstrap creates the UI document, so pressing Play from the configured scene is sufficient.
