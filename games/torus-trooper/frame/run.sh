#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
export LUB_BACKEND=openxr
export SDL_VIDEO_DRIVER=dummy
export LUB_NATIVE_LIB="$PWD/native/liblub.so"
export LD_LIBRARY_PATH="$PWD/native${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
case "${LUB_RUNTIME:-coreclr}" in
    coreclr) exec taskset -c 2-7 ./TorusTrooper "$@" ;;
    lua) exec taskset -c 2-7 ./native/lub game.lua "$@" ;;
    *) echo "LUB_RUNTIME must be coreclr or lua" >&2; exit 2 ;;
esac
