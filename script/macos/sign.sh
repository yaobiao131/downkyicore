#!/bin/bash
APP_NAME="哔哩下载姬.app"
ENTITLEMENTS="DownKyi.entitlements"
SIGNING_IDENTITY="biao yao" # matches Keychain Access certificate name

find "$APP_NAME/Contents/MacOS/" | while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$fname"
  fi
done

echo "[INFO] Signing app file"

codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$APP_NAME"
