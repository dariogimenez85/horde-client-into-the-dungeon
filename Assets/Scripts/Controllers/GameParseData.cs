using System;
using System.Collections.Generic;
using Leguar.TotalJSON;
using UnityEngine;

namespace Controllers
{
    public class GameParseData : MonoBehaviour
    {
        [Header("Game Data"), Space]
        [SerializeField] private GameStateData gameData;

        private bool firstLoad = true;
        private Vector2Int sizeBoard;
        private Vector2Int initialPositionPlayer;
        private Vector2Int tresaurePosition;
        private int[,] gridBoardValues;


        private void OnEnable()
        {
            GameBridge.OnUpdateStateGame += SyncFromStateData;
            GameBridge.OnResyncData += ResyncFromStateData;
            GameBridge.OnGameClosed += Reset;
        }

        private void OnDisable()
        {
            GameBridge.OnUpdateStateGame -= SyncFromStateData;
            GameBridge.OnResyncData -= ResyncFromStateData;
            GameBridge.OnGameClosed -= Reset;
        }

        private void Start()
        {
            GameBridge.instance.InitGame();
        }

        private void SyncFromStateData()
        {
            // con cada actualización del juego, actualizamos los valores de la grilla
            GetGameBoardData();

            GameSnapshot newState = new GameSnapshot
            {

                goblins = GetGameStageGoblinData(),
                level = GetGameStageLevelData(),
                path = GetMovedCoordinates(),
                score = GetGameScoreData(),
                scoreBonus = GetGameScoreBonusData(),
                treasureBonus = GetGameTreasureBonusData(),
                gameOver = GetGameOverData()
            };

            gameData.UpdateState(newState);

            if (firstLoad)
            {
                firstLoad = false;
                AudioController.Instance.PlayBgm();
            }
        }

        // Resincronización: el tablero y el estado nuevos, sin camino para que nada se anime.
        private void ResyncFromStateData()
        {
            GetGameBoardData();
            gameData.Overwrite(new GameSnapshot
            {
                goblins = GetGameStageGoblinData(),
                level = GetGameStageLevelData(),
                path = null,
                score = GetGameScoreData(),
                scoreBonus = GetGameScoreBonusData(),
                treasureBonus = GetGameTreasureBonusData(),
                gameOver = GetGameOverData()
            });
        }

        public bool GetGameOverData() => GameBridge.instance.StagingInfo.GetBool("gameOver");

        public int GetGameScoreData() => GameBridge.instance.StagingInfo.GetInt("score");


        private void GetGameBoardData()
        {
            JArray jsonBoard = GameBridge.instance.StagingInfo.GetJArray("table");

            if (jsonBoard == null || jsonBoard.Length == 0)
            {
                Debug.LogWarning("La tabla del juego ('table') está vacía o no fue enviada.");
                return;
            }

            int rows = jsonBoard.Length;

            JArray firstRow = jsonBoard.GetJArray(0);
            if (firstRow == null || firstRow.Length == 0)
            {
                Debug.LogWarning("La primera fila de la tabla está vacía.");
                return;
            }

            int cols = firstRow.Length;
            sizeBoard = new Vector2Int(cols, rows);

            int[,] result = new int[rows, cols];

            for (int y = 0; y < rows; y++)
            {
                JArray row = jsonBoard.GetJArray(y);
                for (int x = 0; x < cols; x++)
                {
                    int value = row.GetInt(x);
                    result[x, y] = value;

                    switch (value)
                    {
                        case 50:
                            initialPositionPlayer = new Vector2Int(x, y);
                            break;
                        case 100:
                            tresaurePosition = new Vector2Int(x, y);
                            break;
                    }
                }
            }

            gridBoardValues = result;

        }

        public int GetCellValue(int x, int y) => gridBoardValues[x, y];

        public Vector2Int GetGameBoardSize() => sizeBoard;

        public Vector2Int GetGameInitialPositionPlayer() => initialPositionPlayer;

        public Vector2Int GetGameTresaurePosition() => tresaurePosition;


        // public List<Vector2Int> GetMovedCoordinates()
        // {
        //     if (GameBridge.instance?.StagingInfo == null)
        //     {
        //         Debug.LogWarning("StagingInfo es null en GetMovedCoordinates");
        //         return null;
        //     }

        //     JArray coordsArray = GameBridge.instance.StagingInfo.GetJArray("coordinatesMove");

        //     if (coordsArray == null || coordsArray.Length == 0)
        //     {
        //         return null;
        //     }

        //     List<Vector2Int> result = new List<Vector2Int>(coordsArray.Length);

        //     for (int i = 0; i < coordsArray.Length; i++)
        //     {
        //         JArray indexes = coordsArray.GetJArray(i);
        //         if (indexes == null || indexes.Length < 2)
        //         {
        //             Debug.LogWarning($"Elemento en index {i} no es un par válido");
        //             continue;
        //         }

        //         int x = indexes.GetInt(0);
        //         int y = indexes.GetInt(1);

        //         result.Add(new Vector2Int(x, y));
        //     }

        //     return result;
        // }


        public List<MovedStruct> GetMovedCoordinates()
        {
            if (GameBridge.instance?.StagingInfo == null)
            {
                Debug.LogWarning("StagingInfo es null en GetMovedCoordinates");
                return null;
            }

            JArray coordsArray = GameBridge.instance.StagingInfo.GetJArray("coordinatesMove");

            if (coordsArray == null || coordsArray.Length == 0)
            {
                return null;
            }

            List<MovedStruct> result = new List<MovedStruct>(coordsArray.Length);

            for (int i = 0; i < coordsArray.Length; i++)
            {
                JSON item = coordsArray.GetJSON(i);
                if (item == null)
                {
                    Debug.LogWarning($"Elemento en index {i} no es un objeto válido");
                    continue;
                }

                JArray indexes = item.GetJArray("coordinate");
                if (indexes == null || indexes.Length < 2)
                {
                    Debug.LogWarning($"'coordinate' en index {i} no es un par válido");
                    continue;
                }

                int x = indexes.GetInt(0);
                int y = indexes.GetInt(1);
                int score = item.GetInt("score"); // ← también obtenés el score

                result.Add(new MovedStruct(new Vector2Int(x, y), score));
            }

            return result;
        }


        public int GetGameScoreBonusData() => GameBridge.instance.StagingInfo.GetInt("scoreBonus");

        public int GetGameTreasureBonusData() => GameBridge.instance.StagingInfo.GetInt("treasureScore");

        public int GetGameStageLevelData() => GameBridge.instance.StagingInfo.GetInt("levelPlayer");

        public int GetGameStageGoblinData() => GameBridge.instance.StagingInfo.GetInt("goblins");

        public int GetGoblinBonusData() => GameBridge.instance.StagingInfo.GetInt("bonusGoblins");


        private void Reset()
        {
            firstLoad = true;
            sizeBoard = Vector2Int.zero;
            initialPositionPlayer = Vector2Int.zero;
            tresaurePosition = Vector2Int.zero;
            gridBoardValues = null;
            gameData.ResetState();
        }

    }
}