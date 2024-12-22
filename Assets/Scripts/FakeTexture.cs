using UnityEngine;

public class FakeTexture
{
    public Color[] colors;
    public int width;
    public int height;

    public FakeTexture(Color[] colors, int width, int height)
    {
        this.colors = colors;
        this.width = width;
        this.height = height;
    }

    public FakeTexture(Texture2D tex) : this(tex.GetPixels(), tex.width, tex.height)
    {
    }

    public Color GetPixel(int x, int y)
    {
        return colors[y * width + x];
    }

    public void SetPixel(int x, int y, Color color)
    {
        colors[y * width + x] = color;
    }

    public void Apply()
    {
        
    }
}