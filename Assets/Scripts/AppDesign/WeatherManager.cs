using System;
using System.Collections;
using System.Collections.Generic;
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
        
        // This method returns the proper icon string based on iconCode and isDay (1=Day, 0=Night)
        public static string GetWeatherIcon(int iconCode, int isDay)
        {
            // Define icon names you provided
            // Clear, Cloudy, HeavyRain, LightPrecip, PartlyCloud_Day, PartlyCloud_Night,
            // PartlyHeavyRain, PartlyRaining, PartlySnowing, Raining, Snowing,
            // Sunny, ThunderStorm, Windy

            // Mapping WeatherAPI codes to your icon names
            // Some codes differ by day/night, handled accordingly
            var iconMap = new Dictionary<int, string>
            {
                {1006, "Cloudy"},
                {1009, "Cloudy"},  // Overcast
                {1030, "LightPrecip"}, // Mist
                {1063, "PartlyRaining"}, // Patchy rain Possible
                {1066, "PartlySnowng"}, // Patchy snow Possible
                {1069, "PartlySnowing"}, // Patchy Sleet Possible
                {1072, "PartlySnowing"}, // Patchy freezing drizzle Possible
                {1087, "ThunderStorm"},
                {1114, "Snowing"}, // Blowing Snow
                {1117, "Snowing"}, // Blizzard
                {1135, "LightPrecip"}, // Fog
                {1147, "LightPrecip"}, // Freezing Fog
                {1150, "LightPrecip"}, // Patchy light drizzle
                {1153, "LightPrecip"}, // Light drizzle
                {1168, "LightPrecip"}, // Freezing drizzle
                {1171, "LightPrecip"}, // Heavy Freezing drizzle
                {1180, "LightPrecip"}, // Patchy Light Rain
                {1183, "LightPrecip"}, // Light Rain
                {1186, "PartlyRaining"}, // Moderate rain at times
                {1189, "Raining"}, // Moderate rain
                {1192, "HeavyRain"},
                {1195, "HeavyRain"},
                {1198, "LightPrecip"}, // Light Freezing Rain
                {1201, "Raining"}, // Moderate or Heavy Freezing Rain
                {1204, "LightPrecip"}, //Light Sleet
                {1207, "Raining"}, //Moderate or Heavy Sleet
                {1210, "PartlySnowing"}, // Patchy Light Snow
                {1213, "Snowing"}, // Light Snow
                {1216, "PartlySnowing"}, // Patchy Moderate Snow
                {1225, "Snowing"}, // Heavy Snow
                {1237, "Ice Pellets"}, // Heavy Snow
                {1240, "PartlyRaining"}, // Light Rain Showers
                {1243, "PartlyRaining"}, // Moderate or Heavy Rain Showers
                {1246, "HeavyRain"}, // Torrential rain showers
                {1249, "LightPrecip"}, // Light Sleet Showers
                {1252, "LightPrecip"}, // Moderate or heavy Sleet Showers
                {1255, "PartlySnowing"}, // Light Snow Showers
                {1258, "Snowing"}, // Moderate or heavy Snow Showers
                {1161, "LightPrecip"}, // Light showers of ice pellets
                {1264, "Snowing"}, // Moderate or heavy showers of ice pellets
                {1273, "PartlyHeavyRain"},
                {1276, "ThunderStorm"},
                {1279, "ThunderStorm"}, // Patchy light snow with thunder
                {1282, "ThunderStorm"} // Moderate or heavy snow with thunder
            };

            // Codes with different day/night icons
            if (iconCode == 1000) // Clear / Sunny
            {
                return isDay == 1 ? "Sunny" : "Clear";
            }

            if (iconCode == 1003) // Partly cloudy day/night variants
            {
                return isDay == 1 ? "PartlyCloud_Day" : "PartlyCloud_Night";
            }

            // Fallback to dictionary
            if (iconMap.ContainsKey(iconCode))
            {
                return iconMap[iconCode];
            }

            // Default fallback icon if unknown code
            return "Clear";
        }
    }
}
