using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public int width;
    public int height;
    public int cellsize;
    public GameObject gridSpawnPOS;
    [HideInInspector] public Custom_Grid grid;

    private void Awake()
    {
        grid = new Custom_Grid(width, height, cellsize, gridSpawnPOS.transform.position);
    }
}
