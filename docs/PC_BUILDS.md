# Local PC builds

Fox's Tale can produce Windows x64, macOS, and Linux x64 standalone players locally. These targets are not uploaded to a store and do not use mobile signing credentials.

## Commands

Run the commands from the project root on macOS, with the Unity Editor closed:

```sh
scripts/release-test.sh windows
scripts/release-test.sh macos
scripts/release-test.sh linux
scripts/release-test.sh pc
```

`pc` builds all three players. Outputs and Unity logs are written under `Builds/CI/PC/` and `Builds/CI/logs/`. The script rejects an existing output instead of overwriting it.

The Windows player uses Unity's `Win64` target and is written to `Windows/FoxsTale.exe`. The macOS player uses `OSXStandalone` and is written to `macOS/FoxsTale.app`. The Linux player uses `Linux64` and is written to `Linux/FoxsTale.x86_64`. The standalone catalog check verifies that each build contains an Addressables catalog targeting `StandaloneWindows64`, `StandaloneOSX`, or `StandaloneLinux64`.

The current Unity installation's Windows Mono, macOS IL2CPP, and Linux IL2CPP modules are used by these targets. The pipeline restores the project's previous Standalone scripting backend and active build target after each build. This only prepares local artifacts; it does not configure Steam or another distribution service.
