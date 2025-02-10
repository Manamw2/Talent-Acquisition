namespace HrBackOffice.Models
{
    public class SearchResult
    {
        public List<SearchResultItem> Results { get; set; }
        public bool IsExact { get; set; }
    }
}
