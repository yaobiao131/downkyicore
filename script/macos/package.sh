#!/bin/bash
APP_NAME="./哔哩下载姬.app"
DMG_NAME="./DownKyi.dmg"
PUBLISH_OUTPUT_DIRECTORY="../../DownKyi/bin/Release/net6.0/osx-x64/publish/."

INFO_PLIST="./Info.plist"
ICON_FILE="./logo.icns"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/$ICON_FILE"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"

create-dmg --hdiutil-quiet --icon "哔哩下载姬.app" 165 175 --icon-size 120 --app-drop-link 495 175 --window-size 660 400 $DMG_NAME $APP_NAME