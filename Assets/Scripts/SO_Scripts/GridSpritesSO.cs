using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GridSprites", menuName = "Game/GridSprites")]
public class GridSpritesSO : ScriptableObject
{
    [System.Serializable]
    public class GridSprite
    {
        public SpritesGridType id;        // Identificador único
        public Sprite sprite;    // Sprite asociado
    }

    [System.Serializable]
    public class GridSpriteList
    {
        public List<GridSprite> gridSprites; // Lista de GridSprite
    }

    public List<GridSpriteList> cornerSpritesList = new List<GridSpriteList>(); // Lista de listas serializadas

    // Función para obtener los sprites de la grilla por su identificador
    public Sprite GetSpriteById(int listIndex, SpritesGridType spriteType)
    {
        if (listIndex < 0 || listIndex >= cornerSpritesList.Count)
        {
            Debug.LogError("Índice de lista fuera de rango.");
            return null; // Si el índice es inválido, retornamos null.
        }

        var spriteList = cornerSpritesList[listIndex]; // Obtener la lista específica usando el índice

        foreach (var gridSprite in spriteList.gridSprites)
        {
            if (gridSprite.id == spriteType)
            {
                return gridSprite.sprite;
            }
        }

        return null; // Si no se encuentra el sprite en esa lista, retornamos null.
    }
}
