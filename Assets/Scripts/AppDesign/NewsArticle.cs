namespace AppDesign
{
    [System.Serializable]
    public class NewsArticle
    {
        public string title;
        public string description;
        public string url;
    }

    [System.Serializable]
    public class NewsApiResponse
    {
        public NewsArticle[] articles;
    }
}