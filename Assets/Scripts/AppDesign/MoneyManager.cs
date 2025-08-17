using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class MoneyManager : MonoBehaviour
    {
        private const string ExchangeApiUrl = "https://open.er-api.com/v6/latest/USD";
        private ScrollView _moneyScrollview;
        private Label _moneyLastUpdated;

        void Start()
        {
            StartCoroutine(GetExchangeRates());
        }

        private IEnumerator GetExchangeRates()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(ExchangeApiUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonText = webRequest.downloadHandler.text;

                    try
                    {
                        // Use JsonConvert to deserialize the JSON into your class.
                        MoneyData responseData = JsonConvert.DeserializeObject<MoneyData>(jsonText);

                        if (responseData != null && responseData.Rates != null)
                        {
                            // Debug.Log($"Base Currency: {responseData.BaseCode}");

                            // Example: Print the rate for EUR
                            if (responseData.Rates.ContainsKey("EUR"))
                            {
                                // Debug.Log($"1 {responseData.BaseCode} = {responseData.Rates["EUR"]} EUR");
                                DisplayCurrencyRates(responseData.Rates, responseData.TimeLastUpdateUtc);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Debug.LogError("Failed to deserialize JSON: " + ex.Message);
                    }
                }
                else
                {
                    Debug.LogError($"Error retrieving data: {webRequest.error}");
                }
            }
        }

        private void DisplayCurrencyRates(Dictionary<string, double> data, string _timeLastUpdated)
        {
            _moneyLastUpdated.text = _timeLastUpdated;
            // Clear any existing content in the ScrollView.
            _moneyScrollview.contentContainer.Clear();

            // Convert the dictionary to a list to iterate in a fixed order.
            var dataList = new List<KeyValuePair<string, double>>(data);
        
            for (int i = 0; i < dataList.Count; i += 3)
            {
                var groupContainer = new VisualElement();
                groupContainer.name = "money-group-rate-container";
                groupContainer.AddToClassList("money-group-rate-container");

                for (int j = 0; j < 3; j++)
                {
                    if (i + j < dataList.Count)
                    {
                        var currencyPair = dataList[i + j];

                        var itemContainer = new VisualElement();
                        itemContainer.name = "money-rate-container";
                        itemContainer.AddToClassList("money-rate-container");
                    
                        var tradingSymbolLabel = new Label(currencyPair.Key);
                        tradingSymbolLabel.name = "money-tradingSymbol";
                        tradingSymbolLabel.AddToClassList("money-tradingSymbol");
                    
                        var exchangeRateLabel = new Label(currencyPair.Value.ToString("F2"));
                        exchangeRateLabel.name = "money-exchangeRate";
                        exchangeRateLabel.AddToClassList("money-exchangeRate");

                        itemContainer.Add(tradingSymbolLabel);
                        itemContainer.Add(exchangeRateLabel);
                        groupContainer.Add(itemContainer);
                    }
                }

                // Add the group container to the ScrollView's content container.
                _moneyScrollview.contentContainer.Add(groupContainer);
            }
        }

        public void SetMoneyScrollview(ScrollView scrollView, Label moneyLastUpdated)
        {
            _moneyScrollview = scrollView;
            _moneyLastUpdated = moneyLastUpdated;
        }
    }
}