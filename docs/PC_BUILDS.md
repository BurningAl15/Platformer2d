# Local PC builds

Fox's Tale can produce Windows x64 and macOS standalone players locally. These targets are not uploaded to a store and do not use mobile signing credentials.

## Commands

Run the commands from the project root on macOS, with the Unity Editor closed:

```sh
scripts/release-test.sh windows
scripts/release-test.sh macos
scripts/release-test.sh pc
```

`pc` builds both players. Outputs and Unity logs are written under `Builds/CI/PC/` and `Builds/CI/logs/`. The script rejects an existing output instead of overwriting it.

The Windows player uses Unity's `Win64` target and is written to `Windows/FoxsTale.exe`. The macOS player uses `OSXStandalone` and is written to `macOS/FoxsTale.app`. The standalone catalog check verifies that each build contains an Addressables catalog whose runtime path targets `StandaloneWindows64` or `StandaloneOSX`, respectively.

The current Unity installation's Windows Mono and macOS IL2CPP modules are used by these targets. The pipeline restores the project's previous Standalone scripting backend and active build target after each build. This only prepares local artifacts; it does not configure Steam or another distribution service.
