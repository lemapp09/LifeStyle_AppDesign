using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class FunFactsManager : MonoBehaviour
    {
        private const string ApiUrl = "https://api.viewbits.com/v1/uselessfacts?mode=random";
        private Label _funfactsText;
        private Label _funfactsSource;
        
        public void FunFactsStart()
        {
            StartCoroutine(GetRandomFact());
        }

        private IEnumerator GetRandomFact()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(ApiUrl))
            {
                // Wait for the web request to complete
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // Get the JSON string from the web request
                    string jsonText = webRequest.downloadHandler.text;
                    Debug.Log($"Received JSON: {jsonText}");

                    try
                    {
                        // Deserialize the JSON into the UselessFact C# object
                        FunFactsData fact = JsonConvert.DeserializeObject<FunFactsData>(jsonText);
                    
                        if (fact != null)
                        {
                            // Now you can use the data from the object
                            //Debug.Log($"Fact: {fact.Text}");
                            // Debug.Log($"Source: {fact.Source}");

                            _funfactsText.text = fact.Text;
                            _funfactsSource.text = fact.Source;
                        }
                    }
                    catch (JsonSerializationException ex)
                    {
                        Debug.LogError($"Failed to deserialize JSON: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"Error fetching data: {webRequest.error}");
                }
            }
        }

        public void SetLabels(Label funfactsText, Label funfactsSource)
        {
            _funfactsText = funfactsText;
            _funfactsSource = funfactsSource;
        }
    }
}