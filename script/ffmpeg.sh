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

version="b4.4.1"

download_ffmpeg_macos() {
  local url="https://github.com/eugeneware/ffmpeg-static/releases/download/${version}/darwin-$arch"
  create_dir "$ffmpeg_save_path/osx-$arch/ffmpeg"
  curl -kL "$url" -o "$ffmpeg_save_path/osx-$arch/ffmpeg/ffmpeg"
  chmod +x "$ffmpeg_save_path/osx-$arch/ffmpeg/ffmpeg"
}

download_ffmpeg_linux() {
  local url="https://github.com/eugeneware/ffmpeg-static/releases/download/${version}/linux-$arch"
  create_dir "$ffmpeg_save_path/linux-$arch/ffmpeg"
  curl -kL "$url" -o "$ffmpeg_save_path/linux-$arch/ffmpeg/ffmpeg"
  chmod +x "$ffmpeg_save_path/linux-$arch/ffmpeg/ffmpeg"
}

if [ "$os" == "mac" ]; then
  download_ffmpeg_macos
elif [ "$os" == "linux" ]; then
  download_ffmpeg_linux
else
  echo "不支持的操作系统"
fi
