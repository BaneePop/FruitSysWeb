#!/usr/bin/env python3
"""
🔥 DARK THEME - AUTOMATIC INLINE STYLE FIX
Automatski menja sve white/light pozadine u dark theme boje
"""

import os
import re

# Dark theme boje
DARK_REPLACEMENTS = {
    r'background:\s*white': 'background: #1a2332',
    r'background-color:\s*white': 'background-color: #1a2332',
    r'background:\s*#fff\b': 'background: #1a2332',
    r'background-color:\s*#fff\b': 'background-color: #1a2332',
    r'background:\s*#ffffff': 'background-color: #1a2332',
    r'background-color:\s*#ffffff': 'background-color: #1a2332',
    r'background:\s*rgb\(255,\s*255,\s*255\)': 'background: #1a2332',
    r'color:\s*#0f5132': 'color: #e5e7eb',  # Zeleni header -> svetli tekst
    r'color:\s*#495057': 'color: #9ca3af',  # Tamni tekst -> sivi tekst
    r'color:\s*#6c757d': 'color: #9ca3af',  # Sivi tekst
}

def fix_inline_styles(file_path):
    """Fixuje inline styles u jednom fajlu"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        changes_made = []
        
        # Proveri da li ima <style> tag
        if '<style>' not in content:
            return None
        
        # Primeni sve zamene
        for pattern, replacement in DARK_REPLACEMENTS.items():
            matches = re.findall(pattern, content, re.IGNORECASE)
            if matches:
                content = re.sub(pattern, replacement, content, flags=re.IGNORECASE)
                changes_made.append(f"{len(matches)}x {pattern} → {replacement}")
        
        # Ako su napravljene izmene, sačuvaj fajl
        if changes_made:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return changes_made
        
        return None
        
    except Exception as e:
        print(f"❌ Greška pri obradi {file_path}: {e}")
        return None

def main():
    """Glavna funkcija"""
    pages_dir = '/Users/Bane/FruitSysWeb/Components/Pages'
    
    print("🔥 DARK THEME - AUTOMATIC INLINE STYLE FIX")
    print("=" * 60)
    print(f"📁 Pretraživanje: {pages_dir}")
    print()
    
    total_files = 0
    total_fixes = 0
    
    # Prođi kroz sve .razor fajlove
    for filename in os.listdir(pages_dir):
        if filename.endswith('.razor'):
            file_path = os.path.join(pages_dir, filename)
            total_files += 1
            
            changes = fix_inline_styles(file_path)
            
            if changes:
                total_fixes += 1
                print(f"✅ {filename}")
                for change in changes:
                    print(f"   - {change}")
                print()
    
    print("=" * 60)
    print(f"📊 REZULTAT:")
    print(f"   - Pregledano fajlova: {total_files}")
    print(f"   - Ažurirano fajlova: {total_fixes}")
    print()
    print("🎉 FIX ZAVRŠEN!")
    print()
    print("📝 SLEDEĆI KORACI:")
    print("   1. Zaustavi aplikaciju (Ctrl+C)")
    print("   2. dotnet clean && dotnet build")
    print("   3. dotnet run")
    print("   4. Hard refresh (Ctrl+Shift+F5)")

if __name__ == '__main__':
    main()
