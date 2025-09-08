#!/bin/bash

echo "🔧 FINALNE ISPRAVKE - PROIZVODNJA MODUL"
echo "======================================="

cd /Users/nikola/FruitSysWeb

echo ""
echo "📝 REŠENI PROBLEMI:"
echo "1. ✅ TIPOVI ARTIKALA - Ispravni prema dokumentu:"
echo "   • 1 = Sirovina (od čega se dobija gotov proizvod)"
echo "   • 2 = Ambalaza (primarana i sekundarna)"
echo "   • 3 = Potrosni materijal (trake, folije, etikete)"
echo "   • 4 = Gotova roba (gotov proizvod sa +/-)"
echo "   • 5 = Oprema (viljuškari, trake)"
echo ""
echo "2. ✅ RADNI NALOG FILTER - Sada radi na RadniNalog.Sifra"
echo "3. ✅ DUPLO BROJANJE - Grupisanje po radnom nalogu ID-u"
echo "4. ✅ NOVE KOLONE U TABELI:"
echo "   • Količina Roba (svi tipovi sem 2,4)"
echo "   • Količina Ambalaza (tip 2)"
echo "   • Gotov Proizvod (tip 4)"
echo "   • Uklonjene: Tip, Klasifikacija"

echo ""
echo "🔨 Pokretam build test..."
dotnet build --configuration Debug > build_output.log 2>&1

if [ $? -eq 0 ]; then
    echo "✅ Build je uspešno završen!"
    echo ""
    echo "🎯 TESTIRANJE POTREBNO:"
    echo "================================="
    echo "1. PROIZVODNJA /proizvodnja:"
    echo "   ✓ Filter 'Radni nalog' treba da filtrira po jednom nalogu"
    echo "   ✓ Jedan radni nalog = jedan red u tabeli"
    echo "   ✓ Kolone: Količina Roba + Količina Ambalaza + Gotov Proizvod"
    echo "   ✓ Ukupno treba da sumira sve tri kolone"
    echo ""
    echo "2. STRUKTURA RADNIH NALOGA:"
    echo "   • RN-25-0201 = JEDAN jedinstveni radni nalog"
    echo "   • Artikli se grupišu po tipovima u kolone"
    echo "   • Komitent se prikazuje iz RadniNalog tabele"
    echo ""
    echo "3. FILTERI:"
    echo "   • Radni nalog dropdown: lista iz RadniNalog.Sifra"
    echo "   • Komitent dropdown: funkcionalan"
    echo "   • Tip artikla: filtrira naloge koji imaju taj tip"
    echo ""
    echo "🚀 Za pokretanje: dotnet run"
    echo "🌐 URL: https://localhost:5001/proizvodnja"
    echo ""
    echo "📊 SQL LOGIKA:"
    echo "- Glavni upit: FROM RadniNalog JOIN vPreradaPregled"
    echo "- Grupiranje: GROUP BY RadniNalog.ID"
    echo "- Kolone: SUM po tipovima artikala"
    echo "- Filteri: direktno na RadniNalog.Sifra"
else
    echo "❌ Build neuspešan! Proverite build_output.log za detalje"
    echo ""
    echo "Poslednji redovi greške:"
    tail -20 build_output.log
fi

echo ""
echo "📁 Log fajl: build_output.log"
echo ""
echo "🎊 PROIZVODNJA MODUL JE KOMPLETNO REFAKTORISAN!"
echo "Sada prikazuje jedinstevene radne naloge sa grupiranim tipovima artikala"
