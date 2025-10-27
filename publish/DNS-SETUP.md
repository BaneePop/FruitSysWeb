# DNS SETUP - FRUITSYS.RS

## KORAK 1: PRONALAŽENJE IP ADRESE SERVERA

**Na Windows 11 serveru (računaru koji će biti server), otvorite PowerShell:**

```powershell
# Pronađite vašu javnu IP adresu
(Invoke-WebRequest -Uri "https://api.ipify.org").Content
```

**ZAPISITE OVU IP ADRESU!** Npr: `123.45.67.89`

---

## KORAK 2: PRISTUP REGISTRARU DOMENA

Prijavite se na sajt gde ste kupili domen **fruitsys.rs**

Najčešći registrari za .rs domene:
- rnids.rs
- domains.rs
- loopia.rs
- superdomain.rs

---

## KORAK 3: DNS ZONA / DNS UPRAVLJANJE

Pronađite opciju:
- "DNS Zona" ili
- "DNS Management" ili
- "Upravljanje DNS-om" ili
- "Nameservers"

---

## KORAK 4: DODAVANJE DNS ZAPISA

### Dodajte PRVI zapis:

```
Tip/Type:      A Record
Host/Ime:      @        (ili ostavite prazno, ili "fruitsys.rs")
Value/Vrednost: VASA_IP_ADRESA_IZ_KORAKA_1
TTL:           3600     (ili ostavite default)
```

**Primer:**
```
Type: A
Host: @
Value: 123.45.67.89
TTL: 3600
```

### Dodajte DRUGI zapis:

```
Tip/Type:      A Record
Host/Ime:      www
Value/Vrednost: VASA_IP_ADRESA_IZ_KORAKA_1
TTL:           3600
```

**Primer:**
```
Type: A
Host: www
Value: 123.45.67.89
TTL: 3600
```

---

## KORAK 5: SAČUVAJTE IZMENE

Kliknite na **"Save"** / **"Sačuvaj"** / **"Apply Changes"**

---

## KORAK 6: ČEKANJE DNS PROPAGACIJE

DNS propagacija može trajati:
- **Minimum:** 1 sat
- **Prosek:** 2-4 sata
- **Maksimum:** 24-48 sati

---

## PROVERA DNS PROPAGACIJE

### Opcija 1: Online alat
Idite na: https://www.whatsmydns.net/
- Unesite: **fruitsys.rs**
- Proverite da li se prikazuje vaša IP adresa

### Opcija 2: PowerShell komanda

```powershell
nslookup fruitsys.rs
nslookup www.fruitsys.rs
```

Trebali bi videti vašu IP adresu.

---

## FINALNA PROVERA

Kada DNS propagacija bude gotova, testirajte:

```powershell
# Test ping
ping fruitsys.rs
ping www.fruitsys.rs

# Test DNS resolve
Resolve-DnsName fruitsys.rs
Resolve-DnsName www.fruitsys.rs
```

**Ako sve radi, možete nastaviti sa SSL instalacijom!**

---

## PRIMER - KAKO IZGLEDA U PRAKSI

### rnids.rs panel:
```
┌─────────────────────────────────────────────────┐
│ DNS Zona za: fruitsys.rs                        │
├─────────────────────────────────────────────────┤
│ Tip    │ Host │ Vrednost      │ TTL             │
├─────────────────────────────────────────────────┤
│ A      │ @    │ 123.45.67.89  │ 3600            │
│ A      │ www  │ 123.45.67.89  │ 3600            │
└─────────────────────────────────────────────────┘
```

### domains.rs panel:
```
Add DNS Record:
┌──────────────────────────┐
│ Type:     [A Record ▼]   │
│ Name:     [@]            │
│ IP:       [123.45.67.89] │
│ TTL:      [3600]         │
│                          │
│      [Add Record]        │
└──────────────────────────┘

Add DNS Record:
┌──────────────────────────┐
│ Type:     [A Record ▼]   │
│ Name:     [www]          │
│ IP:       [123.45.67.89] │
│ TTL:      [3600]         │
│                          │
│      [Add Record]        │
└──────────────────────────┘
```

---

## TROUBLESHOOTING

### Problem: "Moja IP adresa se stalno menja"

**Rešenje 1:** Kontaktirajte ISP (internet provajdera) i tražite **statičnu IP adresu**
- Može biti besplatno ili uz malu nadoplatu (~5-10 EUR/mesečno)

**Rešenje 2:** Koristite **Dynamic DNS** servis
- No-IP.com (besplatno)
- DynDNS.com
- afraid.org

### Problem: "Ne znam gde je DNS panel kod registrara"

**Kontaktirajte podršku registrara** i pitajte:
> "Gde mogu podesiti A Record za moj domen fruitsys.rs?"

### Problem: "DNS propagacija ne radi nakon 24h"

Proverite:
1. Da li ste upisali **tačnu IP adresu** (bez razmaka)
2. Da li ste **sačuvali** izmene kod registrara
3. Da li registrar koristi **svoje nameservere** ili koristite eksterne (npr. CloudFlare)
4. Pokušajte komandu: `nslookup fruitsys.rs 8.8.8.8` (proverava preko Google DNS-a)

---

## DODATNA PODEŠAVANJA (OPCIONO)

### Za bolje performanse, možete dodati:

**CAA Record** (za Let's Encrypt):
```
Type: CAA
Host: @
Value: 0 issue "letsencrypt.org"
```

**MX Record** (samo ako planirate email):
```
Type: MX
Host: @
Priority: 10
Value: mail.fruitsys.rs
```

---

## VAŽNA NAPOMENA O ROUTER-U

### Ako je vaš Windows 11 server IZA ROUTER-A:

1. **Port Forwarding** - morate podesiti na router-u:
   ```
   External Port 80  → Internal IP servera : Port 80
   External Port 443 → Internal IP servera : Port 443
   ```

2. **Pronađite internu IP adresu Windows 11 servera:**
   ```powershell
   ipconfig
   # Tražite "IPv4 Address" - npr: 192.168.1.100
   ```

3. **Pristupite router-u:**
   - Obično: http://192.168.1.1 ili http://192.168.0.1
   - Login: admin / admin (ili pogledajte nalepnicu na router-u)

4. **Pronađite "Port Forwarding" sekciju**
   - Može biti u: Advanced → NAT → Port Forwarding
   - Dodajte pravila za portove 80 i 443

**Primer konfiguracije:**
```
Servis:       HTTP
External Port: 80
Internal IP:   192.168.1.100  (IP Windows 11 servera)
Internal Port: 80
Protocol:      TCP

Servis:       HTTPS
External Port: 443
Internal IP:   192.168.1.100
Internal Port: 443
Protocol:      TCP
```

---

## NAKON USPEŠNE DNS KONFIGURACIJE

✅ DNS je konfigurisan
✅ DNS propagacija je završena
✅ ping fruitsys.rs radi

**SLEDEĆI KORAK:**
- Nastavite sa deploymentom aplikacije
- Pogledajte: **BRZI-START.md** ili **DEPLOYMENT-UPUTSTVA.md**

---

**Sreća!** 🚀
