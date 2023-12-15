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
    $url="https://github.com/sudo-nautilus/FFmpeg-Builds-Win32/releases/download/latest/ffmpeg-n4.4-latest-win32-gpl-4.4.zip";
    $dir="ffmpeg-n4.4-latest-win32-gpl-4.4"
}

if ($arch -eq "x64") {
    $url="https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-n4.4-latest-win64-gpl-4.4.zip";
    $dir="ffmpeg-n4.4-latest-win64-gpl-4.4"
}

Start-BitsTransfer -Source $url -Destination ".\downloads\ffmpeg-$arch.zip";

$destDir="..\DownKyi.Core\Binary\win-$arch\ffmpeg\";

Expand-Archive -Path ".\downloads\ffmpeg-$arch.zip" -DestinationPath ".\ffmpeg" -Force
Create-Dir $destDir

Copy-Item ".\ffmpeg\$dir\bin\ffmpeg.exe" "$destDir\ffmpeg.exe" -Force
Copy-Item ".\ffmpeg\$dir\LICENSE.txt" "$destDir\LICENSE.txt" -Force

Remove-Item ".\ffmpeg" -Recurse -Force