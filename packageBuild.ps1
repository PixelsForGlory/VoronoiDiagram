$destinationFolder = "$env:STAGING_DIR\ReleaseContents\Plugins\"
if (!(Test-Path -path $destinationFolder)) {New-Item $destinationFolder -Type Directory}
Copy-Item $env:APPVEYOR_BUILD_FOLDER\VoronoiDiagram\bin\Release\VoronoiDiagram.dll -Destination $destinationFolder -Force 

$destinationFolder = "$env:STAGING_DIR\ReleaseContents\"
if (!(Test-Path -path $destinationFolder)) {New-Item $destinationFolder -Type Directory}
Copy-Item $env:APPVEYOR_BUILD_FOLDER\LICENSE -Destination $destinationFolder -Force 