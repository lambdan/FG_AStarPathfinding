using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomScroll : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 1f;
    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }
    
    // Update is called once per frame
    void Update()
    {
        _cam.orthographicSize -= (Input.GetAxisRaw("Mouse ScrollWheel") * _scrollSpeed);
        if (_cam.orthographicSize < 1)
        {
            _cam.orthographicSize = 1;
        } else if (_cam.orthographicSize > 40)
        {
            _cam.orthographicSize = 40;
        }
    }
}
