using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class NewsUIManager : MonoBehaviour
    {
        private VisualElement _newsContainer;

        public void SetNewsContainer(VisualElement container)
        {
            _newsContainer = container;
        }
        
        public void PopulateNews(NewsArticle[] articles)
        {
            if (articles == null) return;
            int newsCount = 0;
            _newsContainer.Clear();
            foreach (var article in articles)
            {
                var articleElement = new VisualElement();
                switch (newsCount % 3)
                {
                    case 0:
                        articleElement.AddToClassList("news-article01");
                        break;
                    case 1:
                        articleElement.AddToClassList("news-article02");
                        break;
                    case 2:
                        articleElement.AddToClassList("news-article03");
                        break;
                    default:
                        // This should never be hit, but included as good practice
                        articleElement.AddToClassList("news-article01");
                        break;
                }

                newsCount++;

                var title = new Label(article.title);
                title.AddToClassList("news-title");

                var description = new Label(StripHtml(article.description));
                description.AddToClassList("news-description");

                articleElement.Add(title);
                articleElement.Add(description);

                if (!string.IsNullOrEmpty(article.url))
                {
                    var linkIcon = new VisualElement();
                    linkIcon.AddToClassList("news-link-icon");
                    linkIcon.tooltip = "Read full article";
                    linkIcon.RegisterCallback<ClickEvent>(evt => Application.OpenURL(article.url));
                    articleElement.Add(linkIcon);
                }

                _newsContainer.Add(articleElement);
            }
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return Regex.Replace(input, "<.*?>", string.Empty);
        }
    }
}