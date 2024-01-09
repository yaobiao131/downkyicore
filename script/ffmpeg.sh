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
  local filename=""
  case $arch in
  x64)
    filename=ffmpeg-x86_64-apple-darwin_static.zip
    ;;
  arm64)
    filename=ffmpeg-aarch64-apple-darwin_static.zip
    ;;
  esac
  local url="https://github.com/yaobiao131/downkyi-ffmpeg-build/releases/download/continuous/$filename"
  create_dir "$ffmpeg_save_path/osx-$arch/ffmpeg"
  curl -kL "$url" -o "$download_dir/ffmpeg.zip"
  unzip -d "$ffmpeg_save_path/osx-$arch/ffmpeg/" -o "$download_dir/ffmpeg.zip"
  chmod +x "$ffmpeg_save_path/osx-$arch/ffmpeg/ffmpeg"
}

download_ffmpeg_linux() {
  local filename=""
  case $arch in
  x64)
    filename=ffmpeg-x86_64-linux-musl_static.zip
    ;;
  esac
  local url="https://github.com/yaobiao131/downkyi-ffmpeg-build/releases/download/continuous/$filename"
  create_dir "$ffmpeg_save_path/linux-$arch/ffmpeg"
  curl -kL "$url" -o "$download_dir/ffmpeg.zip"
  unzip -d "$ffmpeg_save_path/linux-$arch/ffmpeg/" -o "$download_dir/ffmpeg.zip"
  chmod +x "$ffmpeg_save_path/linux-$arch/ffmpeg/ffmpeg"
}

if [ "$os" == "mac" ]; then
  download_ffmpeg_macos
elif [ "$os" == "linux" ]; then
  download_ffmpeg_linux
else
  echo "不支持的操作系统"
fi
