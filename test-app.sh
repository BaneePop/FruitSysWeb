#!/bin/bash

# Test aplikacije - kratka verzija
echo "🧪 Krećem test FruitSysWeb aplikacije..."

# Samo build bez pokretanja 
echo "🔨 Build aplikacije..."
dotnet build --configuration Debug

if [ $? -eq 0 ]; then
    echo "✅ Build je uspešno završen!"
    echo "🚀 Aplikacija je spremna za pokretanje sa 'dotnet run'"
    echo "📍 URL: https://localhost:5001"
else
    echo "❌ Build je neuspešan!"
    exit 1
fi
