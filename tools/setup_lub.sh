#!/usr/bin/env bash
set -euo pipefail

revision=ec65d1914cd8ddf23a7a092a0a7a1b12565848b9
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
    dotnet build third_party/tcs/Transpiler/Transpiler.csproj -c Release
    dotnet build third_party/tcs/tcs2c/tcs2c.csproj -c Release
    dotnet build tools/lub-gen/lub-gen.csproj -c Release
    cmake -S third_party/tcs -B third_party/tcs/build -DCMAKE_BUILD_TYPE=Release
    cmake --build third_party/tcs/build --target lua32 --parallel 8
    bash web/scripts/fetch-slang-wasm.sh
)
