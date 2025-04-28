namespace hrconnectbackend.Helper
{
    // Adding a constraint to ensure that T implements IEnumerable<T>
    public class PageList<T> where T : IEnumerable<T>
    {
        public List<T> Items { get; set; }
        private int PageIndex { get; set; }
        private int PageSize { get; set; }
        private int TotalCount { get; set; }
        private int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); // Calculate total pages

        // Constructor for PageList
        private PageList(List<T> items, int pageIndex, int pageSize, int totalCount)
        {
            Items = items ?? new List<T>();
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        // Method to get the next page
        public static PageList<T> GetNextPage(List<T> allItems, int pageIndex, int pageSize)
        {
            if (pageIndex < 0) pageIndex = 0;
            var skipCount = pageIndex * pageSize;
            var items = allItems.Skip(skipCount).Take(pageSize).ToList();
            var totalCount = allItems.Count;

            return new PageList<T>(items, pageIndex, pageSize, totalCount);
        }

        // Method to get the previous page
        public static PageList<T> GetPreviousPage(List<T> allItems, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;  // Prevent going below page 1
            var skipCount = (pageIndex - 1) * pageSize; // Calculate the start index for the previous page
            var items = allItems.Skip(skipCount).Take(pageSize).ToList();
            var totalCount = allItems.Count;

            return new PageList<T>(items, pageIndex, pageSize, totalCount);
        }

        // Method to check if there is a next page
        public bool HasNextPage()
        {
            return PageIndex < TotalPages - 1;
        }

        // Method to check if there is a previous page
        public bool HasPreviousPage()
        {
            return PageIndex > 0;
        }
    }

    // Extension method for List<T> to get the next and previous pages
    public static class ListExtensions
    {
        public static PageList<T> NextPage<T>(this List<T> items, int pageIndex, int pageSize) where T : IEnumerable<T>
        {
            return PageList<T>.GetNextPage(items, pageIndex, pageSize);
        }

        public static PageList<T> PreviousPage<T>(this List<T> items, int pageIndex, int pageSize) where T : IEnumerable<T>
        {
            return PageList<T>.GetPreviousPage(items, pageIndex, pageSize);
        }
    }
}
