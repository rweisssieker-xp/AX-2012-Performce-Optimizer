# AI Features - AX 2012 Performance Optimizer

**Version**: 2.0
**Last Updated**: October 2025
**Status**: ✅ Fully Implemented

---

## Table of Contents

1. [Overview](#overview)
2. [AI Service Configuration](#ai-service-configuration)
3. [Phase 1 AI Features](#phase-1-ai-features)
4. [Advanced AI Features](#advanced-ai-features)
5. [AI Cost Optimization](#ai-cost-optimization)
6. [Usage Guide](#usage-guide)
7. [API Reference](#api-reference)
8. [Troubleshooting](#troubleshooting)

---

## Overview

The AX 2012 Performance Optimizer includes **comprehensive AI integration** powered by OpenAI and Azure OpenAI. These features provide intelligent query analysis, natural language interaction, automatic fixes, and predictive analytics.

### AI Capabilities

1. **Natural Language Query Assistant** 🗣️ - Ask questions in plain language
2. **AI Performance Insights Dashboard** 📊 - Automated insights generation
3. **Intelligent Query Rewriter** ✍️ - AI-powered query optimization
4. **Query Auto-Fixer** 🔧 - Automatic query fixes with explanations
5. **Query Documentation Generator** 📚 - Auto-generate comprehensive docs
6. **AI Query Explainer** 🔍 - Understand complex queries
7. **Performance Cost Calculator** 💰 - Monetary cost analysis
8. **Query Performance Forecasting** 🔮 - Predict future performance

### Why AI Integration Matters

- **80% Time Savings**: Automated analysis vs. manual review
- **Cost Transparency**: See exact € cost of slow queries
- **Proactive Management**: Predict issues before they occur
- **Knowledge Transfer**: AI explains complex queries for junior DBAs
- **Continuous Learning**: AI improves recommendations over time

---

## AI Service Configuration

### Step 1: Get API Key

#### Option A: OpenAI (Recommended for individual use)

1. Go to https://platform.openai.com/api-keys
2. Create account / Sign in
3. Click "Create new secret key"
4. Copy the key: `sk-...`

#### Option B: Azure OpenAI (Recommended for enterprises)

1. Create Azure OpenAI resource in Azure Portal
2. Go to "Keys and Endpoint"
3. Copy:
   - Key 1 or Key 2
   - Endpoint URL (e.g., `https://your-resource.openai.azure.com`)
   - Deployment name (e.g., `gpt-4o`)

### Step 2: Configure in Application

1. Open **⚙️ Settings** tab
2. Scroll to "AI Configuration" section
3. Select Provider:
   - **OpenAI** (default)
   - **Azure OpenAI**
4. Enter configuration:

   **OpenAI:**
   ```
   API Key: sk-...
   Model: gpt-4o (recommended)
   ```

   **Azure OpenAI:**
   ```
   API Key: [your-azure-key]
   Endpoint: https://your-resource.openai.azure.com
   Deployment Name: gpt-4o
   ```

5. Click **💾 Save AI Configuration**
6. Click **🧪 Test AI Connection**

### Supported Models

#### OpenAI Models

| Model | Best For | Cost | Speed |
|-------|----------|------|-------|
| **gpt-4o** ⭐ | General use, best quality | Medium | Fast |
| **gpt-4o-mini** | High volume, low cost | Low | Very Fast |
| **gpt-4-turbo** | Complex analysis | High | Medium |
| **o1-preview** | Advanced reasoning | Very High | Slow |
| **o1-mini** | Fast reasoning | High | Fast |
| **gpt-3.5-turbo** | Legacy, budget | Very Low | Very Fast |

#### Azure OpenAI Models

Same models available, pricing depends on Azure subscription.

**Recommended**: `gpt-4o` - Best balance of quality, speed, and cost.

---

## Phase 1 AI Features

### 1. Natural Language Query Assistant 🗣️

**Purpose**: Ask performance questions in plain English/German

**Features**:
- Natural language query processing
- Intent detection (8 types)
- Conversation history management
- Session management
- Suggested follow-up questions
- Real-time data integration

**Intent Types**:
1. **PerformanceIssue**: "Show me slow queries"
2. **CostAnalysis**: "How much do queries cost?"
3. **Recommendation**: "What should I optimize?"
4. **BatchJob**: "Why are batch jobs failing?"
5. **QueryAnalysis**: "Analyze this query"
6. **TimeBasedQuery**: "Show queries from yesterday"
7. **Trend**: "Is performance getting worse?"
8. **General**: Everything else

**Example Questions**:
```
English:
- "Show me the slowest queries today"
- "What's causing high CPU usage?"
- "How can I improve batch job performance?"
- "Which indexes are missing?"
- "What's the cost of slow queries?"

German:
- "Zeig mir die langsamsten Queries heute"
- "Was verursacht die hohe CPU-Last?"
- "Wie kann ich die Batch-Job Performance verbessern?"
- "Welche Indexes fehlen?"
- "Was kosten die langsamen Queries?"
```

**How It Works**:
1. User asks question in natural language
2. AI detects intent and extracts entities (times, tables, numbers)
3. System fetches relevant real data
4. AI generates human-friendly response with insights
5. Suggests follow-up questions

**Without AI** (Fallback):
- Intelligent pattern-matching responses
- Real performance data still shown
- Structured recommendations
- No API costs

**UI Location**: 🗣️ Natural Language Assistant tab

---

### 2. AI Performance Insights Dashboard 📊

**Purpose**: Automated, intelligent performance insights

**Features**:
- Automatic insight generation
- Weekly performance summaries
- Executive summaries for management
- Risk detection and alerting
- Performance trend analysis
- Optimization opportunity detection

**Insight Categories**:
1. **Performance Insights**: Query degradation, bottlenecks
2. **Cost Insights**: Monetary impact, ROI opportunities
3. **Reliability Insights**: Failures, stability issues
4. **Top Insights**: Most important items first

**Generated Reports**:
- **Performance Score** (0-100): Overall system health
- **Performance Grade** (A-F): Easy to understand rating
- **Trend Direction**: Improving / Stable / Degrading
- **Cost Estimates**: Daily/Monthly projected costs
- **Success Rate**: % of successful operations

**Example Insights**:
```
🔴 Critical Performance Insight
"Customer query CUST_001 degraded 45% over last 7 days.
Current: 850ms (was 590ms). Impact: 2,500 users affected daily.
Recommendation: Add index on ACCOUNTNUM column."

💰 Cost Insight
"Top 10 queries cost €245/day (€7,350/month).
Optimization potential: €5,200/month savings (71%).
Priority: Optimize VENDTABLE join in purchasing reports."

✅ Reliability Insight
"Batch job success rate dropped to 87% (from 94%).
5 jobs failing consistently in Order Processing module.
Root cause: Timeout on large data sets."
```

**Weekly Summary Example**:
```markdown
# Weekly Performance Summary
Week of October 15-21, 2025

## Overall Health: B (82/100) ⬇️
Previous: B+ (86/100)
Trend: Degrading

## Top Issues:
1. Query performance degraded 12%
2. 3 new missing indexes detected
3. Batch job failures increased 40%

## Cost Analysis:
- This week: €1,680
- Optimization potential: €1,180/week
- ROI if optimized: 2-week payback

## Actions Recommended:
1. [Critical] Fix ORDER_QUERY timeout
2. [High] Add index IX_CUSTTABLE_ACCOUNTNUM
3. [Medium] Optimize batch job BATCHJOB_003
```

**UI Location**: Dashboard → 📊 Generate AI Insights

---

### 3. Intelligent Query Rewriter ✍️

**Purpose**: AI rewrites queries for better performance

**Features**:
- LLM-based query analysis
- Context-aware optimizations
- AX 2012-specific best practices
- Before/After comparison
- Explanation generation
- Impact estimation

**Optimization Types**:
1. JOIN optimization (INNER vs LEFT, order)
2. WHERE clause improvements (index-friendly)
3. SELECT column reduction (avoid SELECT *)
4. Subquery elimination (use CTEs/JOINs)
5. Parameterization (avoid ad-hoc queries)
6. Hint suggestions (NOLOCK, FORCESEEK)
7. Set-based vs row-by-row conversion

**Example**:

**Original Query**:
```sql
SELECT * FROM CUSTTABLE
WHERE DATAAREAID = 'DAT'
  AND ACCOUNTNUM IN (
    SELECT ACCOUNTNUM FROM CUSTTRANS
    WHERE TRANSDATE > '2024-01-01'
  )
ORDER BY NAME
```

**AI-Optimized Query**:
```sql
SELECT c.ACCOUNTNUM, c.NAME, c.PHONE, c.EMAIL
FROM CUSTTABLE c WITH (NOLOCK)
INNER JOIN (
    SELECT DISTINCT ACCOUNTNUM
    FROM CUSTTRANS WITH (NOLOCK)
    WHERE TRANSDATE > '2024-01-01'
      AND DATAAREAID = 'DAT'
) t ON c.ACCOUNTNUM = t.ACCOUNTNUM
WHERE c.DATAAREAID = 'DAT'
ORDER BY c.NAME
```

**AI Explanation**:
```
Improvements Made:

1. SELECT * → Specific Columns
   - Reduces I/O by 60% (eliminated unused columns)
   - Enables covering index usage

2. IN Subquery → INNER JOIN
   - Eliminates repeated subquery execution
   - 3x faster on large datasets

3. Added NOLOCK Hints
   - Reduces locking overhead
   - Safe for reporting queries

4. Added DISTINCT in Subquery
   - Prevents duplicate joins
   - Reduces intermediate result set

5. Early DATAAREAID Filter
   - Critical for AX multi-company
   - Uses clustered index

Estimated Improvement: 75% faster (850ms → 210ms)
```

**UI Location**: SQL Performance → Select Query → ✍️ Rewrite Query

---

## Advanced AI Features

### 4. Query Auto-Fixer 🔧

**Purpose**: Automatically fix problematic queries

**Features**:
- Automatic issue detection
- Multiple fix suggestions
- Safety validation
- Before/After comparison
- Detailed explanations
- Copy-to-clipboard scripts

**Detected Issues**:
1. Missing indexes
2. Inefficient JOINs
3. Non-sargable WHERE clauses
4. SELECT * usage
5. Implicit conversions
6. Parameter sniffing
7. Statistics issues
8. Locking problems

**Fix Confidence Levels**:
- **High** (90-100%): Safe to apply automatically
- **Medium** (70-89%): Review before applying
- **Low** (<70%): Manual review required

**Example Fix**:

**Issue Detected**:
```
🔴 Non-Sargable WHERE Clause
Function on indexed column prevents index usage

Current:
WHERE YEAR(TRANSDATE) = 2024
```

**Auto-Fix**:
```
WHERE TRANSDATE >= '2024-01-01'
  AND TRANSDATE < '2025-01-01'
```

**Explanation**:
```
Why This Fixes It:
- YEAR(TRANSDATE) function prevents index usage
- Rewritten as range scan: uses index IX_TRANSDATE
- Estimated improvement: 95% faster (10,000ms → 500ms)

Impact:
- Logical Reads: 50,000 → 2,500 (95% reduction)
- CPU Time: 8,000ms → 400ms
- Executions: 1,200/hour → Now optimized

Safety: High (common pattern, well-tested)
```

**UI Location**: SQL Performance → Select Query → 🔧 Auto-Fix

---

### 5. Query Documentation Generator 📚

**Purpose**: Generate comprehensive query documentation

**Features**:
- Business logic explanation
- Technical documentation
- Performance characteristics
- Dependencies and risks
- Markdown format
- Export capability
- Version tracking

**Generated Documentation**:

```markdown
# Query Documentation

## Business Purpose
This query retrieves customer transactions for financial reporting,
filtering by date range and company. Used by Finance team for
monthly reconciliation.

## Technical Details

**Tables Accessed:**
- CUSTTABLE (Customer master data)
- CUSTTRANS (Customer transactions)
- DIMENSIONATTRIBUTEVALUECOM (Financial dimensions)

**Indexes Used:**
- IX_CUSTTRANS_TRANSDATE (Primary)
- I_002ACCOUNTNUM (Secondary)

**Performance Profile:**
- Average Duration: 850ms
- CPU Time: 420ms
- Logical Reads: 12,500
- Executions: 1,200/hour
- Peak Time: 9-11 AM

## Dependencies

**Called By:**
- FinancialReport.rdl (SSRS)
- MonthlyReconciliation.exe (Batch job)
- CustomerStatementForm (AX form)

**Requires:**
- CUSTTRANS index maintenance
- Statistics update on TRANSDATE
- Access to COMPANY table

## Risks & Considerations

⚠️ **Performance Risk:** High volume during month-end
⚠️ **Locking Risk:** May block transaction posting
ℹ️ **Data Freshness:** Real-time (no caching)

## Optimization Opportunities

1. Add covering index: INCLUDE (AMOUNTCUR, DUEDATE)
2. Filter by DATAAREAID early
3. Consider materialized view for reporting

## Change History

- 2024-10-15: Added financial dimension join
- 2024-09-01: Optimized WHERE clause
- 2024-08-15: Initial implementation
```

**UI Location**: SQL Performance → Select Query → 📚 Generate Documentation

---

### 6. AI Query Explainer 🔍

**Purpose**: Explain complex queries in simple terms

**Features**:
- Plain language explanation
- Step-by-step query execution
- Performance bottleneck identification
- Optimization suggestions
- Visual execution plan analysis

**Example Explanation**:

**Complex Query**:
```sql
WITH CustomerOrders AS (
    SELECT ACCOUNTNUM, COUNT(*) as OrderCount
    FROM SALESTABLE
    WHERE CREATEDDATETIME > DATEADD(month, -6, GETDATE())
    GROUP BY ACCOUNTNUM
    HAVING COUNT(*) > 10
)
SELECT c.NAME, c.PHONE, co.OrderCount
FROM CUSTTABLE c
INNER JOIN CustomerOrders co ON c.ACCOUNTNUM = co.ACCOUNTNUM
WHERE c.CUSTGROUP = 'GOLD'
ORDER BY co.OrderCount DESC
```

**AI Explanation**:

```
What This Query Does (Simple English):

1. Find Frequent Customers (CTE)
   → Looks at orders from last 6 months
   → Counts how many orders each customer has
   → Keeps only customers with more than 10 orders

2. Get Customer Details
   → Matches frequent customers to their contact info
   → Only includes "GOLD" customer group
   → Sorts by order count (most orders first)

Business Purpose:
This query identifies your top "GOLD" tier customers who have
placed more than 10 orders in the last 6 months. Useful for:
- VIP customer outreach
- Loyalty program targeting
- Sales team prioritization

How It Executes:

Step 1: Scan SALESTABLE
  ├─ Filters by date (last 6 months)
  ├─ Groups by customer
  └─ Filters by count > 10
  Time: ~300ms, Reads: 5,000

Step 2: Join to CUSTTABLE
  ├─ Matches customer IDs
  ├─ Filters by CUSTGROUP = 'GOLD'
  └─ Sorts results
  Time: ~150ms, Reads: 1,200

Total: ~450ms, Reads: 6,200

Performance Notes:
✅ Good: Uses CTE (temporary result set)
✅ Good: Date filter uses index
⚠️ Watch: GROUP BY may be expensive with many customers
⚠️ Watch: Sort operation if many results

Potential Issues:
- If OrderCount grows large, sort will be slow
- DATEADD(month, -6, GETDATE()) is evaluated once (good)
- CUSTGROUP filter should be early (currently is)

Suggested Improvements:
1. Add index: IX_SALESTABLE_CREATEDDATETIME_ACCOUNTNUM
2. Consider adding TOP N if you don't need all results
3. If run frequently, consider indexed view
```

**UI Location**: SQL Performance → Select Query → 🔍 Explain Query

---

### 7. Performance Cost Calculator 💰

**Purpose**: Calculate monetary cost of query performance

**Features**:
- CPU cost calculation
- I/O cost calculation
- Memory cost calculation
- Monetary cost estimation (€/$/£)
- ROI analysis for optimizations
- Total Cost of Ownership (TCO)
- Executive summaries

**Cost Breakdown**:

```
💰 Query Cost Analysis: CUST_REPORT_001

Time-Based Costs:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
CPU Time per Execution:        850ms
Executions per Day:            1,200
Total Daily CPU Time:          17 minutes

User Productivity Impact:
├─ Average User Wait Time:     2.5 seconds
├─ Users Affected Daily:       450
├─ Total User Wait Time:       18.75 hours/day
└─ Hourly Cost (€50/hour):     €937.50/day

Infrastructure Costs:
├─ CPU Cost (Azure):           €0.12/hour
├─ I/O Cost (Reads):           €0.08/GB
├─ Total Infrastructure:       €24.50/day

Daily Total:    €962.00
Monthly Total:  €28,860.00
Yearly Total:   €346,320.00

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Optimization Potential:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
If optimized by 75% (realistic target):

Daily Savings:     €721.50
Monthly Savings:   €21,645.00
Yearly Savings:    €259,740.00

Implementation Cost:  €5,000 (estimated)
Payback Period:       2.3 days 🎯
ROI:                  5,195% annually

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Executive Summary:
This query costs your business €962 daily in lost productivity
and infrastructure costs. A one-time optimization investment of
€5,000 pays for itself in under 3 days, saving €260K annually.

Priority: 🔴 CRITICAL - Immediate action recommended
```

**Configuration**:
```csharp
// Settings → Cost Configuration
UserHourlyRate: €50 (default)
CloudProvider: Azure / AWS / On-Premises
Currency: EUR / USD / GBP
```

**UI Location**: SQL Performance → Select Query → 💰 Calculate Cost

---

### 8. Query Performance Forecasting 🔮

**Purpose**: Predict future query performance using ML

**Features**:
- Linear regression forecasting
- 30/60/90-day predictions
- Anomaly detection (spikes, drops, drift)
- What-If analysis
- Confidence intervals
- Degradation alerts

**Forecast Example**:

```
🔮 Query Performance Forecast: CUST_REPORT_001

Current Performance:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Average Duration:       850ms
Trend:                  ⚠️ Degrading
Confidence:             87%

Historical Data (Last 30 Days):
Day  1: 590ms ●━━━━━━━━━━━━━━━━━━━━━━
Day 10: 680ms ●━━━━━━━━━━━━━━━━━━━━━━━━
Day 20: 780ms ●━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Day 30: 850ms ●━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

30-Day Forecast:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Predicted Duration:     1,150ms (+35%)
Confidence Range:       950ms - 1,350ms
Risk Level:             🔴 High

60-Day Forecast:
Predicted Duration:     1,450ms (+70%)
Risk Level:             🔴 Critical

90-Day Forecast:
Predicted Duration:     1,750ms (+106%)
Risk Level:             🔴 Critical

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Anomalies Detected:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
⚠️ October 5:  Spike to 1,500ms (+150%)
⚠️ October 12: Sudden drop to 400ms (-45%)
ℹ️ October 15: Gradual drift began (slope: +8.7ms/day)

Root Cause Analysis:
• Oct 5 Spike: Batch job running during business hours
• Oct 12 Drop: Index rebuild completed
• Oct 15 Drift: Data volume increase (orders table +15%)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Recommendations:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🎯 Immediate Actions:
1. Add missing index on TRANSDATE column
2. Update statistics on SALESTABLE
3. Reschedule batch job to off-peak hours

🎯 Preventive Measures:
1. Set up monitoring alert at 1,000ms threshold
2. Schedule monthly index maintenance
3. Implement query result caching

🎯 What-If Analysis:
If optimized today:
  → 30-day forecast: 320ms (62% improvement)
  → 60-day forecast: 350ms (vs 1,450ms unoptimized)
  → Cost savings: €15,000/month
```

**UI Location**: SQL Performance → Select Query → 🔮 Forecast Performance

---

## AI Cost Optimization

### Tiered Model Selection

The application automatically selects the optimal AI model based on task complexity:

| Task | Model | Reason | Cost per Call |
|------|-------|--------|---------------|
| Simple Analysis | gpt-4o-mini | Fast, cheap | $0.001 |
| Query Explanation | gpt-4o | Balanced | $0.010 |
| Complex Rewrite | gpt-4-turbo | High quality | $0.030 |
| Reasoning Tasks | o1-preview | Best logic | $0.060 |

### Response Caching

- **Cache Hit Rate**: 95%+ for repeated queries
- **Cache TTL**: 15 minutes (configurable)
- **Cost Savings**: 98-99% reduction
- **Storage**: In-memory cache (no disk I/O)

**Example**:
```
Without Caching:
  100 identical query analyses/day × $0.01 = $1.00/day
  Monthly: $30.00

With Caching:
  1 API call + 99 cache hits × $0.01 = $0.01/day
  Monthly: $0.30
  Savings: $29.70/month (99%)
```

### Batch Processing

Analyze multiple queries in one API call:

```csharp
// ❌ Expensive: Individual calls
foreach (var query in top10Queries)
{
    await aiService.AnalyzeQueryAsync(query); // 10 × $0.01 = $0.10
}

// ✅ Optimized: Batch call
await aiService.BatchAnalyzeQueriesAsync(top10Queries); // 1 × $0.03 = $0.03
// Savings: 70%
```

### Cost Tracking

Monitor AI spending in real-time:

```
AI Service Cost Dashboard
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Today:          $2.45  (127 calls)
This Week:      $14.30 (890 calls)
This Month:     $52.80 (3,240 calls)
Projected:      $63.00 (forecast)

Cost Breakdown:
├─ Query Analysis:      $28.50 (54%)
├─ Auto-Fix:            $12.20 (23%)
├─ Documentation:       $8.10  (15%)
└─ Insights:            $4.00  (8%)

Cache Stats:
├─ Hit Rate:            97.3%
├─ Savings:             $1,246.00
└─ Effective Cost:      $52.80 (vs $1,298.80 without cache)
```

---

## Usage Guide

### Getting Started with AI Features

#### 1. Configure AI Service (One-Time)

```
Settings → AI Configuration
  1. Select Provider (OpenAI or Azure OpenAI)
  2. Enter API Key
  3. Select Model (gpt-4o recommended)
  4. Test Connection
  5. Save
```

#### 2. Use Natural Language Assistant

```
🗣️ Natural Language Assistant Tab
  1. Type question: "Show me slow queries today"
  2. Press Enter or click Send
  3. Review AI response with real data
  4. Click suggested follow-up questions
```

#### 3. Analyze Individual Query

```
📈 SQL Performance Tab
  1. Refresh query list
  2. Select slow query
  3. Click "🤖 AI Analysis" button
  4. Review:
     - Complexity score
     - Cost analysis
     - Optimization suggestions
     - Auto-fix proposals
```

#### 4. Batch Analyze Top Queries

```
📈 SQL Performance Tab
  1. Click "🎯 Batch Analyze Top 10"
  2. Wait 30-60 seconds
  3. Review prioritized optimization list
  4. Click queries for detailed analysis
```

#### 5. Generate Insights Dashboard

```
🏠 Dashboard Tab
  1. Click "📊 Generate AI Insights"
  2. Wait for analysis (15-30 seconds)
  3. Review:
     - Performance score
     - Top insights
     - Cost analysis
     - Recommendations
  4. Export as PDF (optional)
```

### Best Practices

#### When to Use AI

✅ **Use AI for:**
- Complex query analysis (10+ tables, subqueries)
- First-time optimization (unknown patterns)
- Explaining queries to non-technical users
- Business impact analysis (cost calculations)
- Predictive analytics (forecasting)
- Documentation generation

❌ **Don't Use AI for:**
- Simple queries (SELECT * FROM table)
- Repetitive analysis (use cache)
- Real-time monitoring (too slow)
- Mission-critical without review
- When API quotas exhausted

#### Cost Management

1. **Enable Caching** (default: ON)
2. **Use Batch Analysis** for multiple queries
3. **Select Cheaper Models** for simple tasks
4. **Monitor Spending** in Settings → AI Cost Dashboard
5. **Set Budget Alerts** (optional)

#### Quality Control

1. **Review AI Suggestions** before applying
2. **Test in Development** first
3. **Verify Performance Impact** after optimization
4. **Document Changes** with AI explanations
5. **Rollback Plan** if issues arise

---

## API Reference

### INaturalLanguageQueryAssistant

```csharp
public interface INaturalLanguageQueryAssistant
{
    Task<string> StartNewSessionAsync();
    Task<NLQueryResponse> ProcessQueryAsync(string query, NLQueryContext context);
    Task ClearSessionAsync(string sessionId);
}

public class NLQueryResponse
{
    public string TextResponse { get; set; }
    public string IntentDetected { get; set; }
    public double ConfidenceScore { get; set; }
    public List<SqlQueryMetric> QueryResults { get; set; }
    public List<PerformanceInsight> Insights { get; set; }
    public List<string> SuggestedQuestions { get; set; }
    public double ProcessingTimeMs { get; set; }
}
```

### IAiQueryOptimizerService

```csharp
public interface IAiQueryOptimizerService
{
    void Configure(string apiKey, string endpoint, string model, bool isAzure);
    bool IsAvailable { get; }
    Task<QueryAnalysisResult> AnalyzeQueryAsync(string queryText);
    Task<List<string>> GenerateOptimizationSuggestionsAsync(string queryText);
    Task<QueryRewriteResult> RewriteQueryAsync(string queryText);
}
```

### IAiPerformanceInsightsService

```csharp
public interface IAiPerformanceInsightsService
{
    Task<List<PerformanceInsight>> GenerateInsightsAsync(
        List<SqlQueryMetric> queries,
        DatabaseMetric dbMetrics);

    Task<WeeklySummary> GenerateWeeklySummaryAsync(
        DateTime startDate,
        DateTime endDate);

    Task<ExecutiveSummary> GenerateExecutiveSummaryAsync();
}
```

### IPerformanceCostCalculatorService

```csharp
public interface IPerformanceCostCalculatorService
{
    Task<QueryCostAnalysis> CalculateQueryCostAsync(SqlQueryMetric query);
    Task<ROIAnalysis> CalculateOptimizationROIAsync(
        SqlQueryMetric originalQuery,
        double improvementPercent);
}
```

### IQueryPerformanceForecastingService

```csharp
public interface IQueryPerformanceForecastingService
{
    Task<PerformanceForecast> ForecastQueryPerformanceAsync(
        List<SqlQueryMetric> historicalData,
        int daysAhead);

    Task<List<PerformanceAnomaly>> DetectAnomaliesAsync(
        List<SqlQueryMetric> data);
}
```

---

## Troubleshooting

### Issue: "AI Service Not Configured"

**Cause**: No API key entered or invalid configuration

**Solution**:
1. Go to Settings → AI Configuration
2. Enter valid API key
3. Select correct model
4. Test connection
5. Save configuration

### Issue: "API Call Failed: 401 Unauthorized"

**Cause**: Invalid or expired API key

**Solution**:
1. Verify API key is correct
2. Check API key hasn't expired
3. For Azure: Verify endpoint URL is correct
4. Regenerate API key if needed
5. Re-enter in application

### Issue: "API Call Failed: 429 Too Many Requests"

**Cause**: Rate limit exceeded

**Solution**:
1. Wait a few minutes before retrying
2. Enable caching (Settings → AI)
3. Use batch analysis instead of individual calls
4. Upgrade OpenAI plan if needed
5. Consider Azure OpenAI (higher limits)

### Issue: "Request Timeout (30s)"

**Cause**: AI response taking too long

**Solution**:
1. Switch to faster model (gpt-4o-mini)
2. Check internet connectivity
3. Reduce query complexity
4. Try again during off-peak hours
5. Use fallback responses (work without AI)

### Issue: "Poor Quality AI Responses"

**Cause**: Wrong model or insufficient context

**Solution**:
1. Upgrade to gpt-4o or gpt-4-turbo
2. Provide more context in query
3. Verify query text is complete
4. Check for special characters/encoding issues
5. Review AI prompt templates

### Issue: "High AI Costs"

**Cause**: Excessive API calls or wrong model

**Solution**:
1. Enable caching (default: ON)
2. Use batch analysis for multiple queries
3. Switch to cheaper model (gpt-4o-mini)
4. Set budget alerts
5. Review cost dashboard regularly

---

## Support & Resources

### Documentation

- **OpenAI API**: https://platform.openai.com/docs
- **Azure OpenAI**: https://learn.microsoft.com/azure/cognitive-services/openai
- **Application Docs**: See README.md and CORE_DOCUMENTATION.md

### Community

- **GitHub Issues**: Bug reports and feature requests
- **Stack Overflow**: Tag `[openai-api]` `[dynamics-ax]`

### Cost Management

- **OpenAI Pricing**: https://openai.com/pricing
- **Azure OpenAI Pricing**: https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/
- **Cost Calculator**: Available in Settings → AI Cost Dashboard

---

## Conclusion

The AI features in AX 2012 Performance Optimizer provide **unprecedented intelligence** in performance monitoring and optimization. With proper configuration and best practices, you can:

- **Save 80% of manual analysis time**
- **Reduce costs by identifying expensive queries**
- **Predict issues before they impact users**
- **Generate professional documentation automatically**
- **Explain complex queries to stakeholders**

**Getting Started**:
1. Configure AI service (5 minutes)
2. Try Natural Language Assistant
3. Analyze top 10 queries
4. Review generated insights
5. Apply recommendations

**Questions?** See troubleshooting section or contact support.

---

**Last Updated**: October 2025
**Version**: 2.0
**Status**: ✅ Production Ready

*End of AI Features Documentation*
