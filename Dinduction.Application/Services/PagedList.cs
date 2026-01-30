using System;
using System.Collections.Generic;
using System.Linq;
using Dinduction.Application.Interfaces;

namespace Dinduction.Application.Services;

/// <summary>
/// Paged list implementation for pagination
/// </summary>
/// <typeparam name="T">Type of items</typeparam>
[Serializable]
public class PagedList<T> : List<T>, IPagedList<T>
{
    /// <summary>
    /// Constructor from IQueryable source
    /// </summary>
    /// <param name="source">Queryable source</param>
    /// <param name="pageIndex">Page index (0-based)</param>
    /// <param name="pageSize">Page size</param>
    public PagedList(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var total = source.Count();
        TotalCount = total;
        TotalPages = total / pageSize;

        if (total % pageSize > 0)
            TotalPages++;

        PageSize = pageSize;
        PageIndex = pageIndex;
        AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
    }

    /// <summary>
    /// Constructor from IList source
    /// </summary>
    /// <param name="source">List source</param>
    /// <param name="pageIndex">Page index (0-based)</param>
    /// <param name="pageSize">Page size</param>
    public PagedList(IList<T> source, int pageIndex, int pageSize)
    {
        TotalCount = source.Count;
        
        // Ceiling division for total pages
        TotalPages = (int)Math.Ceiling(TotalCount / (double)pageSize);

        PageSize = pageSize;
        PageIndex = pageIndex;

        // Get data for current page
        AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
    }

    /// <summary>
    /// Constructor with explicit total count
    /// </summary>
    /// <param name="source">Enumerable source</param>
    /// <param name="pageIndex">Page index (0-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="totalCount">Total count</param>
    public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
    {
        TotalCount = totalCount;
        TotalPages = totalCount / pageSize;

        if (totalCount % pageSize > 0)
            TotalPages++;

        PageSize = pageSize;
        PageIndex = pageIndex;
        AddRange(source);
    }

    public int PageIndex { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }

    public bool HasPreviousPage => PageIndex > 0;
    public bool HasNextPage => PageIndex + 1 < TotalPages;
}