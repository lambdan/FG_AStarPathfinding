using UnityEngine;

public class SquareScript : MonoBehaviour
{
    [SerializeField] private Color _openColor = Color.green;
    [SerializeField] private Color _closedColor = Color.red;
    private bool isOpen;
    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void OpenSquare()
    {
        isOpen = true;
        UpdateColor();
    }

    public void CloseSquare()
    {
        isOpen = false;
        UpdateColor();
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public bool IsBlocked()
    {
        return !isOpen;
    }
    
    void UpdateColor()
    {
        if (isOpen)
        {
            _sr.color = _openColor;
        }
        else
        {
            _sr.color = _closedColor;
        }
    }
}
