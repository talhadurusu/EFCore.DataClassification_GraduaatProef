using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Document {
        public int Id { get; set; }

        [DataClassification("Docs", "Title", SensitivityRank.None)]
        public string Title { get; set; } = string.Empty;

        [DataClassification("Docs", "Body", SensitivityRank.High)]
        public string Body { get; set; } = string.Empty;

        [DataClassification("Docs", "Writer Name", SensitivityRank.High)]
        public string Writer { get; set; } = string.Empty;

        [DataClassification("Docs", "Summary", SensitivityRank.Low)]
        public string? Summary { get; set; }
    }
}











