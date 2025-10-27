using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Helpers
{

    public static class PaginationHelper
    {

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, int pageNumber, int pageSize)
        {

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;


            var totalCount = await query.CountAsync();

            // Lấy dữ liệu cho trang hiện tại
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        public class PagedResult<T>
        {
            public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
            public int PageNumber { get; init; }
            public int PageSize { get; init; }
            public int TotalCount { get; init; }
            public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
            public bool HasPrevious => PageNumber > 1;
            public bool HasNext => PageNumber < TotalPages;
        }

    }
    }
