#!/usr/bin/env sh
set -eu
cd "$(dirname -- "$0")"
export LD_LIBRARY_PATH="$PWD/native${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
export LUB_NATIVE_LIB="$PWD/native/liblub.so"
mode="${ABA_MODE:-desktop}"
case "${1:-}" in
  --vr) mode=vr; shift ;;
  --desktop) mode=desktop; shift ;;
esac
case "$mode" in
  vr) export LUB_BACKEND=openxr; export SDL_VIDEO_DRIVER=dummy ;;
  desktop) export LUB_BACKEND=vulkan; unset SDL_VIDEO_DRIVER ;;
  *) printf 'Unknown ABA_MODE: %s\n' "$mode" >&2; exit 2 ;;
esac
exec ./AbaCollection "$@"
