using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Document {
        public int Id { get; set; }

        // CASE: Rank = None → SQL'de RANK yazılmamalı
        [DataClassification("Docs", "Title", SensitivityRank.None)]
        public string Title { get; set; } = string.Empty;

        // EDGE CASE: Classification değişti (Medium -> High)
        [DataClassification("Docs", "Body", SensitivityRank.High)]
        public string Body { get; set; } = string.Empty;

        // EDGE CASE: Column rename (Author -> Writer) + classification değişti
        [DataClassification("Docs", "Writer Name", SensitivityRank.High)]
        public string Writer { get; set; } = string.Empty;

        // CASE: başlangıçta classif. yok → sonradan ekleme testi için
        [DataClassification("Docs", "Summary", SensitivityRank.Low)]
        public string? Summary { get; set; }

        // EDGE CASE: Column silindi (InternalRef kaldırıldı)
    }
}











