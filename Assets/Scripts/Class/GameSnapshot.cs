using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSnapshot
{
    public int goblins;
    public int level;
    public List<MovedStruct> path;
    public int score;
    public int scoreBonus;
    public int treasureBonus;
    public bool gameOver;

    // Método para copiar valores de otro snapshot
    public void CopyFrom(GameSnapshot other)
    {
        goblins = other.goblins;
        level = other.level;
        path = other.path;
        score = other.score;
        scoreBonus = other.scoreBonus;
        treasureBonus = other.treasureBonus;
        gameOver = other.gameOver;
    }

    // Método para verificar si dos snapshots son iguales
    public bool EqualsTo(GameSnapshot stateToCompare)
    {
        if (stateToCompare == null) return false;

        return goblins == stateToCompare.goblins &&
               level == stateToCompare.level &&
               path == stateToCompare.path &&
               score == stateToCompare.score &&
               scoreBonus == stateToCompare.scoreBonus &&
               treasureBonus == stateToCompare.treasureBonus &&
               gameOver == stateToCompare.gameOver;
    }

    // // Método para crear una copia del snapshot
    // public GameSnapshot Clone()
    // {
    //     return new GameSnapshot
    //     {
    //         goblins = this.goblins,
    //         level = this.level,
    //         score = this.score,
    //         scoreBonus = this.scoreBonus,
    //         treasureBonus = this.treasureBonus,
    //         gameOver = this.gameOver
    //     };
    // }

    // Override ToString para debugging
    public override string ToString()
    {
        return $"Goblins: {goblins}, Level: {level}, Score: {score}, " + 
               $"Path: {string.Join(", ", path)}, " + 
               $"ScoreBonus: {scoreBonus}, TreasureBonus: {treasureBonus}, " +
               $"GameOver: {gameOver}";
    }
}