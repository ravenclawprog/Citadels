#!/usr/bin/env bash
set -euo pipefail

# Usage: ./BuildScripts/linux/build.sh [dotnet build options]
# Requires the .NET 8 SDK or a compatible newer SDK.
script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
project_root="$(cd -- "$script_dir/../.." && pwd)"

if ! command -v dotnet >/dev/null 2>&1; then
    echo "Error: dotnet was not found. Install the .NET 8 SDK and add it to PATH." >&2
    exit 127
fi

dotnet build "$project_root/Citadel.sln" --configuration Release "$@"
