#!/bin/bash
echo "🔨 Building FruitSysWeb project..."
echo "===================================="

cd /Users/nikola/FruitSysWeb

# Clean first
echo "🧹 Cleaning..."
dotnet clean

# Restore packages
echo "📦 Restoring packages..."
dotnet restore

# Build
echo "🔨 Building..."
dotnet build --configuration Debug --verbosity normal

echo "✅ Build completed!"
echo "To run: dotnet run"
