# Infection Ball

Unity Developer test task. The player ball sits at the bottom-left of the field and the door is at the top-right. To get there, the ball has to clear a path through dense obstacles by firing parts of itself.

- **Unity:** 6000.3.27f2, URP, New Input System
- **Platform:** Android (portrait)
- **Build:** `Builds/InfectionBall.apk`
- **Video:** _link_

## How to play

- **Hold:** a shot grows out of the ball and the ball shrinks. Total volume is conserved.
- **Release:** the shot flies forward and hits the first obstacle blocking the ball's lane. The infection then spreads through the group and the infected obstacles explode.
- **Movement:** after each shot the ball hops forward along the cleared lane. The lane narrows together with the ball.
- **Door:** it opens when the ball is within 5 m.
- **Lose:** holding for too long drains the ball below the critical size (*Overcharged*). You also lose when the path is blocked and the ball can no longer afford even the smallest shot (*Out of shots*).

## Architecture

```
Assets/Scripts/Runtime
  Model/      plain C#: LevelModel, GameRules, LevelGenerator, LevelSolver (no MonoBehaviour, no Physics)
  Core/       GameManager: single state machine that wires everything together
  Player/     PlayerBall, ShotCharger, PlayerMover
  Level/      ObstacleField/ObstacleView, LanePath, Door (views only)
  Shooting/   Projectile
  Input/      InputReader over the generated GameInput class
  UI/         HudView, ResultView, SafeArea
  Configs/    GameBalanceConfig, LevelConfig (ScriptableObjects, every tuning value lives here)
```

- **The model is separate from the views.** The game is 2D on the XZ plane: obstacles, ball and shots are circles. All geometry and rules live in `GameRules` and run on `LevelModel`. The MonoBehaviours only play back the result. The rules are deterministic and immune to tunnelling, and they are shared with the solver.
- **`GameManager`** is the only orchestrator. It runs the state machine `Ready → Charging → ShotInFlight → Resolving → Moving → Ready / Win / Lose`. The components don't know about each other: they expose methods and C# events, and `GameManager` connects them through serialized references.
- **Input** goes through the New Input System. The `GameInput.inputactions` asset contains a single `Charge` button bound to touch, mouse and space. `InputReader` is the only class that knows about it. It also ignores taps over UI and releases the press when the app loses focus.

## Rules worth knowing

- **Volume:** `V = r³`. While charging, `chargeRate · curve(holdTime)` of the *current* ball volume moves into the shot every second. This makes charging feel the same at any ball size.
- **Chain infection:** the shot carries energy `E = k · r^1.5`.
  - Infecting an obstacle costs `hopCost + gap · gapCost`, where `gap` is the distance to the previous obstacle in the chain.
  - The spread works like Dijkstra on a precomputed neighbour graph, keeping the highest remaining energy per obstacle.
  - Energy is the reach measured in hops, so the infected area grows as `E² ∝ r³`, which is proportional to the shot's volume. Every unit of size is therefore equally efficient, and only density gives a bonus. A small shot is enough for a single obstacle, while dense groups reward bigger shots.

## The 20% reserve is a mechanic, not a hand-picked number

The start size is computed for every generated level:

1. **`LevelSolver` plays the level** using the same `GameRules` as the player. At each blocked step it tries a small, a medium and a large shot (a human-like choice) and picks the best progress per spent volume.
2. **Bisection finds the smallest start volume** whose finished run still has a reserve: `volume left above critical ≥ 20% × (volume spent + critical)`.
   - The reserve is checked on the finished run rather than added as `1.2 × V_min`, because a bigger ball has a wider lane and spends more.
3. **The game replays the same model.** If a level is unsolvable within the size limits, the next seed is used.

Every level is therefore solvable by construction with at least a 20% reserve. In the editor the solver's plan is drawn as gizmos and logged, for example: `[Solver] seed …: N shots, V0 …, reserve 21%`.

**How it was verified:** during tuning, a temporary bot played the game through real input events.
- Replaying the solver's plan wins every time with about 17% of the start size left.
- A "human" heuristic (picks the shot size from how dense the group is, with ±10% imprecision) won **10/10** different levels in a row, with 15–30% of the size left.

## Tuning

Everything lives in `Assets/Configs`:

- **`GameBalanceConfig`:** sizes, charge, infection, movement, door distance, solver.
- **`LevelConfig`:** seed, field size, start and door points, obstacle density.
