using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class VisualSort : MonoBehaviour
{
    [Range(0,1000)]public int delay = 5;
    public float maxHeight = 10f;
    public float maxWidth = 20f;
    public int amount = 500;
    
    public int[] values;

    [ContextMenu("Create Numbers")]
    public void CreateNumbers()
    {
        values = new int[amount];
        for (int i = 0; i < amount; i++)
        {
            values[i] = Random.Range(0, 10000);
        }
    }

    [ContextMenu("Selection Sort")]
    public void SelectionSortStart()
    {
        SelectionSort(values);
    }
    
    
    public void SelectionSort(int[] input) // Selection sort
    {
        for (int i = 0; i < input.Length; i++)
        {
            int min = i;
            for (int j = i + 1; j < input.Length; j++)
            {
                if (input[j] < input[min])
                {
                    min = j;
                }

                values = input;
            }

            int temp = input[i];
            input[i] = input[min];
            input[min] = temp;
        }
    }

    void DrawThings()
    {
        if (values.Length == 0)
        {
            return;
        }
        
        Vector2 offset = new Vector2(transform.position.x, transform.position.y);
        float spacing = maxWidth / values.Length;
        float max = Mathf.Max(values);
        

        for (int i = 0; i < values.Length; i++)
        {
            Vector2 a = offset + new Vector2(i * spacing, 0);
            float h = (values[i] / max) * maxHeight;
            Vector2 b = offset + new Vector2(i * spacing, h);
            Gizmos.DrawLine(a, b);
        }
    }
}
