using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Car {

        public int Id { get; set; }
       
        [DataClassification("Public", "Car model", SensitivityRank.None)]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        [DataClassification("Confidential", "Vehicle Identification Number", SensitivityRank.Critical)]
        public string VIN { get; set; } = string.Empty;

        [DataClassification("Confidential", "Unique Car Identifier", SensitivityRank.High)]
        public int UniqueId { get; set; }
    }
}
