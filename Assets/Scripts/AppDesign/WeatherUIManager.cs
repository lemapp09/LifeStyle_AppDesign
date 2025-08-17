using UnityEngine;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class WeatherUIManager : MonoBehaviour
    {
        
        private  VisualElement _weatherContainer;

        public void SetWeatherContainer(VisualElement container)
        {
            _weatherContainer = container;
        }
        public void PopulateWeather(WeatherForecast.WeatherRoot weatherData)
        {
            _weatherContainer.Clear();
            
            _weatherContainer.Add(CreateWeatherDisplay(weatherData));
        }

        public VisualElement CreateWeatherDisplay(WeatherForecast.WeatherRoot weatherData)
        {
            // The main container for all weather information
            var mainContainer = new VisualElement();
            mainContainer.style.flexDirection = FlexDirection.Row; // Horizontal layout

            // The left-side container for the large temperature display and "FEELS LIKE" text
            var temperatureContainer = new VisualElement();
            temperatureContainer.style.flexGrow = 1; // Allows it to take up available space
            temperatureContainer.style.alignItems = Align.Center;
            
            // Weather Icon
            var weatherIconContainer = new VisualElement();
            var icon = Resources.Load<Texture2D>("WeatherIcons/" + WeatherManager.GetWeatherIcon(weatherData.current.condition.code, weatherData.current.is_day));
            weatherIconContainer.style.backgroundImage = new StyleBackground(icon);
            weatherIconContainer.AddToClassList("weather-icon");
            temperatureContainer.Add(weatherIconContainer);

            // The large temperature value
            var temperatureLabel = new Label(weatherData.current.temp_f.ToString() + "°");
            temperatureLabel.style.fontSize = 200;
            //temperatureLabel.style.unityFontWeight = FontWeight.Bold;
            temperatureContainer.Add(temperatureLabel);

            // The "FEELS LIKE" text
            var feelsLikeLabel = new Label("FEELS LIKE: " + weatherData.current.feelslike_f.ToString() + "°");
            feelsLikeLabel.style.fontSize = 48;
            temperatureContainer.Add(feelsLikeLabel);

            mainContainer.Add(temperatureContainer);

            // The right-side container for the detailed conditions
            var detailsContainer = new VisualElement();
            detailsContainer.style.flexGrow = 1;

            // Function to create a detail line (e.g., "WIND: " + weatherData.current.wind_dir + "weatherData.current.wind_mph.ToString() " +  + " MPH")
            void AddDetailLine(string labelText)
            {
                var detailLabel = new Label(labelText);
                detailLabel.style.fontSize = 48;
                detailsContainer.Add(detailLabel);
            }

            AddDetailLine(
                "WIND: " + weatherData.current.wind_dir + " "+ weatherData.current.wind_mph.ToString()  + " MPH");
            AddDetailLine("PRESSURE: " + weatherData.current.pressure_in.ToString() + "\"");
            AddDetailLine("DEWPOINT: " + weatherData.current.dewpoint_f.ToString() + "°");
            AddDetailLine("HUMIDITY: " + weatherData.current.humidity.ToString() + "%");

            mainContainer.Add(detailsContainer);

            // The lower row container for weatherAPI's logo
            var logoContainer = new VisualElement();
            logoContainer.style.flexGrow = 1; // Allows it to take up available space
            logoContainer.style.alignItems = Align.Center;
            
            // Weather API logo
            var logo = Resources.Load<Texture2D>("WeatherIcons/weatherapi_logo");
            logoContainer.style.backgroundImage = new StyleBackground(logo);
            logoContainer.RegisterCallback<ClickEvent>(evt => Application.OpenURL("https://www.weatherapi.com/"));
            logoContainer.AddToClassList("weather-logo");
            
            var displayContainer = new VisualElement();
            displayContainer.style.flexGrow = 1;
            displayContainer.style.alignItems = Align.FlexEnd;
            displayContainer.Add(mainContainer);
            displayContainer.Add(logoContainer);

            return displayContainer;
        }
    }
}