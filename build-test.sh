#!/bin/bash

echo "🧪 Test kompajliranja FruitSysWeb aplikacije..."

cd /Users/nikola/FruitSysWeb

echo "🔨 Proveravam build..."
dotnet build --configuration Debug 2>&1

if [ $? -eq 0 ]; then
    echo "✅ Aplikacija je uspešno kompajlirana!"
    echo ""
    echo "🎯 STANJE PROJEKTA:"
    echo "✅ Finansije - kompletno završeno"
    echo "✅ Proizvodnja - padajući meniji dodani" 
    echo "🔄 Lager - treba testiranje"
    echo ""
    echo "🚀 Za pokretanje koristite: dotnet run"
    echo "🌐 URL: https://localhost:5001"
else
    echo "❌ Build neuspešan - ima grešaka u kodu!"
fi
