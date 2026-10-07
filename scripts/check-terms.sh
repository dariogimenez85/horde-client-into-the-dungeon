#!/usr/bin/env bash
# Verifica que el proyecto, el historial y el build no tengan términos del sistema anterior, con
# el script y la lista de hashes del monorepo de horde (carpeta hermana, o HORDE_MONOREPO).
#
# Única excepción (--allow-calls): la función matemática del mismo nombre en código de terceros,
# como los shaders de TextMesh Pro y los que Unity compila en el build.
set -euo pipefail
cd "$(dirname "$0")/.."

monorepo="${HORDE_MONOREPO:-../monorepo}"
script="$monorepo/scripts/forbidden_terms.py"
list="$monorepo/.forbidden-terms.sha256"
if [ ! -f "$script" ] || [ ! -f "$list" ]; then
  echo "No está el monorepo de horde en $monorepo (se puede indicar con HORDE_MONOREPO)." >&2
  exit 1
fi

paths=(Assets Packages ProjectSettings scripts README.md .gitattributes .gitignore)
if [ -d Builds/into-the-dungeon ]; then
  # El build está comprimido: se descomprime una copia para ver sus cadenas.
  tmp="$(mktemp -d)"
  trap 'rm -rf "$tmp"' EXIT
  cp -R Builds/into-the-dungeon/. "$tmp/"
  node -e '
    const fs = require("fs"), path = require("path"), zlib = require("zlib");
    const dir = path.join(process.argv[1], "Build");
    for (const name of fs.readdirSync(dir)) {
      if (!name.endsWith(".unityweb")) continue;
      const file = path.join(dir, name), data = fs.readFileSync(file);
      for (const unzip of [zlib.brotliDecompressSync, zlib.gunzipSync]) {
        try { fs.writeFileSync(file, unzip(data)); break; } catch {}
      }
    }' "$tmp"
  paths+=("$tmp")
fi

uv run --project "$monorepo" python "$script" --paths "${paths[@]}" --allow-calls --list "$list"
if git rev-parse --verify HEAD >/dev/null 2>&1; then
  uv run --project "$monorepo" python "$script" --range=--all --allow-calls --list "$list"
fi
