using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Car {

        public int Id { get; set; }
       
        [DataClassification("Intern", "Car model", SensitivityRank.None)]
        public string Model { get; set; } = string.Empty;

        [DataClassification("Intern", "car relase year", SensitivityRank.None)]
        public int Year { get; set; }

       
        public string VIN { get; set; } = string.Empty;

        [DataClassification("Confidential", "Unique Car Identifier", SensitivityRank.Medium)]
        public int UniqueId { get; set; }
    }
}
