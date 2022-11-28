using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AStarGrid : MonoBehaviour
{
    [Tooltip("How fast the character will move")]
    [Range(1,50)] public float moveSpeed = 10;

    public Grid grid;
    public Transform goalTransform;
    
    private List<Vector3> pts = new List<Vector3>();
    private List<Vector2Int> directions = new List<Vector2Int>()
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        new Vector2Int(-1, -1), // diagnoals
        new Vector2Int(1,1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1)
    };
    
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
    private Vector2 currentDestination;
    private int destinationIndex;

    private LineRenderer _lineRenderer;

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }
    
    

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click to set new destination
        {
            goalTransform.position = (Vector2)V3toV2Int(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            Debug.Log(goalTransform.position);
            
            destinationIndex = 1;

            pts.Clear();
            _openList.Clear();
            _closedList.Clear();
            
            if (Pathfinding(V3toV2Int(transform.position), V3toV2Int(goalTransform.position)))
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
        _lineRenderer.positionCount = pts.Count - destinationIndex;
        for (int i = 0; i < _lineRenderer.positionCount; i++)
        {
            _lineRenderer.SetPosition(i, pts[i]);
        }
    }
    
    private void FixedUpdate()
    {
        if (isMoving)
        {
            currentDestination = (Vector2)pts[pts.Count - destinationIndex];
            transform.position = new Vector3(currentDestination.x, currentDestination.y, -10);
            if ((Vector2)transform.position == currentDestination)
            {
                UpdateLineRender();
                if (pts.Count - destinationIndex == 0)
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
        
        // create and add start node to open list
        Node startNode = new Node(startPos, 0, Vector2Int.Distance(startPos, goalPos));
        _openList.Add(startNode);
        
        while (_openList.Count > 0)
        {
            int smallestFindex = SmallestF(_openList); // get node with smallest F
            Node smallestFnode = _openList[smallestFindex];
            _openList.RemoveAt(smallestFindex); // move it from open to closed list
            _closedList.Add(smallestFnode);
            
            foreach (Vector2Int dir in directions)
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
                        pts.Add((Vector2)child.position);
                        Node n = child.parent;
                        while (n != startNode) // iterate backwards through the parents
                        {
                            pts.Add((Vector2)n.position);
                            n = n.parent;
                        }
                        pts.Add((Vector2)startNode.position);
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
        bool state = grid.IsBlocked(pos.x, pos.y);
        Debug.Log(pos + " is blocked: " + state);
        return state;
    }

    Vector2Int V3toV2Int(Vector3 v3)
    {
        return new Vector2Int((int)v3.x, (int)v3.y);
    }
        
}
