# Voronoi Diagram Library
Library to create voronoi diagrams in Unity3D.

Build status:

[![Build status](https://ci.appveyor.com/api/projects/status/2hrdm7nq8y487mf5/branch/master?svg=true)](https://ci.appveyor.com/project/LlamaBot/voronoidiagram/branch/master)

## Building
The solution has a reference set for `UnityEngine.dll`, but the path for this assembly is not set. To build the library, a reference path must be set to the Managed directory (Default is C:\Program Files\Unity\Editor\Data\Managed).  

Additionally, this library is also dependent on the library from [PixelsForGlory/Extensions](https://github.com/PixelsForGlory/Extensions).  If using NuGet package manager, you can add the Pixels for Glory NuGet repository at http://pixelsforglory.azurewebsites.net/nuget to your package manager.  If not, the dependency can be resolved manually by downloading the library from the [latest release](https://github.com/PixelsForGlory/Extensions/releases).

## Installation
From a build or downloaded release, copy the `VoronoiDiagram.dll` to `[PROJECT DIR]\Assets\Plugins`.

If using the Pixels for Glory NuGet repository, install the `PixelsForGlory.VoronoiDiagram` package into your own class library project or install the `PixelsForGlory.Unity3D.VoronoiDiagram` package into a Unity3D project.

## Usage

The following code:

    using PixelsForGlory.VoronoiDiagram;
    
    int width = 4096;
    int height = 4096;
    
    var voronoiDiagram = new VoronoiDiagram(new Rect(0f, 0f, width, height));    

    var points = new List<Vector2>();
    while(points.Count < 1000)
    {
        int randX = Random.Range(0, width - 1);
        int randY = Random.Range(0, height - 1);

        if(!points.Contains(new Vector2(randX, randY);
        {
            points.Add(new Vector2(randX, randY);
        }
    }

    voronoiDiagram.AddPoints(points);
    voronoiDiagram.GenerateSites(2);
    
    for(int i = 0; i < voronoiDiagram.GeneratedSites.Count; i++)
    {
        voronoiDiagram.GeneratedSites[i].Color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    var outImg = new Texture2D(width, height);
    outImg.SetPixels(voronoiDiagram.GenerateColorList().ToArray());
    outImg.Apply();

    System.IO.File.WriteAllBytes("diagram.png", outImg.EncodeToPNG());

Will create a image similar to:

![Voronoi Diagram](./VoronoiDiagramTest/OriginalImages/Diagram.png?raw=true "Voronoi Diagram")

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
