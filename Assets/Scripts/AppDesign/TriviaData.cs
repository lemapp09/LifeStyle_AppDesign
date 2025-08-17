using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AppDesign
{
    [Serializable]
    public class TriviaResponse
    {
        [JsonProperty("results")]
        public List<TriviaQuestion> Results { get; set; }
    }

    [Serializable]
    public class TriviaQuestion
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("difficulty")]
        public string Difficulty { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("question")]
        public string QuestionText { get; set; }

        [JsonProperty("correct_answer")]
        public string CorrectAnswer { get; set; }

        [JsonProperty("incorrect_answers")]
        public List<string> IncorrectAnswers { get; set; }
    }
}