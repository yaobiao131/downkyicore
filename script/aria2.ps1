param($arch)
function Create-Dir($dir) {
    if (!(Test-Path -Path $dir)) {
        New-Item $dir -ItemType "directory"
    }
}

Create-Dir ".\downloads"

$url="";
if ($arch -eq "x86") {
    $url="https://github.com/abcfy2/aria2-static-build/releases/download/1.37.0/aria2-i686-w64-mingw32_static.zip";
}

if ($arch -eq "x64") {
    $url="https://github.com/abcfy2/aria2-static-build/releases/download/1.37.0/aria2-x86_64-w64-mingw32_static.zip";
}

Start-BitsTransfer -Source $url -Destination ".\downloads\aria2-$arch.zip";

$destDir="..\DownKyi.Core\Binary\win-$arch\aria2\";

Expand-Archive -Path ".\downloads\aria2-$arch.zip" -DestinationPath ".\aria2" -Force
Create-Dir $destDir

Copy-Item ".\aria2\aria2c.exe" "$destDir\aria2c.exe" -Force

Remove-Item ".\aria2" -Recurse -Force