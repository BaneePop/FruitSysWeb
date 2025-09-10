#!/bin/bash
echo "🔍 Checking for build errors..."
echo "================================="

cd /Users/nikola/FruitSysWeb

# Samo build bez clean/restore
echo "🔨 Attempting build..."
dotnet build --verbosity quiet 2>&1

echo "Done."
