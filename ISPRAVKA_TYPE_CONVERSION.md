# 🔧 ISPRAVKA - Type Conversion Error (CS1503)

## ❌ **PROBLEM:**
```
Argument 1: cannot convert from 'long' to 'int'
Line 40, Column 100-110
```

**Uzrok:** `Artikal.Id` je tipa `long`, a koristili smo `int` u `izabraniArtikliIds`.

---

## ✅ **REŠENJE:**

### **1. RobaNaZalihamaModel.cs**
```diff
- public int ArtikalID { get; set; }
+ public long ArtikalID { get; set; }
```

### **2. IFinansijeService.cs**
```diff
- Task<List<RobaNaZalihamaModel>> UcitajRobuNaZalihama(List<int> artikalIds, ...)
+ Task<List<RobaNaZalihamaModel>> UcitajRobuNaZalihama(List<long> artikalIds, ...)
```

### **3. FinansijeService.cs**
```diff
- public async Task<List<RobaNaZalihamaModel>> UcitajRobuNaZalihama(List<int> artikalIds, ...)
+ public async Task<List<RobaNaZalihamaModel>> UcitajRobuNaZalihama(List<long> artikalIds, ...)

// Cast-ovi:
- ArtikalID = (int)nabavka.ArtikalID
+ ArtikalID = Convert.ToInt64(nabavka.ArtikalID)

- var prodaja = prodajaData.FirstOrDefault(p => (int)p.ArtikalID == model.ArtikalID)
+ var prodaja = prodajaData.FirstOrDefault(p => Convert.ToInt64(p.ArtikalID) == model.ArtikalID)

- var lager = lagerData.FirstOrDefault(l => (int)l.ArtikalID == model.ArtikalID)
+ var lager = lagerData.FirstOrDefault(l => Convert.ToInt64(l.ArtikalID) == model.ArtikalID)
```

### **4. RobaNaZalihama.razor**
```diff
- private List<int> izabraniArtikliIds = new List<int>();
+ private List<long> izabraniArtikliIds = new List<long>();

- .Select(v => int.TryParse(v, out var id) ? id : 0)
+ .Select(v => long.TryParse(v, out var id) ? id : 0)

- private void IzaberiPoTipu(int artikalId)
+ private void IzaberiPoTipu(long artikalId)
```

---

## ✅ **IZMENJENI FAJLOVI:**
1. `/Users/Bane/FruitSysWeb/Models/RobaNaZalihamaModel.cs`
2. `/Users/Bane/FruitSysWeb/Services/Interfaces/IFinansijeService.cs`
3. `/Users/Bane/FruitSysWeb/Services/Implementations/IzvestajService/FinansijeService.cs`
4. `/Users/Bane/FruitSysWeb/Components/Shared/RobaNaZalihama.razor`

---

## 🧪 **TEST:**
```bash
dotnet build
```

**Očekivano:** Build succeeds bez grešaka ✅

---

**🎉 Greška ispravljena! Možeš nastaviti sa testiranjem!**
