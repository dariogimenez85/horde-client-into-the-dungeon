using System;
using System.Collections;
using System.Runtime.InteropServices;
using Art.Shaders;
using Controllers;
using UnityEngine;
using Leguar.TotalJSON;

// Puente con la página de juego: recibe sus mensajes (SendMessage al objeto GameBridge de la
// escena Preload) y le habla con las funciones de GameBridge.jslib. Contrato: el puente de la
// spec 010 del monorepo de horde (contracts/unity-bridge.md).
public class GameBridge : MonoBehaviour
{
    // GameNetworkStates
    public GameStates currentGameState;

    public static Action OnGameStart = delegate { };
    public static Action OnGameOver = delegate { };
    public static Action OnUpdateStateGame = delegate { };
    public static Action<int, bool> OnUpdateTime = delegate { };

    public static Action OnChangeResolution = delegate { };
    public static Action<string> OnOrientationChange = delegate { };
    public static Action<int> OnScoreRetrieved = delegate { };
    public static Action OnTimeUp = delegate { };
    public static Action<string> OnExtraTimeBooster = delegate { };
    public static Action<bool> OnRevealCristalBall = delegate { };
    public static Action OnExtraGoblins = delegate { };
    public static Action OnPlayerQuit = delegate { };
    public static Action OnGameClosed = delegate { };

    // Jugada rechazada por el servidor: se borra el camino y se vuelve a esperar jugada.
    public static Action OnMoveRejectedByServer = delegate { };

    // Resincronización a mitad de partida, en dos fases: primero se leen los datos y después se
    // redibujan tablero, vidas y puntaje, sin animar jugadas.
    public static Action OnResyncData = delegate { };
    public static Action OnResyncView = delegate { };


    public static GameBridge instance;

    #region JSLIB FUNCTIONS

    [DllImport("__Internal")]
    public static extern void ConnectToGameApi();

    [DllImport("__Internal")]
    public static extern void CheckForInitialStatus();

    [DllImport("__Internal")]
    public static extern void CloseGame();

    [DllImport("__Internal")]
    public static extern void SendGameInput(string actionJson);

    [DllImport("__Internal")]
    public static extern void ChangeCanvasOpacity();

    [DllImport("__Internal")]
    public static extern void CloseSplash();

    [DllImport("__Internal")]
    public static extern void ShowUIControls();

    [DllImport("__Internal")]
    public static extern void HideUIControls();

    #endregion

    private JSON stagingInfo;
    private int stageTimeLeft;
    private int totalScore;
    private bool engineReady = false;
    private bool gameOver = false;

    public Vector2 canvasSize;
    public bool isGameClosed = false;
    public bool isGameStarted = false;
    public bool engineStarted = false;

    public JSON StagingInfo
    {
        get => stagingInfo;
        set => stagingInfo = value;
    }

    public int StageTimeLeft
    {
        get => stageTimeLeft;
        set => stageTimeLeft = value;
    }

    public bool IsGameClosed
    {
        get => isGameClosed;
        set => isGameClosed = value;
    }


    private void Awake()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
				    WebGLInput.captureAllKeyboardInput = false;
#endif

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);
    }

    public void InitGame()
    {
        CheckForInitialStatus();
    }

    public void ScreenData(string data)
    {
        string[] parts = data.Split(',');

        if (parts.Length != 3)
        {
            Debug.LogWarning("Formato inválido en ScreenData: " + data);
            return;
        }

        string orientation = parts[0];

        if (!int.TryParse(parts[1], out int width) ||
            !int.TryParse(parts[2], out int height))
        {
            Debug.LogWarning("No se pudieron parsear width/height en ScreenData: " + data);
            return;
        }

        canvasSize = new Vector2(width, height);
        StartCoroutine(DelayChangeResolution(orientation));
    }

    private IEnumerator DelayChangeResolution(string orientation)
    {
        yield return new WaitUntil(() => FindObjectOfType<UILocation>() != null);
        OnChangeResolution();
        OnOrientationChange(orientation);
    }

    public void GoToResults()
    {
        print("GoToResults...");
        CloseGame();
        OnGameClosed();
        engineReady = false;
        currentGameState = GameStates.StartGame;
    }

    private bool ParseStateJson(string s)
    {
        StagingInfo = JSON.ParseString(s, "StagingInfo");
        if (instance == null || StagingInfo == null) return false;

        SetStagingTimeLeft(false);

        if (stagingInfo.GetBool("gameOver"))
        {
            if (stagingInfo.GetInt("goblins") == 0)
            {
                OnUpdateStateGame();
            }
            else
            {
                currentGameState = GameStates.ReceiveEndGame;
            }

            return true;
        }

        if (currentGameState == GameStates.SendingInput ||
            currentGameState == GameStates.WaitingForData ||
            currentGameState == GameStates.SendingBoosterGoblins ||
            currentGameState == GameStates.SendingBoosterTime ||
            currentGameState == GameStates.SendingBoosterCrystalBall)
        {
            OnUpdateStateGame();
        }

        currentGameState = GameStates.ReceiveInput;
        return true;
    }

    public void ReceivePick(string s)
    {
        Debug.Log("Received staging data from pick!");
    }

    public void SendActionToEngine(string s)
    {
        print("Current Game State: " + currentGameState);

        if (currentGameState == GameStates.SendingInput ||
            currentGameState == GameStates.WaitingForData ||
            string.IsNullOrEmpty(s) || currentGameState == GameStates.ReceiveEndGame) return;

        currentGameState = GameStates.SendingInput;
        SendGameInput(s);
    }

    private IEnumerator _ProcessFetchData(string s)
    {
        Debug.Log("ProcessFetchData");
        currentGameState = GameStates.WaitingForData;
        yield return new WaitUntil(() => ParseStateJson(s));
        OnGameStart();
    }

    private void SetStagingTimeLeft(bool animate)
    {
        StageTimeLeft = StagingInfo.GetJSON("time").GetJNumber("left").AsInt();

        if (StageTimeLeft <= 0) OnTimeUp();

        OnUpdateTime(StageTimeLeft, animate);
    }

    public void ShowUIWrapper()
    {
        ShowUIControls();
    }

    public void HideUIWrapper()
    {
        HideUIControls();
    }

    public void OnBoosterUsed(string dataJson)
    {
        JSON json = JSON.ParseString(dataJson);
        string boosterId = json.GetString("boosterId");
        JSON responseJson = json.GetJSON("response");
        string response = responseJson.CreateString();

        switch (boosterId)
        {
            case "Extra Time":
                BoostExtraTime(response);
                break;
            case "Extra Goblins":
                BoostExtraGoblins(response);
                break;
            case "Crystal Ball":
                BoostCrystalBall(response);
                break;
        }
    }

    private void BoostExtraTime(string s)
    {
        ApplyBooster(GameStates.SendingBoosterTime, "Extra time Booster!", s, true);
    }

    private void BoostExtraGoblins(string s)
    {
        ApplyBooster(GameStates.SendingBoosterGoblins, "Extra life Booster!", s);
    }

    private void BoostCrystalBall(string s)
    {
        ApplyBooster(GameStates.SendingBoosterCrystalBall, "Crystal Ball Booster!", s);
    }

    private void ApplyBooster(GameStates newState, string logMessage, string json, bool setStagingTime = false)
    {
        if (currentGameState == GameStates.ReceiveEndGame) return;

        Debug.Log(logMessage);
        currentGameState = newState;

        if (setStagingTime)
            SetStagingTimeLeft(true);

        StartCoroutine(WaitForBooster(json, newState));
    }

    private IEnumerator WaitForBooster(string s, GameStates boosterToApply)
    {
        // si es necesario esperar a que se completen animaciones poner aca yield return ... 
        yield return new WaitUntil(() => ParseStateJson(s));
        switch (boosterToApply)
        {
            case GameStates.SendingBoosterTime:
                OnExtraTimeBooster("Extra-Time!");
                break;

            case GameStates.SendingBoosterGoblins:
                OnExtraGoblins();
                OnExtraTimeBooster("Extra-Goblins!");
                break;

            case GameStates.SendingBoosterCrystalBall:
                OnRevealCristalBall(true);
                OnExtraTimeBooster("True-Sight!");
                break;
        }
    }

    public void TurnAudioOn()
    {
        AudioController.Instance.EnableSound();
    }

    public void TurnAudioOff()
    {
        AudioController.Instance.DisableSound();
    }


    public void OnSoundChanged(string isActiveStr) => (isActiveStr == "1" ? (Action)TurnAudioOn : TurnAudioOff)();


    public void ReceiveApiMessage(string data)
    {
        Debug.Log("---ReceiveApiMessage---", this);
        ParseStateJson(data);
        if (StagingInfo.GetBool("gameOver") == true)
        {
            ReceiveEndGame();
        }
        else
        {
            ReceiveInputResult();
        }
    }

    public void ReceiveStartData(string data = "")
    {
        Debug.Log("---Received start data from api---", this);

        CloseSplash();
        ChangeCanvasOpacity();

        // Un solo estado inicial, que se lee una sola vez: leerlo dos veces disparaba dos veces
        // el fin de una parte ya terminada.
        if (engineReady) return;
        Debug.Log("Received staging data from start!");
        gameOver = false;
        engineReady = true;
        StartCoroutine(_ProcessFetchData(data));
    }

    // La página avisa que la jugada fue rechazada: Unity vuelve a esperar jugada.
    public void OnMoveRejected()
    {
        if (currentGameState != GameStates.SendingInput &&
            currentGameState != GameStates.WaitingForData) return;

        Debug.Log("---OnMoveRejected---", this);
        currentGameState = GameStates.ReceiveInput;
        OnMoveRejectedByServer();
        ShowUIControls();
    }

    // Después de reconectar a mitad de partida: vuelve a dibujar tablero, vidas, puntaje y reloj
    // sin animar jugadas.
    public void ReceiveResync(string data)
    {
        if (!engineReady)
        {
            ReceiveStartData(data);
            return;
        }

        JSON state = JSON.ParseString(data, "StagingInfo");
        if (state == null) return;
        if (state.GetBool("gameOver"))
        {
            ReceiveApiMessage(data);
            return;
        }

        Debug.Log("---ReceiveResync---", this);
        StagingInfo = state;
        currentGameState = GameStates.ReceiveInput;
        OnResyncData();
        OnResyncView();
        SetStagingTimeLeft(false);
        ShowUIControls();
    }


    public void ReceiveInputResult()
    {
        Debug.Log("---ReceivedInputResult---", this);
    }

    public void ReceiveEndGame()
    {
        Debug.Log("---ReceiveEndGame---", this);
        if (gameOver) return;
        
        engineStarted = false;
        gameOver = true;
        IsGameClosed = true;
        OnGameOver();
    }

    public void OnGameEnded(string s)
    {
        Debug.Log($"Player Quit: {s}");
        OnPlayerQuit();
        ReceiveEndGame();
    }
}