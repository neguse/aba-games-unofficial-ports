# Native Host regression checks

Run from the repository root with .NET 10 and Python 3:

```sh
python3 tests/native/test_profiles.py
dotnet run --project tests/native/HostBridgeTests.csproj -p:LubRoot=/path/to/lub -m:1
dotnet run --project tests/native/HostProtocolTests.csproj -m:1
```

`HostBridgeTests` builds against the actual shared Lub facade. It verifies all 13
browser-to-native key tables, special twin-stick bit layouts, save/replay
round-trips and isolation, managed callback lifecycle, and Masashikun's native
menu, ranking, and name-entry messages. It does not need a window or audio device.

`HostProtocolTests` compiles the same production bridge and panel against a small
deterministic Lub test double. It checks queued decode cancellation, channel
replacement/retrigger/stop, one-shot and looping music, the browser's 1.28-second
fade, pointer scaling, relative mouse sensitivity, focus loss, ranking button
release, and destructive-clear confirmation. This is protocol coverage rather
than a substitute for testing real audio hardware, gamepad devices, or VR.

`test_profiles.py` prevents the native keyboard/audio manifest from drifting away
from the actual browser game configs. Change both mappings deliberately if a
protocol changes.
