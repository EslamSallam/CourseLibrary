﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CourseLibrary.API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious =>  (CurrentPage > 0);
        public bool HasNext => (CurrentPage < TotalPages);

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count/ (double)pageSize);
            AddRange(items);
        }

        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList();
            return new PagedList<T>(items,count,pageNumber,pageSize);
        }

    }
}