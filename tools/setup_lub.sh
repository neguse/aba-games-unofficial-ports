#!/usr/bin/env bash
set -euo pipefail

revision=88c9d86559195aa0dfc3c20c63a9ca1e1e4fa01e
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
