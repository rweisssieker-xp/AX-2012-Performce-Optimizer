# Quick Start Guide - AX 2012 Performance Optimizer

## 🚀 Start in 5 Minuten

### Schritt 1: App starten

```powershell
# Executable ausführen
.\AX2012PerformanceOptimizer.WpfApp\bin\Debug\net8.0-windows\AX2012PerformanceOptimizer.WpfApp.exe
```

### Schritt 2: Connection konfigurieren

1. **⚙️ Settings Tab** öffnen
2. **➕ New Profile** klicken
3. Folgende Daten eingeben:

   | Feld | Beispielwert | Beschreibung |
   |------|--------------|--------------|
   | Profile Name | `Production AX` | Beschreibender Name |
   | SQL Server Name | `SQLSERVER\AX2012` | Ihr SQL Server |
   | Database Name | `MicrosoftDynamicsAX` | AX Datenbank |
   | ☑️ Windows Auth | ✓ Aktiviert | Empfohlen |
   | AOS Server | `AOSSERVER` | AX Application Server |
   | AOS Port | `2712` | Standard Port |
   | Company | `DAT` | Ihre Company |

4. **💾 Save** klicken
5. **✅ Connect** klicken

### Schritt 3: Dashboard nutzen

1. **🏠 Dashboard Tab** öffnen
2. **🔄 Load Data** klicken
3. Metriken werden geladen:
   - 👥 Active Users
   - ⏱️ Running Batch Jobs
   - 💾 Database Size
   - ⚠️ Expensive Queries

### Schritt 4: Performance analysieren

**SQL Performance prüfen**:
```
📈 SQL Performance Tab → 🔄 Refresh
→ Top expensive queries sehen
→ Query mit höchster CPU Time auswählen
→ Query-Details analysieren
```

**Database Health prüfen**:
```
💾 Database Health Tab → 🔄 Refresh
→ 🔧 Fragmented Indexes Tab öffnen
→ Indexes > 70% Fragmentierung notieren
→ ➕ Missing Indexes Tab öffnen
→ Highest Impact Score Indexes notieren
```

**Recommendations erhalten**:
```
💡 Recommendations Tab → 🔄 Refresh
→ Critical/High Priority durchgehen
→ Recommendation auswählen
→ 📋 Copy Script klicken
→ In SQL Server Management Studio einfügen
→ Ausführen
→ ✅ Mark Implemented klicken
```

---

## ⚡ Schnellreferenz

### Wichtigste Tabs

| Tab | Zweck | Hauptaktion |
|-----|-------|-------------|
| 🏠 Dashboard | Überblick | Load Data |
| ⚙️ Settings | Verbindung | Connect |
| 📈 SQL Performance | Query-Analyse | Refresh |
| 💾 Database Health | DB-Zustand | Check Fragmentation |
| 💡 Recommendations | Optimierungen | Copy Script |

### Typischer Tagesablauf

**Morgens** (5 Min):
```
1. App starten
2. Dashboard → Load Data
3. Metriken prüfen
```

**Wöchentlich** (15 Min):
```
1. Database Health → Fragmented Indexes
2. SQL Performance → Top Queries
3. Batch Jobs → Failed Jobs
4. Recommendations → Generate & Review
```

**Monatlich** (30 Min):
```
1. Alle Recommendations durchgehen
2. Scripts kopieren und ausführen
3. Vorher/Nachher vergleichen
4. Dokumentieren
```

---

## 🔧 Konfiguration

### Connection String Format

**Windows Authentication** (empfohlen):
```
Server=SQLSERVER\AX2012;
Database=MicrosoftDynamicsAX;
Integrated Security=True;
```

**SQL Authentication**:
```
Server=SQLSERVER\AX2012;
Database=MicrosoftDynamicsAX;
User ID=axmonitor;
Password=****** (verschlüsselt gespeichert)
```

### Minimale SQL Permissions

```sql
-- Read-only Zugriff
ALTER ROLE db_datareader ADD MEMBER [YourUser];

-- DMV Zugriff
USE master;
GRANT VIEW SERVER STATE TO [YourUser];
GRANT VIEW DATABASE STATE TO [YourUser];
```

---

## 📊 Metriken verstehen

### Active Users
- **Was es ist**: Anzahl aktiver AX-Benutzer-Sessions
- **Quelle**: `SYSCLIENTSESSIONS` Tabelle
- **Normal**: 50-200 je nach Unternehmensgröße
- **Achtung bei**: Plötzlichen Spitzen oder Drops

### Running Batch Jobs
- **Was es ist**: Aktuell ausgeführte Batch-Jobs
- **Quelle**: `BATCHJOB` Tabelle
- **Normal**: 5-20 während Geschäftszeiten
- **Achtung bei**: Lange laufende Jobs (>2h)

### Database Size
- **Was es ist**: Gesamtgröße der AX-Datenbank
- **Quelle**: `sys.database_files`
- **Normal**: 50-500 GB je nach Nutzung
- **Achtung bei**: Schnellem Wachstum (>10% pro Monat)

### Expensive Queries
- **Was es ist**: Queries mit hoher CPU/I/O
- **Quelle**: `sys.dm_exec_query_stats`
- **Normal**: 0-10 problematische Queries
- **Achtung bei**: Queries mit >5 Sekunden durchschnittlicher Laufzeit

---

## 🎯 Performance-Ziele

### Empfohlene Schwellenwerte

| Metrik | Gut | Warnung | Kritisch |
|--------|-----|---------|----------|
| Query CPU Time | <100ms | 100-1000ms | >1000ms |
| Index Fragmentation | <10% | 10-30% | >30% |
| Batch Job Fehler | 0 | 1-5 | >5 |
| Database Growth | <5%/Monat | 5-10%/Monat | >10%/Monat |

---

## ✅ Checkliste für Produktiv-Einsatz

### Vor Go-Live:
- [ ] Test mit AX 2012 R3 CU13 Server
- [ ] SQL Server 2016 Verbindung testen
- [ ] Alle Tabs durchklicken
- [ ] Permissions verifizieren
- [ ] Backup der Konfiguration erstellen

### Nach Go-Live:
- [ ] Baseline-Metriken dokumentieren
- [ ] Tägliche Dashboard-Checks einplanen
- [ ] Wöchentliche Reviews schedulen
- [ ] Monatliche Optimierungen durchführen
- [ ] Performance-Trends tracken

---

## 📞 Support

Bei Problemen:

1. **Logs prüfen**: `%LocalAppData%\AX2012PerformanceOptimizer\`
2. **README.md lesen**: Vollständige Dokumentation
3. **DEMO_GUIDE.md**: Ausführliche Feature-Beschreibungen
4. **GitHub Issues**: Bug Reports und Feature Requests

---

## 🎓 Weiterführende Ressourcen

- **SQL Server DMVs**: https://docs.microsoft.com/sql/relational-databases/system-dynamic-management-views/
- **AX 2012 Performance**: https://docs.microsoft.com/dynamicsax-2012/
- **Index Optimization**: https://docs.microsoft.com/sql/relational-databases/indexes/

---

**Version**: 1.0.0  
**Zuletzt aktualisiert**: Oktober 2025  
**Erstellt mit**: ❤️ und .NET 8 + WPF

