using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "Game/Grid Matrix")]
public class GridMatrix : ScriptableObject
{
    [HideInInspector] public int filas;
    [HideInInspector] public int columnas;

    [SerializeField] private Transform[] posiciones;

    public void Initialize(int f, int c)
    {
        filas = f;
        columnas = c;
        posiciones = new Transform[f * c];
    }

    public void SetPosition(int fila, int columna, Transform t)
    {
        posiciones[fila * columnas + columna] = t;
    }

    public Transform GetPosition(int fila, int columna)
    {
        if (fila < 0 || fila >= filas || columna < 0 || columna >= columnas)
        {
            Debug.LogError($"Coordenada inválida: fila={fila}, columna={columna}");
            return null;
        }

        int index = fila * columnas + columna;
        return posiciones[index];
    }


    public Transform[,] GetMatrix2D()
    {
        Transform[,] matrix = new Transform[filas, columnas];
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                matrix[i, j] = GetPosition(i, j);
            }
        }
        return matrix;
    }
}
