using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Game {

        public int Id { get; set; }

        // PHASE 2 - SMOKE TEST 11: Column drop (PublisherID silindi)

        // PHASE 2 - SMOKE TEST 12: DC add (Title'a DC eklendi)
        [DataClassification("Public", "Game Title", SensitivityRank.None)]
        public string Title { get; set; } = string.Empty;

        // PHASE 2 - SMOKE TEST 13: Column rename (Genre -> Category) + DC change (None -> Low)
        [DataClassification("Public", "Game Category", SensitivityRank.Low)]
        public string Category { get; set; } = string.Empty;

        // PHASE 2 - SMOKE TEST 14: AlterColumn type (Description: string? -> string) + DC remove
        public string Description { get; set; } = string.Empty;

        // PHASE 2 - SMOKE TEST 15: DC change (Price: Medium -> High)
        [DataClassification("Financial", "Game Price", SensitivityRank.High)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // PHASE 2 - SMOKE TEST 16: Column drop (Rating silindi)

        // PHASE 2 - SMOKE TEST 17: Column rename (Developer -> Studio) + DC change (Low -> Medium)
        [DataClassification("Internal", "Game Studio", SensitivityRank.Medium)]
        public string? Studio { get; set; }

        // PHASE 2 - SMOKE TEST 18: Column add + DC
        [DataClassification("Public", "Release Year", SensitivityRank.None)]
        public int ReleaseYear { get; set; }

        // PHASE 2 - SMOKE TEST 19: Column add without DC
        public bool IsMultiplayer { get; set; }
    }
}
