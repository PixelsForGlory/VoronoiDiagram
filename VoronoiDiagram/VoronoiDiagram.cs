// Copyright 2016 afuzzyllama. All Rights Reserved.
using System.Collections.Generic;
using PixelsForGlory.Extensions;
using UnityEngine;

namespace PixelsForGlory.VoronoiDiagram
{
    /// <summary>
    /// Generates a voronoi diagram
    /// </summary>
    public class VoronoiDiagram
    {
        // Bounds of the Voronoi Diagram
        public Rect Bounds;

        // Generated sites.  Filled after GenerateSites() is called
        public Dictionary<int, VoronoiDiagramGeneratedSite> GeneratedSites;

        private readonly List<Vector2> _originalSites;

        // Stored added points as Sites that are currently being processed.  Ordered lexigraphically by y and then x
        private readonly List<VoronoiDiagramSite> _sites;

        // Stores the bottom most site when running GenerateEdges
        private VoronoiDiagramSite _bottomMostSite;

        // Stores the current site index when running GenerateEdges
        private int _currentSiteIndex;

        // Stores the minimum values of the points in site array.
        private Vector2 _minValues;

        // Stores the delta values of the minimum and maximum values.
        private Vector2 _deltaValues;

        public VoronoiDiagram(Rect bounds)
        {
            Bounds = bounds;
            GeneratedSites = new Dictionary<int, VoronoiDiagramGeneratedSite>();
            _originalSites = new List<Vector2>();
            _sites = new List<VoronoiDiagramSite>();
        }

        /// <summary>
        /// Adds points to the diagram to be used at generation time
        /// </summary>
        /// <param name="points">List of points to be added.</param>
        /// <returns>True if added successful, false otherwise.  If false, no points are added.</returns>
        public bool AddPoints(List<Vector2> points)
        {
            foreach(Vector2 point in points)
            {
                if(!Bounds.Contains(point))
                {
                    Debug.LogError(string.Format("point ({0}, {1}) out of diagram bounds ({2}, {3})", point.x, point.y,
                        Bounds.width, Bounds.height));
                    return false;
                }
            }

            foreach(Vector2 point in points)
            {
                _originalSites.Add(point);
            }

            return true;
        }

        /// <summary>
        /// Runs Fortune's Algorithm to generate sites with edges for the diagram
        /// </summary>
        /// <param name="relaxationCycles">Number of relaxation cycles to run</param>
        public void GenerateSites(int relaxationCycles)
        {
            if(_originalSites.Count == 0)
            {
                Debug.LogError("No points added to the diagram.  Sites cannot be generated");
                return;
            }

            _sites.Clear();
            foreach(Vector2 site in _originalSites)
            {
                _sites.Add(new VoronoiDiagramSite(_sites.Count, site));
            }
            SortSitesAndSetValues();

            // Cycles related to Lloyd's algorithm
            for(int cycles = 0; cycles < relaxationCycles; cycles++)
            {
                // Fortune's Algorithm
                int numGeneratedEdges = 0;
                int numGeneratedVertices = 0;
                _currentSiteIndex = 0;

                var priorityQueue = new VoronoiDiagramPriorityQueue(_sites.Count, _minValues, _deltaValues);
                var edgeList = new VoronoiDiagramEdgeList(_sites.Count, _minValues, _deltaValues);

                Vector2 currentIntersectionStar = Vector2.zero;
                VoronoiDiagramSite currentSite;

                var generatedEdges = new List<VoronoiDiagramEdge>();

                bool done = false;
                _bottomMostSite = GetNextSite();
                currentSite = GetNextSite();
                while(!done)
                {
                    if(!priorityQueue.IsEmpty())
                    {
                        currentIntersectionStar = priorityQueue.GetMinimumBucketFirstPoint();
                    }

                    VoronoiDiagramSite bottomSite;
                    VoronoiDiagramHalfEdge bisector;
                    VoronoiDiagramHalfEdge rightBound;
                    VoronoiDiagramHalfEdge leftBound;
                    VoronoiDiagramVertex vertex;
                    VoronoiDiagramEdge edge;
                    if(
                        currentSite != null &&
                        (
                            priorityQueue.IsEmpty() ||
                            currentSite.Coordinate.y < currentIntersectionStar.y ||
                            (
                                currentSite.Coordinate.y.IsAlmostEqualTo(currentIntersectionStar.y) &&
                                currentSite.Coordinate.x < currentIntersectionStar.x
                                )
                            )
                        )
                    {
                        // Current processed site is the smallest
                        leftBound = edgeList.GetLeftBoundFrom(currentSite.Coordinate);
                        rightBound = leftBound.EdgeListRight;
                        bottomSite = GetRightRegion(leftBound);

                        edge = VoronoiDiagramEdge.Bisect(bottomSite, currentSite);
                        edge.Index = numGeneratedEdges;
                        numGeneratedEdges++;

                        generatedEdges.Add(edge);

                        bisector = new VoronoiDiagramHalfEdge(edge, VoronoiDiagramEdgeType.Left);
                        edgeList.Insert(leftBound, bisector);

                        vertex = VoronoiDiagramVertex.Intersect(leftBound, bisector);
                        if(vertex != null)
                        {
                            priorityQueue.Delete(leftBound);

                            leftBound.Vertex = vertex;
                            leftBound.StarY = vertex.Coordinate.y + currentSite.GetDistanceFrom(vertex);

                            priorityQueue.Insert(leftBound);
                        }

                        leftBound = bisector;
                        bisector = new VoronoiDiagramHalfEdge(edge, VoronoiDiagramEdgeType.Right);

                        edgeList.Insert(leftBound, bisector);

                        vertex = VoronoiDiagramVertex.Intersect(bisector, rightBound);
                        if(vertex != null)
                        {
                            bisector.Vertex = vertex;
                            bisector.StarY = vertex.Coordinate.y + currentSite.GetDistanceFrom(vertex);

                            priorityQueue.Insert(bisector);
                        }

                        currentSite = GetNextSite();
                    }
                    else if(priorityQueue.IsEmpty() == false)
                    {
                        // Current intersection is the smallest
                        leftBound = priorityQueue.RemoveAndReturnMinimum();
                        VoronoiDiagramHalfEdge leftLeftBound = leftBound.EdgeListLeft;
                        rightBound = leftBound.EdgeListRight;
                        VoronoiDiagramHalfEdge rightRightBound = rightBound.EdgeListRight;
                        bottomSite = GetLeftRegion(leftBound);
                        VoronoiDiagramSite topSite = GetRightRegion(rightBound);

                        // These three sites define a Delaunay triangle
                        // Bottom, Top, EdgeList.GetRightRegion(rightBound);
                        // Debug.Log(string.Format("Delaunay triagnle: ({0}, {1}), ({2}, {3}), ({4}, {5})"),
                        //        bottom.Coordinate.x, bottom.Coordinate.y,
                        //      top.Coordinate.x, top.Coordinate.y,
                        //      edgeList.GetRightRegion(leftBound).Coordinate.x,
                        //      edgeList.GetRightRegion(leftBound).Coordinate.y);

                        var v = leftBound.Vertex;
                        v.Index = numGeneratedVertices;
                        numGeneratedVertices++;

                        leftBound.Edge.SetEndpoint(v, leftBound.EdgeType);
                        rightBound.Edge.SetEndpoint(v, rightBound.EdgeType);

                        edgeList.Delete(leftBound);

                        priorityQueue.Delete(rightBound);
                        edgeList.Delete(rightBound);

                        var edgeType = VoronoiDiagramEdgeType.Left;
                        if(bottomSite.Coordinate.y > topSite.Coordinate.y)
                        {
                            var tempSite = bottomSite;
                            bottomSite = topSite;
                            topSite = tempSite;
                            edgeType = VoronoiDiagramEdgeType.Right;
                        }

                        edge = VoronoiDiagramEdge.Bisect(bottomSite, topSite);
                        edge.Index = numGeneratedEdges;
                        numGeneratedEdges++;

                        generatedEdges.Add(edge);

                        bisector = new VoronoiDiagramHalfEdge(edge, edgeType);
                        edgeList.Insert(leftLeftBound, bisector);

                        edge.SetEndpoint(v, edgeType == VoronoiDiagramEdgeType.Left ? VoronoiDiagramEdgeType.Right : VoronoiDiagramEdgeType.Left);

                        vertex = VoronoiDiagramVertex.Intersect(leftLeftBound, bisector);
                        if(vertex != null)
                        {
                            priorityQueue.Delete(leftLeftBound);

                            leftLeftBound.Vertex = vertex;
                            leftLeftBound.StarY = vertex.Coordinate.y + bottomSite.GetDistanceFrom(vertex);

                            priorityQueue.Insert(leftLeftBound);
                        }

                        vertex = VoronoiDiagramVertex.Intersect(bisector, rightRightBound);
                        if(vertex != null)
                        {
                            bisector.Vertex = vertex;
                            bisector.StarY = vertex.Coordinate.y + bottomSite.GetDistanceFrom(vertex);

                            priorityQueue.Insert(bisector);
                        }
                    }
                    else
                    {
                        done = true;
                    }
                }

                GeneratedSites.Clear();
                // Bound the edges of the diagram
                foreach(VoronoiDiagramEdge currentGeneratedEdge in generatedEdges)
                {
                    currentGeneratedEdge.GenerateClippedEndPoints(Bounds);
                }

                foreach(VoronoiDiagramSite site in _sites)
                {
                    site.GenerateCentroid(Bounds);
                }

                foreach(VoronoiDiagramSite site in _sites)
                {
                    var generatedSite = new VoronoiDiagramGeneratedSite(site.Index, site.Coordinate, site.Centroid,
                        Color.white, site.IsCorner, site.IsEdge);
                    generatedSite.Vertices.AddRange(site.Vertices);

                    foreach(VoronoiDiagramEdge siteEdge in site.Edges)
                    {
                        // Only add edges that are visible
                        // Don't need to check the Right because they will both be float.MinValue
                        if(siteEdge.LeftClippedEndPoint == new Vector2(float.MinValue, float.MinValue))
                        {
                            continue;
                        }

                        generatedSite.Edges.Add(new VoronoiDiagramGeneratedEdge(siteEdge.Index,
                            siteEdge.LeftClippedEndPoint, siteEdge.RightClippedEndPoint));

                        if(siteEdge.LeftSite != null && !generatedSite.NeighborSites.Contains(siteEdge.LeftSite.Index))
                        {
                            generatedSite.NeighborSites.Add(siteEdge.LeftSite.Index);
                        }

                        if(siteEdge.RightSite != null && !generatedSite.NeighborSites.Contains(siteEdge.RightSite.Index))
                        {
                            generatedSite.NeighborSites.Add(siteEdge.RightSite.Index);
                        }
                    }
                    GeneratedSites.Add(generatedSite.Index, generatedSite);

                    // Finished with the edges, remove the references so they can be removed at the end of the method
                    site.Edges.Clear();
                }

                // Clean up
                _bottomMostSite = null;
                _sites.Clear();

                // Lloyd's Algorithm
                foreach(KeyValuePair<int, VoronoiDiagramGeneratedSite> generatedSite in GeneratedSites)
                {
                    var centroidPoint = new Vector2(Mathf.RoundToInt(generatedSite.Value.Centroid.x),
                        Mathf.RoundToInt(generatedSite.Value.Centroid.y));
                    if(!Bounds.Contains(centroidPoint))
                    {
                        Debug.LogError("Centroid point outside of diagram bounds");
                        return;
                    }

                    _sites.Add(new VoronoiDiagramSite(_sites.Count, new Vector2(centroidPoint.x, centroidPoint.y)));
                }
                SortSitesAndSetValues();
            }
        }

        /// <summary>
        /// Sorts sites and calculates _minValues and _deltaValues
        /// </summary>
        private void SortSitesAndSetValues()
        {
            _sites.Sort(
                delegate(VoronoiDiagramSite siteA, VoronoiDiagramSite siteB)
                {
                    if(Mathf.RoundToInt(siteA.Coordinate.y) < Mathf.RoundToInt(siteB.Coordinate.y))
                    {
                        return -1;
                    }

                    if(Mathf.RoundToInt(siteA.Coordinate.y) > Mathf.RoundToInt(siteB.Coordinate.y))
                    {
                        return 1;
                    }

                    if(Mathf.RoundToInt(siteA.Coordinate.x) < Mathf.RoundToInt(siteB.Coordinate.x))
                    {
                        return -1;
                    }

                    if(Mathf.RoundToInt(siteA.Coordinate.x) > Mathf.RoundToInt(siteB.Coordinate.x))
                    {
                        return 1;
                    }

                    return 0;
                });

            var currentMin = new Vector2(float.MaxValue, float.MaxValue);
            var currentMax = new Vector2(float.MinValue, float.MinValue);
            foreach(VoronoiDiagramSite site in _sites)
            {
                if(site.Coordinate.x < currentMin.x)
                {
                    currentMin.x = site.Coordinate.x;
                }

                if(site.Coordinate.x > currentMax.x)
                {
                    currentMax.x = site.Coordinate.x;
                }

                if(site.Coordinate.y < currentMin.y)
                {
                    currentMin.y = site.Coordinate.y;
                }

                if(site.Coordinate.y > currentMax.y)
                {
                    currentMax.y = site.Coordinate.y;
                }
            }

            _minValues = currentMin;
            _deltaValues = new Vector2(currentMax.x - currentMin.x, currentMax.y - currentMin.y);

        }

        /// <summary>
        /// Returns the next site and increments _currentSiteIndex
        /// </summary>
        /// <returns>The next site</returns>
        private VoronoiDiagramSite GetNextSite()
        {
            if(_currentSiteIndex < _sites.Count)
            {
                VoronoiDiagramSite nextSite = _sites[_currentSiteIndex];
                _currentSiteIndex++;
                return nextSite;
            }

            return null;
        }

        /// <summary>
        /// Returns the left region in relation to a half edge
        /// </summary>
        /// <param name="halfEdge">The half edge to calculate from</param>
        /// <returns>The left region</returns>
        private VoronoiDiagramSite GetLeftRegion(VoronoiDiagramHalfEdge halfEdge)
        {
            if(halfEdge.Edge == null)
            {
                return _bottomMostSite;
            }

            if(halfEdge.EdgeType == VoronoiDiagramEdgeType.Left)
            {
                return halfEdge.Edge.LeftSite;
            }
            else
            {
                return halfEdge.Edge.RightSite;
            }
        }

        /// <summary>
        /// Returns the right region in relation to a half edge
        /// </summary>
        /// <param name="halfEdge">The half edge to calculate from</param>
        /// <returns>The right region</returns>
        private VoronoiDiagramSite GetRightRegion(VoronoiDiagramHalfEdge halfEdge)
        {
            if(halfEdge.Edge == null)
            {
                return _bottomMostSite;
            }

            if(halfEdge.EdgeType == VoronoiDiagramEdgeType.Left)
            {
                return halfEdge.Edge.RightSite;
            }
            else
            {
                return halfEdge.Edge.LeftSite;
            }
        }
    }
}
