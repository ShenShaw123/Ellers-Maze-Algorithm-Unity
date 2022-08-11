using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using static GridBuildingSystem3D;

public class EllersMaze_Waves : MonoBehaviour
{

    [Header("Maze Options")]
    [SerializeField] int mazeWidth;
    [SerializeField] int mazeHeight;
    [SerializeField] int yvalue;
    [SerializeField] GameObject spawnPos;

    [Header("References")]
    [SerializeField] GridBuildingSystem3D gridBuildingSys;
    [SerializeField] GameObject DEBUG;
    [SerializeField] GameObject Wall;

    [Range(0, 100)]
    [SerializeField] int R_wallSpawnPercen = 0;
    [Range(0, 100)]
    [SerializeField] int B_wallSpawnPercen = 0;

    private GridXZ<GridObject> gridXZ;
    int max_set_val = 1;

    //Waves:
    public List<List<GameObject>> _RowWalls = new List<List<GameObject>>();

    void Awake()
    {
    }

    private void Start()
    {
        gridXZ = gridBuildingSys.grid;

        gridXZ.GetXZ(spawnPos.transform.position, out int x, out int z);
        StartCoroutine(Generate_Ellers_Maze(new int[] { x, z }));
    }

    IEnumerator Generate_Ellers_Maze(int[] mazeSpawnPos)
    {
        var first_row = new List<(int[], int)>();   //List of Tuples (int[], int): coords, set#.

        //Setting first row: (1, 2, 3, ...)
        for (int x = mazeSpawnPos[0]; x < mazeSpawnPos[0] + mazeWidth; x++)
        {
            int[] _coords = { x, mazeSpawnPos[1] };

            var newTup = (_coords, max_set_val);
            first_row.Add(newTup);

            max_set_val += 1;
        }

        //WorldText_Row(first_row, Color.white, 2);

        //Top Walls:
        PlaceWalls_SameDir(first_row, Vector3.right);

        //Maze Generation Loop:
        var row = first_row;
        bool createdFinalRow = false;
        for (int z = 0; z < mazeHeight; z++)
        {
            yield return new WaitForSeconds(.1f);

            //Delete Row:
            if (_RowWalls.ElementAtOrDefault(z) != null)
            {
                foreach (var wall in _RowWalls[z])
                {
                    Destroy(wall);
                }
                _RowWalls.Remove(_RowWalls[z]);
            }

            List<GameObject> allWallsRow = new List<GameObject>();

            //Final Row:
            if (z == mazeHeight - 1)
            {
                Debug.Log("Final Row");
                JoinRow_and_HoriztonalWalls(row, z, allWallsRow);

                //R_wallSpawnPercen = UnityEngine.Random.Range(30, 70);
                //B_wallSpawnPercen = UnityEngine.Random.Range(40, 101);

                if (createdFinalRow == false)
                {
                    PlaceWalls_SameDir(IncreaseAxis_Row(false, 1, row), Vector3.right);
                    createdFinalRow = true;
                }

                _RowWalls.Insert(z, allWallsRow);

                row = IncreaseAxis_Row(false, (mazeHeight * -1) + 1, row);

                z = -1;
            }
            else
            {
                //Maze Loop:
                var joined_row = JoinRow_and_HoriztonalWalls(row, z, allWallsRow);
                BottomWalls(joined_row, out List<int[]> bWallCells, allWallsRow);
                var new_row = NewRow(joined_row, bWallCells);
                row = new_row;

                _RowWalls.Insert(z, allWallsRow);
            }
        }


    }

    private List<(int[], int)> JoinRow_and_HoriztonalWalls(List<(int[], int)> row, int index, List<GameObject> allWallsRow)
    {

        //Far Left Edge Wall
        PlaceWall_XZ(Vector3.forward, row[0].Item1[0], row[0].Item1[1]);

        var row_copy = new List<(int[], int)>(row);
        for (int i = 0; i < row_copy.Count; i++)
        {
            if (i + 1 < row_copy.Count)
            {
                var cur_cell = row_copy[i];
                var next_cell = row_copy[i + 1];
                bool join = false;

                //Deciding Wall or Join:
                if (cur_cell.Item2 == next_cell.Item2)  //if in same set, wall.
                {
                    //Debug.Log("Same Set, Wall");
                    join = false;
                }
                else
                {
                    //Set Wall & Join Percentage: Make sure it adds to 100.
                    int rand_num = UnityEngine.Random.Range(0, 101);

                    if (inRange_Inclusive(rand_num, 0, R_wallSpawnPercen)) join = false; //wall % (greater %, more narrow halls)
                    if (inRange_Inclusive(rand_num, R_wallSpawnPercen + 1, 100)) join = true;    //join cell % (greater % , weird affect)
                }

                //Wall
                if (join == false)
                {
                    var coord = next_cell.Item1;

                    var _wall = PlaceWall_XZ(Vector3.forward, coord[0], coord[1]);
                    allWallsRow.Add(_wall);
                }
                //Join
                else if (join == true)
                {
                    var newCell = (next_cell.Item1, cur_cell.Item2);
                    row_copy[i + 1] = newCell;
                }
            }
        }

        //Far Right Edge Wall
        var wall = PlaceWall_XZ(Vector3.forward, row[row.Count - 1].Item1[0] + 1, row[row.Count - 1].Item1[1]);
        allWallsRow.Add(wall);

        return row_copy;
    }

    private void BottomWalls(List<(int[], int)> row, out List<int[]> bWallLoc, List<GameObject> allWallsRow)
    {

        var row_copy = new List<(int[], int)>(row);
        var no_bwall_cells = new List<(int[], int)>();
        var _bWallCells = new List<(int[], int)>();

        foreach (var set_list in Row_SortdBySet(row_copy))
        {
            //foreach list (which is grouped by set) get a list of locations in new row where it will not have a bottom wall (at least 1 always).

            var shuffledSets = set_list.OrderBy(a => rng.Next()).ToList();

            foreach (var (i, cell) in shuffledSets.Enumerate())
            {
                if (i == 0)
                {
                    no_bwall_cells.Add(cell);
                    continue;
                }

                bool bWall = false;
                int rand_num = UnityEngine.Random.Range(0, 101);
                if (inRange_Inclusive(rand_num, 0, B_wallSpawnPercen)) bWall = true; //wall % (greater %, more narrow halls)
                if (inRange_Inclusive(rand_num, B_wallSpawnPercen + 1, 100)) bWall = false;    //join cell % (greater % , weird affect)

                if (bWall)
                {
                    _bWallCells.Add(cell);
                }
                else
                {
                    no_bwall_cells.Add(cell);
                }
            }
        }

        //getting cells w/ bottom wall
        _bWallCells = row_copy.Except(no_bwall_cells).ToList();

        //getting bottom wall locations + out assingment
        var bWallLocs = new List<int[]>();
        foreach (var tuple in _bWallCells)
        {
            bWallLocs.Add(tuple.Item1);
        }
        bWallLoc = bWallLocs;

        //Place Walls - Only don't at start

        foreach (var tup in IncreaseAxis_Row(false, 1, _bWallCells))
        {
            var coord = tup.Item1;

            var wall = PlaceWall_XZ(Vector3.right, coord[0], coord[1]);
            allWallsRow.Add(wall);
        }
    }

    private List<(int[], int)> NewRow(List<(int[], int)> row, List<int[]> bottomwall_loc)
    {
        //NewRow: Remove cells with a bottom-wall from their set, give new set.
        var new_row = new List<(int[], int)>(Coord_SortedList(row));
        foreach (var (i, cell) in row.Enumerate())
        {
            foreach (var tup in bottomwall_loc)
            {
                if (cell.Item1[0] == tup[0] && cell.Item1[1] == tup[1])  //if same coords (if x=x & z=z), give new set value.
                {
                    var new_cell = (cell.Item1, max_set_val);
                    new_row[i] = new_cell;
                    max_set_val += 1;
                }
            }
        }

        return IncreaseAxis_Row(false, 1, new_row);
    }

    //Shuffle Lists:
    private static System.Random rng = new System.Random();

    private List<(int[], T)> Coord_SortedList<T>(List<(int[], T)> row)
    {
        var coord_SortedList = row.OrderBy(tup => tup.Item1[0]).ToList();
        return coord_SortedList;
    }

    private bool inRange_Inclusive(int num, int minRange, int maxRange)
    {
        if (minRange <= num && num <= maxRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private GameObject PlaceWall_XZ(Vector3 dir, int x, int z)
    {
        Vector3 position = gridXZ.GetWorldPosition(x, z);
        position.y = yvalue;
        var go = GameObject.Instantiate(Wall, position, Quaternion.LookRotation(dir));

        return go;
    }
    private void PlaceWalls_SameDir<T>(List<(int[], T)> wall_locations, Vector3 dir)
    {
        //var uniqueLocations = wall_locations.Except(dup_list).ToList(); //perhaps error here?

        foreach (var tup in wall_locations)
        {
            var coords = tup.Item1;

            PlaceWall_XZ(dir, coords[0], coords[1]);
        }
    }

    private List<(int[], int)> IncreaseAxis_Row(bool xAxis, int addAmount, List<(int[], int)> row)
    {
        var new_row = new List<(int[], int)>();
        foreach (var tup in row)
        {
            var coords = tup.Item1;
            var set = tup.Item2;

            int x;
            int z;

            if (xAxis)
            {
                x = coords[0] + addAmount;
                z = coords[1];
            }
            else
            {
                x = coords[0];
                z = coords[1] + addAmount;
            }

            int[] new_coords = { x, z };

            var new_cell = (new_coords, set);

            new_row.Add(new_cell);
        }

        return new_row;
    }

    private List<List<(int[], int)>> Row_SortdBySet(List<(int[], int)> source)
    {
        return source
            .Select((x) => new { Value = x })
            .GroupBy(x => x.Value.Item2)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }

    private void WorldText_Row(List<(int[], int)> row, Color textColor, int yVal)
    {
        GameObject rowObj = new GameObject();
        foreach (var tup in row)
        {
            var coords = tup.Item1;
            var _set = tup.Item2;

            //place world text
            var text = new GameObject();
            var mesh = text.AddComponent<TextMesh>();
            mesh.text = _set.ToString();
            mesh.color = textColor;

            Vector3 gridPos = gridXZ.GetWorldPosition(coords[0], coords[1]);
            gridPos = new Vector3(gridPos.x + gridXZ.GetCellSize() / 2, yVal, gridPos.z + gridXZ.GetCellSize() / 2);
            text.transform.position = gridPos;
            text.transform.parent = rowObj.transform;
        }
    }
}
