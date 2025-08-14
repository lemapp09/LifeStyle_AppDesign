using System;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace AppDesign
{
    public class WeatherManager : MonoBehaviour
    {
        [Header("Request Settings")] 
        private string apiKey = "cdb938738a1a412d928185206241507";

        [Serializable]
        public class WeatherAPI_Response
        {
            public string lastUpdated;
            public float rain;
            public string condition;
            public int iconCode;
            public float tempF;
            public float tempC;
            public float feelslikeF;
            public float feelslikeC;
            public float windchillF;
            public float windchillC;
            public float windMPH;
            public float windKPH;
            public float fog;
            public float lightning;
        }

        // ✅ Event to notify when weather data has been retrieved
        public event Action<WeatherForecast.WeatherRoot> OnWeatherRetrieved;

        private async Task<WeatherForecast.WeatherRoot> GetWeatherDataAsync(string city)
        {
            string url = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={city}";
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();

                        var weatherData = JsonUtility.FromJson<WeatherForecast.WeatherRoot>(json);
                        return weatherData;
                    }
                    else
                    {
                        Debug.LogWarning("API Error: " + response.ReasonPhrase);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Request Failed: " + ex.Message);
                }
            }
            return null;
        }

        public IEnumerator GetWeather(string _cityName)
        {
            var weatherDataTask = GetWeatherDataAsync(_cityName);
            yield return new WaitUntil(() => weatherDataTask.IsCompleted);

            var weatherData = weatherDataTask.Result;
            if (weatherData != null)
            {
                // ✅ Trigger event so AppManager (or anyone) can respond
                OnWeatherRetrieved?.Invoke(weatherData);
            }
        }
    }
}
