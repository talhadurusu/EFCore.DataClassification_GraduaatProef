using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Bike {
        public int Id { get; set; }

        public string Brand { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;

        public int GearCount { get; set; }

        [DataClassification("Confidential", "Bike Serial Number", SensitivityRank.Medium)]
        public string SerialNumber { get; set; } = string.Empty;

        public string? Color { get; set; }
    }
}
