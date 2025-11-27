using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ScanwordGenerator
{
    public class WordImages
    {
        [JsonPropertyName("square")]
        public string Square { get; set; }     // Файл у папці animals_s

        [JsonPropertyName("horizontal")]
        public string Horizontal { get; set; } // Файл у папці animal_h

        [JsonPropertyName("vertical")]
        public string Vertical { get; set; }   // Файл у папці animal_v

        public bool HasAny => !string.IsNullOrEmpty(Square) || !string.IsNullOrEmpty(Horizontal) || !string.IsNullOrEmpty(Vertical);
    }

    public class WordData
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("term")] public string Term { get; set; }
        [JsonPropertyName("questions")] public List<string> Questions { get; set; }
        [JsonPropertyName("images")] public WordImages Images { get; set; }

        public string GetRandomQuestion()
        {
            if (Questions == null || Questions.Count == 0) return "No question";
            return Questions[new Random().Next(Questions.Count)];
        }
    }
}