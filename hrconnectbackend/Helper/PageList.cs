using hrconnectbackend.Models.EmployeeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using hrconnectbackend.Models;

namespace hrconnectbackend.Helper
{
    public class PageList<T>(List<T> items, int pageIndex, int pageSize, int totalCount)
    {
        public List<T> Items { get; set; } = items;
        public int PageIndex { get; set; } = pageIndex;
        public int PageSize { get; set; } = pageSize;
        public int TotalCount { get; set; } = totalCount;

        // Constructor for PageList

        // This will be used to get the next page
        public static PageList<T> GetNextPage(List<T> allItems, int pageIndex, int pageSize)
        {
            // Calculate the start and end index for the next page
            var skipCount = pageIndex * pageSize;
            var items = allItems.Skip(skipCount).Take(pageSize).ToList();
            var totalCount = allItems.Count;

            return new PageList<T>(items, pageIndex, pageSize, totalCount);
        }

        public static PageList<T> GetPreviousPage(List<T> allItems, int pageIndex, int pageSize)
        {
            var skipCount = pageIndex * pageSize;
            var items = allItems.Take(skipCount).ToList();
            var totalCount = allItems.Count;
            
            return new PageList<T>(items, pageIndex, pageSize, totalCount);
        }
    }

    // Extension method for List<Employee> to get the next page
    public static class ListExtensions
    {
        public static PageList<Employee> NextPage(this List<Employee> employees, int pageIndex, int pageSize)
        {
            return PageList<Employee>.GetNextPage(employees, pageIndex, pageSize);
        }
        
        public static PageList<Employee> PreviousPage(this List<Employee> employees, int pageIndex, int pageSize)
        {
            return PageList<Employee>.GetPreviousPage(employees, pageIndex, pageSize);
        }
    }
}