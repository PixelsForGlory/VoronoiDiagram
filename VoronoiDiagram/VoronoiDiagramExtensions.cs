using UnityEngine;
using System.Collections.Generic;
using PixelsForGlory.VoronoiDiagram;

public static class VoronoiDiagramExtensions
{
    /// <summary>
    /// Creates a list of colors of the Voronoi Diagram. Assumes that diagram has been run through GenerateDiagram
    /// </summary>
    /// <param name="voronoiDiagram">Voronoi Diagram</param>
    /// <returns>List of colors</returns>
    public static List<Color> GenerateColorList(this VoronoiDiagram voronoiDiagram)
    {
        var colorData = new List<Color>();

        for (int i = 0; i < (int)voronoiDiagram.Bounds.width * (int)voronoiDiagram.Bounds.height; i++)
        {
            colorData.Add(Color.white);
        }

        foreach (KeyValuePair<int, VoronoiDiagramGeneratedSite> site in voronoiDiagram.GeneratedSites)
        {
            if (site.Value.Vertices.Count == 0)
            {
                continue;
            }

            Vector2 minimumVertex = site.Value.Vertices[0];
            Vector2 maximumVertex = site.Value.Vertices[0];

            for (int i = 1; i < site.Value.Vertices.Count; i++)
            {
                if (site.Value.Vertices[i].x < minimumVertex.x)
                {
                    minimumVertex.x = site.Value.Vertices[i].x;
                }

                if (site.Value.Vertices[i].y < minimumVertex.y)
                {
                    minimumVertex.y = site.Value.Vertices[i].y;
                }

                if (site.Value.Vertices[i].x > maximumVertex.x)
                {
                    maximumVertex.x = site.Value.Vertices[i].x;
                }

                if (site.Value.Vertices[i].y > maximumVertex.y)
                {
                    maximumVertex.y = site.Value.Vertices[i].y;
                }
            }

            if (minimumVertex.x < 0.0f)
            {
                minimumVertex.x = 0.0f;
            }

            if (minimumVertex.y < 0.0f)
            {
                minimumVertex.y = 0.0f;
            }

            if (maximumVertex.x > voronoiDiagram.Bounds.width)
            {
                maximumVertex.x = voronoiDiagram.Bounds.width;
            }

            if (maximumVertex.y > voronoiDiagram.Bounds.height)
            {
                maximumVertex.y = voronoiDiagram.Bounds.height;
            }

            for (int x = (int)minimumVertex.x; x <= maximumVertex.x; ++x)
            {
                for (int y = (int)minimumVertex.y; y <= maximumVertex.y; ++y)
                {
                    if (PointInVertices(new Vector2(x, y), site.Value.Vertices))
                    {
                        if (voronoiDiagram.Bounds.Contains(new Vector2(x, y)))
                        {
                            int index = x + y * (int)voronoiDiagram.Bounds.width;
                            colorData[index] = site.Value.Color;
                        }
                    }
                }
            }
        }

        return colorData;
    }

    /// <summary>
    /// Does the passed in point lie inside of the vertices passed in.  The vertices are assumed to be sorted.
    /// </summary>
    /// <param name="point">Point that lies inside the vertices?</param>
    /// <param name="vertices">Sorted vertices</param>
    /// <returns></returns>
    public static bool PointInVertices(Vector2 point, List<Vector2> vertices)
    {
        int i;
        int j = vertices.Count - 1;

        bool oddNodes = false;

        for (i = 0; i < vertices.Count; ++i)
        {
            if (
                (vertices[i].y < point.y && vertices[j].y >= point.y ||
                 vertices[j].y < point.y && vertices[i].y >= point.y) &&
                (vertices[i].x <= point.x || vertices[j].x <= point.x)
                )
            {
                if (vertices[i].x +
                   (point.y - vertices[i].y) / (vertices[j].y - vertices[i].y) * (vertices[j].x - vertices[i].x) <
                   point.x)
                {
                    oddNodes = !oddNodes;
                }
            }
            j = i;
        }

        return oddNodes;
    }
}