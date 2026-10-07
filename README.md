# Into the Dungeon (cliente Unity)

El cliente Unity de Into the Dungeon para horde. La página que lo carga, la conexión con el
servidor de juego y la interfaz alrededor del juego son del wrapper (`packages/game-wrapper` del
monorepo de horde). Este repo aporta solo el build WebGL.

El contrato entre los dos está en la spec 010 del monorepo:
`specs/010-game-client/contracts/unity-bridge.md`.

## Requisitos

- Unity Hub y el editor de `ProjectSettings/ProjectVersion.txt`, con el módulo WebGL Build Support.
- Git LFS (`brew install git-lfs`): las imágenes, el audio, las fuentes, los modelos y las DLL van
  en LFS.
- El monorepo de horde como carpeta hermana (`../monorepo`) y uv, para la verificación de términos.

## Build

```bash
scripts/build-webgl.sh     # deja Builds/into-the-dungeon/Build/into-the-dungeon.*
```

El build tiene nombres de archivo fijos, Brotli con descompresión de respaldo y el template
mínimo de Unity: el `index.html` que genera Unity no se usa. En local, el wrapper lo sirve desde
`GAME_WRAPPER_UNITY_BUILD`.

## Verificación de términos

```bash
scripts/check-terms.sh
```

Busca términos del sistema anterior en el proyecto, en el historial y, si hay build, en el build
descomprimido, con el script y la lista de hashes del monorepo. La única excepción es la función
matemática del mismo nombre en código de terceros (los shaders de TextMesh Pro y los que Unity
compila en el build). Ningún archivo, nombre ni commit de este repo lleva un término del sistema
anterior.

## Puente con la página

- La página le habla a Unity con `SendMessage` al objeto `GameBridge` de la escena Preload (y a
  `PreloadCam` para autorizar la carga).
- Unity le habla a la página con las funciones de `Assets/Plugins/GameBridge.jslib`, que llaman a
  `window.hordeGame`.
