using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Bike {
        public int Id { get; set; }

        [DataClassification("Public", "Bike Brand", SensitivityRank.Low)]
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        [DataClassification("Confidential", "Bike Owner", SensitivityRank.High)]
        public string Owner { get; set; } = string.Empty;

        public int GearCount { get; set; }
    }
}
