using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class AStarScript : MonoBehaviour
{
    [SerializeField] private bool debug_obstacles;
    [SerializeField] private bool debug_node_positions;
    [SerializeField] private bool debug_node_directions;
    public bool pathFound;
    
    public HashSet<Vector2Int> closedPositions = new HashSet<Vector2Int>();
    
    public Transform startTransform;
    public Transform goalTransform;
    public float maxHCostScaler = 3f;
    public GameObject obstacles;

    public List<Node> nodes = new List<Node>();

    private Vector2Int[] directions = new Vector2Int[4];
    
    public class Node
    {
        public Vector2Int position;
        public Node parent; // node you came from to get to this node
        public float g; // distance travelled from start
        
        public float h; // distance from goal
        public float f => g + h; // cost...?

        public Node(Vector2Int position, float g, float h, Node parent = null)
        {
            this.position = position;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }

        public void SetParentAndG(Node newParent, float newG)
        {
            parent = newParent;
            g = newG;
        }
    }

    private void OnDrawGizmos()
    {
        directions = new[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        
        startTransform = transform;

        if (debug_obstacles)
        {
            foreach (Vector2 v in closedPositions)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere((Vector2)v, 0.5f);
            }
        }

        List<Vector2Int> path = GetPath(new Vector2Int((int)startTransform.position.x, (int)startTransform.position.y), new Vector2Int((int)goalTransform.position.x, (int)goalTransform.position.y));

        // convert V2Int to V3 for DrawAAPolyLine
        List<Vector3> drawPts = new List<Vector3>();
        foreach (Vector2Int p in path)
        {
            drawPts.Add(new Vector3(p.x, p.y, 0));
        }
        Handles.DrawAAPolyLine(drawPts.ToArray());
    }
    
    private Vector2Int Vector3ToVector2Int(Vector3 position) => new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));

    private List<Vector2Int> GetPath(Vector2Int start, Vector2Int goal)
    {
        pathFound = false;
        bool pathfindingActive = false;
        closedPositions.Clear();
        List<Vector2Int> pts = new List<Vector2Int>(); // will store coords for AAPolyLine
        
        nodes.Clear();

        pts.Add(start);
        Node startNode = new Node(start, 0, Vector2Int.Distance(start, goal));
        nodes.Add(startNode);
        
        // do pathfinding here
        
        // find where obstacles are (our path cannot cross that area)
        for (int i = 0; i < obstacles.transform.childCount; i++)
        {
            GameObject o = obstacles.transform.GetChild(i).gameObject;
            if (!o.activeSelf)
            {
                continue;
            }
            SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
            Vector3 topLeft = sr.bounds.center - new Vector3(sr.transform.localScale.x * sr.size.x / 2, -sr.transform.localScale.y * sr.size.y / 2, 0);
            Vector3 topRight = sr.bounds.center - new Vector3(sr.transform.localScale.x * -sr.size.x / 2, -sr.transform.localScale.y * sr.size.y / 2, 0);
            Vector3 bottomLeft = sr.bounds.center - new Vector3(sr.transform.localScale.x * sr.size.x / 2, sr.transform.localScale.y * sr.size.y / 2, 0);
            Vector3 bottomRight = sr.bounds.center - new Vector3(sr.transform.localScale.x * -sr.size.x / 2, sr.transform.localScale.y * sr.size.y / 2, 0);

            Vector2Int TL = Vector3ToVector2Int(topLeft);
            Vector2Int TR = Vector3ToVector2Int(topRight);
            Vector2Int BL = Vector3ToVector2Int(bottomLeft);
            Vector2Int BR = Vector3ToVector2Int(bottomRight);

            if (debug_obstacles)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine((Vector2)TL, topLeft);
                Gizmos.DrawLine((Vector2)TR, topRight);
                Gizmos.DrawLine((Vector2)BL, bottomLeft);
                Gizmos.DrawLine((Vector2)BR, bottomRight);
            }


            // add these positions to closedPositions
            for (int j = TL.x; j <= TR.x; j++)
            {
                for (int k = TL.y; k >= BL.y; k--)
                {
                    // Debug.Log(j + "," + k);
                    Vector2Int closed = new Vector2Int(j, k);
                    closedPositions.Add(closed);
                }
            }
            
        }
        
        pathfindingActive = true;
        int its = 0; // to prevent infinite loop
        while (pathfindingActive)
        {
            Vector2Int currPos = nodes.Last().position;
            if (nodes.Last().h > BestNode(nodes).h)
            {
                currPos = BestNode(nodes).position;
            }

            for (int d = 0; d < directions.Length; d++)
            {
                Vector2Int thisPos = currPos + directions[d];
                if (closedPositions.Contains(thisPos) || NodeExists(thisPos))
                {
                    continue;
                }
                
                // create node for this position
                Node thisNode = new Node(thisPos, nodes.Last().g + 1,
                    Vector2Int.Distance(thisPos, goal), nodes.Last());
                nodes.Add(thisNode);

                if (debug_node_directions)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine((Vector2)currPos, (Vector2)thisPos);
                }
            }

            its++; // fail safe lol
            if (its > 1000)
            {
                pathfindingActive = false;
            }
        }

        if (debug_node_positions)
        {
            foreach (Node n in nodes)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere((Vector2)n.position, 0.2f);
            }
        }

        if (BestNode(nodes).position == goal)
        {
            pathFound = true;
        }
        
        // end of pathfinding
        
        pts.Add(goal);

        return pts;
    }

    bool NodeExists(Vector2Int pos)
    {
        foreach (Node n in nodes)
        {
            if (pos == n.position)
            {
                return true;
            }
        }
        return false;
    }

    Node BestNode(List<Node> nodeList)
    {
        Node nearest = null;
        foreach (Node n in nodeList)
        {
            if (nearest == null || n.h < nearest.h)
            {
                nearest = n;
            }
        }

        return nearest;
    }

}
