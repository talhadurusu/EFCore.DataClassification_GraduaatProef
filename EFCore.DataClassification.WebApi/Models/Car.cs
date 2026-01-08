using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Car {

        public int Id { get; set; }
       
        // EDGE CASE 1: Nullable değişikliği + classification ekleme (Model: NOT NULL -> NULL + classification ekle)
        [DataClassification("Public", "Car Model Name", SensitivityRank.Low)]
        public string? Model { get; set; }

        // EDGE CASE 2: Column type değişikliği + classification değişikliği (Year: int -> string + rank None -> Low)
        [DataClassification("Internal", "Car Manufacturing Year", SensitivityRank.Low)]
        public string Year { get; set; } = string.Empty;

        // EDGE CASE 3: Rename + classification değişikliği birlikte (VIN -> VehicleIdentificationNumber, rank High -> Critical)
        [DataClassification("Confidential", "Vehicle Identification Number", SensitivityRank.Critical)]
        public string VehicleIdentificationNumber { get; set; } = string.Empty;

        // CASE: Medium sensitivity (unique identifier)
        [DataClassification("Confidential", "Unique Car Identifier", SensitivityRank.Medium)]
        public int UniqueId { get; set; }

        // CASE: High sensitivity (PII - Owner information)
        [DataClassification("Confidential", "Car Owner Name", SensitivityRank.High)]
        public string OwnerName { get; set; } = string.Empty;

        // CASE: Nullable + High classification
        [DataClassification("Confidential", "Car Owner Email", SensitivityRank.High)]
        public string? OwnerEmail { get; set; }

        // CASE: Unclassified field (classification kaldırıldı - test case)
        public string ColorPreference { get; set; } = string.Empty;

        // CASE: Classification değişti (Low -> High) - test case
        [DataClassification("Confidential", "Car Notes", SensitivityRank.High)]
        public string? Notes { get; set; }

        // CASE: Column silindi (LicensePlate kaldırıldı - test case)

        // CASE: Unclassified field (library bunu görmez, migration'a satır düşmez)
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
