using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Game {

        public int Id { get; set; }

        [DataClassification("Very Confidential", "Publisher Unique Unit ID", SensitivityRank.Medium)]
        public string PublisherUnikeUnitID { get; set; } = string.Empty;

        [DataClassification("Public", "Game Title", SensitivityRank.None)]
        public string Title { get; set; } = string.Empty;
        [DataClassification("Public", "Game Genre", SensitivityRank.None)]
        public string Genre { get; set; } = string.Empty;

        [DataClassification("Intern", "Release Date", SensitivityRank.None)]
        public DateTime ReleaseDate { get; set; }

        [DataClassification("Public", "Game Description", SensitivityRank.None)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
