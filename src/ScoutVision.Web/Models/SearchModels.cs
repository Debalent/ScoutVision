namespace ScoutVision.Web.Models
{
    public class SearchResult
    {
        public string Title { get; set; } = "";
        public string Type { get; set; } = "";
        public string Subtitle { get; set; } = "";
        public string Id { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public enum SkeletonType
    {
        PlayerCard,
        Chart,
        Table,
        Dashboard
    }
}