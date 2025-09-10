#!/bin/bash
cd /Users/nikola/FruitSysWeb
echo "🔍 Testing build after fixes..."
dotnet build --no-restore --verbosity minimal 2>&1
echo "Build test complete."
