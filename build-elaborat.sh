#!/usr/bin/env bash
# Builds elaborat.pdf from elaborat.md:  Markdown -> styled HTML -> Chrome print-to-PDF.
# Usage:  bash build-elaborat.sh
set -euo pipefail
cd "$(dirname "$0")"

CHROME="/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"

npx -y marked -i elaborat.md -o elaborat.content.html   # Markdown -> HTML fragment
node wrap-elaborat.js                                    # wrap with print CSS -> elaborat.html
"$CHROME" --headless=new --disable-gpu --no-pdf-header-footer \
  --print-to-pdf="$PWD/elaborat.pdf" "file://$PWD/elaborat.html"

rm -f elaborat.content.html elaborat.html                # tidy intermediates
echo "Built elaborat.pdf"
