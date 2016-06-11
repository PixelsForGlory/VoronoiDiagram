# Run unit test
Copy-Item $env:APPVEYOR_BUILD_FOLDER\VoronoiDiagram\bin\Release\VoronoiDiagram.* -Destination $env:APPVEYOR_BUILD_FOLDER\VoronoiDiagramTest\bin\Release\ -Force
vstest.console $env:APPVEYOR_BUILD_FOLDER\VoronoiDiagramTest\bin\Release\VoronoiDiagramTest.dll /Settings:$env:APPVEYOR_BUILD_FOLDER\VoronoiDiagramTest\test.runsettings /logger:Appveyor
