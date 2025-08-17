using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace AppDesign
{
    public class SportsManager : MonoBehaviour
    {
        private const string ApiKey = "0d0794a91b694097be2358c9a5375d30"; // Replace with your actual API key
        private const string ApiUrl = "https://newsapi.org/v2/everything?q=sports&apiKey=" + ApiKey;

        public IEnumerator GetSports(System.Action<NewsArticle[]> onSportsReceived)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(ApiUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.DataProcessingError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onSportsReceived?.Invoke(null);
                }
                else
                {
                    NewsApiResponse response = JsonUtility.FromJson<NewsApiResponse>(webRequest.downloadHandler.text);
                    onSportsReceived?.Invoke(response.articles);
                }
            }
        }
    }
}