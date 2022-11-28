using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private GameObject _squarePrefab;
    [SerializeField][Range(3,30)] private int _width = 6;
    [SerializeField][Range(3,30)] private int _height = 6;
    [SerializeField] [Range(0, 1)] private float _closedChance = 0.2f;

    private Dictionary<int, Dictionary<int, SquareScript>> _gridDict;
    

    void Start()
    {
        InitializeGrid();
        
        // focus camera on center of grid
        Camera.main.transform.position = _gridDict[_width / 2][_height / 2].transform.position;
        Camera.main.orthographicSize = (Mathf.Max(_width,_height) / 2) + 1;
    }

    void InitializeGrid()
    {
        _gridDict = new Dictionary<int, Dictionary<int, SquareScript>>();
        for (int x = 0; x < _width; x++)
        {
            _gridDict[x] = new Dictionary<int, SquareScript>();
            for (int y = 0; y < _height; y++)
            {
                GameObject block = SpawnBlock(x, y);
                _gridDict[x][y] = block.GetComponent<SquareScript>();
            }
        }
        RandomizeGrid();
    }

    [ContextMenu("Randomize Grid")]
    public void RandomizeGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (Random.Range(0, 1f) <= _closedChance)
                {
                    _gridDict[x][y].CloseSquare();
                }
                else
                {
                    _gridDict[x][y].OpenSquare();
                }
            }
        }
    }
    
    GameObject SpawnBlock(int x, int y)
    {
        GameObject square = Instantiate(_squarePrefab, new Vector2(x, y), Quaternion.identity, transform);
        square.name = x + "," + y;
        return square;
    }

    public Dictionary<int, Dictionary<int, SquareScript>> GetGrid()
    {
        return _gridDict;
    }

    public bool IsBlocked(int x, int y)
    {
        // report squares outside the grid as blocked
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return true; 
        }
        
        return _gridDict[x][y].IsBlocked();
    }
}
