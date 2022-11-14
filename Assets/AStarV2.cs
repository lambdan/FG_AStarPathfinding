using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AStarV2 : MonoBehaviour
{
    public bool debugDraws;
    public Transform goalTransform;
    public GameObject obstacles;
    private List<Vector3> pts = new List<Vector3>();
    private List<Vector2Int> directions = new List<Vector2Int>()
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        new Vector2Int(-1, -1), // diagnoals
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1)
    };

    public class Node
    {
        public Vector2Int position;
        public Node parent; // node you came from to get to this node
        public float g; // distance travelled from start
        public float h; // distance from goal
        // public float f;

        public float f => g + h;

        public Node(Vector2Int position, float g, float h, Node parent = null)
        {
            this.position = position;
            this.parent = parent;
            this.g = g;
            this.h = h;
            // this.f = g + h;
        }
    }

    public List<Node> openList = new List<Node>();
    public List<Node> closedList = new List<Node>();
    public List<Vector2Int> blockedPos = new List<Vector2Int>();

    private void OnDrawGizmos()
    {
        pts.Clear();
        blockedPos.Clear();
        openList.Clear();
        closedList.Clear();

        Obstacles();

        Pathfinding();


        if (debugDraws)
        {
            Gizmos.color = Color.red;
            foreach (Node n in closedList)
            {
                Vector2 v = n.position;
                Gizmos.DrawSphere(v, 0.1f);
            }

            Gizmos.color = Color.green;
            foreach (Node n in openList)
            {
                Vector2 v = n.position;
                Gizmos.DrawSphere(v, 0.1f);
            }

            foreach (Node n in openList)
            {
                Handles.color = Color.green;
                Handles.Label((Vector2)n.position, n.f.ToString("f=00"));
            }

            foreach (Node n in closedList)
            {
                Handles.color = Color.red;
                Handles.Label((Vector2)n.position, n.f.ToString("00"));
            }
        }


        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere((Vector2)Vector3ToVector2Int(transform.position), 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere((Vector2)Vector3ToVector2Int(goalTransform.position), 0.2f);
        Handles.color = Color.magenta;
        Handles.DrawAAPolyLine(pts.ToArray());
    }

    void Pathfinding()
    {
        // hehgeheheh
        Vector2Int startPos = Vector3ToVector2Int(transform.position);
        Vector2Int goalPos = Vector3ToVector2Int(goalTransform.position);

        // create start node
        Node startNode = new Node(startPos, 0, Vector2.Distance(startPos, goalPos));

        // create end node
        // Node endNode = new Node(goalPos, 0, 0);

        // add start node
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            
            int oi = SmallestF(openList);
            Node q = openList[oi];
            openList.RemoveAt(oi);
            closedList.Add(q);


            foreach (Vector2Int dir in directions)
            {
                Vector2Int thisPos = q.position + dir;

                // if (Vector2Int.Distance(thisPos, goalPos) > 50)
                // {
                //     continue;
                // }
                
                if (blockedPos.Contains(thisPos) || NodeAtThisPosition(closedList, thisPos) >= 0) // blocked or on closed list
                {
                    continue;
                }
                
                Node child = new Node(thisPos, q.g + Vector2Int.Distance(thisPos, q.position), Vector2Int.Distance(thisPos, goalPos), q);
                // child.f = child.g + child.h;
                child.parent = q;

                int openListIndex = NodeAtThisPosition(openList, thisPos);

                if (openListIndex < 0) // not in open list
                {
                    openList.Add(child);

                    if (child.position == goalPos) // found it
                    {
                        pts.Clear(); // generate points for AADrawLine
                        pts.Add((Vector2)child.position);
                        Node n = child.parent;
                        while (n != startNode)
                        {
                            pts.Add((Vector2)n.position);
                            n = n.parent;
                        }
                        pts.Add((Vector2)startNode.position);
                        break;
                    }
                }
                else
                {
                    // already in open list, update it if new path is shorter
                    Node n = openList[openListIndex];
                    if (child.g < n.g)
                    {
                        n.parent = q;
                        n.g = q.g + 1;
                        // n.f = n.g + n.h;
                        openList[openListIndex] = n;
                    }
                }
            }
        }
    }

    void Obstacles()
    {
        // find where obstacles are (our path cannot cross that area)
        for (int i = 0; i < obstacles.transform.childCount; i++)
        {
            GameObject o = obstacles.transform.GetChild(i).gameObject;
            if (!o.activeSelf)
            {
                continue;
            }

            SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
            Vector3 topLeft = sr.bounds.center - new Vector3(sr.transform.localScale.x * sr.size.x / 2,
                -sr.transform.localScale.y * sr.size.y / 2, 0);
            Vector3 topRight = sr.bounds.center - new Vector3(sr.transform.localScale.x * -sr.size.x / 2,
                -sr.transform.localScale.y * sr.size.y / 2, 0);
            Vector3 bottomLeft = sr.bounds.center - new Vector3(sr.transform.localScale.x * sr.size.x / 2,
                sr.transform.localScale.y * sr.size.y / 2, 0);
            Vector3 bottomRight = sr.bounds.center - new Vector3(sr.transform.localScale.x * -sr.size.x / 2,
                sr.transform.localScale.y * sr.size.y / 2, 0);

            Vector2Int TL = Vector3ToVector2Int(topLeft);
            Vector2Int TR = Vector3ToVector2Int(topRight);
            Vector2Int BL = Vector3ToVector2Int(bottomLeft);
            Vector2Int BR = Vector3ToVector2Int(bottomRight);


            if (debugDraws)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine((Vector2)TL, topLeft);
                Gizmos.DrawLine((Vector2)TR, topRight);
                Gizmos.DrawLine((Vector2)BL, bottomLeft);
                Gizmos.DrawLine((Vector2)BR, bottomRight);
            }



            // add these positions to blocked positions
            for (int j = TL.x; j <= TR.x; j++)
            {
                for (int k = TL.y; k >= BL.y; k--)
                {
                    // Debug.Log(j + "," + k);
                    Vector2Int closed = new Vector2Int(j, k);
                    blockedPos.Add(closed);
                }
            }
        }
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

        // Debug.Log("smallestF returning " + smallestIndex);
        return smallestIndex;
    }
    
    private Vector2Int Vector3ToVector2Int(Vector3 position) =>
        new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
}
