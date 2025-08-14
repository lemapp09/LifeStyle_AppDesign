using System;
using UnityEditor;

namespace AppDesign
{
    public class WeatherForecast
    {
        [Serializable]
        public class WeatherRoot
        {
            public Current current;
            public FilePathAttribute.Location location;
        }

        [Serializable]
        public class Current
        {
            public string last_updated; //Local time when the real time data was updated.
            public int last_updated_epoch; //Local time when the real time data was updated in unix time.
            public float temp_c; //Temperature in Celsius
            public float temp_f; //Temperature in Fahrenheit
            public float feelslike_c; //Feels like temperature in Celsius
            public float feelslike_f; //Feels like temperature in Cahrenheit
            public float windchill_c; //Windchill temperature in Celcius
            public float windchill_f; //Windchill temperature in Cahrenheit
            public float heatindex_c; //Heat index in Celcius
            public float heatindex_f; //Heat index in Cahrenheit
            public float dewpoint_c; //Dew point in Celcius
            public float dewpoint_f; //Dew point in Fahrenheit
            public float wind_mph; //Wind speed in miles per hour
            public float wind_kph; //Wind speed in kilometer per hour
            public int wind_degree; //Wind direction in degrees
            public string wind_dir; //Wind direction as 16 point compass. e.g.: NSW
            public float pressure_mb; //Pressure in millibars
            public float pressure_in; //Pressure in inches
            public float precip_mm; //Precipitation amount in millimeters
            public float precip_in; //Precipitation amount in inches
            public int humidity; //Humidity as percentage
            public int cloud; //Cloud cover as percentage
            public int is_day; //1 = Yes 0 = No -- Whether to show day condition icon or night icon
            public float uv; //UV Index
            public float gust_mph; //Wind gust in miles per hour
            public float gust_kph; //Wind gust in kilometer per hour

            public Condition condition;
            // Add more fields as per the WeatherAPI response
        }

        [Serializable]
        public class Condition
        {
            public string text;
            public string icon;
            public int code;
        }

    }
}