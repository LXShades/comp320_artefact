using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Debug shape drawers
/// </summary>
public static class DebugDraw
{
    /// <summary>
    /// Draws a cross at the given position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    public static void Point(Vector3 position, Color color, float size = 0.25f)
    {
        float halfSize = size * 0.5f;

        Debug.DrawLine(position - new Vector3(halfSize, 0, 0), position + new Vector3(halfSize, 0, 0), color);
        Debug.DrawLine(position - new Vector3(0, halfSize, 0), position + new Vector3(0, halfSize, 0), color);
        Debug.DrawLine(position - new Vector3(0, 0, halfSize), position + new Vector3(0, 0, halfSize), color);
    }

    /// <summary>
    /// Draws a line. Equivalent to Debug.DrawLine
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="color"></param>
    public static void Line(Vector3 start, Vector3 end, Color color)
    {
        Debug.DrawLine(start, end, color);
    }
}
