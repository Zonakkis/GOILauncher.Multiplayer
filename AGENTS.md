# Repository Instructions

## Build and Test

Use `scripts\build.ps1` for all repository builds and test runs. Do not invoke `dotnet build` or
`dotnet test` directly — the script owns the test gate described below.

- Build: `.\scripts\build.ps1 -Configuration Debug -SkipRestore`
- Build and test: `.\scripts\build.ps1 -Configuration Debug -SkipRestore -RunTests`
- One project: `.\scripts\build.ps1 -Project DedicatedServer -Configuration Debug -SkipRestore -RunTests`

`-Project` takes a project name from `src\` (`Core`, `Client`, `Server`, `Unity`, `Unity.Desktop`,
`DedicatedServer`) and restricts the run to that project and its dependency closure. Omit it to work
on the whole solution. Note that `Server` is a project name, not the standalone server executable —
that one is `DedicatedServer`.

Without `-Project`, every test project runs. With `-Project`, the script runs the test projects that
transitively reference the named project: `-Project DedicatedServer` runs only `DedicatedServer.Tests`,
while `-Project Core` runs every test project that depends on `Core`, because the selection follows
`ProjectReference` entries rather than directory naming. Test projects themselves are still discovered
by convention — a directory ending in `.Test` or `.Tests` under `src\` or `tests\`.

`-RunTests` is a hard gate: no matching test project, a missing result file, or a run that executed
zero tests all fail the build. If it reports missing packages, re-run without `-SkipRestore`.

Building `DedicatedServer` also builds the React Web UI into `src\DedicatedServer\wwwroot`, because
the server serves that panel; every other target skips it. `-SkipWebUI` skips it in either case.

## Collaboration Guidelines

When encountering something you do not understand or cannot resolve, ask clarifying questions instead of repeatedly guessing and experimenting.

Before changing gameplay, player lifecycle, networking, or state synchronization code, read:

- `CONTEXT.md`
- `docs/agent/game-runtime.md`
- `docs/agent/state-synchronization.md`

Before changing anything the UI layer touches (`Unity.Desktop`, `IUnityClient`, `IUnityServer`, `IGameManager`, `MultiplayerUnityCore`), read `docs/agent/ui-facade.md`. UI accesses multiplayer business through `IUnityClient` and `IUnityServer`, and may use `IGameManager` for scene state and in-game object references, not business operations. `MultiplayerUnityCore` initializes the container. Add business capabilities to the existing client/server facades and game references to `IGameManager` instead of introducing new service roots.

Treat Unity object names and casing in `docs/agent/game-runtime.md` as exact. Do not infer object or component responsibilities from names alone; confirm them from code, runtime inspection, or the user.

Keep `CONTEXT.md` limited to domain vocabulary and relationships. Record Unity hierarchy, components, coordinate spaces, and runtime behavior in `docs/agent/game-runtime.md`. Update the relevant document as new facts are confirmed.

Treat the synchronization model inherited from `master` as a tested legacy baseline, not as the final design for the current rewrite. Keep legacy facts and new-version decisions distinct.
