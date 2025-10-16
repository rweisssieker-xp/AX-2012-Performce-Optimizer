# 🔮 Query Performance Forecasting - Benutzerhandbuch

## Überblick

**Query Performance Forecasting** ist ein ML-basiertes Predictive Analytics Feature, das **zukünftige Query-Performance** vorhersagt und **Probleme erkennt, bevor sie auftreten**!

---

## 🎯 Was macht es einzigartig?

### Warum andere Tools das nicht haben:
- ❌ Andere Tools: "Query ist langsam" (Reaktiv)
- ✅ Unser Tool: "Query wird in 12 Tagen zum Problem!" (Proaktiv)

### Business-Wert:
- **Proaktive Wartung** statt reaktivem Firefighting
- **Vorhersage von Ausfällen** bevor sie passieren
- **Kapazitätsplanung** mit What-If Szenarien
- **Anomalie-Erkennung** für ungewöhnliche Performance-Muster

---

## 💡 Features

### 1. Performance Forecast 🔮
**"Wie wird die Performance in 30 Tagen sein?"**

```
📊 Forecast für die nächsten 30 Tage:
Trend: Degrading (+2.35ms/Tag)
Aktuell: 450ms
Vorhergesagt: 520ms (+15.6%)
Konfidenz: 87%

⚠️ Query wird in 12 Tagen zum Performance-Problem!
Estimated Issue Date: 2025-10-27
Severity: High
```

**Key Features:**
- **Linear Regression** für Trend-Berechnung
- **Confidence Score** (R-squared basiert)
- **Forecast Points** für Visualisierung (mit Upper/Lower Bounds)
- **Early Warning** wenn Schwellenwerte bald überschritten werden
- **Severity Levels**: None, Low, Medium, High, Critical

**Use Case:**
```csharp
var forecast = await forecastingService.ForecastPerformanceAsync(
    query,
    historicalData,
    forecastDays: 30);

if (forecast.WillBecomeIssue)
{
    Console.WriteLine($"⚠️ Warnung: {forecast.AlertMessage}");
    Console.WriteLine($"Datum: {forecast.EstimatedIssueDate:yyyy-MM-dd}");
    Console.WriteLine($"Severity: {forecast.Severity}");
}
```

---

### 2. Anomaly Detection ⚠️
**"Warum war die Query gestern so langsam?"**

```
⚠️ 3 Anomalie(n) erkannt:

[2025-10-14] Spike - Critical
  Performance-Spike: 8,500ms (Erwartet: 450ms)
  Mögliche Ursache: Hohe Physical Reads (kalter Cache), Hohe Wait-Time (Locking/Blocking)

[2025-10-12] Drift - Medium
  Performance-Drift erkannt: 45% Verschlechterung über Zeit
  Mögliche Ursache: Daten-Wachstum, fehlende Index-Wartung, oder zunehmende Fragmentierung

[2025-10-10] Spike - High
  Performance-Spike: 6,200ms (Erwartet: 450ms)
  Mögliche Ursache: Hohe Benutzer-Last
```

**Detection Methods:**
- **3-Sigma Rule** für Spike/Drop Erkennung
- **Drift Detection** für graduelle Verschlechterung
- **Root Cause Analysis** basierend auf Metriken

**Anomaly Types:**
1. **Spike** - Plötzlicher Performance-Einbruch
2. **Drop** - Unerwartete Verbesserung
3. **Drift** - Graduelle Verschlechterung über Zeit

**Use Case:**
```csharp
var anomalies = await forecastingService.DetectAnomaliesAsync(query, historicalData);

if (anomalies.HasAnomalies)
{
    foreach (var anomaly in anomalies.Anomalies)
    {
        Console.WriteLine($"[{anomaly.DetectedAt:yyyy-MM-dd}] {anomaly.Type}");
        Console.WriteLine($"  {anomaly.Description}");
        Console.WriteLine($"  Cause: {anomaly.PossibleCause}");
    }
}
```

---

### 3. Performance Issue Prediction 🚨
**"Wann wird diese Query zum Problem?"**

```
🔮 Performance-Vorhersage:

⚠️ High - Query wird in 12 Tagen den Schwellenwert von 5000ms überschreiten

Präventive Maßnahmen:
  • 🔍 Führen Sie eine Index-Analyse durch
  • 📊 Überprüfen Sie Execution Plans auf Scan-Operationen
  • ⚙️ Erwägen Sie Query-Optimierung (SELECT *, Joins)
  • 💾 Prüfen Sie, ob Daten archiviert werden können
  • 📈 Überwachen Sie Daten-Wachstum und Fragmentierung
```

**Features:**
- **Threshold Violation Detection** mit Timeline
- **Days Until Issue** Berechnung
- **Severity Classification** (None, Low, Medium, High, Critical)
- **Preventive Action Plan** - konkrete Empfehlungen

**Use Case:**
```csharp
var thresholds = new PerformanceThresholds
{
    MaxAvgElapsedTimeMs = 5000.0,
    MaxExecutionsPerHour = 100000,
    MaxCpuTimeMs = 3000.0
};

var prediction = await forecastingService.PredictPerformanceIssueAsync(
    query,
    historicalData,
    thresholds);

if (prediction.WillBecomeIssue)
{
    Console.WriteLine($"🚨 Action required in {prediction.DaysUntilIssue} days!");

    foreach (var action in prediction.PreventiveActions)
    {
        Console.WriteLine($"  {action}");
    }
}
```

---

### 4. Trend Analysis 📈
**"Wie hat sich die Performance über Zeit entwickelt?"**

```
📈 Trend-Analyse:
Overall: Degrading (Stärke: 65%)
Query-Performance verschlechtert sich. Untersuchung empfohlen.

7 Tage:  Up (+8.5%) - Ø 485ms
30 Tage: Up (+15.2%) - Ø 465ms
90 Tage: Up (+32.1%) - Ø 425ms

📊 Saisonalität erkannt: Weekly
```

**Analysis Periods:**
- **7-Day Trend** - Kurzfristige Entwicklung
- **30-Day Trend** - Mittelfristige Entwicklung
- **90-Day Trend** - Langfristige Entwicklung

**Key Metrics:**
- Average/Min/Max Elapsed Time
- Trend Direction (Up, Down, Flat)
- Change Percentage
- Trend Slope

**Seasonality Detection:**
- Erkennt wiederkehrende Muster (Daily, Weekly, Monthly)
- Hilfreich für Kapazitätsplanung

**Use Case:**
```csharp
var trendAnalysis = await forecastingService.AnalyzeTrendAsync(query, historicalData);

Console.WriteLine($"Overall Trend: {trendAnalysis.OverallTrend}");
Console.WriteLine($"Trend Strength: {trendAnalysis.TrendStrength:F0}%");
Console.WriteLine($"{trendAnalysis.Interpretation}");

if (trendAnalysis.HasSeasonality)
{
    Console.WriteLine($"Seasonality Pattern: {trendAnalysis.SeasonalityPattern}");
}
```

---

### 5. What-If Simulator 🎮
**"Was passiert, wenn wir 3x mehr User haben?"**

```
🔮 What-If Analyse: Black Friday (3x mehr User)
Load-Multiplikator: 3.0x

Vorhergesagte Auswirkungen:
  Elapsed Time: 450ms → 1,755ms (+190%)
  Executions: 10,000 → 30,000
  Impact Level: Severe

Bedenken:
  ⚠️ Performance-Degradierung: 190%
  ❌ Kapazitätsgrenze überschritten (351% Auslastung)
  ⏱️ Signifikante Queue-Time erwartet: 200ms
  🔥 Hohe Last-Steigerung - Lock-Contention wahrscheinlich

Empfehlungen:
  • 🔍 Führen Sie Performance-Tests unter Last durch
  • 📊 Erwägen Sie Query-Optimierung vor Skalierung
  • 💾 Prüfen Sie Index-Strategie für höhere Last
  • ⚙️ Erwägen Sie Read-Replicas oder Caching
  • 🚨 Hardware-Upgrade oder Architektur-Änderung erforderlich
```

**Simulation Scenarios:**
- Black Friday (3x-5x mehr Last)
- Neues Feature-Release (2x mehr Queries)
- User-Wachstum (1.5x mehr User)
- Data-Migration (10x mehr Daten)

**Impact Levels:**
- **Minimal** (<10% Degradierung)
- **Moderate** (10-30% Degradierung)
- **Significant** (30-70% Degradierung)
- **Severe** (>70% Degradierung)

**Use Case:**
```csharp
var whatIf = await forecastingService.SimulateLoadChangeAsync(
    query,
    loadMultiplier: 3.0,
    scenario: "Black Friday (3x mehr User)");

Console.WriteLine($"Impact Level: {whatIf.ImpactLevel}");
Console.WriteLine($"Capacity: {whatIf.CapacityUtilization:F0}%");

if (whatIf.WillExceedCapacity)
{
    Console.WriteLine("🚨 WARNUNG: Kapazitätsgrenze überschritten!");

    foreach (var recommendation in whatIf.Recommendations)
    {
        Console.WriteLine($"  {recommendation}");
    }
}
```

---

## 🚀 Verwendung

### Backend API

#### 1. Performance Forecast

```csharp
var forecastingService = serviceProvider.GetRequiredService<IQueryPerformanceForecastingService>();

// Get historical data (from HistoricalDataService or database)
var history = await historicalDataService.GetHistoricalSnapshotsAsync(
    query.QueryHash,
    DateTime.Now.AddDays(-90),
    DateTime.Now);

// Generate 30-day forecast
var forecast = await forecastingService.ForecastPerformanceAsync(
    query,
    history,
    forecastDays: 30);

Console.WriteLine(forecast.Summary);

// Check for issues
if (forecast.WillBecomeIssue)
{
    SendAlert($"Query {query.QueryHash} wird in {(forecast.EstimatedIssueDate.Value - DateTime.Now).Days} Tagen zum Problem!");
}
```

#### 2. Anomaly Detection

```csharp
var anomalyResult = await forecastingService.DetectAnomaliesAsync(query, history);

if (anomalyResult.HasAnomalies)
{
    Console.WriteLine($"⚠️ {anomalyResult.Anomalies.Count} Anomalien erkannt!");

    foreach (var anomaly in anomalyResult.Anomalies.OrderByDescending(a => a.Severity))
    {
        Console.WriteLine($"[{anomaly.DetectedAt:yyyy-MM-dd HH:mm}] {anomaly.Type} - {anomaly.Severity}");
        Console.WriteLine($"  Value: {anomaly.Value:F0}ms (Expected: {anomaly.ExpectedValue:F0}ms)");
        Console.WriteLine($"  Deviation: {anomaly.Deviation:F1} sigma");
        Console.WriteLine($"  Cause: {anomaly.PossibleCause}");
    }
}
```

#### 3. Issue Prediction

```csharp
var thresholds = new PerformanceThresholds
{
    MaxAvgElapsedTimeMs = 5000.0,
    MaxExecutionsPerHour = 100000,
    MaxCpuTimeMs = 3000.0,
    MaxLogicalReads = 1000000,
    MaxWaitTimeMs = 2000.0
};

var prediction = await forecastingService.PredictPerformanceIssueAsync(
    query,
    history,
    thresholds);

if (prediction.WillBecomeIssue)
{
    Console.WriteLine($"🚨 {prediction.Severity} Issue predicted!");
    Console.WriteLine($"Days until issue: {prediction.DaysUntilIssue}");
    Console.WriteLine($"Reason: {prediction.Reason}");

    Console.WriteLine("\nPreventive Actions:");
    foreach (var action in prediction.PreventiveActions)
    {
        Console.WriteLine($"  {action}");
    }
}
```

#### 4. Trend Analysis

```csharp
var trendAnalysis = await forecastingService.AnalyzeTrendAsync(query, history);

Console.WriteLine($"Overall Trend: {trendAnalysis.OverallTrend}");
Console.WriteLine($"Strength: {trendAnalysis.TrendStrength:F0}%");
Console.WriteLine($"\n{trendAnalysis.Interpretation}");

// 7-day trend
if (trendAnalysis.Last7Days.Samples > 0)
{
    Console.WriteLine($"\n7-Day Trend:");
    Console.WriteLine($"  Direction: {trendAnalysis.Last7Days.Direction}");
    Console.WriteLine($"  Change: {trendAnalysis.Last7Days.ChangePercent:+0.0;-0.0}%");
    Console.WriteLine($"  Average: {trendAnalysis.Last7Days.AverageElapsedTime:F0}ms");
}

// Check for seasonality
if (trendAnalysis.HasSeasonality)
{
    Console.WriteLine($"\n📊 Seasonality Pattern: {trendAnalysis.SeasonalityPattern}");
    Console.WriteLine("Consider load balancing based on seasonal patterns");
}
```

#### 5. What-If Simulation

```csharp
// Scenario 1: Black Friday
var blackFriday = await forecastingService.SimulateLoadChangeAsync(
    query,
    loadMultiplier: 3.0,
    scenario: "Black Friday (3x mehr User)");

Console.WriteLine(blackFriday.Summary);

if (blackFriday.WillExceedCapacity)
{
    Console.WriteLine($"⚠️ Capacity exceeded: {blackFriday.CapacityUtilization:F0}%");
    Console.WriteLine("Recommendations:");
    foreach (var rec in blackFriday.Recommendations)
    {
        Console.WriteLine($"  • {rec}");
    }
}

// Scenario 2: User Growth
var userGrowth = await forecastingService.SimulateLoadChangeAsync(
    query,
    loadMultiplier: 1.5,
    scenario: "User-Wachstum (+50%)");

Console.WriteLine($"Impact: {userGrowth.ImpactLevel}");
```

---

## 📊 Use Cases

### Use Case 1: Proaktive Wartung

**Szenario:** IT-Team möchte Probleme verhindern, bevor sie auftreten

```csharp
// Daily Monitoring Job
var topQueries = await sqlMonitor.GetTopExpensiveQueriesAsync(50);

foreach (var query in topQueries)
{
    var history = await historicalData.GetHistoricalSnapshotsAsync(
        query.QueryHash,
        DateTime.Now.AddDays(-30),
        DateTime.Now);

    var prediction = await forecastingService.PredictPerformanceIssueAsync(
        query,
        history,
        new PerformanceThresholds());

    if (prediction.WillBecomeIssue && prediction.DaysUntilIssue < 14)
    {
        // Send alert to team
        await SendAlert(new Alert
        {
            Title = $"Query {query.QueryHash} needs attention",
            Severity = prediction.Severity,
            DaysUntilIssue = prediction.DaysUntilIssue,
            Actions = prediction.PreventiveActions
        });
    }
}
```

---

### Use Case 2: Capacity Planning

**Szenario:** Unternehmen plant Black Friday, erwartet 3x mehr Last

```csharp
var criticalQueries = await sqlMonitor.GetTopExpensiveQueriesAsync(20);

var report = new StringBuilder();
report.AppendLine("🛒 BLACK FRIDAY CAPACITY ANALYSIS");
report.AppendLine();

foreach (var query in criticalQueries)
{
    var whatIf = await forecastingService.SimulateLoadChangeAsync(
        query,
        loadMultiplier: 3.0,
        scenario: "Black Friday");

    report.AppendLine($"Query: {query.QueryHash.Substring(0, 8)}...");
    report.AppendLine($"  Current: {query.AvgElapsedTimeMs:F0}ms");
    report.AppendLine($"  Predicted: {whatIf.PredictedAvgElapsedTimeMs:F0}ms");
    report.AppendLine($"  Impact: {whatIf.ImpactLevel}");

    if (whatIf.WillExceedCapacity)
    {
        report.AppendLine($"  ❌ CRITICAL - Capacity exceeded!");
        report.AppendLine($"  Recommendations:");
        foreach (var rec in whatIf.Recommendations)
        {
            report.AppendLine($"    • {rec}");
        }
    }
    else
    {
        report.AppendLine($"  ✅ OK - Within capacity");
    }

    report.AppendLine();
}

File.WriteAllText("black_friday_capacity_report.txt", report.ToString());
```

---

### Use Case 3: Root Cause Analysis

**Szenario:** "Warum war die Query gestern Abend so langsam?"

```csharp
var history = await historicalData.GetHistoricalSnapshotsAsync(
    query.QueryHash,
    DateTime.Now.AddDays(-7),
    DateTime.Now);

var anomalies = await forecastingService.DetectAnomaliesAsync(query, history);

if (anomalies.HasAnomalies)
{
    var yesterday = anomalies.Anomalies
        .Where(a => a.DetectedAt.Date == DateTime.Now.AddDays(-1).Date)
        .OrderByDescending(a => a.Deviation)
        .FirstOrDefault();

    if (yesterday != null)
    {
        Console.WriteLine($"🔍 Root Cause Analysis:");
        Console.WriteLine($"Time: {yesterday.DetectedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Type: {yesterday.Type}");
        Console.WriteLine($"Value: {yesterday.Value:F0}ms (Expected: {yesterday.ExpectedValue:F0}ms)");
        Console.WriteLine($"Deviation: {yesterday.Deviation:F1} sigma");
        Console.WriteLine($"\nPossible Cause:");
        Console.WriteLine($"  {yesterday.PossibleCause}");
    }
}
```

---

### Use Case 4: Trend Monitoring Dashboard

**Szenario:** Weekly Executive Report

```csharp
var criticalQueries = await sqlMonitor.GetTopExpensiveQueriesAsync(10);

var report = new StringBuilder();
report.AppendLine("📊 WEEKLY PERFORMANCE TREND REPORT");
report.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd}");
report.AppendLine();

int degrading = 0;
int stable = 0;
int improving = 0;

foreach (var query in criticalQueries)
{
    var history = await historicalData.GetHistoricalSnapshotsAsync(
        query.QueryHash,
        DateTime.Now.AddDays(-90),
        DateTime.Now);

    var trend = await forecastingService.AnalyzeTrendAsync(query, history);

    switch (trend.OverallTrend)
    {
        case "Degrading": degrading++; break;
        case "Stable": stable++; break;
        case "Improving": improving++; break;
    }

    report.AppendLine($"Query: {query.QueryHash.Substring(0, 8)}...");
    report.AppendLine($"  Trend: {trend.OverallTrend} (Strength: {trend.TrendStrength:F0}%)");
    report.AppendLine($"  7-Day: {trend.Last7Days.Direction} ({trend.Last7Days.ChangePercent:+0.0;-0.0}%)");
    report.AppendLine($"  30-Day: {trend.Last30Days.Direction} ({trend.Last30Days.ChangePercent:+0.0;-0.0}%)");
    report.AppendLine();
}

report.AppendLine("SUMMARY:");
report.AppendLine($"  Degrading: {degrading}");
report.AppendLine($"  Stable: {stable}");
report.AppendLine($"  Improving: {improving}");

File.WriteAllText("weekly_trend_report.txt", report.ToString());
```

---

## ⚙️ Konfiguration

### Performance Thresholds

```csharp
var thresholds = new PerformanceThresholds
{
    MaxAvgElapsedTimeMs = 5000.0,      // 5 Sekunden
    MaxExecutionsPerHour = 100000,      // 100k Executions/Stunde
    MaxCpuTimeMs = 3000.0,              // 3 Sekunden CPU
    MaxLogicalReads = 1000000,          // 1M Reads
    MaxWaitTimeMs = 2000.0              // 2 Sekunden Wait
};
```

**Empfohlene Werte:**

#### OLTP-System (Order Processing, User Transactions):
```csharp
new PerformanceThresholds
{
    MaxAvgElapsedTimeMs = 1000.0,      // 1 Sekunde
    MaxExecutionsPerHour = 500000,
    MaxCpuTimeMs = 500.0,
    MaxLogicalReads = 100000,
    MaxWaitTimeMs = 500.0
}
```

#### Reporting-System (BI, Analytics):
```csharp
new PerformanceThresholds
{
    MaxAvgElapsedTimeMs = 30000.0,     // 30 Sekunden
    MaxExecutionsPerHour = 10000,
    MaxCpuTimeMs = 20000.0,
    MaxLogicalReads = 10000000,
    MaxWaitTimeMs = 5000.0
}
```

#### Batch-Processing (Nightly Jobs):
```csharp
new PerformanceThresholds
{
    MaxAvgElapsedTimeMs = 300000.0,    // 5 Minuten
    MaxExecutionsPerHour = 100,
    MaxCpuTimeMs = 180000.0,
    MaxLogicalReads = 100000000,
    MaxWaitTimeMs = 60000.0
}
```

---

## 🎯 Business Benefits

### Für IT-Management:
- ✅ **Proaktives Monitoring** - Probleme erkennen, bevor User betroffen sind
- ✅ **Kapazitätsplanung** - Wissen, wann Hardware-Upgrade nötig ist
- ✅ **Budget-Rechtfertigung** - "Wir brauchen neuen Server in 3 Monaten"
- ✅ **SLA-Einhaltung** - Performance-Probleme verhindern

### Für Developer:
- ✅ **Root Cause Analysis** - Verstehen, warum Query langsam war
- ✅ **Priorisierung** - Welche Queries zuerst optimieren?
- ✅ **Performance-Testing** - What-If Szenarien testen

### Für Business:
- ✅ **Verfügbarkeit** - Weniger Ausfälle durch proaktive Wartung
- ✅ **User-Experience** - Queries bleiben schnell
- ✅ **Kosteneinsparung** - Ungeplante Downtimes vermeiden

---

## 📈 Beispiel-Szenarien

### Szenario 1: "Query wird langsamer"

```
Problem: Users beschweren sich über langsame Order-Suche

Analyse:
1. Trend Analysis zeigt: Degrading (45% über 30 Tage)
2. Anomaly Detection: Keine Spikes, aber kontinuierlicher Drift
3. Forecast: Query wird in 18 Tagen Schwellenwert überschreiten
4. Root Cause: Daten-Wachstum (Orders-Tabelle +2M Zeilen)

Lösung:
- Archivierung alter Orders (>2 Jahre)
- Neuer Index auf OrderDate + Status
- Result: Performance wieder bei 250ms (war 890ms)
```

---

### Szenario 2: "Black Friday Vorbereitung"

```
Frage: Ist unser System bereit für 3x mehr Last?

What-If Analyse:
- Load Multiplier: 3.0x
- 15 von 20 kritischen Queries: Severe Impact
- 8 Queries: Capacity Exceeded

Action Plan:
1. Optimize Top 8 Queries (Index-Tuning)
2. Implement Query Caching für Read-Only Queries
3. Add Read-Replicas für Produktkatalog
4. Load-Testing mit 3x Last

Result:
✅ Black Friday: 99.8% Uptime, Ø Response Time 380ms
```

---

### Szenario 3: "Mysterious Performance Spike"

```
Problem: Jeden Montag 9:00 Uhr langsame Performance

Anomaly Detection:
- 4 Spikes erkannt (alle Montag 9:00)
- Mögliche Ursache: Hohe Physical Reads (kalter Cache)
- Pattern: Weekly Seasonality

Trend Analysis:
- Seasonality: Weekly
- Monday Morning: +180% Elapsed Time

Lösung:
- Cache-Warming Job um 8:30 Uhr
- Index-Rebuild am Wochenende
- Result: Montag-Morning Spike eliminiert
```

---

### Szenario 4: "Gradual Degradation"

```
Problem: "System ist langsamer geworden, aber wir wissen nicht warum"

Drift Detection:
- Performance-Drift: 65% Verschlechterung über 90 Tage
- Keine Spikes, kontinuierliche Degradierung
- Cause: Index-Fragmentierung + Statistik-Probleme

Forecast:
- In 45 Tagen: Critical Performance Issues
- In 60 Tagen: System unusable

Preventive Actions:
- Index-Rebuild + Statistics Update
- Implement Weekly Maintenance Job
- Result: Performance zurück auf Ausgangslevel
```

---

## 🔮 Roadmap

### Phase 2 Features (geplant):
- **Machine Learning Models** (ARIMA, Prophet für bessere Forecasts)
- **Multi-Metric Forecasting** (CPU, Memory, IO gleichzeitig)
- **Seasonality Decomposition** (Trend, Seasonal, Residual)
- **Alert Rules Engine** (Configurable Alerting)
- **Historical Replay** ("Was wäre passiert wenn...")
- **Forecasting Dashboard** (Real-Time Visualisierung)

---

## 📊 Integration mit Cost Calculator

**Kombiniere Forecasting + Cost Calculator für ROI-Planung:**

```csharp
// 1. Forecast Performance
var forecast = await forecastingService.ForecastPerformanceAsync(query, history, 90);

// 2. Calculate Current Cost
var costParams = new CostParameters { AverageHourlySalary = 50.0, ActiveUsers = 100 };
var currentCost = await costCalculator.CalculateQueryCostAsync(query, costParams);

// 3. Calculate Future Cost (if not optimized)
var futureQuery = new SqlQueryMetric
{
    QueryHash = query.QueryHash,
    AvgElapsedTimeMs = forecast.PredictedAvgElapsedTimeMs,
    ExecutionCount = forecast.PredictedExecutionCount,
    AvgCpuTimeMs = forecast.PredictedCpuTimeMs
};
var futureCost = await costCalculator.CalculateQueryCostAsync(futureQuery, costParams);

// 4. Show Business Impact
Console.WriteLine($"📊 Cost Impact ohne Optimierung:");
Console.WriteLine($"Aktuell: €{currentCost.YearlyTotalCost:N2}/Jahr");
Console.WriteLine($"In 90 Tagen: €{futureCost.YearlyTotalCost:N2}/Jahr");
Console.WriteLine($"Zusätzliche Kosten: €{(futureCost.YearlyTotalCost - currentCost.YearlyTotalCost):N2}/Jahr");
```

---

## 📞 Support

Bei Fragen:
- **Dokumentation:** `QUERY_PERFORMANCE_FORECASTING_GUIDE.md`
- **Code:** `QueryPerformanceForecastingService.cs`
- **Interface:** `IQueryPerformanceForecastingService.cs`

---

**Query Performance Forecasting macht Ihre SQL-Optimierung proaktiv statt reaktiv! 🔮**
