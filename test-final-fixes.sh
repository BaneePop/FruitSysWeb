#!/bin/bash

echo "🔧 FINALNE ISPRAVKE FRUITSYSWEB APLIKACIJE"
echo "=========================================="

cd /Users/nikola/FruitSysWeb

echo ""
echo "📝 PROBLEM REŠENI:"
echo "1. ✅ FINANSIJE - Ispravljena cena za prodaju (Duguje/Količina)"
echo "2. ✅ PROIZVODNJA - Uklonjen nepotreban 'Min Količina' filter"
echo "3. ✅ PROIZVODNJA - Ispravljen SQL da učitava sve artikle, ne samo tip 4"
echo "4. ✅ LAGER - Ispravljena logika za RadniNalogLager (Proizvedeno vs Potrebno)"
echo "5. ✅ HOME - Dodano IMagacinLagerService za statistike"

echo ""
echo "🔨 Pokretam build test..."
dotnet build --configuration Debug > build_output.log 2>&1

if [ $? -eq 0 ]; then
    echo "✅ Build je uspešno završen!"
    echo ""
    echo "🎯 TESTIRANJE POTREBNO:"
    echo "================================="
    echo "1. FINANSIJE:"
    echo "   - Cena treba da se računa iz Duguje za prodaju"
    echo "   - Filteri treba da rade selektivno"
    echo ""
    echo "2. PROIZVODNJA:"
    echo "   - Treba da učitava podatke iz vPreradaPregled"
    echo "   - Uklonjen Min Količina filter"
    echo ""
    echo "3. LAGER:"
    echo "   - Kolona 'Potrebna Količina' iz RadniNalog.Kolicina"
    echo "   - Kolona 'Proizvedeno' iz vwRadniNalogLager.Kolicina"
    echo ""
    echo "4. HOME:"
    echo "   - Statistike treba da se učitavaju bez greške"
    echo ""
    echo "🚀 Za pokretanje: dotnet run"
    echo "🌐 URL: https://localhost:5001"
    echo ""
    echo "📊 STRUKTURA BAZE PODATAKA:"
    echo "- vPrometFinansijev9 (finansije)"
    echo "- vPreradaPregled (proizvodnja)"
    echo "- vwMagacinLager (lager magacin)"
    echo "- vwRadniNalogLager (lager proizvodnje)"
    echo "- Artikal (master data)"
    echo "- Komitent (master data)"
    echo "- RadniNalog (radni nalozi)"
else
    echo "❌ Build neuspešan! Proverite build_output.log za detalje"
    echo ""
    echo "Poslednji redovi greške:"
    tail -20 build_output.log
fi

echo ""
echo "📁 Log fajl: build_output.log"
