using UnityEngine;
using System;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "GameStateData", menuName = "Game/GameStateData", order = 1)]
public class GameStateData : ScriptableObject
{
    [Header("Game State")] public GameSnapshot currentStateData = new GameSnapshot();
    public GameSnapshot previousStateData = new GameSnapshot();

    // Eventos para detectar cambios en valores específicos
    public event Action<int> OnUpdateScore;
    public event Action<int> OnUpdateGoblins;
    public event Action<int> OnUpdateLevel;
    public event Action<List<MovedStruct>> OnUpdatePath;
    public event Action<int> OnUpdateScoreBonus;
    public event Action<int> OnUpdateTreasureBonus;
    public event Action<bool> OnUpdateGameOver;

    // flag para controlar el movimiento del goblin si entramos por primera vez
    private bool onFirstTimeLoad = true;

    // Evento general para cualquier cambio de estado
    public event Action<GameSnapshot, GameSnapshot> OnStateChanged;

    public void UpdateState(GameSnapshot newState)
    {
        if (currentStateData.EqualsTo(newState)) return;

        Debug.Log("Actualizando el estado del juego...");

        // Guardar el estado anterior
        previousStateData.CopyFrom(currentStateData);

        // Detectar cambios específicos y disparar eventos
        DetectAndTriggerChanges(newState);

        // Actualizar el estado actual
        currentStateData.CopyFrom(newState);

        // Disparar evento general de cambio de estado
        OnStateChanged?.Invoke(previousStateData, currentStateData);

        // Si entra por primera vez no ejecutamos animación de movimiento
        onFirstTimeLoad = false;
    }

    private void DetectAndTriggerChanges(GameSnapshot newState)
    {
        // Verificar cambios en score
        if (currentStateData.score != newState.score)
        {
            OnUpdateScore?.Invoke(newState.score);
        }

        // Verificar cambios en goblins
        if (currentStateData.goblins != newState.goblins)
        {
            OnUpdateGoblins?.Invoke(newState.goblins);
        }

        // Verificar cambios en level
        if (currentStateData.level != newState.level)
        {
            OnUpdateLevel?.Invoke(newState.level);
        }

        // verificar que currentPath no sea null
        if (newState.path != null && currentStateData.path != newState.path)
        {
            //if (currentStateData.path.Count == 0 || currentStateData.path == null) return;
            Debug.Log("Actualizando el path...");
            if (!onFirstTimeLoad) OnUpdatePath?.Invoke(newState.path);
        }
        else
        {
            Debug.Log($"newState es null");
        }

        // Verificar cambios en scoreBonus
        if (currentStateData.scoreBonus != newState.scoreBonus)
        {
            OnUpdateScoreBonus?.Invoke(newState.scoreBonus);
        }

        // Verificar cambios en treasureBonus
        if (currentStateData.treasureBonus != newState.treasureBonus)
        {
            OnUpdateTreasureBonus?.Invoke(newState.treasureBonus);
        }

        // Verificar cambios en gameOver
        if (currentStateData.gameOver != newState.gameOver)
        {
            OnUpdateGameOver?.Invoke(newState.gameOver);
        }
    }
    
    // Resincronización: reemplaza el estado sin disparar eventos, así no se anima nada. Quien
    // dibuja lee el estado nuevo con GameBridge.OnResyncView.
    public void Overwrite(GameSnapshot newState)
    {
        previousStateData.CopyFrom(currentStateData);
        currentStateData.CopyFrom(newState);
        onFirstTimeLoad = false;
    }

    public void ResetState()
    {
        currentStateData = new GameSnapshot();
        previousStateData = new GameSnapshot();
        onFirstTimeLoad = true;
    }
}