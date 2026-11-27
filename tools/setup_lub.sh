#!/usr/bin/env bash
set -euo pipefail

revision=fcc4d8a71b08f8357bd964e7339909540412fc96
directory=.cache/lub
mkdir -p .cache
if [[ ! -d "$directory/.git" ]]; then
    git clone https://github.com/neguse/lub.git "$directory"
fi
git -C "$directory" fetch origin "$revision"
git -C "$directory" checkout --detach "$revision"
git -C "$directory" submodule update --init --recursive
# tools/lub_tools.py builds lub's tcs and lub-gen when a build first uses them.
(cd "$directory" && bash web/scripts/fetch-slang-wasm.sh)
