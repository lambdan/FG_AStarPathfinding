using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    [SerializeField] private GameObject _squarePrefab;
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private Transform _player;
    [SerializeField] [Range(5, 30)] private int _width = 20;
    [SerializeField] [Range(5, 30)] private int _height = 20;
    [SerializeField] [Range(0, 1)] private float _closedChance = 0.2f;

    private Dictionary<int, Dictionary<int, SquareScript>> _gridDict;

    void Start()
    {
        InitializeGrid();
        DrawGridLines();

        // focus camera on center of grid
        Camera.main.transform.position = _gridDict[_width / 2][_height / 2].transform.position;
        Camera.main.orthographicSize = (Mathf.Max(_width, _height) / 2) + 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomizeGrid();
        }
    }

    void DrawGridLines()
    {
        GameObject lineParent = new GameObject("Grid Lines");
        lineParent.transform.parent = transform;
        for (int i = 0; i <= _width; i++)
        {
            LineRenderer a = new GameObject("Vertical Line " + i).AddComponent<LineRenderer>();
            a.gameObject.transform.parent = lineParent.transform;
            a.positionCount = 2;
            a.startWidth = a.endWidth = 0.08f;
            a.material = _lineMaterial;
            a.SetPosition(0, new Vector3(-0.5f + i * 1f, -0.5f, -10));
            a.SetPosition(1, new Vector3(-0.5f + i * 1f, _height - 0.5f, -10));
        }

        for (int i = 0; i <= _height; i++)
        {
            LineRenderer a = new GameObject("Horizontal Line " + i).AddComponent<LineRenderer>();
            a.gameObject.transform.parent = lineParent.transform;
            a.positionCount = 2;
            a.startWidth = a.endWidth = 0.08f;
            a.material = _lineMaterial;
            a.SetPosition(0, new Vector3(-0.5f, -0.5f + i * 1f, -10));
            a.SetPosition(1, new Vector3(_width - 0.5f, -0.5f + i * 1f, -10));
        }
    }

    void InitializeGrid()
    {
        GameObject squareParent = new GameObject("Squares");
        squareParent.transform.parent = transform;

        _gridDict = new Dictionary<int, Dictionary<int, SquareScript>>();
        for (int x = 0; x < _width; x++)
        {
            _gridDict[x] = new Dictionary<int, SquareScript>();
            for (int y = 0; y < _height; y++)
            {
                GameObject block = SpawnBlock(x, y);
                block.transform.parent = squareParent.transform;
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
                if (x == CommonFunctions.V3toV2Int(_player.position).x && y == CommonFunctions.V3toV2Int(_player.position).y)
                {
                    _gridDict[x][y].OpenSquare(); // leave square player is on open
                    
                }
                else if (Random.Range(0, 1f) <= _closedChance)
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
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            // report squares outside the grid as blocked
            return true;
        }

        return _gridDict[x][y].IsBlocked();
    }
}