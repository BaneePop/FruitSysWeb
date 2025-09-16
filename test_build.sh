#!/bin/bash
cd /Users/nikola/FruitSysWeb
echo "Testing build..."
dotnet build --no-restore --verbosity minimal
echo "Build test complete."
