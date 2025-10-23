﻿namespace SEP490_BE.DAL.DTOs.Common
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}
