using UnityEngine;
using System.Collections;

/// <summary>
/// Utility class to paint a rectangle on screen.
/// </summary>
public class RectanglePainter : MonoBehaviour
{
    private const int width = 1;

    private Rect paintedRectangle;

    private Vector2 startPosition;

    private Texture2D texture;

    private void SetColor(Color color)
    {
        texture = new Texture2D(width, width);
        color.a = 0.3f;
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                texture.SetPixel(i, j, color);
            }
        }
        texture.Apply();
    }

    /// <summary>
    /// Creates a new painter, painting its rectangles with the given color.
    /// </summary>
    public static RectanglePainter GetWithColor(Color color)
    {
        RectanglePainter painter = new GameObject("Painter").AddComponent<RectanglePainter>();
        painter.SetColor(color);
        return painter;
    }

    void OnGUI()
    {
        if (texture != null && paintedRectangle.width > 0 && paintedRectangle.height > 0)
        {
            GUI.DrawTexture(paintedRectangle, texture, ScaleMode.StretchToFill, true);
        }
    }

    /// <summary>
    /// Start painting the rectangle at the given position.
    /// </summary>
    public void StartPainting(Vector3 mousePosition)
    {
        startPosition = mousePosition;
        paintedRectangle = new Rect(mousePosition.x, Screen.height - mousePosition.y, 0, 0);
    }

    /// <summary>
    /// Continue painting the rectangle at the given position.
    /// </summary>
    public void ContinuePainting(Vector3 mousePosition)
    {
        paintedRectangle.x = startPosition.x;
        paintedRectangle.y = Screen.height - startPosition.y;
        float width = mousePosition.x - startPosition.x;
        if (width < 0)
        {
            paintedRectangle.x = mousePosition.x;
            width = -width;
        }
        float height = startPosition.y - mousePosition.y;
        if (height < 0)
        {
            paintedRectangle.y = Screen.height - mousePosition.y;
            height = -height;
        }
        paintedRectangle.width = width;
        paintedRectangle.height = height;
    }

    /// <summary>
    /// Stop painting the rectangle.
    /// </summary>
    public void StopPainting()
    {
        paintedRectangle.width = 0;
        paintedRectangle.height = 0;
    }
}
