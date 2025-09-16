#!/bin/bash

echo "🚀 FruitSysWeb - ApexCharts.NET Build & Test"
echo "============================================="

# Build the project
echo "📦 Building project..."
dotnet build

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    
    # Run the application
    echo "🌐 Starting application..."
    echo "📊 Dashboard will be available at: http://localhost:5073/"
    echo "🧪 Charts Test page: http://localhost:5073/charts-test"
    echo ""
    echo "Press Ctrl+C to stop the application"
    echo ""
    
    dotnet run
else
    echo "❌ Build failed! Please check the errors above."
    exit 1
fi
