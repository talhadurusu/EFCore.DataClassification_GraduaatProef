using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Bike {
        public int Id { get; set; }

        // ✅ Classified - will test Add/Change/Remove
        [DataClassification("Internal", "Bike Brand", SensitivityRank.Low)]
        public string Brand { get; set; } = string.Empty;

        // ✅ Unclassified - mixed-case tests
        public string Type { get; set; } = string.Empty;

        [DataClassification("Public", "Bike Gear Count", SensitivityRank.High)]
        public int GearCount { get; set; }

        [DataClassification("Confidential", "Bike Serial", SensitivityRank.High)]
        public string SerialNumber { get; set; } = string.Empty;

    }
}
