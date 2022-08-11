using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Custom_Grid : MonoBehaviour
{
    private int width;
    private int height;
    private int cellsize;
    private Vector3 originPosition;
    private int[,] gridArray;

    public Custom_Grid(int width, int height, int cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellsize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < gridArray.GetLength(1); z++)
            {
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 1000f);
                Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 1000f);
            }
        }
    }

    public int GetCellSize()
    {
        return cellsize;
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * cellsize + originPosition;
    }

    public int[] GetXZ(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - originPosition).x / cellsize);
        int z = Mathf.FloorToInt((worldPosition - originPosition).z / cellsize);

        return new int[] { x, z };
    }
}
