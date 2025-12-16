using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Document {
        public int Id { get; set; }

        // CASE: Rank = None → SQL’de RANK yazılmamalı
        [DataClassification("Docs", "Title", SensitivityRank.None)]
        public string Title { get; set; } = string.Empty;

        // CASE: normal (Rank var)
        [DataClassification("Docs", "Body", SensitivityRank.Medium)]
        public string Body { get; set; } = string.Empty;

        [DataClassification("Docs", "Author", SensitivityRank.Low)]
        public string Author { get; set; } = string.Empty;

        // CASE: başlangıçta classif. yok → sonradan ekleme testi için
        [DataClassification("Docs", "Summary", SensitivityRank.Low)]
        public string? Summary { get; set; }

        // CASE: opsiyonel bir “daha hassas” alan (sonradan kaldırma/değiştirme için)
        [DataClassification("Docs", "InternalRef", SensitivityRank.High)]
        public string? InternalRef { get; set; }
    }
}





