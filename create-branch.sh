#!/bin/bash

# Kreiranje git branch-a za refactoring
echo "🚀 Kreiranje git branch za refactoring..."

# Proveriti trenutno stanje
echo "📍 Trenutno stanje:"
git status --short
echo ""

# Prikazati trenutnu granu  
echo "📍 Trenutna grana:"
git branch --show-current
echo ""

# Commitovati trenutne promene ako ih ima
echo "💾 Commitovanje trenutnih promena..."
git add .
git commit -m "Pre refactoring: Početno stanje sa duplikovanom logikom

- Duplikovani tipovi artikala u Lager.razor, Proizvodnja.razor, Funansije.razor
- GetBadgeClass() funkcija ponovljena u svakom fajlu  
- Dropdown opcije hardkodovane svugde
- Utils/ArtikalHelper.cs koristi stare tipove (1-5)
- FilterRequest.cs ima helper metode sa starima tipovima

Ready for refactoring centralization."

# Kreirati novu granu
echo "🌿 Kreiranje nove grane: feature/centralize-types..."
git checkout -b feature/centralize-types

# Prikazati status
echo "✅ Kreirana grana!"
git branch --show-current

echo ""
echo "🎯 Sledeći koraci:"
echo "1. Kreirati Constants/ folder"
echo "2. Implementirati MagacinTypes.cs"
echo "3. Refactor po fazama"
echo ""
echo "📋 Progress tracking u: REFACTORING_PROGRESS.md"
