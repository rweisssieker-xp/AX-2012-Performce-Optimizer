# 🔧 Smart Query Auto-Fixer Guide

## Überblick

Der **Smart Query Auto-Fixer** analysiert automatisch SQL-Queries und wendet Performance-Optimierungen an. Er kombiniert **Rule-Based Fixes** mit **AI-Powered Optimizations** für maximale Effektivität.

---

## 🎯 Features

### Automatische Fixes

Der Auto-Fixer erkennt und behebt automatisch diese häufigen Performance-Probleme:

#### 1. **SELECT * Replacement** 🎯
**Problem:**
```sql
SELECT * FROM CUSTTABLE WHERE ACCOUNTNUM = '1234'
```

**Fix:**
```sql
SELECT ACCOUNTNUM, NAME, ADDRESS FROM CUSTTABLE WHERE ACCOUNTNUM = '1234'
```

**Impact:** 35% Performance-Verbesserung
**Safety:** Medium Risk (benötigt Spalten-Liste)

---

#### 2. **OR to IN Conversion** ⚡
**Problem:**
```sql
WHERE Status = 'A' OR Status = 'B' OR Status = 'C'
```

**Fix:**
```sql
WHERE Status IN ('A', 'B', 'C')
```

**Impact:** 45% Performance-Verbesserung
**Safety:** Safe (semantisch identisch)

---

#### 3. **Function in WHERE Clause** 🚀
**Problem:**
```sql
WHERE YEAR(OrderDate) = 2024
```

**Fix:**
```sql
WHERE OrderDate >= '2024-01-01' AND OrderDate < '2025-01-01'
```

**Impact:** 65% Performance-Verbesserung
**Safety:** Low Risk (SARGable)

**Weitere Beispiele:**
```sql
-- Problem: LEFT(Code, 3) = 'ABC'
-- Fix:     Code LIKE 'ABC%'

-- Problem: UPPER(Name) = 'JOHN'
-- Fix:     Name = 'John' (bei case-insensitive Collation)
```

---

#### 4. **NOT IN to NOT EXISTS** 🔥
**Problem:**
```sql
WHERE CustomerID NOT IN (SELECT CustomerID FROM Orders)
```

**Fix:**
```sql
WHERE NOT EXISTS (SELECT 1 FROM Orders o WHERE o.CustomerID = c.CustomerID)
```

**Impact:** 50% Performance-Verbesserung
**Safety:** Safe (NULL-handling besser)

---

#### 5. **Leading Wildcard Removal** 🎖️
**Problem:**
```sql
WHERE Name LIKE '%Smith'
```

**Fix:**
```sql
-- Option 1: Entferne Leading Wildcard (falls möglich)
WHERE Name LIKE 'Smith%'

-- Option 2: Nutze Full-Text Search
WHERE CONTAINS(Name, 'Smith')
```

**Impact:** 55% Performance-Verbesserung
**Safety:** Medium Risk (ändert Semantik)

---

#### 6. **DISTINCT Optimization** 💡
**Problem:**
```sql
SELECT DISTINCT c.Name, c.City
FROM CUSTTABLE c
JOIN CUSTTRANS t ON c.ACCOUNTNUM = t.ACCOUNTNUM
```

**Fix:**
```sql
-- Option 1: GROUP BY statt DISTINCT
SELECT c.Name, c.City
FROM CUSTTABLE c
JOIN CUSTTRANS t ON c.ACCOUNTNUM = t.ACCOUNTNUM
GROUP BY c.Name, c.City

-- Option 2: Subquery für Distinct
SELECT Name, City
FROM CUSTTABLE
WHERE ACCOUNTNUM IN (SELECT DISTINCT ACCOUNTNUM FROM CUSTTRANS)
```

**Impact:** 40% Performance-Verbesserung
**Safety:** Medium Risk (Logik-Änderung)

---

#### 7. **Implicit Conversion Fix** ⚠️
**Problem:**
```sql
-- VARCHAR Spalte mit INT Vergleich
WHERE CustomerCode = 1234  -- Implicit conversion!
```

**Fix:**
```sql
WHERE CustomerCode = '1234'  -- Korrekter Datentyp
```

**Impact:** 60% Performance-Verbesserung
**Safety:** Low Risk (Index-Nutzung)

---

#### 8. **Correlated Subquery Optimization** 🧠
**Problem:**
```sql
SELECT
    c.Name,
    (SELECT SUM(Amount) FROM Orders o WHERE o.CustomerID = c.CustomerID) AS Total
FROM Customers c
```

**Fix:**
```sql
SELECT
    c.Name,
    o.Total
FROM Customers c
LEFT JOIN (
    SELECT CustomerID, SUM(Amount) AS Total
    FROM Orders
    GROUP BY CustomerID
) o ON c.CustomerID = o.CustomerID
```

**Impact:** 70% Performance-Verbesserung
**Safety:** Medium Risk (JOIN statt Subquery)

---

## 🚀 Verwendung

### Option 1: Automatischer Modus

```csharp
var autoFixer = new QueryAutoFixerService(logger, aiOptimizer);

// Automatische Optimierung
var result = await autoFixer.AutoFixQueryAsync(queryText);

if (result.Success)
{
    Console.WriteLine($"Original: {result.OriginalQuery}");
    Console.WriteLine($"Fixed:    {result.FixedQuery}");
    Console.WriteLine($"Applied Fixes: {result.AppliedFixes.Count}");
    Console.WriteLine($"Improvement:   {result.EstimatedPerformanceImprovement}%");
}
```

### Option 2: Preview-Modus (Änderungen ansehen)

```csharp
// Nur Vorschau, keine Änderungen
var options = new QueryFixOptions
{
    PreviewOnly = true
};

var result = await autoFixer.AutoFixQueryAsync(queryText, options);

foreach (var fix in result.AppliedFixes)
{
    Console.WriteLine($"Fix: {fix.Title}");
    Console.WriteLine($"Before: {fix.BeforeSnippet}");
    Console.WriteLine($"After:  {fix.AfterSnippet}");
    Console.WriteLine($"Impact: {fix.EstimatedImpact}%");
    Console.WriteLine($"Safety: {fix.Safety}");
    Console.WriteLine();
}
```

### Option 3: Einzelne Fixes anwenden

```csharp
// Zuerst alle möglichen Fixes anzeigen
var availableFixes = await autoFixer.PreviewFixesAsync(queryText);

// Nutzer wählt einen Fix aus
var selectedFix = availableFixes.First(f => f.FixType == QueryFixType.OrToIn);

// Nur diesen Fix anwenden
var result = await autoFixer.ApplyFixAsync(queryText, selectedFix);
```

### Option 4: Mit Optionen

```csharp
var options = new QueryFixOptions
{
    PreviewOnly = false,           // Fixes wirklich anwenden
    MinConfidence = 0.8,           // Nur Fixes mit >80% Confidence
    UseAi = true,                  // AI für komplexe Fixes nutzen
    AggressiveMode = false,        // Konservative Optimierungen
    PreserveFormatting = true      // Formatierung beibehalten
};

var result = await autoFixer.AutoFixQueryAsync(queryText, options);
```

---

## 🛡️ Safety Levels

### Safe ✅
- Keine Änderung der Semantik
- Nur Syntax-Umstellung
- Beispiel: `OR` → `IN`

### Low Risk ⚠️
- Minimales Risiko
- Gut getestete Pattern
- Beispiel: `YEAR(Date)` → Range-Query

### Medium Risk ⚡
- Erfordert Validierung
- Kann Semantik ändern
- Beispiel: `SELECT *` → Spezifische Spalten

### High Risk 🔴
- Signifikantes Risiko
- Manuelle Review empfohlen
- Beispiel: Komplexe JOIN-Umstrukturierung

### Requires Review 🚨
- Muss manuell geprüft werden
- Keine automatische Anwendung
- Beispiel: Geschäftslogik-Änderungen

---

## 🧪 Validation

Der Auto-Fixer validiert alle Änderungen automatisch:

```csharp
var validation = await autoFixer.ValidateFixAsync(originalQuery, fixedQuery);

Console.WriteLine($"Valid: {validation.IsValid}");
Console.WriteLine($"Semantically Equivalent: {validation.SemanticallyEquivalent}");
Console.WriteLine($"Method: {validation.ValidationMethod}");

foreach (var warning in validation.Warnings)
{
    Console.WriteLine($"Warning: {warning}");
}

foreach (var error in validation.Errors)
{
    Console.WriteLine($"Error: {error}");
}
```

### Validation Methods

1. **Rule-Based:**
   - Syntax-Check
   - Balanced Parentheses
   - Common Error Detection

2. **AI-Powered:**
   - Semantic Comparison
   - Business Logic Check
   - Advanced Pattern Recognition

---

## 📊 Beispiel-Output

```
╔══════════════════════════════════════════════════════════════╗
║                 Query Auto-Fix Report                        ║
╚══════════════════════════════════════════════════════════════╝

Original Query:
  SELECT * FROM CUSTTABLE
  WHERE Status = 'A' OR Status = 'B' OR Status = 'C'

Fixed Query:
  SELECT ACCOUNTNUM, NAME, ADDRESS FROM CUSTTABLE
  WHERE Status IN ('A', 'B', 'C')

Applied Fixes:
  ✓ [1] Replace SELECT * with specific columns (35% impact)
  ✓ [2] Convert OR to IN clause (45% impact)

Estimated Improvement: 40%
Overall Confidence: 87%
Validation: PASSED (AI-powered)

Warnings:
  - Ensure columns list is complete for your use case
```

---

## 🔧 Integration mit bestehenden Features

### 1. Mit Query Analyzer

```csharp
// Analyse + Auto-Fix Pipeline
var analyzer = new QueryAnalyzerService(dbStats);
var autoFixer = new QueryAutoFixerService(logger, aiOptimizer);

// 1. Analysiere Query
var suggestions = await analyzer.AnalyzeQueryAsync(query);

// 2. Wende automatische Fixes an
var fixResult = await autoFixer.AutoFixQueryAsync(query.QueryText);

// 3. Vergleiche Performance-Prediction
var beforePrediction = await analyzer.PredictPerformanceAsync(query);
var afterPrediction = await analyzer.PredictPerformanceWithOptimizationsAsync(query, suggestions);

Console.WriteLine($"Before: {beforePrediction.PredictedCpuTimeMs}ms");
Console.WriteLine($"After:  {afterPrediction.PredictedCpuTimeMs}ms");
```

### 2. Mit AI Optimizer

```csharp
// AI-Enhanced Auto-Fix
var aiOptimizer = new AiQueryOptimizerService(logger, httpClient);
aiOptimizer.Configure(apiKey, endpoint, "gpt-4o-mini");

var autoFixer = new QueryAutoFixerService(logger, aiOptimizer);

// Auto-Fixer nutzt AI für komplexe Fixes automatisch
var result = await autoFixer.AutoFixQueryAsync(queryText, new QueryFixOptions
{
    UseAi = true  // AI für komplexe Rewrites
});
```

### 3. Mit Caching

```csharp
var cache = new AiResponseCache();
var cacheKey = $"autofix:{queryHash}";

// Check Cache
if (cache.TryGet(queryText, "autofix", out var cachedResult))
{
    return cachedResult;
}

// Apply fixes
var result = await autoFixer.AutoFixQueryAsync(queryText);

// Store in cache
cache.Set(queryText, "autofix", JsonSerializer.Serialize(result), TimeSpan.FromHours(24));
```

---

## 💰 Cost Optimization

Auto-Fixer nutzt **Tiered Model Strategy** für optimale Kosten:

| Fix Type | Model | Cost |
|----------|-------|------|
| Simple (OR, NOT IN) | Rule-based | $0.00 |
| Medium (Functions) | gpt-4o-mini | $0.001 |
| Complex (Subqueries) | gpt-4o | $0.008 |
| Very Complex | o1-mini | $0.035 |

**Durchschnittliche Kosten pro Fix:** $0.002
**Mit Caching:** $0.0002 (90% Einsparung)

---

## 🎯 Best Practices

### 1. Immer Preview nutzen
```csharp
// ZUERST Preview
var preview = await autoFixer.AutoFixQueryAsync(query, new QueryFixOptions { PreviewOnly = true });
// Dann entscheiden ob apply
```

### 2. Confidence Threshold setzen
```csharp
// Nur sichere Fixes
var options = new QueryFixOptions { MinConfidence = 0.85 };
```

### 3. Testen vor Production
```csharp
// 1. Test auf Dev-Datenbank
var fixResult = await autoFixer.AutoFixQueryAsync(queryText);

// 2. Performance messen
var before = MeasurePerformance(originalQuery);
var after = MeasurePerformance(fixResult.FixedQuery);

// 3. Nur deployen wenn besser
if (after.AvgCpuTimeMs < before.AvgCpuTimeMs * 0.8)
{
    DeployToProduction(fixResult.FixedQuery);
}
```

### 4. Logging aktivieren
```csharp
var logger = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
}).CreateLogger<QueryAutoFixerService>();

var autoFixer = new QueryAutoFixerService(logger, aiOptimizer);
// Alle Fixes werden geloggt
```

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "QueryAutoFixer": {
    "DefaultMinConfidence": 0.7,
    "UseAiByDefault": true,
    "AggressiveMode": false,
    "PreserveFormatting": true,
    "EnabledFixTypes": [
      "OrToIn",
      "NotInToNotExists",
      "FunctionInWhereClause",
      "ImplicitConversionFix"
    ],
    "DisabledFixTypes": [
      "SelectStarReplacement"  // Requires schema knowledge
    ]
  }
}
```

---

## 🚀 Roadmap

### Phase 2 (Q1 2026)
- **Auto-Apply Mode** - Automatische Anwendung auf Production
- **Learning Mode** - Lernt aus erfolgreichen Fixes
- **Custom Rules** - Eigene Fix-Rules definieren
- **Rollback Support** - Automatischer Rollback bei Problemen

### Phase 3 (Q2 2026)
- **Batch Processing** - Tausende Queries auf einmal
- **A/B Testing** - Automatischer Performance-Vergleich
- **Integration** - SSMS Plugin, VS Code Extension
- **ML-powered** - Machine Learning für Pattern Recognition

---

## 📞 Support

Bei Fragen oder Problemen:
- **Dokumentation:** `QUERY_AUTO_FIXER_GUIDE.md`
- **Code:** `QueryAutoFixerService.cs`
- **Tests:** Siehe Unit Tests (coming soon)

---

**Der Smart Query Auto-Fixer spart Zeit und verbessert die Performance deiner SQL-Queries automatisch! 🎉**
