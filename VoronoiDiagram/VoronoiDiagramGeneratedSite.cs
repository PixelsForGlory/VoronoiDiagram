// Copyright 2016 afuzzyllama. All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;

namespace PixelsForGlory.VoronoiDiagram
{
    /// <summary>
    /// Stores final information about a site that is returned in GenerateEdges
    /// </summary>
    public class VoronoiDiagramGeneratedSite
    {
        public int Index;
        public Color Color;
        public Vector2 Coordinate;
        public Vector2 Centroid;
        public List<VoronoiDiagramGeneratedEdge> Edges;
        public List<Vector2> Vertices;
        public List<int> NeighborSites;

        public bool IsCorner;
        public bool IsEdge;

        public VoronoiDiagramGeneratedSite(int index, Vector2 coordinate, Vector2 centroid, Color color, bool isCorner,
            bool isEdge)
        {
            Index = index;
            Coordinate = coordinate;
            Centroid = centroid;
            Color = color;
            IsCorner = isCorner;
            IsEdge = isEdge;
            Edges = new List<VoronoiDiagramGeneratedEdge>();
            Vertices = new List<Vector2>();
            NeighborSites = new List<int>();
        }
    }
}
