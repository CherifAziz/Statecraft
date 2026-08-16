# Statecraft — Codex Instructions

## Project

Statecraft is a Unity 6.5 desktop PC geopolitical strategy game focused on incarnating a head of state and managing a country.

Tech:
- Unity 6.5
- C#
- URP
- UI Toolkit
- Desktop PC first

## Core architecture

Keep these layers clearly separated:
- Data
- Simulation
- Presentation / UI
- Country-specific art and themes

Shared UI must remain country-agnostic.

Never add logic such as:
- `if France`
- `if Tunisia`
- `if Macron`

Country-specific presentation must come from data and `CountryTheme`.

## Art direction

Visual quality is a core feature of the game.

Target:
- premium
- clean
- prestigious
- institutional
- cinematic
- elegant
- highly readable
- restrained ornamentation
- generous negative space

Avoid:
- SaaS dashboard aesthetics
- generic mobile UI
- excessive rectangles
- excessive glow
- clutter
- flashy gamer UI
- text fighting with busy artwork

Artworks create atmosphere.
UI surfaces guarantee readability.

Each country has its own visual identity while sharing the same master layouts.

For visual tasks, inspect the relevant reference images under:

`Assets/_Game/Art/References/`

When a reference is explicitly designated as a TARGET, treat it as a visual acceptance target, not loose inspiration.

## Assets

Never flatten an interactive screen into one image.

Keep independent:
- backgrounds
- portraits
- overlays
- emblems
- textures
- skill artworks
- dynamic text
- buttons
- stats

Do not modify source art destructively unless explicitly requested.
Do not generate assets.

## Scope discipline

Implement only the requested milestone.

Do not proactively add:
- economy systems
- warfare
- diplomacy
- AI
- save systems
- world simulation
- packages
- abstractions for hypothetical future needs

unless the current task explicitly requires them.

Prefer simple reusable architecture over speculative frameworks.

## UI

UI Toolkit is the default UI system.

Primary reference resolution:
- 1920×1080

Also preserve reasonable desktop behavior at:
- 2560×1440
- 3440×1440

Do not compromise the 1920×1080 primary composition merely to achieve perfect responsiveness everywhere.

For visual tasks, visual quality and readability take priority over minimizing the diff.
Compilation success alone is never sufficient proof of visual completion.

## Git

Do not include unrelated local Unity settings changes in commits.

When the requested task is:
- complete
- compiled
- validated
- free of regressions

commit it and push directly to `main`.

Use a concise conventional commit message.

## Validation

Before finishing a coding task:
- Runtime assembly: 0 errors, 0 warnings
- Editor assembly: 0 errors, 0 warnings
- test the requested user flow
- check shared/country-agnostic behavior for regressions
- report files created/modified
- report important implementation choices

For country UI work, verify that assets from one country do not leak into another.

For visual work, compare the actual final render with the supplied reference before declaring the task complete.

For build-related changes, validate a real standalone player when practical; Play Mode alone is not sufficient.

## Working style

Inspect existing code before changing architecture.
Reuse existing systems when appropriate.
Do not claim visual parity merely because code compiles.
Prefer completing and validating the requested task over explaining at length.

Keep prompts and implementation scope milestone-focused. Do not re-litigate already established project rules unless the current task changes them.
