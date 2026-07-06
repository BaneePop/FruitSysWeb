using System.Text;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Core;

/// <summary>
/// Base service class containing shared helper methods for all services.
/// Eliminates duplicate code for common operations like date filtering,
/// parameter building, and SQL construction.
/// </summary>
public abstract class BaseService
{
    /// <summary>
    /// Applies date range filter to SQL query.
    /// Adds WHERE clauses for OdDatum and/or DoDatum if they have values.
    /// </summary>
    /// <param name="sql">StringBuilder containing the SQL query</param>
    /// <param name="parameters">Dictionary to add parameters to</param>
    /// <param name="filterRequest">Filter request containing date range</param>
    /// <param name="dateColumnName">Name of the date column in SQL (e.g., "fm.Datum", "vp.Datum")</param>
    /// <param name="useAnd">If true, uses " AND " prefix. If false, uses " WHERE "</param>
    /// <example>
    /// <code>
    /// var sql = new StringBuilder("SELECT * FROM Fakture");
    /// var parameters = new Dictionary&lt;string, object&gt;();
    /// ApplyDateFilter(sql, parameters, filterRequest, "f.Datum", useAnd: false);
    /// // Result: SELECT * FROM Fakture WHERE f.Datum >= @OdDatum AND f.Datum < @DoDatum
    /// </code>
    /// </example>
    protected void ApplyDateFilter(
        StringBuilder sql,
        Dictionary<string, object> parameters,
        FilterRequest filterRequest,
        string dateColumnName,
        bool useAnd = true)
    {
        if (filterRequest.OdDatum.HasValue)
        {
            sql.Append(useAnd ? " AND" : " WHERE");
            sql.Append($" {dateColumnName} >= @OdDatum");
            parameters.Add("@OdDatum", filterRequest.OdDatum.Value.Date);
        }

        if (filterRequest.DoDatum.HasValue)
        {
            sql.Append(useAnd || filterRequest.OdDatum.HasValue ? " AND" : " WHERE");
            sql.Append($" {dateColumnName} < @DoDatum");
            parameters.Add("@DoDatum", filterRequest.DoDatum.Value.Date.AddDays(1));
        }
    }

    /// <summary>
    /// Applies komitent (partner) ID filter to SQL query.
    /// </summary>
    /// <param name="sql">StringBuilder containing the SQL query</param>
    /// <param name="parameters">Dictionary to add parameters to</param>
    /// <param name="filterRequest">Filter request containing komitent ID</param>
    /// <param name="komitentColumnName">Name of the komitent column in SQL (e.g., "fm.KomitentID")</param>
    /// <example>
    /// <code>
    /// ApplyKomitentFilter(sql, parameters, filterRequest, "fm.KomitentID");
    /// // Result: AND fm.KomitentID = @KomitentId
    /// </code>
    /// </example>
    protected void ApplyKomitentFilter(
        StringBuilder sql,
        Dictionary<string, object> parameters,
        FilterRequest filterRequest,
        string komitentColumnName)
    {
        if (filterRequest.KomitentId.HasValue && filterRequest.KomitentId > 0)
        {
            sql.Append($" AND {komitentColumnName} = @KomitentId");
            parameters.Add("@KomitentId", filterRequest.KomitentId.Value);
        }
    }

    /// <summary>
    /// Applies artikal (article) ID filter to SQL query.
    /// </summary>
    /// <param name="sql">StringBuilder containing the SQL query</param>
    /// <param name="parameters">Dictionary to add parameters to</param>
    /// <param name="filterRequest">Filter request containing artikal ID</param>
    /// <param name="artikalColumnName">Name of the artikal column in SQL (e.g., "fm.ArtikalID")</param>
    /// <example>
    /// <code>
    /// ApplyArtikalFilter(sql, parameters, filterRequest, "fm.ArtikalID");
    /// // Result: AND fm.ArtikalID = @ArtikalId
    /// </code>
    /// </example>
    protected void ApplyArtikalFilter(
        StringBuilder sql,
        Dictionary<string, object> parameters,
        FilterRequest filterRequest,
        string artikalColumnName)
    {
        if (filterRequest.ArtikalId.HasValue && filterRequest.ArtikalId > 0)
        {
            sql.Append($" AND {artikalColumnName} = @ArtikalId");
            parameters.Add("@ArtikalId", filterRequest.ArtikalId.Value);
        }
    }

    /// <summary>
    /// Creates a new parameter dictionary.
    /// Helper method to make code more readable.
    /// </summary>
    /// <returns>Empty dictionary for SQL parameters</returns>
    protected Dictionary<string, object> CreateParameters()
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a new SQL StringBuilder.
    /// Helper method to make code more readable.
    /// </summary>
    /// <param name="initialSql">Optional initial SQL string</param>
    /// <returns>StringBuilder for building SQL query</returns>
    protected StringBuilder CreateSqlBuilder(string? initialSql = null)
    {
        return new StringBuilder(initialSql ?? string.Empty);
    }

    /// <summary>
    /// Applies common filters (date, komitent, artikal) to SQL query.
    /// This is a convenience method that calls all three filter methods.
    /// </summary>
    /// <param name="sql">StringBuilder containing the SQL query</param>
    /// <param name="parameters">Dictionary to add parameters to</param>
    /// <param name="filterRequest">Filter request containing all filters</param>
    /// <param name="dateColumnName">Name of the date column in SQL</param>
    /// <param name="komitentColumnName">Name of the komitent column in SQL (optional)</param>
    /// <param name="artikalColumnName">Name of the artikal column in SQL (optional)</param>
    /// <param name="useAndForDate">If true, uses " AND " prefix for date filter. If false, uses " WHERE "</param>
    /// <example>
    /// <code>
    /// var sql = CreateSqlBuilder("SELECT * FROM ViewProdaja");
    /// var parameters = CreateParameters();
    /// ApplyCommonFilters(sql, parameters, filterRequest, "vp.Datum", "vp.KomitentID", "vp.ArtikalID", useAndForDate: false);
    /// </code>
    /// </example>
    protected void ApplyCommonFilters(
        StringBuilder sql,
        Dictionary<string, object> parameters,
        FilterRequest filterRequest,
        string dateColumnName,
        string? komitentColumnName = null,
        string? artikalColumnName = null,
        bool useAndForDate = true)
    {
        ApplyDateFilter(sql, parameters, filterRequest, dateColumnName, useAndForDate);

        if (!string.IsNullOrEmpty(komitentColumnName))
        {
            ApplyKomitentFilter(sql, parameters, filterRequest, komitentColumnName);
        }

        if (!string.IsNullOrEmpty(artikalColumnName))
        {
            ApplyArtikalFilter(sql, parameters, filterRequest, artikalColumnName);
        }
    }

    /// <summary>
    /// Logs SQL query execution with parameters for debugging.
    /// Use this in development to see what queries are being executed.
    /// </summary>
    /// <param name="logger">ILogger instance</param>
    /// <param name="sql">SQL query string</param>
    /// <param name="parameters">SQL parameters</param>
    protected void LogSqlQuery(Microsoft.Extensions.Logging.ILogger logger, string sql, Dictionary<string, object>? parameters = null)
    {
        logger.LogDebug("Executing SQL: {Sql}", sql);
        if (parameters != null && parameters.Any())
        {
            foreach (var param in parameters)
            {
                logger.LogDebug("  Parameter: {Key} = {Value}", param.Key, param.Value);
            }
        }
    }

    /// <summary>
    /// Applies date filter with time offset adjustment.
    /// Useful for working day calculations (e.g., 4-hour offset).
    /// </summary>
    /// <param name="sql">StringBuilder containing the SQL query</param>
    /// <param name="parameters">Dictionary to add parameters to</param>
    /// <param name="filterRequest">Filter request containing date range</param>
    /// <param name="dateColumnName">Name of the date column in SQL</param>
    /// <param name="odDatumHourOffset">Hours to add to OdDatum (default: 0)</param>
    /// <param name="doDatumHourOffset">Hours to add to DoDatum (default: 0)</param>
    /// <param name="doDatumDayOffset">Days to add to DoDatum (default: 0)</param>
    /// <param name="useAnd">If true, uses " AND " prefix. If false, uses " WHERE "</param>
    /// <example>
    /// <code>
    /// // Working day filter with 4-hour offset
    /// ApplyDateFilterWithOffset(sql, parameters, filterRequest, "pl.DatumKreiranja",
    ///     odDatumHourOffset: 4, doDatumHourOffset: 4, doDatumDayOffset: 1);
    /// </code>
    /// </example>
    protected void ApplyDateFilterWithOffset(
        StringBuilder sql,
        Dictionary<string, object> parameters,
        FilterRequest filterRequest,
        string dateColumnName,
        int odDatumHourOffset = 0,
        int doDatumHourOffset = 0,
        int doDatumDayOffset = 0,
        bool useAnd = true)
    {
        if (filterRequest.OdDatum.HasValue)
        {
            sql.Append(useAnd ? " AND" : " WHERE");
            sql.Append($" {dateColumnName} >= @OdDatum");

            var adjustedOdDatum = filterRequest.OdDatum.Value;
            if (odDatumHourOffset != 0)
            {
                adjustedOdDatum = adjustedOdDatum.AddHours(odDatumHourOffset);
            }
            parameters.Add("@OdDatum", adjustedOdDatum);
        }

        if (filterRequest.DoDatum.HasValue)
        {
            sql.Append(useAnd || filterRequest.OdDatum.HasValue ? " AND" : " WHERE");
            sql.Append($" {dateColumnName} < @DoDatum");

            var adjustedDoDatum = filterRequest.DoDatum.Value;
            if (doDatumDayOffset != 0)
            {
                adjustedDoDatum = adjustedDoDatum.AddDays(doDatumDayOffset);
            }
            if (doDatumHourOffset != 0)
            {
                adjustedDoDatum = adjustedDoDatum.AddHours(doDatumHourOffset);
            }
            parameters.Add("@DoDatum", adjustedDoDatum);
        }
    }

    /// <summary>
    /// Builds parametrized IN clause for SQL query.
    /// Prevents SQL injection when using IN (...) clause.
    /// </summary>
    /// <param name="values">List of values for IN clause</param>
    /// <param name="parameterPrefix">Prefix for parameter names (e.g., "ArtikalID")</param>
    /// <param name="parameters">Dictionary to add parameters to</param>
    /// <returns>Comma-separated list of parameter names (e.g., "@ArtikalID0,@ArtikalID1,@ArtikalID2")</returns>
    /// <example>
    /// <code>
    /// var artikalIds = new List&lt;int&gt; { 1, 2, 3 };
    /// var parameters = CreateParameters();
    /// var inClause = BuildInClause(artikalIds, "ArtikalID", parameters);
    /// var sql = $"SELECT * FROM Artikli WHERE ArtikalID IN ({inClause})";
    /// // Result: SELECT * FROM Artikli WHERE ArtikalID IN (@ArtikalID0,@ArtikalID1,@ArtikalID2)
    /// // parameters = { "@ArtikalID0" = 1, "@ArtikalID1" = 2, "@ArtikalID2" = 3 }
    /// </code>
    /// </example>
    protected string BuildInClause<T>(
        IEnumerable<T> values,
        string parameterPrefix,
        Dictionary<string, object> parameters)
    {
        var valuesList = values.ToList();
        var paramNames = new List<string>();

        for (int i = 0; i < valuesList.Count; i++)
        {
            var paramName = $"@{parameterPrefix}{i}";
            paramNames.Add(paramName);
            parameters.Add(paramName, valuesList[i]!);
        }

        return string.Join(",", paramNames);
    }
}
