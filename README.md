# Infection Ball

Unity Developer test task. The player ball sits at the bottom-left of the field and the door is at the top-right. To get there, the ball has to clear a path through dense obstacles by firing parts of itself.

- **Unity:** 6000.3.27f2, URP, New Input System
- **Platform:** Android (portrait)
- **Build:** `Builds/InfectionBall.apk`
- **Video:** https://www.youtube.com/shorts/R5SxKKS9_tM

## How to play

- **Hold:** a shot grows out of the ball and the ball shrinks; the longer the hold, the bigger the shot. A hold too short for a useful shot gives the volume back.
- **Preview:** while you hold, the obstacles the shot would infect light up.
- **Release:** the shot flies into the obstacle that stops the ball and infects every obstacle within its infection radius.
- **Choosing the size:** along the lane dense groups alternate with glades that hold lone obstacles. The infection radius grows a bit faster than the shot volume, so one shot that covers a whole group beats several small ones, a lone obstacle needs only the smallest shot, and a too big shot wastes its reach on the empty glades.
- **Movement:** the ball hops forward along the cleared lane, which narrows together with the ball.
- **Door:** it opens when the ball is within 5 m.
- **Lose:** holding until the ball shrinks to the critical size (*Overcharged*), or the path is still blocked when the ball can't afford even the smallest shot (*Out of shots*).

## Levels

- **Level 1** is a short one-screen level with a forgiving ball size, **level 2** is full-size, later levels reuse the last config with a new seed.
- Progress is saved in `PlayerPrefs`; reset it from the `GameManager` context menu → **Reset Level Progress**.

## Architecture

```
Assets/Scripts/Runtime
  Model/      plain C#: LevelModel, GameRules, LevelGenerator, LevelSolver, LevelFactory
  Core/       GameManager: the state machine that wires everything together
  Player/     PlayerBall, ShotCharger, PlayerMover
  Level/      ObstacleField, ObstacleView, LanePath, Door, Monument
  Shooting/   Projectile
  Input/      InputReader over the generated GameInput class
  UI/         HudView, ResultView, SafeArea
  Configs/    GameBalanceConfig, LevelConfig
```

- **The model is separate from the views.** The game is 2D on the XZ plane: obstacles, ball and shots are circles. The rules live in `GameRules`, run on `LevelModel` and are shared with the solver. MonoBehaviours only play the result back.
- **`GameManager`** runs `Ready → Charging → ShotInFlight → Resolving → Moving → Ready / Win / Lose`. Components expose methods and events and don't know about each other.

## The 20% reserve

The start size is computed for every generated level. `LevelSolver` plays the level with the same `GameRules` as the player and finds the smallest start size from which it reaches the door with 20% of the start volume left. If a level can't be solved within the size limits, the next seed is used.

## Tuning

`Assets/Configs`: `GameBalanceConfig` (sizes, shot, infection radius, movement, solver) and `LevelConfig` (seed, field, start and door points, obstacle density, group and glade lengths).

## Tests

`Assets/Tests/EditMode` (Window → General → Test Runner → EditMode), four tests on the plain C# model:

- the shipped levels reach the door with at least 20% of the start size left;
- a shot infects only the alive obstacles within its reach;
- the smallest shot clears a lone obstacle, while one shot over a dense group costs less than a series of small ones;
- the ball stops before the first obstacle on its lane and reaches the door when the lane is clear.

## Credits

Third-party assets and their licenses are listed in [CREDITS.md](CREDITS.md).
