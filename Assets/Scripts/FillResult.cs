using System.Collections.Generic;
using UnityEngine;

public class FillResult
{
    public int width;
    public int height;
    public Color[] colors;
    public List<Vector2Int> path;

    public Texture2D originalImage;
    public int cropX;
    public int cropY;
    public int croppedWidth;
    public int croppedHeight;

    public float croppedWidthInMeters;

    public Vector2 offsetInMeters;

    public FillResult(int width, int height, Color[] colors, List<Vector2Int> path, Texture2D originalImage, int cropX,
        int cropY, int croppedWidth, int croppedHeight, float croppedWidthInMeters, Vector2 offsetInMeters)
    {
        this.width = width;
        this.height = height;
        this.colors = colors;
        this.path = path;
        this.originalImage = originalImage;
        this.cropX = cropX;
        this.cropY = cropY;
        this.croppedWidth = croppedWidth;
        this.croppedHeight = croppedHeight;
        this.croppedWidthInMeters = croppedWidthInMeters;
        this.offsetInMeters = offsetInMeters;
    }
}