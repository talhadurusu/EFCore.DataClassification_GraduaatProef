using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Game {

        public int Id { get; set; }

        [DataClassification("Private", "Publisher Unique Unit ID", SensitivityRank.High)]
        public string PublisherUnikeUnitID { get; set; }

        [DataClassification("Public", "Game Title", SensitivityRank.Low)]
        public string Title { get; set; }

        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }

        [DataClassification("Internal", "Game Description", SensitivityRank.Low)]
        public string Description { get; set; }

        [DataClassification("Confidential", "Game Price", SensitivityRank.Medium)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
