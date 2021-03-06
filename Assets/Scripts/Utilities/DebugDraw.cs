﻿using System.Collections;
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
    /// <param name="position">The position of the point</param>
    /// <param name="color">The colour of the point surprisingly</param>
    /// <param name="size">The size of the point, in world units</param>
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
    /// <param name="start">The starting position of the line in world space</param>
    /// <param name="end">The end position of the line in world space</param>
    /// <param name="color">The colour of the line</param>
    public static void Line(Vector3 start, Vector3 end, Color color)
    {
        Debug.DrawLine(start, end, color);
    }

    /// <summary>
    /// Draws a box encompassing the boundaries 'min' and 'max'
    /// </summary>
    /// <param name="min">The coordinates of the minimal corner of the box</param>
    /// <param name="max">The coordinates of the maximal corner of the box</param>
    /// <param name="color">The colour of the box</param>
    public static void Box(Vector3 min, Vector3 max, Color color)
    {
        Debug.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z), color);
        Debug.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z), color);
        Debug.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), color);

        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), color);
        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), color);

        Debug.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z), color);
        Debug.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), color);

        Debug.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), color);
        Debug.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), color);

        Debug.DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(max.x, max.y, min.z), color);
        Debug.DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(max.x, min.y, max.z), color);
        Debug.DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z), color);
    }

    /// <summary>
    /// Draws a square
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name=""></param>
    public static void Square(Vector3 centre, Vector3 up, Vector3 right, Color color)
    {
        Debug.DrawLine(centre + up - right, centre + up + right, color);
        Debug.DrawLine(centre + up + right, centre - up + right, color);
        Debug.DrawLine(centre - up + right, centre - up - right, color);
        Debug.DrawLine(centre - up - right, centre + up - right, color);
    }
}
