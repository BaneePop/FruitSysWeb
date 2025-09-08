#!/bin/bash
echo "Testing FruitSysWeb build..."
cd /Users/nikola/FruitSysWeb
echo "Running dotnet build..."
dotnet build
echo "Build completed with exit code: $?"
