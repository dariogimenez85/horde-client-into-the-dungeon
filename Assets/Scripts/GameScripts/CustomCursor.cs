
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = new Vector2(0, 0);
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

}
