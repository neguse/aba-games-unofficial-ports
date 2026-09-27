#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
export LUB_BACKEND=openxr
export SDL_VIDEO_DRIVER=dummy
export LUB_NATIVE_LIB="$PWD/native/liblub.so"
export LD_LIBRARY_PATH="$PWD/native${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
exec ./GearToyGear "$@"
