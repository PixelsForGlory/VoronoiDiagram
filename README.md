# Voronoi Diagram Library
Library to create voronoi diagrams in Unity3D.

## Prerequisite
- [PixelsForGlory.Extensions](https://github.com/PixelsForGlory/Extensions) needs to be installed first.

## Installation
- Point to this repository to [install as a package in a Unity project](https://docs.unity3d.com/Manual/upm-git.html)

## Usage

The following code:

    using PixelsForGlory.VoronoiDiagram;
    
    int width = 4096;
    int height = 4096;
    
    var voronoiDiagram = new VoronoiDiagram<Color>(new Rect(0f, 0f, width, height));    

    var points = new List<VoronoiDiagramSite<Color>>();
    while(points.Count < 1000)
    {
        int randX = Random.Range(0, width - 1);
        int randY = Random.Range(0, height - 1);

        var point = new Vector2(randX, randY);
        if(!points.Any(item => item.Coordinate == point))
        {
            points.Add(new VoronoiDiagramSite<Color>(point, new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }

    voronoiDiagram.AddPoints(points);
    voronoiDiagram.GenerateSites(2);

    var outImg = new Texture2D(width, height);
    outImg.SetPixels(voronoiDiagram.Get1DSampleArray().ToArray());
    outImg.Apply();

    System.IO.File.WriteAllBytes("diagram.png", outImg.EncodeToPNG());

Will create a image similar to:

![Voronoi Diagram](./Diagram.png?raw=true "Voronoi Diagram")

Additional information can be stored with a site through the generic type parameter.  This is accessed through the `SiteData` property.

Citations:
----------
Fortune's Algorithm as outlined in:
Steve J. Fortune (1987). "A Sweepline Algorithm for Voronoi Diagrams". Algorithmica 2, 153-174. 

Lloyd's algorithm as outlined in:
Lloyd, Stuart P. (1982), "Least squares quantization in PCM", IEEE Transactions on Information Theory 28 (2): 129–137

Monotone Chain Convex Hull Algorithm outlined in:
A. M. Andrew, "Another Efficient Algorithm for Convex Hulls in Two Dimensions", Info. Proc. Letters 9, 216-219 (1979)

Based off of:
---------
[as3delaunay](http://nodename.github.io/as3delaunay/)
