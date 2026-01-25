using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Game {

        public int Id { get; set; }

        [DataClassification("Public", "Game Title", SensitivityRank.None)]
        public string Title { get; set; } = string.Empty;

        [DataClassification("Public", "Game Category", SensitivityRank.Low)]
        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [DataClassification("Financial", "Game Price", SensitivityRank.High)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [DataClassification("Internal", "Game Studio", SensitivityRank.Medium)]
        public string? Studio { get; set; }

        [DataClassification("Public", "Release Year", SensitivityRank.None)]
        public int ReleaseYear { get; set; }

        public bool IsMultiplayer { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
