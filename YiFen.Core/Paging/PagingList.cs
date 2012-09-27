﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace YiFen.Core
{
    public class PagingList<T> : IPagingList<T>
    {
        private readonly IList<T> _dataSource;

        /// <summary>
        /// Creates a new instance of CustomPagination
        /// </summary>
        /// <param name="dataSource">A pre-paged slice of data</param>
        /// <param name="pageNumber">The current page number</param>
        /// <param name="pageSize">The number of items per page</param>
        /// <param name="totalItems">The total number of items in the overall datasource</param>
        public PagingList(IEnumerable<T> dataSource, int pageNumber, int pageSize, int totalItems)
        {
            _dataSource = dataSource.ToList();
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItems = totalItems;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dataSource.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalItems { get; private set; }

        public int TotalPages
        {
            get { return (int)Math.Ceiling(((double)TotalItems) / PageSize); }
        }

        public int FirstItem
        {
            get
            {
                return ((PageNumber - 1) * PageSize) + 1;
            }
        }

        public int LastItem
        {
            get { return FirstItem + _dataSource.Count - 1; }
        }

        public bool HasPreviousPage
        {
            get { return PageNumber > 1; }
        }

        public bool HasNextPage
        {
            get { return PageNumber < TotalPages; }
        }
    }
}
