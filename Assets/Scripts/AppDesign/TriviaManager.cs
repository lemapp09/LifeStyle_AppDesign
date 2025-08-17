using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


namespace AppDesign
{
    public class TriviaManager : MonoBehaviour
    {
        private ScrollView _triviaScrollView;

        private const string TriviaApiUrl =
            "https://opentdb.com/api.php?amount=10&category=18&difficulty=easy&type=multiple";

        public void TriviaStart()
        {
            // Call the coroutine with a lambda to handle the returned data.
            StartCoroutine(GetTriviaData(questions =>
            {
                if (questions != null)
                {
                    DisplayQuestions(questions);
                }
                else
                {
                    Debug.LogError("Failed to retrieve questions.");
                }
            }));
        }

        public IEnumerator GetTriviaData(Action<List<TriviaQuestion>> onComplete)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(TriviaApiUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = webRequest.downloadHandler.text;

                    // Deserialize the JSON string using the JsonHelper class.
                    TriviaResponse responseData = JsonConvert.DeserializeObject<TriviaResponse>(jsonResponse);

                    // Pass the list of questions to the onComplete callback.
                    onComplete?.Invoke(responseData.Results);
                }
                else
                {
                    Debug.LogError("Error: " + webRequest.error);
                    onComplete?.Invoke(null);
                }
            }
        }

        public void SetTriviaScrollview(ScrollView triviaScrollView)
        {
            _triviaScrollView = triviaScrollView;
        }

        private void DisplayQuestions(List<TriviaQuestion> questions)
        {
            _triviaScrollView.Clear();

            foreach (var q in questions)
            {
                // Main container
                var questionContainer = new VisualElement();
                questionContainer.AddToClassList("trivia-question-container");

                // Question text
                var questionLabel = new Label(WebUtility.HtmlDecode(q.QuestionText));
                questionLabel.AddToClassList("trivia-question-text");

                // Answers container
                var answersContainer = new VisualElement();
                answersContainer.AddToClassList("trivia-answers-container");

                // Combine answers and shuffle
                List<string> allAnswers = new List<string>(q.IncorrectAnswers);
                allAnswers.Add(q.CorrectAnswer);
                allAnswers.Sort((a, b) => Random.value.CompareTo(0.5f));

                foreach (var answer in allAnswers)
                {
                    var answerLabel = new Label(WebUtility.HtmlDecode(answer));
                    answerLabel.AddToClassList("trivia-answer");

                    // Capture local copy for closure
                    string capturedAnswer = answer;

                    // Register click callback
                    answerLabel.RegisterCallback<ClickEvent>(evt =>
                    {
                        // Clear previous highlight (optional, if you want one try only)
                        foreach (var child in answersContainer.Children())
                            child.RemoveFromClassList("trivia-answer-correct");
                        foreach (var child in answersContainer.Children())
                            child.RemoveFromClassList("trivia-answer-incorrect");

                        if (capturedAnswer == q.CorrectAnswer)
                        {
                            answerLabel.AddToClassList("trivia-answer-correct");
                        }
                        else
                        {
                            answerLabel.AddToClassList("trivia-answer-incorrect");
                        }
                    });

                    answersContainer.Add(answerLabel);
                }

                questionContainer.Add(questionLabel);
                questionContainer.Add(answersContainer);
                _triviaScrollView.Add(questionContainer);
            }
        }
    }
}