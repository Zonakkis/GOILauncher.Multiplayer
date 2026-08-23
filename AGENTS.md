# Repository Instructions

## Build and Test

Use `scripts\build.ps1` for all repository builds and test runs. Do not invoke `dotnet build` or `dotnet test` directly.

- Build: `.\scripts\build.ps1 -Configuration Debug -SkipRestore`
- Build and test: `.\scripts\build.ps1 -Configuration Debug -SkipRestore -RunTests`

`-RunTests` is a hard gate: missing test tooling, a missing test assembly, or a run that
executed zero tests all fail the build. If it reports missing NuGet packages, re-run without
`-SkipRestore`. Test projects are discovered by convention — a directory ending in `.Test` or
`.Tests` under `src\` or `tests\`, whose assembly name matches the directory name.

## Collaboration Guidelines

When encountering something you do not understand or cannot resolve, ask clarifying questions instead of repeatedly guessing and experimenting.

Before changing gameplay, player lifecycle, networking, or state synchronization code, read:

- `CONTEXT.md`
- `docs/game-runtime.md`
- `docs/state-synchronization.md`

Before changing anything the UI layer touches (`Unity.Desktop`, `IUnityClient`, `IUnityServer`, `MultiplayerUnityCore`), read `docs/ui-facade.md`. The UI layer depends on exactly three facades; new capabilities hang off an existing one instead of introducing a fourth root interface.

Treat Unity object names and casing in `docs/game-runtime.md` as exact. Do not infer object or component responsibilities from names alone; confirm them from code, runtime inspection, or the user.

Keep `CONTEXT.md` limited to domain vocabulary and relationships. Record Unity hierarchy, components, coordinate spaces, and runtime behavior in `docs/game-runtime.md`. Update the relevant document as new facts are confirmed.

Treat the synchronization model inherited from `master` as a tested legacy baseline, not as the final design for the current rewrite. Keep legacy facts and new-version decisions distinct.
