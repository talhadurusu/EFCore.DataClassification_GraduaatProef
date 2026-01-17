using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Car {

        public int Id { get; set; }
       
        [DataClassification("Public", "Car Model Name", SensitivityRank.Low)]
        public string? Model { get; set; }

        [DataClassification("Internal", "Car Manufacturing Year", SensitivityRank.Low)]
        public string Year { get; set; } = string.Empty;

        [DataClassification("Confidential", "Vehicle Identification Number", SensitivityRank.Critical)]
        public string VehicleIdentificationNumber { get; set; } = string.Empty;

        [DataClassification("Confidential", "Unique Car Identifier", SensitivityRank.Medium)]
        public int UniqueId { get; set; }

        [DataClassification("Confidential", "Car Owner Name", SensitivityRank.High)]
        public string OwnerName { get; set; } = string.Empty;

        [DataClassification("Confidential", "Car Owner Email", SensitivityRank.High)]
        public string? OwnerEmail { get; set; }

        public string ColorPreference { get; set; } = string.Empty;

        [DataClassification("Confidential", "Car Notes", SensitivityRank.High)]
        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
