using System.Collections;
using System.Linq;
using Controllers;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Configuración de la Grid"), Space] [SerializeField]
    private GameObject tilePrefab;

    [SerializeField] private int filas = 5;
    [SerializeField] private int columnas = 5;
    [SerializeField] private bool isGridRoundCornered = true;
    [SerializeField] private bool isEmptySprites = false;

    [Header("Referencia a Scriptable's"), Space] [SerializeField]
    private GridMatrix gridMatrixSO;

    [SerializeField] private GridSpritesSO gridSpritesSO;
    [SerializeField] private GameParseData gameParseData;
    [SerializeField] private GameStateData stateData;

    [Header("Referencia a Objetos"), Space] [SerializeField]
    private Transform maskLigthPosition;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private EyePool eyePool;
    [SerializeField] private Transform magicianPosition;
    [SerializeField] private Transform maskLigthBottom;
    [SerializeField] private Transform mainMaskScale;
    [SerializeField] private BloodRandomManager bloodRandomManager;
    [SerializeField] private Transform goblin;
    [SerializeField] private Transform treasure;

    private bool boardReady = false;

    private void OnEnable()
    {
        GameBridge.OnGameStart += InitBoard;
        GameBridge.OnResyncView += Redraw;
        GameBridge.OnRevealCristalBall += RevealTrapsAroundSpot;
        playerController.showTrapsAround += RevealTrapsAroundSpot;
        GameBridge.OnGameClosed += Reset;
        GameBridge.OnChangeResolution += RecalcularDimensionesVisuales;
    }

    private void OnDisable()
    {
        GameBridge.OnGameStart -= InitBoard;
        GameBridge.OnResyncView -= Redraw;
        GameBridge.OnRevealCristalBall -= RevealTrapsAroundSpot;
        playerController.showTrapsAround -= RevealTrapsAroundSpot;
        GameBridge.OnGameClosed -= Reset;
        GameBridge.OnChangeResolution -= RecalcularDimensionesVisuales;
    }

    private void InitBoard()
    {
        if (isEmptySprites) isGridRoundCornered = false;
        filas = gameParseData.GetGameBoardSize().x;
        columnas = gameParseData.GetGameBoardSize().y;

        // Paso 1: Crear Objetos (Solo una vez por juego)
        GenerarEstructuraGrilla();
    }

    // -----------------------------------------------------------------------
    // FASE 1: CREACIÓN (Solo Instancia y Lógica)
    // -----------------------------------------------------------------------
    private void GenerarEstructuraGrilla()
{
    if (tilePrefab == null || filas <= 0 || columnas <= 0)
    {
        Debug.LogWarning("Faltan datos para crear la grilla.");
        return;
    }

    // 1. Esto borra la referencia de datos (tu array se vuelve null)
    gridMatrixSO.Initialize(filas, columnas);

    for (int fila = 0; fila < filas; fila++)
    {
        for (int col = 0; col < columnas; col++)
        {
            GameObject tile;
            TileInfo info;
            
            string tileName = $"Tile_{col}_{fila}";
            Transform existingTransform = transform.Find(tileName);

            if (existingTransform == null)
            {
                // Primera vez: Creamos el objeto
                tile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, transform);
                tile.name = tileName;
                info = tile.AddComponent<TileInfo>();
            }
            else
            {
                tile = existingTransform.gameObject;
                tile.SetActive(true); // Por seguridad
                
                if (!tile.TryGetComponent<TileInfo>(out info)) 
                    info = tile.AddComponent<TileInfo>();
                
                info.BecameNormal(); 
            }

            // 3. Configuramos la celda
            info.indice = new Vector2Int(col, fila);
            SetSPriteToTile(tile, col, fila);

            // Lógica de trampas y datos
            int cellValue = gameParseData.GetCellValue(col, fila);

            if (IsPlaceDead(cellValue))
            {
                int val = gameParseData.GetCellValue(col, fila);
                string strVal = val == 0 ? "" : val.ToString();
                info.BecameTrap(strVal);
                bloodRandomManager.EnableFirstInactiveChild(tile.transform.position);
            }
            else if (cellValue == 10)
            {
                info.ActiveGreenCell();
            }
            
            gridMatrixSO.SetPosition(col, fila, tile.transform);
        }
    }

    boardReady = true;
    RecalcularDimensionesVisuales();
}

    // -----------------------------------------------------------------------
    // FASE 2: VISUALIZACIÓN (Responsive - Reutilizable)
    // -----------------------------------------------------------------------
    private void RecalcularDimensionesVisuales()
    {
        if (!boardReady) return;

        if (filas <= 0 || columnas <= 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // 🔍 Camera Setting
        cam.orthographicSize = 5.6f;
        float camHeight = cam.orthographicSize * 2f;
        float aspectReal = cam.aspect;
        float portraitAspectMax = 9f / 16f;

        // ✅ Si es más ancho que portrait, forzamos el ancho a 9:16 (Responsive Logic)
        float camWidth = aspectReal > portraitAspectMax
            ? camHeight * portraitAspectMax
            : camHeight * aspectReal;

        // 🧮 Calcular ancho disponible en el eje X
        float maxTileSize = camWidth * 0.95f / columnas;

        // Obtener tamaño base del sprite para calcular escala
        SpriteRenderer sr = tilePrefab.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Vector2 spriteSize = sr.sprite.bounds.size;
        float spriteMaxSide = Mathf.Max(spriteSize.x, spriteSize.y);

        // Calcular escala uniforme y tamaño final del tile
        float scaleFactor = maxTileSize / spriteMaxSide;
        float finalTileSize = spriteMaxSide * scaleFactor;
        Vector3 scale = new Vector3(scaleFactor, scaleFactor, 1f);

        // 🧠 Calculamos el offset para centrar
        Vector2 offset = new Vector2(
            (columnas - 1) * finalTileSize * 0.5f,
            (filas - 1) * finalTileSize * 0.5f
        );

        // --- ACTUALIZAR CELDAS EXISTENTES ---
        for (int fila = 0; fila < filas; fila++)
        {
            for (int col = 0; col < columnas; col++)
            {
                Transform tile = gridMatrixSO.GetPosition(col, fila);

                if (tile != null)
                {
                    float visualRow = filas - 1 - fila;

                    Vector3 newPosition = new Vector3(
                        transform.position.x + (col * finalTileSize) - offset.x,
                        transform.position.y + (visualRow * finalTileSize) - offset.y,
                        0f
                    );

                    tile.position = newPosition;
                    tile.localScale = scale;
                }
            }
        }

        // Actualizar elementos dependientes del tamaño de celda
        MaskSizeCalculation(finalTileSize);
        BoundsCalculator(finalTileSize);

        // IMPORTANTE: Si el grid se mueve, los personajes encima también deben moverse
        ActualizarPosicionesPersonajes();
    }

    // Función auxiliar para recolocar goblin y tesoro si la pantalla cambia
    private void ActualizarPosicionesPersonajes()
    {
        if (goblin != null && goblin.gameObject.activeSelf)
        {
            Vector2Int goblinPos = gameParseData.GetGameInitialPositionPlayer();
            Vector2Int currentGoblinIndex = gameParseData.GetGameInitialPositionPlayer();

            Transform targetTile = gridMatrixSO.GetPosition(currentGoblinIndex.x, currentGoblinIndex.y);
            if (targetTile != null) goblin.position = targetTile.position;
        }

        if (treasure != null && treasure.gameObject.activeSelf)
        {
            Vector2Int treasurePos = gameParseData.GetGameTresaurePosition();
            Transform targetTile = gridMatrixSO.GetPosition(treasurePos.x, treasurePos.y);
            if (targetTile != null) treasure.position = targetTile.position;
        }
    }

    private void BoundsCalculator(float tileSize)
    {
        // Aseguramos que gridMatrix tenga datos antes de acceder
        if (gridMatrixSO.GetPosition(0, 0) == null) return;

        float toplimit = gridMatrixSO.GetPosition(0, 0).position.y;
        float bottomLimit = gridMatrixSO.GetPosition(0, columnas - 1).position.y;

        magicianPosition.position = new Vector2(0, toplimit - 0.25f);
        maskLigthPosition.position = new Vector2(0, toplimit - 0.8f);
        maskLigthBottom.position = new Vector2(0, bottomLimit);
        maskLigthBottom.GetComponent<SpriteRenderer>().size = new Vector2(columnas * tileSize, 1.5f);
    }

    private void MaskSizeCalculation(float finalTileSize)
    {
        float gridTotalWidth = columnas * finalTileSize;
        float gridTotalHeight = filas * finalTileSize;

        if (mainMaskScale != null)
        {
            mainMaskScale.localScale = new Vector3(gridTotalWidth, gridTotalHeight, 1f);
            mainMaskScale.position = transform.position;
        }
    }

    // Resincronización: vuelve a dibujar las celdas con los valores nuevos y ubica al goblin y
    // al tesoro, sin animar.
    private void Redraw()
    {
        if (!boardReady) return;
        bloodRandomManager.DisableChilds();
        eyePool.DisableAll();
        GenerarEstructuraGrilla();

        Vector2Int goblinIndexPos = gameParseData.GetGameInitialPositionPlayer();
        Vector2Int treasureIndexPos = gameParseData.GetGameTresaurePosition();
        goblin.GetChild(0).gameObject.SetActive(true);
        treasure.GetChild(0).gameObject.SetActive(true);
        goblin.transform.GetChild(0).GetComponent<Animator>().CrossFade("playerIdle", 0.1f);
        goblin.position = gridMatrixSO.GetPosition(goblinIndexPos.x, goblinIndexPos.y).position;
        goblin.gameObject.SetActive(true);
        treasure.position = gridMatrixSO.GetPosition(treasureIndexPos.x, treasureIndexPos.y).position;
    }

    public void NewGridValues()
    {
        Debug.Log("NewGridValues-Reset Tiles Sprites");

        Vector2Int goblinIndexPos = gameParseData.GetGameInitialPositionPlayer();
        Vector2Int treasureIndexPos = gameParseData.GetGameTresaurePosition();

        Reset();

        goblin.GetChild(0).gameObject.SetActive(true);
        treasure.GetChild(0).gameObject.SetActive(true);

        goblin.transform.GetChild(0).GetComponent<Animator>().CrossFade("playerIdle", 0.1f);

        // Usamos la grid existente para posicionar
        goblin.position = gridMatrixSO.GetPosition(goblinIndexPos.x, goblinIndexPos.y).position;
        goblin.gameObject.SetActive(true);

        treasure.position = gridMatrixSO.GetPosition(treasureIndexPos.x, treasureIndexPos.y).position;

        StartCoroutine(ShowWrapper());
    }
    
    private IEnumerator ShowWrapper()
    {
        yield return new WaitForSeconds(1f);
        GameBridge.instance.ShowUIWrapper();
    }

    private bool IsPlaceDead(int value) => value >= 0 && value < 9;

    private void RevealTrapsAroundSpot(bool isBooster)
    {
        Vector2Int spot;
        bool wasBooster = false;

        if (isBooster)
        {
            wasBooster = true;
            spot = gameParseData.GetGameTresaurePosition();
        }
        else
        {
            var movedCoords = gameParseData.GetMovedCoordinates();

            if (movedCoords == null || !movedCoords.Any())
                return;

            spot = movedCoords.Last().Coordinate;
        }

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = spot.x + dx;
                int ny = spot.y + dy;

                if (nx >= 0 && nx < columnas && ny >= 0 && ny < filas)
                {
                    Transform tileTransform = gridMatrixSO.GetPosition(nx, ny);
                    int val;
                    if (tileTransform != null)
                    {
                        if (tileTransform.TryGetComponent<TileInfo>(out var tileInfo))
                        {
                            val = gameParseData.GetCellValue(nx, ny);
                            if (val > 0 && val < 9 && !tileInfo.isTrap)
                            {
                                if (wasBooster) eyePool.EnableFirstDisabledChild(tileTransform);
                                tileInfo.BecameTrap(val.ToString());
                            }
                        }
                    }
                }
            }
        }
    }

    private void SetSPriteToTile(GameObject tile, int col, int fila)
    {
        if (tile.TryGetComponent<SpriteRenderer>(out var sr))
        {
            if (isEmptySprites)
            {
                sr.sprite = null;
            }
            else if (isGridRoundCornered)
            {
                if (col == 0 && fila == 0)
                    sr.sprite = gridSpritesSO.GetSpriteById(0, SpritesGridType.TopLeft);
                else if (col == columnas - 1 && fila == 0)
                    sr.sprite = gridSpritesSO.GetSpriteById(0, SpritesGridType.TopRight);
                else if (col == 0 && fila == filas - 1)
                    sr.sprite = gridSpritesSO.GetSpriteById(0, SpritesGridType.BottomLeft);
                else if (col == columnas - 1 && fila == filas - 1)
                    sr.sprite = gridSpritesSO.GetSpriteById(0, SpritesGridType.BottomRight);
                else
                    sr.sprite = gridSpritesSO.GetSpriteById(0,
                        (col + fila) % 2 == 0 ? SpritesGridType.Default1 : SpritesGridType.Default2);
            }
            else
            {
                sr.sprite = gridSpritesSO.GetSpriteById(0,
                    (col + fila) % 2 == 0 ? SpritesGridType.Default1 : SpritesGridType.Default2);
            }
        }
    }

    private void Reset()
    {
        bloodRandomManager.DisableChilds();

        for (int fila = 0; fila < filas; fila++)
        {
            for (int col = 0; col < columnas; col++)
            {
                Transform tile = gridMatrixSO.GetPosition(fila, col);
                if (tile != null)
                {
                    tile.GetComponent<TileInfo>().BecameNormal();
                }
            }
        }
    }
}