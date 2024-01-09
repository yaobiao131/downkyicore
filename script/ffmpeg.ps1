param($arch)
function Create-Dir($dir) {
    if (!(Test-Path -Path $dir)) {
        New-Item $dir -ItemType "directory"
    }
}

Create-Dir ".\downloads"
$url="";
$dir="";
if ($arch -eq "x86") {
    $url="https://github.com/yaobiao131/downkyi-ffmpeg-build/releases/download/continuous/ffmpeg-i686-w64-mingw32_static.zip";
}

if ($arch -eq "x64") {
    $url="https://github.com/yaobiao131/downkyi-ffmpeg-build/releases/download/continuous/ffmpeg-x86_64-w64-mingw32_static.zip";
}

Start-BitsTransfer -Source $url -Destination ".\downloads\ffmpeg-$arch.zip";

$destDir="..\DownKyi.Core\Binary\win-$arch\ffmpeg\";

Expand-Archive -Path ".\downloads\ffmpeg-$arch.zip" -DestinationPath $destDir -Force
