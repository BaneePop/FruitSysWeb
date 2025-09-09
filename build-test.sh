#!/bin/bash

echo "🚀 Building FruitSysWeb project..."
echo "=================================="

cd /Users/nikola/FruitSysWeb

echo "📦 Restoring packages..."
dotnet restore

echo "🔨 Building project..."
dotnet build --no-restore

echo "✅ Build completed!"
echo ""
echo "If you see this message without errors above, your project should compile successfully!"
echo ""
echo "To run the project:"
echo "dotnet run"