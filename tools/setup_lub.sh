#!/usr/bin/env bash
set -euo pipefail

revision=0f6732c444cb8f14ad8851b4ef2f79dc5b8c249f
directory=.cache/lub
mkdir -p .cache
if [[ ! -d "$directory/.git" ]]; then
    git clone https://github.com/neguse/lub.git "$directory"
fi
git -C "$directory" fetch origin "$revision"
git -C "$directory" checkout --detach "$revision"
git -C "$directory" submodule update --init --recursive
(
    cd "$directory"
    emcmake cmake --preset wasm-release
    cmake --build build/wasm --target lub --parallel 8
    dotnet build third_party/tcs/Transpiler/Transpiler.csproj -c Release
    cmake -S third_party/tcs -B third_party/tcs/build -DCMAKE_BUILD_TYPE=Release
    cmake --build third_party/tcs/build --target lua32 --parallel 8
    bash web/scripts/fetch-slang-wasm.sh
)
