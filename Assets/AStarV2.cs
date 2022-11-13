using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AStarV2 : MonoBehaviour
{
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

        public float f => g + h;

        public Node(Vector2Int position, float g, float h, Node parent = null)
        {
            this.position = position;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }
    }

    public List<Node> openList = new List<Node>();
    public List<Node> closedList = new List<Node>();
    public List<Vector2Int> blockedPos = new List<Vector2Int>();

    private void OnDrawGizmos()
    {
        pts.Clear();
        
        openList.Clear();
        closedList.Clear();

        if (blockedPos.Count == 0)
        {
            Obstacles();
        }

        foreach (Vector2Int v in blockedPos)
        {
            Node c = new Node(v, 0, 0);
            closedList.Add(c);
        }
        
        Pathfinding();

        Handles.color = Color.magenta;
        Handles.DrawAAPolyLine(pts.ToArray());
    }

    void Pathfinding()
    {
        Vector2Int startPos = Vector3ToVector2Int(transform.position);
        Vector2Int goalPos = Vector3ToVector2Int(goalTransform.position);

        // create start node
        Node startNode = new Node(startPos, 0, 0);

        // create end node
        Node endNode = new Node(goalPos, 0, 0);

        // add start node
        openList.Add(startNode);

        int its = 0;
        while (openList.Count > 0)
        {
            Node q = openList[smallestF(openList)];
            openList.RemoveAt(smallestF(openList));

            List<Node> children = new List<Node>();
            foreach (Vector2Int dir in directions)
            {
                Vector2Int thisPos = q.position + dir;
                Node child = new Node(thisPos, 0, 0, q);
                children.Add(child);
            }

            foreach (Node child in children)
            {
                if (child.position == endNode.position)
                {
                    Debug.Log("found it!!!");
                    break;
                }

                child.g = q.g + Vector2Int.Distance(child.position, q.position);
                child.h = Vector2Int.Distance(child.position, goalPos);
                // f is calculated in node class

                int samePosOpen = NodeAtThisPosition(openList, child.position);
                int samePosClosed = NodeAtThisPosition(closedList, child.position);
                
                if (samePosOpen >= 0 && openList[samePosOpen].f < child.f)
                {
                    continue;
                }
                if (samePosClosed >= 0 && openList[samePosClosed].f < child.f)
                {
                    continue;
                }

                openList.Add(child);
            }

            closedList.Add(q);
            pts.Add((Vector2)q.position);


            its++;
            if (its > 2000)
            {
                break;
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


            Gizmos.color = Color.red;
            Gizmos.DrawLine((Vector2)TL, topLeft);
            Gizmos.DrawLine((Vector2)TR, topRight);
            Gizmos.DrawLine((Vector2)BL, bottomLeft);
            Gizmos.DrawLine((Vector2)BR, bottomRight);


            // add these positions to closedPositions
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
    
    int NodeAtThisPosition(List<Node> nodeList, Vector2Int position)
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].position == position)
            {
                return i;
            }
        }
        
        return -9999;
    }
    
    int smallestF(List<Node> nodes)
    {
        Node smallest = null;
        int smallestIndex = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (smallest == null || nodes[i].f < smallest.f)
            {
                smallest = nodes[i];
                smallestIndex = i;
            }
        }
        return smallestIndex;
    }
    
    private Vector2Int Vector3ToVector2Int(Vector3 position) =>
        new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
}
