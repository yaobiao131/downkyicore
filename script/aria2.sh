#!/bin/bash
download_dir="./downloads"
save_path="../DownKyi.Core/Binary"

if [ ! -d "$download_dir" ]; then
  mkdir "$download_dir"
fi

create_dir() {
  if [ ! -d "$1" ]; then
    mkdir -p "$1"
  fi
}

download_aria2() {
  local download_url
  local save
  case $1 in
  linux-x64)
    save="$save_path/$1/aria2"
    download_url="https://github.com/yaobiao131/downkyi-aria2-static-build/releases/download/1.37.0/aria2-x86_64-linux-musl_static.zip"
    ;;
  linux-arm64)
    save="$save_path/$1/aria2"
    download_url="https://github.com/yaobiao131/downkyi-aria2-static-build/releases/download/1.37.0/aria2-aarch64-linux-musl_static.zip"
    ;;
  osx-x64)
    save="$save_path/$1/aria2"
    download_url="https://github.com/yaobiao131/downkyi-aria2-static-build/releases/download/1.37.0/aria2-x86_64-apple-darwin_static.zip"
    ;;
  osx-arm64)
    save="$save_path/$1/aria2"
    download_url="https://github.com/yaobiao131/downkyi-aria2-static-build/releases/download/1.37.0/aria2-aarch64-apple-darwin_static.zip"
    ;;
  esac

  curl -kL "$download_url" -o "$download_dir/aria2.zip"
  create_dir "$save"
  unzip -o -d "$save" "$download_dir/aria2.zip"
  chmod +x "$save/aria2c"
}

download_aria2 "$@"
