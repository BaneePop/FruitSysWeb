#!/bin/bash

echo "🚨 CRITICAL FIX: MAGACIN MAPPING CORRECTION"

# Add all changes
git add .

# Critical fix commit message
git commit -m "🚨 CRITICAL FIX: Ispravljeno MagacinID mapiranje

GLAVNA GREŠKA OTKRIVENA I ISPRAVLJENA:
❌ Kod je koristio mešovitost: Artikal.Tip + Artikal.MagacinID
✅ SVE REFAKTORISANO da koristi SAMO Artikal.MagacinID

🔧 ISPRAVKE U Constants/MagacinTypes.cs:
- Promenjen sa SIROVINA=1 na SIROVINE=3 (ispravno MagacinID)  
- Promenjen sa AMBALAZA=2 na AMBALAZA=4 (ispravno MagacinID)
- Dodati ispravni ID-jevi: 2,3,4,5,6,8,9,10,11,12
- Dodato ShouldExclude() za ID=7 (Kalo i Rastur)
- Dodato IsValid() za validaciju MagacinID

🔧 ISPRAVKE U ProizvodnjaService.cs:
- ❌ CASE a.Tip WHEN 1 THEN 'Sirovina' 
- ✅ CASE a.MagacinID WHEN 3 THEN 'Sirovine'
- Ispravljena KolicinaRoba/KolicinaAmbalaza/GotovProizvod logika
- a.Tip zamenjen sa a.MagacinID kroz ceo servis

🔧 ISPRAVKE U TypeMappingService.cs:
- GetMagacinBadgeClass() koristi MagacinTypes.GetBadgeClass()
- GetMagacinDisplayName() koristi MagacinTypes.GetDisplayName()  
- GetMagacinDropdownOptions() koristi MagacinTypes.ValidIds
- Uklonjen hardkodovani switch logic

🔧 ISPRAVKE U Lager.razor:
- Quick filters koriste ispravne MagacinID (3,4,6)
- Statistike metode ispravne (GetUsluzniLager, GetPoluproizvodi)
- GetRobaUKilogramima() isključuje ID=7 (Kalo i Rastur)

📊 TAČNO MAPIRANJE PREMA BAZI:
2  - Sveza Roba
3  - Sirovine (zamrznuta malina, kupina, šljiva)  
4  - Ambalaza
5  - PoluProizvodi (odbačena roba prilikom prerade)
6  - Gotov Proizvod
8  - Uslužni Lager Mlečni Proizvodi
9  - Repromaterijal (lepljiva traka)
10 - Đubriva
11 - Uslužni Lager Voće i Povrće
12 - Uslužni Lager Meso

⚠️  ISKLJUČENI: ID=1 (ne postoji), ID=7 (Kalo i Rastur)

IMPACT: Aplikacija će sada ispravno mapirati tipove artikala prema bazi!
TESTING: Potrebno testirati da svi UI elementi prikazuju ispravne tipove."

echo ""
echo "✅ Critical fix committed!"
echo ""
echo "🔍 KEY VALIDATION NEEDED:"
echo "   1. Test /test-mapping - verify MagacinID mapping"
echo "   2. Test Lager.razor dropdowns show correct options"  
echo "   3. Test Proizvodnja.razor displays correct tip names"
echo "   4. Verify statistics cards show correct values"
echo ""
echo "🎯 READY FOR: Funansije.razor refactor (final page)!"
