using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AStarV2 : MonoBehaviour
{
    [Tooltip("Lower value means more precise (takes longer time to calculate)")]
    [Range(0.2f,1.0f)] public float precision = 0.5f;
    
    [Tooltip("How fast the character will move")]
    [Range(1,50)] public float moveSpeed = 10;
    
    public Transform goalTransform;
    public GameObject obstacles;
    private List<Vector3> pts = new List<Vector3>();
    private List<Vector2> directions = new List<Vector2>()
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
        new Vector2(-1,-1), // diagnoals
        new Vector2(-1, 1),
        new Vector2(1, 1),
        new Vector2(1, -1)
    };

    private HashSet<Vector2> _alreadyCheckedObstaclesOpen = new HashSet<Vector2>();
    private HashSet<Vector2> _alreadyCheckedObstaclesClosed = new HashSet<Vector2>();
    private List<Node> _openList = new List<Node>();
    private List<Node> _closedList = new List<Node>();

    public class Node
    {
        public Vector2 position;
        public Node parent; // node you came from to get to this node
        public float g; // distance travelled from start
        public float h; // distance from goal

        public float f => g + h;

        public Node(Vector2 position, float g, float h, Node parent = null)
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
        if (Input.GetKeyDown(KeyCode.Space)) // space to pause
        {
            isMoving = !isMoving;
        }

        if (Input.GetMouseButtonDown(0)) // left click to set new destination
        {
            goalTransform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            destinationIndex = 1;

            pts.Clear();
            _openList.Clear();
            _closedList.Clear();
            
            if (Pathfinding(transform.position, goalTransform.position))
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
            currentDestination = pts[pts.Count - destinationIndex];
            transform.position = Vector2.MoveTowards(transform.position, currentDestination, moveSpeed * Time.fixedDeltaTime);
            if (Vector2.Distance(transform.position, currentDestination) <= precision)
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

    bool Pathfinding(Vector2 startPos, Vector2 goalPos)
    {
        float timeStarted = Time.realtimeSinceStartup;
        
        // round vector components to accuracy (otherwise you may click on a spot that is never accessible)
        startPos = new Vector2(transform.position.x - (transform.position.x % precision), transform.position.y - (transform.position.y % precision));
        goalPos = new Vector2(goalPos.x - (goalPos.x % precision), goalPos.y - (goalPos.y % precision));

        // create and add start node to open list
        Node startNode = new Node(startPos, 0, Vector2.Distance(startPos, goalPos));
        _openList.Add(startNode);
        
        while (_openList.Count > 0)
        {
            int smallestFindex = SmallestF(_openList); // get node with smallest F
            Node smallestFnode = _openList[smallestFindex];
            _openList.RemoveAt(smallestFindex); // move it from open to closed list
            _closedList.Add(smallestFnode);
            
            foreach (Vector2 dir in directions)
            {
                Vector2 thisPos = smallestFnode.position + (dir*precision);

                if (RectContains(thisPos) || NodeAtThisPosition(_closedList, thisPos) >= 0) 
                {
                    // blocked or on closed list = ignore it
                    continue;
                }

                Node child = new Node(thisPos, smallestFnode.g + Vector2.Distance(thisPos, smallestFnode.position), Vector2.Distance(thisPos, goalPos), smallestFnode);

                int openListIndex = NodeAtThisPosition(_openList, thisPos);

                if (openListIndex < 0) // not in open list
                {
                    _openList.Add(child);
                    if(Vector2.Distance(child.position, goalPos) <= precision)
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

    
    int NodeAtThisPosition(List<Node> nodeList, Vector2 pos)
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
    
    bool RectContains(Vector2 pos)
    {
        // check if we already checked this position
        if (_alreadyCheckedObstaclesClosed.Contains(pos))
        {
            return true;
        }
        else if (_alreadyCheckedObstaclesOpen.Contains(pos))
        {
            return false;
        }
        
        for (int i = 0; i < obstacles.transform.childCount; i++)
        {
            GameObject o = obstacles.transform.GetChild(i).gameObject;
            if (!o.activeSelf)
            {
                continue;
            }

            SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
            if (sr.bounds.Contains(pos))
            {
                _alreadyCheckedObstaclesClosed.Add(pos);
                return true;
            }
            _alreadyCheckedObstaclesOpen.Add(pos);
        }
        return false;
    }
        
}
