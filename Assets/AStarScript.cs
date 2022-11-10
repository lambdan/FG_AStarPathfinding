using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AStarScript : MonoBehaviour
{
    public List<Node> openNodes = new List<Node>();
    public HashSet<Vector2Int> closedPositions = new HashSet<Vector2Int>();
    
    public Transform startTransform;
    public Transform goalTransform;
    public float maxHCostScaler = 3f;
    
    public class Node
    {
        public Vector2Int position; // node you came from to get to this node

        public Node parent;
        
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

        public void SetParentAndG(Node newParent, float newG)
        {
            parent = newParent;
            g = newG;
        }
    }

    private void OnDrawGizmos()
    {
        startTransform = transform;

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
        List<Vector2Int> pts = new List<Vector2Int>();

        pts.Add(start);
        
        
        
        pts.Add(goal);
        
        return pts;
    }

}
