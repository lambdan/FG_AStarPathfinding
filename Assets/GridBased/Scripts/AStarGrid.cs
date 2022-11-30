using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class AStarGrid : MonoBehaviour
{
    [Tooltip("How fast the character will move")]
    [Range(1,50)] public float moveSpeed = 10;

    [SerializeField] private Toggle _diagonalsAllowed;

    public GridScript grid;
    public Transform goalTransform;
    
    private List<Vector3> _pts = new List<Vector3>();

    private List<Node> _openList = new List<Node>();
    private List<Node> _closedList = new List<Node>();

    public class Node
    {
        public Vector2Int position;
        public Node parent; // node you came from to get to this node
        public float g; // distance travelled from start
        public float h; // distance from goal

        public float f => g + h;

        public Node(Vector2Int position, float g, float h, Node parent = null)
        {
            this.position = position;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }
    }
    
    private bool isMoving;
    private Vector3 currentDestination;
    private int destinationIndex;

    private LineRenderer _lineRenderer;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }
    
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isMoving = !isMoving;
        }
        
        if (Input.GetMouseButtonDown(1)) // right click to set new destination
        {
            goalTransform.position = (Vector2)CommonFunctions.V3toV2Int(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            destinationIndex = 1;

            _pts.Clear();
            _openList.Clear();
            _closedList.Clear();
            
            if (Pathfinding(CommonFunctions.V3toV2Int(transform.position), CommonFunctions.V3toV2Int(goalTransform.position)))
            {
                // found path
                isMoving = true;
                UpdateLineRender();
            }
            else
            {
                isMoving = false;
            }
        }
    }

    void UpdateLineRender()
    {
        _lineRenderer.positionCount = _pts.Count - destinationIndex;
        for (int i = 0; i < _lineRenderer.positionCount; i++)
        {
            _lineRenderer.SetPosition(i, _pts[i]);
        }
    }
    
    private void FixedUpdate()
    {
        if (isMoving)
        {
            currentDestination = _pts[_pts.Count - destinationIndex];
            currentDestination.z = -10; // z = -10 to get "on top" of the squares
            transform.position = Vector3.MoveTowards(transform.position, currentDestination, moveSpeed * Time.fixedDeltaTime);
            if (transform.position == currentDestination)
            {
                UpdateLineRender();
                if (_pts.Count - destinationIndex == 0)
                {
                    Debug.Log("Hit goal!");
                    isMoving = false; // hit goal
                }
                else
                {
                    destinationIndex += 1;
                }
            }
        }
    }

    bool Pathfinding(Vector2Int startPos, Vector2Int goalPos)
    {
        float timeStarted = Time.realtimeSinceStartup;
        List<Vector2Int> allowedDirections = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        if (_diagonalsAllowed.isOn)
        {
            allowedDirections.Add(new Vector2Int(-1, -1));
            allowedDirections.Add(new Vector2Int(1, 1));
            allowedDirections.Add(new Vector2Int(1, -1));
            allowedDirections.Add(new Vector2Int(-1, 1));
        }
        
        // create and add start node to open list
        Node startNode = new Node(startPos, 0, Vector2Int.Distance(startPos, goalPos));
        _openList.Add(startNode);
        
        while (_openList.Count > 0)
        {
            int smallestFindex = SmallestF(_openList); // get node with smallest F
            Node smallestFnode = _openList[smallestFindex];
            _openList.RemoveAt(smallestFindex); // move it from open to closed list
            _closedList.Add(smallestFnode);
            
            foreach (Vector2Int dir in allowedDirections)
            {
                Vector2Int thisPos = smallestFnode.position + dir;

                if (IsBlocked(thisPos) || NodeAtThisPosition(_closedList, thisPos) >= 0) 
                {
                    // blocked or on closed list = ignore it
                    continue;
                }

                Node child = new Node(thisPos, smallestFnode.g + Vector2.Distance(thisPos, smallestFnode.position), Vector2.Distance(thisPos, goalPos), smallestFnode);

                int openListIndex = NodeAtThisPosition(_openList, thisPos);

                if (openListIndex < 0) // not in open list
                {
                    _openList.Add(child);
                    if (child.position == goalPos) // found it!
                    {
                        // draw points
                        _pts.Add((Vector2)child.position);
                        Node n = child.parent;
                        while (n != startNode) // iterate backwards through the parents
                        {
                            _pts.Add((Vector2)n.position);
                            n = n.parent;
                        }
                        _pts.Add((Vector2)startNode.position);
                        float timeTook = Time.realtimeSinceStartup - timeStarted;
                        Debug.Log("Path found! Took " + timeTook + " secs");
                        return true;
                    }
                }
                else
                {
                    // already in open list, update it if new path is shorter
                    Node n = _openList[openListIndex];
                    if (child.g < n.g)
                    {
                        n.parent = smallestFnode;
                        n.g = smallestFnode.g + Vector2.Distance(n.position, smallestFnode.position);
                        _openList[openListIndex] = n;
                    }
                }
                
            }
        }

        Debug.LogError("Couldnt find path?");
        return false;
    }

    
    int NodeAtThisPosition(List<Node> nodeList, Vector2Int pos)
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].position == pos)
            {
                return i;
            }
        }
        return -1;
    }
    
    int SmallestF(List<Node> nodes)
    {
        Node smallest = null;
        int smallestIndex = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i == 0 || nodes[i].f < smallest.f)
            {
                smallest = nodes[i];
                smallestIndex = i;
            }
        }
        return smallestIndex;
    }
    
    bool IsBlocked(Vector2Int pos)
    {
        return grid.IsBlocked(pos.x, pos.y);
    }

    public bool IsMoving()
    {
        return isMoving;
    }

}
