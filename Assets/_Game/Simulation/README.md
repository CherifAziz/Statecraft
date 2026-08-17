# Simulation

## Definition is not runtime state

`CountryDefinition`, leaders, themes and `CountrySimulationSetup` are immutable authoring assets. A running game never writes into these ScriptableObjects. `GameSessionFactory` copies their simulation values into fresh `CountryState` instances.

`CountrySimulationSetup` contains temporary **game-design tuning**, not factual economic or political claims. France and Tunisia may therefore start with different approval, stability, political capital and treasury values without country-specific code in the simulation.

## Runtime ownership

- `GameRuntime` owns the optional active session and implements start/end/reset-by-restart lifecycle.
- `GameSession` owns the player country, the runtime states for definitions in the current `CountryCatalog`, and one `GameClock`.
- `CountryState` owns mutable population, GDP, treasury, public approval, stability and political capital. Mutations go through its methods so validation and clamping remain centralized.
- `GameClock` starts on the V1 gameplay date of 1 September 2026 and advances explicitly in whole days.

The simulation exposes local C# events (`Changed`, `DateChanged`, `StateChanged`, `SessionChanged`) for future UI binding. It has no dependency on UI Toolkit and no global event bus.

Population uses `long`; GDP and treasury use `double`, which is pragmatic for large game-scale values but is not intended as accounting-grade decimal arithmetic. Percentage gameplay values use `float` and are clamped to 0–100.

## Extending later

Add future gameplay systems beside this runtime core and let them mutate `CountryState` through explicit rules. Do not place mutable game state in definitions, themes, screens or static globals. Economy, diplomacy, war, decisions, events, AI and persistence are intentionally outside Simulation Foundation V1.
