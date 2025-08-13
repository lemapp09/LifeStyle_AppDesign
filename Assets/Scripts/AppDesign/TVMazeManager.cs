using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AppDesign
{
    public class TVMazeManager : MonoBehaviour
    {
        private const string ApiUrl = "https://api.tvmaze.com";

        public IEnumerator SearchShows(string query, System.Action<List<Show>> onComplete)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"{ApiUrl}/search/shows?q={query}"))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    ShowSearchResult[] searchResults = JsonHelper.FromJson<ShowSearchResult>(jsonResponse);
                    List<Show> shows = new List<Show>();
                    foreach (var result in searchResults)
                    {
                        shows.Add(result.show);
                    }
                    onComplete?.Invoke(shows);
                }
                else
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onComplete?.Invoke(null);
                }
            }
        }

        public IEnumerator GetEpisodes(int showId, System.Action<List<Episode>> onComplete)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"{ApiUrl}/shows/{showId}/episodes"))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    List<Episode> episodes = JsonHelper.FromJson<Episode>(jsonResponse).ToList();
                    onComplete?.Invoke(episodes);
                }
                else
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onComplete?.Invoke(null);
                }
            }
        }

        public IEnumerator GetCast(int showId, System.Action<List<Cast>> onComplete)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"{ApiUrl}/shows/{showId}/cast"))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    List<Cast> cast = JsonHelper.FromJson<Cast>(jsonResponse).ToList();
                    onComplete?.Invoke(cast);
                }
                else
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onComplete?.Invoke(null);
                }
            }
        }
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"items\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
}

