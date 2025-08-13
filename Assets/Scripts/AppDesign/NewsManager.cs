using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace AppDesign
{
    public class NewsManager : MonoBehaviour
    {
        private const string ApiKey = "0d0794a91b694097be2358c9a5375d30"; // Replace with your actual API key
        private const string ApiUrl = "https://newsapi.org/v2/top-headlines?country=us&apiKey=" + ApiKey;

        public IEnumerator GetNews(System.Action<NewsArticle[]> onNewsReceived)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(ApiUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onNewsReceived?.Invoke(null);
                }
                else
                {
                    NewsApiResponse response = JsonUtility.FromJson<NewsApiResponse>(webRequest.downloadHandler.text);
                    onNewsReceived?.Invoke(response.articles);
                }
            }
        }
    }
}