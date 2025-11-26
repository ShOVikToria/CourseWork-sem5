using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Переконайтеся, що цей using є

namespace ScanwordGenerator
{
    public class WordData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("term")]
        public string Term { get; set; }

        [JsonPropertyName("questions")]
        public List<string> Questions { get; set; }

        public string GetRandomQuestion()
        {
            if (Questions == null || Questions.Count == 0) return "No question";
            return Questions[new Random().Next(Questions.Count)];
        }
    }
}