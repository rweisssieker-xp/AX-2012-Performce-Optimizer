# 📚 Query Documentation Generator Guide

## Überblick

Der **Query Documentation Generator** erstellt automatisch umfassende Dokumentation für SQL-Queries. Von einfachen Inline-Kommentaren bis zu kompletten HTML-Reports - alles automatisch generiert!

---

## 🎯 Features

### 1. **Comprehensive Documentation** 📋

Erstellt vollständige Query-Dokumentation mit:
- **Purpose** - Wofür ist die Query?
- **Description** - Detaillierte Beschreibung
- **Tables** - Verwendete Tabellen
- **Columns** - Abgerufene Spalten
- **Parameters** - Query-Parameter
- **Complexity Analysis** - Komplexitäts-Score (0-100)
- **Performance Metrics** - Execution Time, Reads, etc.
- **Business Rules** - Erkannte Business-Logik
- **Use Cases** - Typische Verwendungszwecke

### 2. **Simple Explanations** 💬

Generiert verständliche Erklärungen in natürlicher Sprache:

**Beispiel:**
```
Input: SELECT c.Name, SUM(o.Amount) FROM Customers c
       JOIN Orders o ON c.ID = o.CustomerID
       WHERE c.Status = 'Active' GROUP BY c.Name

Output: "This query retrieves data from Customers, Orders,
         combining data from 2 tables with specific filtering
         conditions and calculates aggregated values (SUM)."
```

### 3. **Inline Comments** 💡

Fügt automatisch hilfreiche Kommentare ein:

**Vorher:**
```sql
SELECT c.ACCOUNTNUM, c.NAME, c.ADDRESS
FROM CUSTTABLE c
JOIN CUSTTRANS t ON c.ACCOUNTNUM = t.ACCOUNTNUM
WHERE c.DATAAREAID = 'DAT'
GROUP BY c.ACCOUNTNUM, c.NAME, c.ADDRESS
ORDER BY c.NAME
```

**Nachher:**
```sql
-- Retrieve the following columns:
SELECT c.ACCOUNTNUM, c.NAME, c.ADDRESS
-- From table:
FROM CUSTTABLE c
-- Join with related table:
JOIN CUSTTRANS t ON c.ACCOUNTNUM = t.ACCOUNTNUM
-- Filter conditions:
WHERE c.DATAAREAID = 'DAT'
-- Group results by:
GROUP BY c.ACCOUNTNUM, c.NAME, c.ADDRESS
-- Sort results by:
ORDER BY c.NAME
```

### 4. **Query Catalog** 📚

Generiert README-Dokumentation für mehrere Queries:

```markdown
# Query Catalog Documentation

Generated: 2025-10-15 12:00:00 UTC
Total Queries: 50

## Summary Statistics
- Total Executions: 150,234
- Avg CPU Time: 45.67ms
- Total CPU Time: 6,854,321ms

## Queries

### Query 1: Query CUSTTABLE
**Performance:**
- Executions: 15,234
- Avg CPU Time: 23.45ms
- Rating: Good

**Complexity:**
- Level: Medium
- Score: 45/100
- JOINs: 2
```

### 5. **Multiple Output Formats** 📄

- **Markdown** - Für GitHub, GitLab, etc.
- **HTML** - Für Webseiten, Intranets
- **JSON** - Für APIs, weitere Verarbeitung
- **Plain Text** - Für E-Mails, Reports

---

## 🚀 Verwendung

### Einfache Dokumentation generieren

```csharp
var docService = new QueryDocumentationService(logger, aiOptimizer);

// Vollständige Dokumentation
var doc = await docService.GenerateDocumentationAsync(queryText, metrics);

Console.WriteLine($"Query: {doc.QueryName}");
Console.WriteLine($"Purpose: {doc.Purpose}");
Console.WriteLine($"Complexity: {doc.Complexity.Level} ({doc.Complexity.Score}/100)");
Console.WriteLine($"Tables: {string.Join(", ", doc.Tables)}");
```

### Simple Explanation

```csharp
// Kurze, verständliche Erklärung
var explanation = await docService.ExplainQueryAsync(queryText);

Console.WriteLine(explanation);
// Output: "This query retrieves data from CUSTTABLE with 2 JOINs..."
```

### Inline Comments hinzufügen

```csharp
// Kommentare automatisch einfügen
var commented = await docService.AddInlineCommentsAsync(queryText);

File.WriteAllText("query_commented.sql", commented);
```

### Query Catalog erstellen

```csharp
// Liste aller Queries
var queries = await queryMonitor.GetTopExpensiveQueriesAsync(100);

// Katalog-Dokumentation generieren
var catalog = await docService.GenerateCatalogDocumentationAsync(queries);

File.WriteAllText("QUERY_CATALOG.md", catalog);
```

### Als Markdown exportieren

```csharp
var doc = await docService.GenerateDocumentationAsync(queryText, metrics);

// Markdown generieren
var markdown = await docService.GenerateMarkdownAsync(doc);

File.WriteAllText("query_doc.md", markdown);
```

### Als HTML exportieren

```csharp
var doc = await docService.GenerateDocumentationAsync(queryText, metrics);

// HTML generieren
var html = await docService.GenerateHtmlAsync(doc);

File.WriteAllText("query_doc.html", html);
```

---

## 📊 Beispiel-Output

### Markdown Format

```markdown
# Query CUSTTABLE

## Purpose
Retrieve data from CUSTTABLE, CUSTTRANS

## Description
This is a medium query that accesses 2 tables with 1 JOIN.

## Complexity
- **Level:** Medium
- **Score:** 45/100
- **JOINs:** 1
- **Subqueries:** 0
- **Factors:**
  - 1 JOIN(s)
  - 1 Aggregation(s)

## Performance
- **Rating:** Good
- **Avg Execution Time:** 23.45ms
- **Avg Logical Reads:** 8,234
- **Execution Count:** 15,234

## Tables
- `CUSTTABLE`
- `CUSTTRANS`

## Business Rules
- Company-specific data filtering (DATAAREAID)
- Only active records are included

## Use Cases
- Customer data retrieval and reporting

## SQL Query
```sql
SELECT c.ACCOUNTNUM, c.NAME, SUM(t.AMOUNT)
FROM CUSTTABLE c
JOIN CUSTTRANS t ON c.ACCOUNTNUM = t.ACCOUNTNUM
WHERE c.DATAAREAID = 'DAT' AND c.STATUS = 'Active'
GROUP BY c.ACCOUNTNUM, c.NAME
```

---
*Generated: 2025-10-15 12:00:00 UTC*
```

---

## 🎨 HTML Output

Das HTML-Format enthält:
- **Styled Headers** - Professionelles Design
- **Syntax Highlighting** - Farbige SQL-Queries
- **Badges** - Performance-Ratings (Excellent, Good, Fair, Poor)
- **Responsive Layout** - Funktioniert auf allen Geräten
- **Printable** - Optimiert für Druck

---

## 🧠 AI-Enhanced Documentation

Wenn AI aktiviert ist, werden zusätzliche Features genutzt:

```csharp
// AI Optimizer konfigurieren
var aiOptimizer = new AiQueryOptimizerService(logger, httpClient);
aiOptimizer.Configure(apiKey, endpoint, "gpt-4o-mini");

// Documentation Service mit AI
var docService = new QueryDocumentationService(logger, aiOptimizer);

// AI generiert:
// - Bessere Descriptions
// - Business Rules Erkennung
// - Use Case Identifikation
// - Semantic Analysis
var doc = await docService.GenerateDocumentationAsync(queryText);
```

### AI vs. Rule-Based Comparison

| Feature | Rule-Based | AI-Enhanced |
|---------|-----------|-------------|
| Tables | ✅ Regex | ✅ Regex |
| Columns | ✅ Regex | ✅ Regex |
| Complexity | ✅ Heuristic | ✅ Heuristic |
| Purpose | ⚠️ Basic | ✅ Detailed |
| Description | ⚠️ Template | ✅ Natural Language |
| Business Rules | ⚠️ Keywords | ✅ Semantic Analysis |
| Use Cases | ⚠️ Table Names | ✅ Context-Aware |

---

## 📈 Complexity Analysis

### Complexity Levels

**Simple (0-30):**
```sql
SELECT * FROM CUSTTABLE WHERE ACCOUNTNUM = '1234'
```
- Single table
- Simple WHERE
- No JOINs

**Medium (31-60):**
```sql
SELECT c.NAME, SUM(o.AMOUNT)
FROM CUSTTABLE c
JOIN ORDERS o ON c.ID = o.CUSTOMERID
GROUP BY c.NAME
```
- Multiple tables
- JOINs
- Aggregations

**Complex (61-85):**
```sql
SELECT c.NAME,
       (SELECT SUM(Amount) FROM Orders WHERE CustomerID = c.ID) AS Total
FROM CUSTTABLE c
WHERE EXISTS (SELECT 1 FROM CUSTTRANS t WHERE t.ACCOUNTNUM = c.ACCOUNTNUM)
```
- Subqueries
- Correlated subqueries
- Multiple conditions

**Very Complex (86-100):**
```sql
WITH RECURSIVE BomCTE AS (
    SELECT ID, PARENT, LEVEL = 0 FROM BOM WHERE PARENT IS NULL
    UNION ALL
    SELECT b.ID, b.PARENT, r.LEVEL + 1
    FROM BOM b
    JOIN BomCTE r ON b.PARENT = r.ID
)
SELECT * FROM BomCTE
WHERE LEVEL < 5
```
- Recursive CTEs
- Window functions
- Advanced SQL features

---

## 🔧 Integration Examples

### 1. Mit Query Analyzer

```csharp
var analyzer = new QueryAnalyzerService(dbStats);
var docService = new QueryDocumentationService(logger, aiOptimizer);

// Analyse + Dokumentation
var suggestions = await analyzer.AnalyzeQueryAsync(query);
var doc = await docService.GenerateDocumentationAsync(query.QueryText, query);

// Performance-Notes hinzufügen
if (suggestions.Any())
{
    doc.Notes.Add($"Found {suggestions.Count} optimization opportunities");
    foreach (var suggestion in suggestions)
    {
        doc.Notes.Add($"- {suggestion.Title}");
    }
}

var markdown = await docService.GenerateMarkdownAsync(doc);
```

### 2. Mit Auto-Fixer

```csharp
var autoFixer = new QueryAutoFixerService(logger, aiOptimizer);
var docService = new QueryDocumentationService(logger, aiOptimizer);

// Dokumentation VORHER
var beforeDoc = await docService.GenerateDocumentationAsync(originalQuery);

// Auto-Fix anwenden
var fixResult = await autoFixer.AutoFixQueryAsync(originalQuery);

// Dokumentation NACHHER
var afterDoc = await docService.GenerateDocumentationAsync(fixResult.FixedQuery);

// Vergleich generieren
var comparison = $@"
# Query Optimization Report

## Before Optimization
{await docService.GenerateMarkdownAsync(beforeDoc)}

## After Optimization
{await docService.GenerateMarkdownAsync(afterDoc)}

## Applied Fixes
{string.Join("\n", fixResult.AppliedFixes.Select(f => $"- {f.Title}"))}
";
```

### 3. Automated Documentation Pipeline

```csharp
// Pipeline: Monitor → Analyze → Document → Export
var queryMonitor = new SqlQueryMonitorService(connectionManager, logger);
var analyzer = new QueryAnalyzerService(dbStats);
var docService = new QueryDocumentationService(logger, aiOptimizer);

// 1. Get all queries
var queries = await queryMonitor.GetTopExpensiveQueriesAsync(50);

// 2. Analyze each
foreach (var query in queries)
{
    var suggestions = await analyzer.AnalyzeQueryAsync(query);
    var doc = await docService.GenerateDocumentationAsync(query.QueryText, query);

    // 3. Export
    var markdown = await docService.GenerateMarkdownAsync(doc);
    File.WriteAllText($"docs/query_{query.QueryHash}.md", markdown);
}

// 4. Create catalog
var catalog = await docService.GenerateCatalogDocumentationAsync(queries);
File.WriteAllText("docs/QUERY_CATALOG.md", catalog);
```

---

## 💰 Cost Optimization

Documentation Generator nutzt **Smart Model Selection**:

| Operation | Model | Cost |
|-----------|-------|------|
| Explain Query | gpt-4o-mini | $0.001 |
| Generate Docs | gpt-4o-mini | $0.002 |
| Catalog | Rule-based | $0.00 |
| Inline Comments | Rule-based | $0.00 |

**Mit Caching:** 90% Kosteneinsparung für wiederholte Queries!

---

## 🎯 Best Practices

### 1. Dokumentiere alle wichtigen Queries

```csharp
// Automatische Dokumentation für alle Queries >100 Executions
var importantQueries = queries.Where(q => q.ExecutionCount > 100);

foreach (var query in importantQueries)
{
    var doc = await docService.GenerateDocumentationAsync(query.QueryText, query);
    // Export...
}
```

### 2. Nutze Catalog für Übersicht

```csharp
// Wöchentlicher Report
var catalog = await docService.GenerateCatalogDocumentationAsync(allQueries);
File.WriteAllText($"reports/catalog_{DateTime.Now:yyyy-MM-dd}.md", catalog);
```

### 3. Inline Comments für Entwickler

```csharp
// In Stored Procedures oder Views
var storedProc = File.ReadAllText("usp_CustomerReport.sql");
var commented = await docService.AddInlineCommentsAsync(storedProc);
File.WriteAllText("usp_CustomerReport_documented.sql", commented);
```

### 4. HTML für Stakeholders

```csharp
// Executive Reports
var doc = await docService.GenerateDocumentationAsync(criticalQuery, metrics);
var html = await docService.GenerateHtmlAsync(doc);
File.WriteAllText("reports/critical_query_report.html", html);
```

---

## 🚀 Roadmap

### Phase 2 (Q1 2026)
- **Auto-Update** - Dokumentation automatisch aktualisieren
- **Version Control** - Dokumentations-Historie
- **Custom Templates** - Eigene Dokumentations-Templates
- **Export to Confluence** - Integration mit Atlassian

### Phase 3 (Q2 2026)
- **Interactive Docs** - Klickbare HTML mit Query Explorer
- **Video Generation** - Query-Erklärung als Video
- **Multi-Language** - Deutsch, Englisch, Französisch
- **API** - REST API für Dokumentations-Generierung

---

## 📞 Support

Bei Fragen:
- **Dokumentation:** `QUERY_DOCUMENTATION_GUIDE.md`
- **Code:** `QueryDocumentationService.cs`
- **Beispiele:** Siehe Integration Examples

---

**Der Query Documentation Generator macht Code-Dokumentation zum Kinderspiel! 📚✨**
