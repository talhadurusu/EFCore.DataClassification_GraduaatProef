using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Home {
        public int Id { get; set; }

      
        [DataClassification("Location", "Home Address", SensitivityRank.Low)]
        public string Address { get; set; } = string.Empty;

     
        [DataClassification("Property", "Home SIZE", SensitivityRank.Low)]
        public int Size { get; set; }

      
        [DataClassification("Prive", "Home Owner Name", SensitivityRank.Medium)]
        public string OwnerName { get; set; } = string.Empty;

       

        public decimal Price { get; set; }

        public int YearBuilt { get; set; }
    }
}
