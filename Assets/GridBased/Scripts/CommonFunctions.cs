using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonFunctions
{
    public static Vector2Int V3toV2Int(Vector3 v3)
    {
        return new Vector2Int(Mathf.RoundToInt(v3.x), Mathf.RoundToInt(v3.y));
    }
}
