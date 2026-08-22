namespace TaskTaskerAPI.Utilities
{
    public class PagedResult<T>
    {
        public List<T> items { get; set; } = new List<T>();

        public int page { get; set; }

        public int pageSize { get; set; }

        public int total { get; set; }

        public int totalPages => this.pageSize > 0
            ? (int)Math.Ceiling((double)this.total / this.pageSize)
            : 0;

        public PagedResult() { }

        public PagedResult(List<T> items, int page, int pageSize, int total)
        {
            this.items = items;
            this.page = page;
            this.pageSize = pageSize;
            this.total = total;
        }
    }
}
