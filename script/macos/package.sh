#!/bin/bash
arch=$1
APP_NAME="./哔哩下载姬.app"
PUBLISH_OUTPUT_DIRECTORY="../../DownKyi/bin/Release/net6.0/osx-$arch/publish/."

INFO_PLIST="./Info.plist"
ICON_FILE="./logo.icns"

if [ -d "$APP_NAME" ]; then
  rm -rf "$APP_NAME"
fi

mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/$ICON_FILE"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"
if [ ! -x $APP_NAME/Contents/MacOS/aria2/aria2c ]; then
  chmod +x $APP_NAME/Contents/MacOS/aria2/aria2c
fi
if [ ! -x $APP_NAME/Contents/MacOS/ffmpeg/ffmpeg ]; then
  chmod +x $APP_NAME/Contents/MacOS/ffmpeg/ffmpeg
fi
