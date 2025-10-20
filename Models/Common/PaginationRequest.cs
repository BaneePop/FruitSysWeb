namespace FruitSysWeb.Models.Common
{
    /// <summary>
    /// Request parameters for pagination
    /// </summary>
    public class PaginationRequest
    {
        private int _pageNumber = 1;
        private int _pageSize = 50;

        /// <summary>
        /// Current page number (1-based, default: 1)
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Number of items per page (default: 50, max: 1000)
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 50 : (value > 1000 ? 1000 : value);
        }

        /// <summary>
        /// Calculate OFFSET for SQL query
        /// </summary>
        public int Offset => (PageNumber - 1) * PageSize;

        /// <summary>
        /// Default pagination (page 1, 50 items)
        /// </summary>
        public static PaginationRequest Default => new();

        /// <summary>
        /// No pagination (page 1, max items)
        /// </summary>
        public static PaginationRequest All => new() { PageSize = 1000 };
    }
}
