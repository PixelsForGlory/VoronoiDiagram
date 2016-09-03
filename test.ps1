# Run unit test
Copy-Item $env:APPVEYOR_BUILD_FOLDER\VoronoiDiagram\bin\Release\PixelsForGlory.ComputationalSystem.VoronoiDiagram.* -Destination $env:APPVEYOR_BUILD_FOLDER\VoronoiDiagramTest\bin\Release\ -Force
vstest.console $env:APPVEYOR_BUILD_FOLDER\VoronoiDiagramTest\bin\Release\VoronoiDiagramTest.dll /Settings:$env:APPVEYOR_BUILD_FOLDER\VoronoiDiagramTest\test.runsettings /logger:Appveyor
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode)  }
