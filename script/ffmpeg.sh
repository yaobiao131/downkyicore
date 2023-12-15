#!/bin/bash
os=$1
arch=$2

ffmpeg_save_path="../DownKyi.Core/Binary"
download_dir="./downloads"

create_dir() {
  if [ ! -d "$1" ]; then
    mkdir -p "$1"
  fi
}

create_dir "$download_dir"

download_ffmpeg_macos() {
  local url="https://github.com/eugeneware/ffmpeg-static/releases/download/b4.4.1/darwin-$arch"
  create_dir "$ffmpeg_save_path/osx-$arch/ffmpeg"
  curl -kL "$url" -o "$ffmpeg_save_path/osx-$arch/ffmpeg/ffmpeg"
  chmod +x "$ffmpeg_save_path/osx-$arch/ffmpeg/ffmpeg"
}

download_ffmpeg_linux() {
  local realArch=""
  case $arch in
  x64)
    realArch="amd64"
    ;;
  arm64)
    realArch="arm64"
    ;;
  esac
  output=$ffmpeg_save_path/linux-x64/ffmpeg
  local url="https://www.johnvansickle.com/ffmpeg/old-releases/ffmpeg-4.4.1-${realArch}-static.tar.xz"
  if [ ! -f "ffmpeg.tar.xz" ]; then
    curl -kL $url -o "${download_dir}/ffmpeg.tar.xz"
  fi

  if [ ! -d "$output" ]; then
    mkdir -p "$output"
  fi
  tar -xf "${download_dir}/ffmpeg.tar.xz" --strip-components 1 -C "$output" '*/ffmpeg'
  chmod +x "$output/ffmpeg"
}

if [ "$os" == "mac" ]; then
  download_ffmpeg_macos
elif [ "$os" == "linux" ]; then
  download_ffmpeg_linux
else
  echo "不支持的操作系统"
fi
