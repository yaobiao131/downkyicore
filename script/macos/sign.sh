#!/bin/bash
APP_NAME="哔哩下载姬.app"
ENTITLEMENTS="DownKyi.entitlements"
SIGNING_IDENTITY="biao yao" # matches Keychain Access certificate name

find "$APP_NAME/Contents/MacOS/" -type f | while read -r file; do
  if file "$file" | grep -q "Mach-O"; then
    echo "[INFO] Signing $file"
    codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$file"
  fi
done

echo "[INFO] Signing app file"

codesign --deep --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$APP_NAME"
