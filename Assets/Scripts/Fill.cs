using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

public class Fill
{
    public static void fill(RenderTexture originalImage, RenderTexture texture, Vector3 mousePosition,
        Func<FillResult, bool> finishFunction, RawImage rawImage)
    {
        Texture2D tex = new Texture2D(texture.width, texture.height);
        var active = RenderTexture.active;
        RenderTexture.active = texture;
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);


        if (rawImage != null)
        {
            var tex2 = new Texture2D(tex.width, tex.height);
            tex2.SetPixels(tex.GetPixels());
            tex2.Apply();
            rawImage.texture = tex2;
        }


        RenderTexture.active = originalImage;
        var originTex = new Texture2D(originalImage.width, originalImage.height);
        originTex.ReadPixels(new Rect(0, 0, originalImage.width, originalImage.height), 0, 0);


        RenderTexture.active = active;


        var task = new FillTask(originTex, tex, mousePosition, finishFunction);
        new Thread(task.fill).Start();
    }
}

class FillTask
{
    private const float squareSizeInMeters = 0.0160f;
    private Texture2D originalImage;
    private FakeTexture tex;
    private Vector3 mousePosition;
    private Func<FillResult, bool> finishFunction;

    public FillTask(Texture2D originalImage, Texture2D tex, Vector3 mousePosition,
        Func<FillResult, bool> finishFunction)
    {
        this.tex = new FakeTexture(tex);
        this.mousePosition = mousePosition;
        this.finishFunction = finishFunction;
        this.originalImage = originalImage;
    }

    public void fill()
    {
        Debug.Log("FILL method in new thread");

        Queue<Vector2Int> pixelPosition = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        pixelPosition.Enqueue(new Vector2Int((int)mousePosition.x, (int)mousePosition.y));

        while (pixelPosition.Count > 0)
        {
            var pos = pixelPosition.Dequeue();
            visited.Add(pos);

            tex.SetPixel(pos.x, pos.y, Color.black);

            if (checkPixel(visited, pos + Vector2Int.up))
            {
                pixelPosition.Enqueue(pos + Vector2Int.up);
                visited.Add(pos + Vector2Int.up);
            }

            if (checkPixel(visited, pos + Vector2Int.down))
            {
                pixelPosition.Enqueue(pos + Vector2Int.down);
                visited.Add(pos + Vector2Int.down);
            }

            if (checkPixel(visited, pos + Vector2Int.left))
            {
                pixelPosition.Enqueue(pos + Vector2Int.left);
                visited.Add(pos + Vector2Int.left);
            }

            if (checkPixel(visited, pos + Vector2Int.right))
            {
                pixelPosition.Enqueue(pos + Vector2Int.right);
                visited.Add(pos + Vector2Int.right);
            }
        }

        tex.Apply();

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                var p = tex.GetPixel(i, j);
                tex.SetPixel(i, j, p == Color.black ? Color.black : Color.white);
            }
        }

        tex.Apply();


        // calculate lengths
        var lengths = new List<int>();
        for (int j = 0; j < tex.height; j++)
        {
            var inside = false;
            var startX = 0;
            for (int i = 0; i < tex.width; i++)
            {
                if (!inside && tex.GetPixel(i, j) == Color.black)
                {
                    inside = true;
                    startX = i;
                    continue;
                }

                if (inside && tex.GetPixel(i, j) == Color.white)
                {
                    inside = false;
                    lengths.Add(i - startX);
                }
            }
        }

        lengths.Sort();

        if (lengths.Count < 10)
        {
            Debug.Log("Not enough walls detected");
            return;
        }

        var squareSize = lengths[(int)(lengths.Count * 0.1)];


        Debug.Log("I thinks it's :" + squareSize);
        
        var cornerFirstStepIncrement = squareSize / 2 + 3;

        var firstStepFilteredCornerCandidates = new List<Vector2Int>();

        for (int i = 0; i < tex.width; i++)
        {
            if (i % 50 == 0)
            {
                Debug.Log(i + "/" + tex.width);
            }

            for (int j = 0; j < tex.height; j++)
            {
                var pos = new Vector2Int(i, j);
                var percent = blackPercentage(pos, squareSize, cornerFirstStepIncrement);

                percent += 1 - blackPercentage(pos + Vector2Int.up * squareSize, squareSize, cornerFirstStepIncrement);
                percent += 1 - blackPercentage(pos + Vector2Int.left * squareSize, squareSize,
                    cornerFirstStepIncrement);

                if (percent > 2.9)
                {
                    firstStepFilteredCornerCandidates.Add(pos);
                }
            }
        }

        Debug.Log("First step filtered: " + firstStepFilteredCornerCandidates.Count + " / " + tex.width * tex.height);

        var corner = new Vector2Int(-1, -1);
        var cornerMax = 0f;
        
        foreach (var pos in firstStepFilteredCornerCandidates)
        {
            var percent = blackPercentage(pos, squareSize, 10);

            percent += 1 - blackPercentage(pos + Vector2Int.up * squareSize, squareSize, 10);
            percent += 1 - blackPercentage(pos + Vector2Int.left * squareSize, squareSize, 10);

            if (percent > cornerMax)
            {
                cornerMax = percent;
                corner = pos;
            }
        }

        Debug.Log("Corner percent: " + cornerMax);
        Debug.Log("Corner: : " + corner);

        Queue<Vector2Int> pathToVisitPositions = new Queue<Vector2Int>();
        HashSet<Vector2Int> visitedPath = new HashSet<Vector2Int>();

        List<Vector2Int> path = new List<Vector2Int>();

        pathToVisitPositions.Enqueue(corner);

        var minX = 99999f;
        var maxX = 0f;
        var minY = 99999f;
        var maxY = 0f;

        while (pathToVisitPositions.Count > 0)
        {
            var pos = pathToVisitPositions.Dequeue();

            visitedPath.Add(pos);

            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);

            maxX = Mathf.Max(maxX, pos.x + squareSize);
            maxY = Mathf.Max(maxY, pos.y + squareSize);

            path.Add((pos - corner) / squareSize);


            // colorRect(pos, squareSize, Color.blue);

            var up = pos + Vector2Int.up * squareSize;
            var down = pos + Vector2Int.down * squareSize;
            var left = pos + Vector2Int.left * squareSize;
            var right = pos + Vector2Int.right * squareSize;


            var acceptPercent = 0.4;

            if (!visitedPath.Contains(up) && blackPercentage(up, squareSize, 1) > acceptPercent)
            {
                visitedPath.Add(up);
                pathToVisitPositions.Enqueue(up);
            }

            if (!visitedPath.Contains(down) && blackPercentage(down, squareSize, 1) > acceptPercent)
            {
                visitedPath.Add(down);
                pathToVisitPositions.Enqueue(down);
            }

            if (!visitedPath.Contains(left) && blackPercentage(left, squareSize, 1) > acceptPercent)
            {
                visitedPath.Add(left);
                pathToVisitPositions.Enqueue(left);
            }

            if (!visitedPath.Contains(right) && blackPercentage(right, squareSize, 1) > acceptPercent)
            {
                visitedPath.Add(right);
                pathToVisitPositions.Enqueue(right);
            }
        }

        tex.Apply();

        var pathStr = "";

        foreach (var node in path)
        {
            pathStr += node + " ";
            // Debug.Log(node);
        }

        Debug.Log("Path:");
        Debug.Log(pathStr);


        var w = (int)Mathf.Min((maxX - minX) + squareSize, tex.width - 1);
        var h = (int)Mathf.Min((maxY - minY) + squareSize, tex.height - 1);

        var cx = (int)Mathf.Max(minX - squareSize / 2, 0);
        var cy = (int)Mathf.Max(minY - squareSize / 2, 0);

        var croppedWidthInMeters = (float)w / squareSize * squareSizeInMeters;

        Debug.Log("Cropped sizes:");
        Debug.Log("Width: " + w);
        Debug.Log("Square size: " + squareSize);
        Debug.Log("Cropped width in meters: " + croppedWidthInMeters);


        var startPoint = corner - new Vector2Int(cx, cy) + new Vector2Int(squareSize / 2, -squareSize / 2);
        var mid = new Vector2Int(w / 2, h / 2);

        Vector2 offset = startPoint - mid;
        offset += Vector2Int.up * squareSize; // no idea why
        var offsetInMeters = offset / squareSize * squareSizeInMeters;

        Debug.Log("Offset in meters");
        Debug.Log(offsetInMeters);


        finishFunction(
            new FillResult(
                tex.width,
                tex.height,
                tex.colors,
                path,
                originalImage,
                cx,
                cy,
                w,
                h,
                croppedWidthInMeters,
                offsetInMeters
            ));
        // finishFunction(tex.width, tex.height, tex.colors, path, croppedTex);
    }

    private bool checkPixel(HashSet<Vector2Int> visited, Vector2Int position)
    {
        if (position.x >= tex.width)
        {
            return false;
        }

        if (position.y >= tex.height)
        {
            return false;
        }

        if (position.x < 0)
        {
            return false;
        }

        if (position.y < 0)
        {
            return false;
        }

        return !visited.Contains(position) && tex.GetPixel(position.x, position.y) == Color.red;
    }

    private float blackPercentage(Vector2Int startPosition, int size, int increment = 20)
    {
        var all = 0;
        var black = 0;

        for (int i = increment; i < size; i += increment)
        {
            for (int j = increment; j < size; j += increment)
            {
                var pos = startPosition + new Vector2Int(i, j);

                if (pos.x < 0 || pos.y < 0)
                {
                    continue;
                }

                if (pos.x >= tex.width || pos.y >= tex.height)
                {
                    continue;
                }

                if (tex.GetPixel(pos.x, pos.y) == Color.black)
                {
                    black++;
                }

                all++;
            }
        }

        if (all == 0)
        {
            return 0;
        }

        return (float)black / all;
    }

    private void colorRect(Vector2Int startPosition, int size, Color color)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var pos = startPosition + new Vector2Int(i, j);

                if (pos.x < 0 || pos.y < 0)
                {
                    continue;
                }

                if (pos.x >= tex.width || pos.y >= tex.height)
                {
                    continue;
                }

                tex.SetPixel(pos.x, pos.y, color);
            }
        }
    }
}