using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _wallBlock;
    
    public bool generate;
    
    [Range(0,20)] public int width = 5;
    [Range(0, 20)] public int height = 5;
    [Range(0, 8)] public int pointsToHit = 5;

    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;

    private Vector2 _blockSize;

    public List<GameObject> _spawnedBlocks = new List<GameObject>();
    public List<Vector2> _usedSpots = new List<Vector2>();

    private GameObject _wallParent;
    private LineRenderer _lineRenderer;

    private bool[][] _grid;
    
    private void OnDrawGizmos()
    {
        topLeft = new Vector2(width * -0.5f, height * 0.5f);
        topRight = new Vector2(width * 0.5f, height * 0.5f);
        bottomLeft = new Vector2(width * -0.5f, height * -0.5f);
        bottomRight = new Vector2(width * 0.5f, height * -0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

    void CleanUp()
    {
        foreach (GameObject go in _spawnedBlocks)
        {
            Destroy(go);
        }
        _spawnedBlocks.Clear();
        _usedSpots.Clear();
    }
    
    void GenerateGrid()
    {
        CleanUp();
        
        // init array
        _grid = new bool[width][];
        for (int x = 0; x < _grid.Length; x++)
        {
            _grid[x] = new bool[height];
            for (int y = 0; y < _grid[x].Length; y++)
            {
                _grid[x][y] = false;
            }
        }
        
        Vector2 offset = transform.position + new Vector3(width / 2f, height / 2f, 0);

        int startPoint = Random.Range(0, height);
        int endPoint = Random.Range(0, height);
        
        _grid[0][startPoint] = true;
        _grid[width-1][endPoint] = true;
        
        // generate random points
        List<int> usedX = new List<int>();
        usedX.Add(0);
        usedX.Add(width - 1);
        
        int lastY = -1;
        for (int i = 0; i < pointsToHit; i++)
        {
            int thisY = Random.Range(0, height);
            while (Mathf.Abs(lastY - thisY) < 2)
            {
                thisY = Random.Range(0, height);
            }

            int thisX = Random.Range(0, width);
            while (usedX.Contains(thisX))
            {
                thisX = Random.Range(0, width);
            }
            
            _grid[thisX][thisY] = true;
            
            usedX.Add(thisX);
            lastY = thisY;
        }
        
        

        // go through and draw grid
        for (int x = 0; x < _grid.Length; x++)
        {
            for (int y = 0; y < _grid[x].Length; y++)
            {
                if (_grid[x][y] == false)
                {
                    //Debug.Log(x + "," + y);
                    PlaceBlock(new Vector2(x, _grid.Length-y) - offset);
                }
            }
        }


        List<Vector3> boundingBoxPts = new List<Vector3>();
        boundingBoxPts.Add(topLeft);
        boundingBoxPts.Add(topRight);
        boundingBoxPts.Add(bottomRight);
        boundingBoxPts.Add(bottomLeft);
        _lineRenderer.positionCount = boundingBoxPts.Count;
        _lineRenderer.widthCurve = AnimationCurve.Constant(0, 1, 0.05f);
        _lineRenderer.SetPositions(boundingBoxPts.ToArray());





        // bounding walls
        // PlaceBlocksBetween(topLeft, topRight);
        // PlaceBlocksBetween(bottomLeft, bottomRight);
        // PlaceBlocksBetween(topLeft, bottomLeft);
        // PlaceBlocksBetween(topRight, bottomRight);




    }

    void PlaceBlocksBetween(Vector2 a, Vector2 b)
    {
        if (a.y == b.y)
        {
            // same y == horizontal
            for (float i = 0; i <= Mathf.Abs(a.x-b.x); i++)
            {
                PlaceBlock(new Vector2(a.x+i, a.y));
            }
        } else if (a.x == b.x)
        {
            // same x == vertical
            for (float i = 0; i <= Mathf.Abs(a.y-b.y); i++)
            {
                PlaceBlock(new Vector2(a.x, -a.y+i));
            }
        }
    }
    
    void PlaceBlock(Vector2 coords)
    {
        if (_usedSpots.Contains(coords))
        {
            return;
        }
        
        Vector2 adjustedCoords = coords + new Vector2(_blockSize.x, -_blockSize.y) * 0.5f;
        GameObject go = Instantiate(_wallBlock, adjustedCoords, Quaternion.identity, _wallParent.transform);
        go.name = go.name + " " + coords;

        _spawnedBlocks.Add(go);
        _usedSpots.Add(coords);
    }

    void Awake()
    {
        _wallParent = new GameObject("Wall Parent");
        _lineRenderer = GetComponent<LineRenderer>();
        _blockSize = new Vector2(_wallBlock.transform.localScale.x, _wallBlock.transform.localScale.y);
    }
    
    void Start()
    {
        GenerateGrid();
    }
    
    void Update()
    {
        if (generate)
        {
            generate = false;
            GenerateGrid();
        }
    }
}
