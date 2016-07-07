param($installPath, $toolsPath, $package, $project)

$baseDir = (Get-Item $project.FullName).DirectoryName
$assembly = ""
Foreach ($file in $package.GetFiles())
{
    # file paths to all content files and remove them from the directory and the project
    $pathParts = $file.Path.Split("\")
    if($pathParts[0] -ne "content")
    {
        continue
    }
    
    $assembly = $pathParts[($pathParts.Length-1)].Split(".")
    $assembly = $assembly[0]

    $pathParts = $pathParts[1..($pathParts.Length - 1)]

    $path = $pathParts -join "\" | Out-String
    $path = $baseDir + "\" + $path
    $path = $path.Replace("`r", "")
    $path = $path.Replace("`n", "")

     Remove-Item "$path*"
}
$project.Object.References | Where-Object { $_.Name -eq $assembly } | ForEach-Object { $_.Remove() }


