#!/bin/bash

echo "🚀 COMMITTING FAZA 1 & 2 PROGRESS..."

# Add all changes
git add .

# Commit with detailed message
git commit -m "✅ FAZA 1 & 2 ZAVRŠENE: Constants + Core Services

FAZA 1 - CONSTANTS FRAMEWORK:
- ✅ MagacinTypes.cs - 12 tipova sa kompletnim mapiranjem  
- ✅ DocumentStatus.cs - 4 statusa sa transitions i icons
- ✅ SystemConstants.cs - system-wide settings i formatiranje
- ✅ DropdownOption i QuickFilter models

FAZA 2 - CORE SERVICES:
- ✅ TypeMappingService.cs - centralizovani mapping servis
- ✅ ServiceCollectionExtensions.cs - DI registration
- ✅ TestMapping.razor - test stranica (/test-mapping)

PROGRESS: 50% - Ready za FAZU 3 refactor postojećih stranica

NEXT: Refactor Lager.razor, Proizvodnja.razor, Funansije.razor
- Zameniti duplikovane switch statements
- Koristiti TypeMapping.GetMagacinBadgeClass() umesto hardcoded logike"

echo "✅ Commit completed!"
echo ""
echo "📋 Next steps:"
echo "1. Test /test-mapping stranica"  
echo "2. Refactor Lager.razor"
echo "3. Refactor Proizvodnja.razor"
echo "4. Refactor Funansije.razor"
