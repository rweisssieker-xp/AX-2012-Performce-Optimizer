# Demo Guide - AX 2012 Performance Optimizer

## Anwendung starten

```powershell
# Aus dem Build-Verzeichnis
.\AX2012PerformanceOptimizer.WpfApp\bin\Debug\net8.0-windows\AX2012PerformanceOptimizer.WpfApp.exe

# Oder mit dotnet run
dotnet run --project AX2012PerformanceOptimizer.WpfApp/AX2012PerformanceOptimizer.WpfApp.csproj
```

## Vollständige Feature-Tour

### 1. 🏠 Dashboard Tab

**Zweck**: Schneller Überblick über Systemmetriken

**Features**:
- 4 farbcodierte Metrik-Karten:
  - 👥 **Blau**: Active Users - Anzahl aktiver Benutzer-Sessions
  - ⏱️ **Orange**: Running Jobs - Laufende Batch-Jobs
  - 💾 **Lila**: Database (MB) - Datenbankgröße
  - ⚠️ **Rot**: Expensive Queries - Anzahl langsamer Queries

**Aktionen**:
- 🔄 **Load Data**: Lädt aktuelle Metriken
- 🔃 **Refresh**: Aktualisiert Daten
- ⏳ **Loading Indicator**: Zeigt Ladezustand

**Demo ohne SQL Server**:
- Metrik-Karten zeigen 0 (keine Verbindung)
- Status-Nachricht zeigt Fehler wenn nicht verbunden
- UI funktioniert vollständig

---

### 2. ⚙️ Settings Tab

**Zweck**: Connection Profile Management

**Linke Seite - Profile Liste**:
- Zeigt alle gespeicherten Verbindungsprofile
- **➕ New Profile**: Erstellt neues Profil

**Rechte Seite - Profile Editor**:
- **Profile Name**: Beschreibender Name (z.B. "Production AX")
- **SQL Server Name**: Server\Instance Name
- **Database Name**: AX Datenbank Name
- **☑️ Use Windows Authentication**: Windows Auth vs. SQL Auth
- **Username**: SQL Benutzername (wenn nicht Windows Auth)
- **AOS Server Name**: AOS Server Hostname
- **AOS Port**: Standard 2712
- **Company**: Standard "DAT"
- **☑️ Set as Default Profile**: Standard-Profil markieren

**Aktionen**:
- 💾 **Save**: Profil speichern (mit DPAPI-verschlüsseltem Passwort)
- 🔌 **Test Connection**: Verbindung testen
- ✅ **Connect**: Verbindung aktivieren
- 🗑️ **Delete**: Profil löschen

**Demo ohne SQL Server**:
1. Klicken Sie auf "➕ New Profile"
2. Geben Sie Dummy-Daten ein:
   - Name: "Demo Profile"
   - SQL Server: "localhost"
   - Database: "TestDB"
3. Klicken Sie "💾 Save"
4. Profil wird in Liste angezeigt
5. Klicken Sie "🔌 Test Connection" → Zeigt Fehler (normal ohne Server)

**Gespeicherte Konfiguration**:
```
%LocalAppData%\AX2012PerformanceOptimizer\profiles.json
```

---

### 3. 📈 SQL Performance Tab

**Zweck**: SQL Query Performance Analyse

**Features**:
- **Top Expensive Queries Liste**:
  - Query-Text (erste Zeilen, gekürzt)
  - ⏱️ CPU Time (ms)
  - 🔄 Execution Count
  - 📖 Logical Reads
  - Farbcodierte Badges für Metriken

- **Query Details** (bei Selektion):
  - Vollständiger Query-Text in Consolas Font
  - Alle Statistiken

**Aktionen**:
- 🔄 **Refresh**: Queries neu laden
- 📥 **Export**: Daten exportieren (deaktiviert in Demo)

**Datenquelle**:
```sql
-- Aus SQL Server DMV
SELECT * FROM sys.dm_exec_query_stats
CROSS APPLY sys.dm_exec_sql_text(sql_handle)
ORDER BY total_worker_time DESC
```

**Demo ohne SQL Server**:
- Liste bleibt leer
- Refresh-Button funktioniert
- UI ist vollständig vorhanden

---

### 4. 🖥️ AOS Monitoring Tab

**Zweck**: AOS Server Health Monitoring

**Features**:
- **AOS Metrics Card** (blau):
  - Active Sessions
  - Server Name  
  - Health Status (Healthy/Unhealthy)

- **Active User Sessions Liste**:
  - User ID
  - Client Computer
  - Login DateTime
  - Echtzeit-Überwachung

**Aktionen**:
- 🔄 **Refresh**: Daten aktualisieren

**Datenquelle**:
```sql
-- AX Tabelle
SELECT * FROM SYSCLIENTSESSIONS WHERE STATUS = 1
```

**Demo ohne SQL Server**:
- Metrics zeigen Standardwerte
- Sessions-Liste bleibt leer
- UI vollständig funktionsfähig

---

### 5. ⏱️ Batch Jobs Tab

**Zweck**: Batch Job Monitoring

**Features**:
- **Running Batch Jobs** (orange Card):
  - Job Description
  - Status
  - Company
  - Start DateTime
  - Echtzeitüberwachung

- **Failed Batch Jobs** (rote Card):
  - Job Description (in rot)
  - Status
  - Company  
  - Start DateTime
  - Fehleranalyse

**Aktionen**:
- 🔄 **Refresh**: Jobs aktualisieren

**Datenquellen**:
```sql
-- Running Jobs
SELECT * FROM BATCHJOB WHERE STATUS IN (1, 4)

-- Failed Jobs
SELECT * FROM BATCHJOBHISTORY WHERE STATUS = 2
```

**Demo ohne SQL Server**:
- Beide Listen leer
- UI zeigt Platzhalter korrekt an

---

### 6. 💾 Database Health Tab

**Zweck**: Datenbank-Gesundheit überwachen

**Features**:
- **Database Metrics Card** (lila):
  - Total Size (MB)
  - Data Size (MB)
  - Log Size (MB)

- **3 Sub-Tabs**:

  **📊 Top Tables Tab**:
  - Schema.Tabelle Name
  - Row Count
  - Total Space (KB)
  - Sortiert nach Größe

  **🔧 Fragmented Indexes Tab**:
  - Tabelle.Index Name
  - Fragmentierung % (orange hervorgehoben)
  - Page Count
  - Nur Indexes > 30% Fragmentierung

  **➕ Missing Indexes Tab**:
  - Tabellen-Name
  - Empfohlene Spalten
  - 💥 Impact Score (Wichtigkeit)
  - Sortiert nach Impact

**Aktionen**:
- 🔄 **Refresh**: Alle Metriken neu laden

**Datenquellen**:
```sql
-- Fragmentierte Indexes
SELECT * FROM sys.dm_db_index_physical_stats(...)

-- Missing Indexes
SELECT * FROM sys.dm_db_missing_index_details

-- Top Tables
SELECT * FROM sys.tables, sys.partitions...
```

**Demo ohne SQL Server**:
- Alle Listen leer
- Tabs vollständig funktional
- Metrik-Karten zeigen 0

---

### 7. 💡 Recommendations Tab

**Zweck**: Intelligente Optimierungsvorschläge

**Features**:

**Linke Seite - Recommendations Liste**:
- **Priority Badge** (Farbe nach Priorität):
  - Critical (rot)
  - High (orange)
  - Medium (gelb)
  - Low (grau)
- **Category Badge**: Kategorie-Tag
- **Title**: Kurze Beschreibung
- **Description**: Detaillierte Erklärung

**Rechte Seite - Details Panel**:
- **Priority**: Priorität der Empfehlung
- **Category**: Kategorie (SQL, Index, Batch, etc.)
- **Description**: Ausführliche Beschreibung
- **Impact Analysis**: Auswirkungsanalyse
- **Action Script**: SQL-Script zum Umsetzen (Consolas Font)

**Aktionen**:
- 🔄 **Refresh**: Recommendations neu generieren
- 📋 **Copy Script**: SQL-Script in Zwischenablage
- ✅ **Mark Implemented**: Als umgesetzt markieren

**Empfehlungs-Kategorien**:
1. SQL Query Optimization
2. Index Management
3. Statistics Update
4. Batch Job Scheduling
5. AOS Configuration
6. Database Maintenance
7. Memory Optimization
8. Storage Optimization

**Demo ohne SQL Server**:
- Liste bleibt leer (keine Daten zum Analysieren)
- Klicken Sie "🔄 Refresh" um Analysis zu starten
- Bei Verbindung werden automatisch Recommendations generiert

**Beispiel-Recommendation** (bei Verbindung):
```
Title: High CPU Query Detected
Priority: Critical
Category: SqlQueryOptimization
Description: Query with average CPU time of 5234.56ms needs optimization.
Impact: This query has been executed 10000 times...
Script: -- Review and optimize this query:
        SELECT * FROM INVENTTRANS WHERE...
```

---

## Workflow für echte Nutzung

### Initial Setup (einmalig):

1. **Settings Tab öffnen**
2. **➕ New Profile** klicken
3. **Verbindungsdaten eingeben**:
   ```
   Profile Name: Production AX 2012
   SQL Server: SQLPROD\AX2012
   Database: MicrosoftDynamicsAX
   ✓ Windows Authentication
   AOS Server: AOSPROD
   Port: 2712
   Company: DAT
   ✓ Set as Default
   ```
4. **💾 Save** klicken
5. **🔌 Test Connection** (optional)
6. **✅ Connect** klicken

### Tägliche Nutzung:

1. **App starten**
2. **Dashboard Tab**:
   - 🔄 Load Data klicken
   - Metriken überprüfen
   - Bei Auffälligkeiten → Details ansehen

3. **SQL Performance Tab**:
   - Top expensive queries überprüfen
   - Problematische Queries identifizieren
   - Query-Details analysieren

4. **Database Health Tab**:
   - 🔧 Fragmented Indexes überprüfen
   - ➕ Missing Indexes prüfen
   - Bei hoher Fragmentierung → Wartung planen

5. **Recommendations Tab**:
   - 🔄 Refresh klicken
   - Critical/High Priority durchgehen
   - 📋 Script kopieren
   - In SQL Server Management Studio ausführen
   - ✅ Mark Implemented

6. **Batch Jobs Tab**:
   - ❌ Failed Jobs prüfen
   - Bei Fehlern → Logs untersuchen

---

## Demo-Szenarien ohne echten AX Server

### Szenario 1: UI-Navigation testen

```
1. App starten
2. Alle 7 Tabs durchklicken
3. Beobachten: Jeder Tab hat vollständige UI
4. Keine "Coming Soon" Meldungen mehr!
```

### Szenario 2: Profile Management testen

```
1. Settings Tab
2. ➕ New Profile
3. Daten eingeben (beliebig)
4. 💾 Save
5. Neues Profil in Liste
6. Anderes Profil erstellen
7. Zwischen Profilen wechseln
8. 🗑️ Delete testen
```

### Szenario 3: UI-Elemente testen

```
Dashboard:
- Load Data Button (zeigt Fehler - normal)
- Refresh Button
- Status Messages

SQL Performance:
- Refresh Button
- Export Button (disabled)
- Leere Liste angezeigt

Recommendations:
- Alle Buttons vorhanden
- Details-Panel leer
```

---

## Erweiterte Nutzung (mit AX Server)

### Performance-Analyse-Workflow

**Woche 1 - Baseline erstellen**:
```
1. Täglich Dashboard checken
2. SQL Performance Metriken sammeln
3. Database Health einmal prüfen
4. Baseline-Werte notieren
```

**Woche 2-4 - Monitoring**:
```
1. Tägliche Checks
2. Trends beobachten
3. Auffälligkeiten notieren
4. Recommendations sammeln
```

**Wartungsfenster**:
```
1. Recommendations Tab öffnen
2. Alle Critical/High durchgehen
3. Scripts kopieren
4. In SSMS ausführen
5. Als Implemented markieren
6. Nach Wartung: Metriken neu prüfen
```

### Typische Performance-Probleme die erkannt werden:

1. **Langsame Queries**:
   - Angezeigt in SQL Performance Tab
   - Empfehlung: Query Optimierung oder Index

2. **Index Fragmentierung**:
   - Database Health → Fragmented Indexes
   - Empfehlung: REBUILD oder REORGANIZE Script

3. **Missing Indexes**:
   - Database Health → Missing Indexes Tab
   - Empfehlung: CREATE INDEX Script mit Impact Score

4. **Failed Batch Jobs**:
   - Batch Jobs → Failed Jobs Liste
   - Empfehlung: Job-Scheduling überprüfen

5. **Große Datenbank**:
   - Dashboard → Database Size
   - Empfehlung: Archivierung älterer Daten

---

## Keyboard Shortcuts

- **Ctrl+R**: Refresh current view (wenn fokussiert)
- **Ctrl+C**: Copy (bei Text-Selektion)
- **Ctrl+Tab**: Nächster Tab
- **Ctrl+Shift+Tab**: Vorheriger Tab

---

## Troubleshooting während Demo

### Problem: "Connection string has not been set"

**Lösung**: 
```
1. Settings Tab öffnen
2. Profil erstellen oder auswählen
3. ✅ Connect klicken
4. Zurück zu anderem Tab
5. Refresh/Load Data
```

### Problem: Leere Listen

**Normal wenn**:
- Keine SQL Server Verbindung
- Keine Daten in AX Tabellen
- Queries returnen 0 Ergebnisse

**Prüfen**:
```
1. Settings → Test Connection
2. Bei Erfolg: Datenbank könnte leer sein
3. Bei Fehler: Verbindungsdaten prüfen
```

### Problem: App startet nicht

**Lösung**:
```powershell
# Neu kompilieren
dotnet build AX2012PerformanceOptimizer.WpfApp/AX2012PerformanceOptimizer.WpfApp.csproj

# Clean build
dotnet clean
dotnet build

# Prozesse beenden
Get-Process | Where-Object {$_.ProcessName -like "*AX2012*"} | Stop-Process -Force
```

---

## Demo-Präsentation Script

### Einführung (2 Min)

```
"Das ist der AX 2012 Performance Optimizer - eine native Windows-Anwendung 
zur Überwachung und Optimierung von Microsoft Dynamics AX 2012 R3 CU13 
Systemen mit SQL Server 2016.

Die Anwendung bietet:
- Echtzeit Performance-Monitoring
- Grafische Visualisierungen
- Intelligente Optimierungsvorschläge
- Vollständig portable - keine Installation nötig"
```

### Dashboard Demo (3 Min)

```
[Dashboard Tab zeigen]

"Hier sehen Sie das Dashboard mit 4 Hauptmetriken:
- Active Users: Anzahl aktiver AX-Benutzer
- Running Jobs: Laufende Batch-Jobs  
- Database Size: Datenbankgröße in MB
- Expensive Queries: Anzahl problematischer Queries

Mit einem Klick auf 'Load Data' werden alle Metriken aktualisiert."

[Load Data klicken]
```

### Settings Demo (3 Min)

```
[Settings Tab zeigen]

"Im Settings-Bereich verwalten Sie Connection Profiles.
Sie können mehrere Profile für verschiedene Umgebungen haben:
- Entwicklung
- Test
- Produktion

Die Passwörter werden mit Windows DPAPI verschlüsselt gespeichert.

[New Profile erstellen]
[Felder ausfüllen]
[Save klicken]

Profile werden hier gespeichert:
%LocalAppData%\AX2012PerformanceOptimizer\profiles.json"
```

### SQL Performance Demo (3 Min)

```
[SQL Performance Tab zeigen]

"Dieser Bereich zeigt die teuersten SQL Queries:
- Sortiert nach CPU Time
- Mit Execution Count
- Logical/Physical Reads

Bei Problemen können Sie direkt den Query-Text sehen
und Optimierungen vornehmen."
```

### Database Health Demo (3 Min)

```
[Database Health Tab zeigen]

"Hier überwachen Sie die Datenbank-Gesundheit:

[Top Tables Tab]
Größte Tabellen - hilft bei Archivierungs-Planung

[Fragmented Indexes Tab]
Indexes mit hoher Fragmentierung
→ Automatic REBUILD/REORGANIZE Scripts

[Missing Indexes Tab]
SQL Server empfiehlt fehlende Indexes
→ CREATE INDEX Scripts mit Impact Score"
```

### Recommendations Demo (4 Min)

```
[Recommendations Tab zeigen]

"Das Herz der Anwendung: Intelligente Recommendations.

Die Engine analysiert:
- SQL Queries
- Index-Zustand
- Batch Jobs
- Datenbankgröße

Und generiert priorisierte Empfehlungen:

[Beispiel zeigen]
- Priority: Critical/High/Medium/Low
- Category: SQL Optimization, Index Management, etc.
- Impact Analysis: Was wird verbessert
- Action Script: Fertiges SQL-Script

[Copy Script klicken]
'Script ist jetzt in Zwischenablage - direkt in SSMS einfügen'

[Mark as Implemented]
'Tracking umgesetzter Empfehlungen'"
```

### Abschluss (2 Min)

```
"Zusammenfassung:
✅ Native Windows App - keine Installation
✅ Alle AX 2012 Performance-Bereiche abgedeckt
✅ Echtzeit-Monitoring
✅ Actionable Recommendations
✅ Sichere Konfiguration
✅ Komplett mit dotnet CLI baubar

Die Anwendung ist produktionsbereit und kann sofort 
mit Ihrem AX 2012 R3 CU13 System verwendet werden."
```

---

## Features Checklist

### Implementiert ✅:
- ✅ Dashboard mit 4 Metriken
- ✅ Settings/Profile Management
- ✅ SQL Performance Monitoring
- ✅ AOS Monitoring
- ✅ Batch Jobs Tracking
- ✅ Database Health (Tables, Indexes, Missing Indexes)
- ✅ Recommendations Engine
- ✅ Verschlüsselte Passwörter (DPAPI)
- ✅ JSON Configuration
- ✅ Alle 7 Monitoring Services
- ✅ MVVM Pattern
- ✅ Dependency Injection
- ✅ Moderne WPF UI

### Für Produktion vorbereitet ✅:
- ✅ Fehlerbehandlung
- ✅ Logging (Microsoft.Extensions.Logging)
- ✅ Read-only SQL Operations
- ✅ Sichere Verbindungen
- ✅ Portable Konfiguration

### Optional (Future):
- ⏳ Charts/Graphs (LiveCharts2 eingebunden)
- ⏳ Export zu Excel/PDF
- ⏳ Email Alerts
- ⏳ Historical Trending

---

## Nächste Schritte

### Für Entwickler:
```powershell
# Solution öffnen
code AX2012PerformanceOptimizer.sln

# Oder in Visual Studio
start AX2012PerformanceOptimizer.sln

# Tests hinzufügen
dotnet new xunit -n AX2012PerformanceOptimizer.Tests
```

### Für Deployment:
```powershell
# Release Build erstellen
dotnet publish AX2012PerformanceOptimizer.WpfApp/AX2012PerformanceOptimizer.WpfApp.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output ./publish `
  /p:PublishSingleFile=true

# Ergebnis:
# ./publish/AX2012PerformanceOptimizer.WpfApp.exe (ca. 100-150 MB)
```

### Für Endbenutzer:
```
1. AX2012PerformanceOptimizer.WpfApp.exe auf Desktop kopieren
2. Doppelklick zum Starten
3. Settings → Verbindung konfigurieren
4. Dashboard → Monitoring starten
```

---

## Support & Dokumentation

- **README.md**: Hauptdokumentation mit allen Features
- **DEPLOYMENT.md**: Deployment-Anleitung für IT
- **DEVELOPER_GUIDE.md**: Entwickler-Onboarding
- **IMPLEMENTATION_SUMMARY.md**: Technische Details

---

**Viel Erfolg mit dem AX 2012 Performance Optimizer!** 🚀

Bei Fragen: GitHub Issues oder E-Mail an Support-Team.


