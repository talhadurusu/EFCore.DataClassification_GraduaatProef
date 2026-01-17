using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Document {
        public int Id { get; set; }

        // CASE: Rank = None → SQL’de RANK yazılmamalı
        [DataClassification("Docs", "Title", SensitivityRank.None)]
        public string Title { get; set; } = string.Empty;

        // CASE: normal (Rank var)
        [DataClassification("Docs", "Body", SensitivityRank.High)]
        public string Body { get; set; } = string.Empty;

        // CASE: başlangıçta classif. yok → sonradan ekleme testi için
        public string? Summary { get; set; }

        [DataClassification("Docs", "Reviewer", SensitivityRank.Low)]
        public string? Reviewer { get; set; }

        public DateTime CreatedAt {
            get; set;
        }

    }
}








