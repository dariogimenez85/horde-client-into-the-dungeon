#!/usr/bin/env bash
# Compila el build WebGL en Builds/into-the-dungeon/ con nombres de archivo fijos
# (Build/into-the-dungeon.*). El editor es el de ProjectSettings/ProjectVersion.txt, instalado con
# Unity Hub, o el que indique UNITY_EDITOR.
set -euo pipefail
cd "$(dirname "$0")/.."

version="$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt)"
unity="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/$version/Unity.app/Contents/MacOS/Unity}"
if [ ! -x "$unity" ]; then
  echo "No está el editor de Unity $version en $unity (se puede indicar con UNITY_EDITOR)." >&2
  exit 1
fi

rm -rf Builds/into-the-dungeon
"$unity" -batchmode -quit -nographics \
  -projectPath "$PWD" \
  -buildTarget WebGL \
  -customBuildTarget WebGL \
  -customBuildPath "$PWD/Builds/" \
  -customBuildName into-the-dungeon \
  -executeMethod BuildCommand.PerformBuild \
  -logFile -

ls -la Builds/into-the-dungeon/Build
