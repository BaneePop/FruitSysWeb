# 🎯 REFACTORING PLAN - FruitSysWeb Centralizacija

## 📋 TRENUTNO STANJE - ANALIZA DUPLIKOVANE LOGIKE

### 🔍 IDENTIFICIRANI PROBLEMI

#### **1. DUPLIKOVANI TIPOVI ARTIKALA/MAGACINID**
```
❌ PROBLEM: 4+ različita mesta sa različitim tipovima
├─ Utils/ArtikalHelper.cs          → Stari tipovi (1-5)
├─ Lager.razor                     → Novi MagacinID (1-12) 
├─ Proizvodnja.razor               → Miks starih/novih
├─ Funansije.razor                 → MagacinID (1-12)
└─ FilterRequest.cs                → Helper metode sa starima
```

#### **2. DUPLIKOVANE BADGE KLASE**
```
❌ PROBLEM: GetBadgeClass() funkcija u svakom Razor fajlu
├─ Lager.razor                     → switch (magacinId) => "bg-primary"
├─ Proizvodnja.razor               → switch (magacinId) => "bg-success"  
├─ Funansije.razor                 → switch (magacinId) => "bg-warning"
└─ Utils/ArtikalHelper.cs          → GetBadgeClass() stara logika
```

#### **3. DUPLIKOVANI DROPDOWN OPCIJE**
```
❌ PROBLEM: Hardkodovane <option> vrednosti svugde
├─ Lager.razor                     → 12 option value linija
├─ Proizvodnja.razor               → 12 option value linija
├─ Funansije.razor                 → 12 option value linija
└─ Components/Shared/Filters/      → Možda se ne koriste?
```

#### **4. DUPLIKOVANA DISPLAY IMENA**
```
❌ PROBLEM: GetDisplayName() logika ponovljena
├─ Lager.razor                     → switch za "Sveza Roba", "Sirovine", etc
├─ Proizvodnja.razor               → switch za tipove
├─ FilterRequest.cs                → GetArtikalTipNaziv() 
└─ Utils/ArtikalHelper.cs          → TipoviArtikala dictionary
```

#### **5. NEKORIŠĆENE/NEDOVRŠENE KOMPONENTE**
```
❓ NEJASNO: Da li se koriste?
├─ Components/Shared/Filters/*.razor  → Filter komponente
├─ Services/DashboardService.cs       → Mock podatci
├─ Controllers/*.cs                   → API endpoints
└─ Components/Charts/*.razor          → Chart komponente
```

---

## 🎯 CILJ REFACTORING-A

**JEDAN SOURCE OF TRUTH za sve tipove, nazive, boje, dropdown opcije!**

---

## 📅 PLAN IMPLEMENTACIJE

### **FAZA 1: CONSTANTS & ENUMS (Dan 1)**
```
/Constants/
├─ MagacinTypes.cs           → MagacinID konstante (1-12) sa nazivima
├─ DocumentStatus.cs         → Status konstante (1-4)
├─ BadgeClasses.cs          → CSS badge mapiranje
├─ DisplayNames.cs          → Svi display nazivi
└─ DropdownOptions.cs       → Dropdown opcije
```

### **FAZA 2: CORE SERVICES (Dan 2)**
```
/Services/Core/
├─ TypeMappingService.cs     → Centralizovano mapiranje tipova
├─ UIHelperService.cs        → Badge klase, dropdown, display logika
└─ FilterHelperService.cs    → Unified filter helper metode
```

### **FAZA 3: REFACTOR EXISTING (Dan 3-4)**
```
✅ Zameni svu logiku u:
├─ Lager.razor
├─ Proizvodnja.razor  
├─ Funansije.razor
├─ Utils/ArtikalHelper.cs
└─ FilterRequest.cs
```

### **FAZA 4: CLEANUP & TESTING (Dan 5)**
```
🧹 Ukloni duplikovanu logiku
🧪 Testiranje da sve radi
📚 Dokumentacija
```

---

## 💡 TEHNIČKA IMPLEMENTACIJA

### **1. MagacinTypes.cs**
```csharp
public static class MagacinTypes 
{
    public const int NEPOSTOJI = 1;
    public const int SVEZA_ROBA = 2;
    public const int SIROVINE = 3;
    // ... svih 12 tipova
    
    public static readonly Dictionary<int, string> DisplayNames = new()
    {
        { SVEZA_ROBA, "Sveža Roba" },
        { SIROVINE, "Sirovine" },
        // ...
    };
    
    public static readonly Dictionary<int, string> BadgeClasses = new()
    {
        { SVEZA_ROBA, "bg-success" },
        { SIROVINE, "bg-primary" },
        // ...
    };
}
```

### **2. TypeMappingService.cs**
```csharp
public class TypeMappingService
{
    public string GetDisplayName(int magacinId) => MagacinTypes.DisplayNames[magacinId];
    public string GetBadgeClass(int magacinId) => MagacinTypes.BadgeClasses[magacinId];
    public List<DropdownOption> GetDropdownOptions() => // ...
}
```

### **3. Refactor Razor Pages**
```razor
@* UMESTO OVOGA: *@
@{
    var tipNaziv = tip switch {
        2 => "Sveza Roba",
        3 => "Sirovine",
        // 10 linija...
    };
}

@* KORISTIMO OVO: *@
@TypeMappingService.GetDisplayName(tip)
```

---

## 🚦 SUCCESS CRITERIA

### ✅ **ZAVRŠENO KADA:**
1. **Jedan fajl** kontroliše sve tipove artikala
2. **Jedna funkcija** za badge klase  
3. **Jedan servis** za dropdown opcije
4. **Nema duplikovane logike** nigde
5. **Sve radi** kao pre refactoring-a

### 📊 **METRICS:**
- **Pred refactoring:** ~200 linija duplikovane logike
- **Posle refactoring:** ~50 linija centralizovane logike  
- **Smanjenje za:** ~75% duplikovane logike

---

## ⚠️ RIZICI & MITIGATION

### **RIZIK:** Pokvariti postojeću funkcionalnost
**MITIGATION:** Korak-po-korak refactoring sa testiranjem

### **RIZIK:** Previše kompleksno
**MITIGATION:** Početi sa jednostavnim constants fajlovima

### **RIZIK:** Zavisnosti između komponenti  
**MITIGATION:** Dependency injection za servise

---

## 🔄 ROLLBACK PLAN

Svaki korak je **git commit** - možemo se vratiti bilo kad!

---

**📅 START DATUM:** {DATUM_POCETKA}  
**👤 ODGOVORNO:** Claude + Developer  
**⏱ PROCENJENO VREME:** 5 dana

