# FusionSolar API — Uputstvo za integraciju u FruitSysWeb

**Kontekst:** Bane ima Owner nalog na FusionSolar cloud portalu (postrojenje **Odetta**, Logger SN: **102536559502**). Cilj je da FruitSysWeb (.NET) čita real-time podatke, KPI i alarme sa postrojenja i upisuje ih u `fruitsysdb_v2`.

**Napomena:** Endpoint-i i strukture ispod su rekonstruisani iz stvarnih HTTP zapisa FusionSolar mobilne aplikacije (iOS iCleaner) koji koriste **isti nalog**. Ovo NIJE zvanični Northbound OpenAPI — ovo je interni "phoneapp/pvms" API. Radi, ali može se menjati bez najave. Za produkciju idealno migrirati na zvanični OpenAPI kad Frucom pošalje kredencijale.

---

## 1. Osnovni podaci

| Parametar | Vrednost |
|-----------|----------|
| **Region host** | `uni005eu5.fusionsolar.huawei.com` |
| **Port** | `32800` (HTTPS) |
| **Base URL** | `https://uni005eu5.fusionsolar.huawei.com:32800` |
| **Login user** | Odetta_sabac (Owner) |
| **Postrojenje** | Odetta |
| **Uređaji** | 3× Inverter (SUN2000-110KTL-M2, SUN2000-110KTL-M2, SUN2000-100KTL-M2) + 1× PowerMeter |
| **Logger SN** | 102536559502 |

---

## 2. Autentifikacija

FusionSolar phoneapp API koristi **cookie-based session** sa `JSESSIONID` i CSRF tokenom (`roarand`).

### Korak 1: Login

```
POST https://uni005eu5.fusionsolar.huawei.com:32800/rest/dpcloud/auth/v1/login
Content-Type: application/json

{
  "userName": "Odetta_sabac",
  "value": "<sifra>",
  "grantType": "password"
}
```

Server vraća `Set-Cookie: JSESSIONID=...; XSRF-TOKEN=...` — moraš ih čuvati i slati sa svakim narednim zahtevom.

### Korak 2: Osnovna korisnička provera

```
GET /rest/neteco/phoneapp/v1/datacenter/getuserdetailinfo
GET /rest/neteco/phoneapp/v2/common/commoner   ← lista podržanih feature-a
```

### Održavanje sesije

- TCP heartbeat / session refresh na svakih 3 minuta
- Ako sesija istekne → server vraća `redirect` na `/relogin.asp` → ponovi login

---

## 3. Ključni endpoint-i za FruitSysWeb

### 3.1 Lista postrojenja

```
POST /rest/pvms/web/station/v1/station/station-list
Body: {} ili {"pageNo":1,"pageSize":20}
```

**Odgovor:**
```json
{
  "data": {
    "pageCount": 1,
    "total": 1,
    "pageNo": 1,
    "list": [
      {
        "plantType": 2,
        "plantAddress": "...",
        "buildState": "null",
        "name": "Odetta",
        "plantStatus": "connected",
        "stationDn": "NE=182162915"
      }
    ]
  }
}
```

**Zapamti `stationDn`** — koristi se za sve dalje pozive.

### 3.2 Real-time KPI (najvažniji endpoint)

```
GET /rest/pvms/web/station/v1/station/total-real-kpi?stationDn=NE=182162915
```

**Odgovor:**
```json
{
  "data": {
    "currentPower": "65.943",       // trenutna snaga u kW
    "dailyEnergy": "241.5",          // proizvedeno danas u kWh
    "cumulativeEnergy": "273931.31", // ukupno od instalacije u kWh
    "dailyIncome": "24.4"            // prihod danas
  }
}
```

**Ovo je glavni endpoint za dashboard.** Pool na 5 min.

### 3.3 KPI dnevni chart

```
GET /rest/pvms/web/station/v1/overview/station-kpi-data?stationDn=NE=182162915
GET /rest/pvms/web/report/v1/station/home-station-kpi-chart?stationDn=NE=182162915&queryTime=<epoch_ms>&timeDim=2
```

`timeDim`: 2=dan, 3=mesec, 4=godina, 5=lifetime

### 3.4 Društveni doprinos (CO₂ redukcija, ekvivalent stabala)

```
GET /rest/pvms/web/station/v1/station/social-contribution?stationDn=NE=182162915
```

**Odgovor:**
```json
{
  "data": {
    "co2Reduction": 106176.87,
    "co2ReductionByYear": 50176.5,
    "equivalentTreePlanting": 146,
    "equivalentTreePlantingByYear": 69,
    "standardCoalSavings": 89412.10,
    "standardCoalSavingsByYear": 42253.90
  }
}
```

### 3.5 Lista uređaja (invertera, meter-a, logger-a)

```
GET /rest/neteco/web/config/device/v1/device-list?parentDn=<stationDn>
```

Vraća listu uređaja sa `deviceDn`, `mocId`, `status`. Ključno:

| `mocId` | Tip uređaja |
|---------|-------------|
| 20812 | Postrojenje (station) |
| 20816 | PowerMeter |
| 20821 | SmartLogger |
| 20822 | Inverter (SUN2000) |
| 90001 | Grid |
| 90002 | Consumer/Load |

### 3.6 Real-time signali sa uređaja

```
GET /rest/pvms/web/device/v1/device-signals?deviceDn=<deviceDn>
GET /rest/pvms/web/device/v1/deviceExt/get-device-signals?deviceDn=<deviceDn>
```

**Odgovor:**
```json
{
  "data": [
    {"id": 11008, "value": "1234.5", "unit": "W"},
    {"id": 11007, "value": "230.1", "unit": "V"}
  ]
}
```

`id` je Huawei signal ID. Mapiranje najbitnijih za SUN2000 inverter:

| Signal ID | Značenje | Jedinica |
|-----------|----------|----------|
| 10025 | Active Power | kW |
| 10029 | Daily Energy | kWh |
| 10032 | Total Energy | kWh |
| 10034 | Efficiency | % |
| 10037 | Inverter Temperature | °C |
| 11007 | Voltage (Grid) | V |
| 11008 | Current (Grid) | A |
| 11015 | Frequency | Hz |

*(Napomena: kompletno mapiranje pošalje Frucom sa Modbus register map dokumentom; ove su najčešće korišćene.)*

### 3.7 Istorijski podaci uređaja

```
GET /rest/pvms/web/device/v1/device-history-data?deviceDn=<deviceDn>&signalId=<id>&startTime=<epoch>&endTime=<epoch>
```

Za izveštaje po danu/mesecu.

### 3.8 Energetski tok (flow)

```
GET /rest/pvms/web/station/v3/overview/energy-flow?stationDn=<stationDn>
GET /rest/pvms/web/station/v3/overview/energy-balance?stationDn=<stationDn>
```

Vraća graf PV → Inverter → Grid/Load sa smerovima toka.

### 3.9 Status uređaja (broj Connected/Disconnected/Trouble)

```
GET /rest/pvms/web/station/v1/station/station-status-count?stationDn=<stationDn>
```

**Odgovor:**
```json
{
  "data": {
    "trouble": 0,
    "disconnected": 0,
    "connected": 4,
    "total": 4
  }
}
```

Koristi za alarme u FruitSysWeb.

---

## 4. Preporučena arhitektura za FruitSysWeb

```
┌─────────────────────┐         ┌──────────────────────────┐
│  FruitSysWeb (.NET) │────────▶│  FusionSolarClient       │
│                     │         │  (RestSharp/HttpClient)  │
└─────────────────────┘         └──────────────────────────┘
         │                                  │
         │                                  ▼
         │                    ┌──────────────────────────────┐
         │                    │  uni005eu5.fusionsolar        │
         │                    │  .huawei.com:32800            │
         │                    └──────────────────────────────┘
         ▼
┌─────────────────────┐
│  fruitsysdb_v2      │
│  (MySQL)            │
│  - solar_kpi        │
│  - solar_devices    │
│  - solar_signals    │
│  - solar_alarms     │
└─────────────────────┘
```

**Predlog šeme (za novu tabelu u `fruitsysdb_v2`):**

```sql
CREATE TABLE solar_kpi (
  id INT AUTO_INCREMENT PRIMARY KEY,
  station_dn VARCHAR(50),
  timestamp DATETIME,
  current_power DECIMAL(10,3),
  daily_energy DECIMAL(10,3),
  cumulative_energy DECIMAL(12,3),
  daily_income DECIMAL(10,2),
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_station_time (station_dn, timestamp)
);

CREATE TABLE solar_devices (
  id INT AUTO_INCREMENT PRIMARY KEY,
  device_dn VARCHAR(100),
  device_name VARCHAR(100),
  device_type VARCHAR(50),
  serial_number VARCHAR(50),
  moc_id INT,
  status VARCHAR(20),
  updated_at DATETIME
);

CREATE TABLE solar_signals (
  id INT AUTO_INCREMENT PRIMARY KEY,
  device_dn VARCHAR(100),
  signal_id INT,
  signal_value VARCHAR(50),
  unit VARCHAR(20),
  timestamp DATETIME,
  INDEX idx_dev_sig_time (device_dn, signal_id, timestamp)
);

CREATE TABLE solar_alarms (
  id INT AUTO_INCREMENT PRIMARY KEY,
  device_dn VARCHAR(100),
  alarm_code VARCHAR(50),
  alarm_name VARCHAR(200),
  severity VARCHAR(20),
  status VARCHAR(20),
  raised_at DATETIME,
  cleared_at DATETIME NULL
);
```

---

## 5. .NET C# skeleton (za Claude Code)

```csharp
public class FusionSolarClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl = "https://uni005eu5.fusionsolar.huawei.com:32800";
    private CookieContainer _cookies = new();

    public async Task<bool> LoginAsync(string user, string pass)
    {
        var body = new { userName = user, value = pass, grantType = "password" };
        var res = await _http.PostAsJsonAsync($"{_baseUrl}/rest/dpcloud/auth/v1/login", body);
        return res.IsSuccessStatusCode;
    }

    public async Task<StationKpi> GetRealKpiAsync(string stationDn)
    {
        var url = $"{_baseUrl}/rest/pvms/web/station/v1/station/total-real-kpi?stationDn={stationDn}";
        return await _http.GetFromJsonAsync<StationKpiResponse>(url).Data;
    }

    public async Task<List<Device>> GetDevicesAsync(string stationDn) { /* ... */ }
    public async Task<List<Signal>> GetDeviceSignalsAsync(string deviceDn) { /* ... */ }
}
```

**Konfiguracija u `appsettings.json`:**
```json
{
  "FusionSolar": {
    "BaseUrl": "https://uni005eu5.fusionsolar.huawei.com:32800",
    "Username": "Odetta_sabac",
    "Password": "<u user-secrets>",
    "StationDn": "NE=182162915",
    "PollIntervalSeconds": 300
  }
}
```

---

## 6. Redosled implementacije (za Claude Code sesiju)

1. Napravi `FusionSolarClient` klasu sa login-om i cookie persistencijom
2. Testiraj login → očekivan 200 sa `Set-Cookie`
3. Implementiraj `GetStationListAsync` → pokupi `stationDn`
4. Implementiraj `GetRealKpiAsync` — glavni test da čitanje radi
5. Implementiraj `GetDevicesAsync` → pokupi listu uređaja
6. Napravi `SolarPollingService` (BackgroundService) koji svakih 5 min povlači KPI i piše u `solar_kpi`
7. Napravi API endpoint u FruitSysWeb `/api/solar/current` koji čita poslednji red iz `solar_kpi`
8. Dodaj Blazor/HTML komponentu za dashboard

---

## 7. Rizici i napomene

- **Bane:** Endpoint-i nisu zvanično dokumentovani od Huawei-a. Ako Frucom pošalje Northbound OpenAPI creds, prebaci se na njih (`/thirdData/*` endpoints).
- **Bane:** Rate limit — ne pozivaj češće od svakih 60s po endpoint-u da ne bi ban-ovao nalog.
- **Bane:** Login šifru čuvaj u .NET user-secrets ili environment varijablama, ne u kodu.
- **Bane:** Sesija ističe — implementiraj auto-relogin kad server vrati 401 ili redirect na relogin.asp.
- **CSRF token (`roarand`):** neki POST endpoint-i traže ga u `X-CSRF-TOKEN` header-u — čitaj ga iz cookie-ja posle logina.
