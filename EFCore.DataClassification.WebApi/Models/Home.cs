using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Home {
        public int Id { get; set; }

      // rename
        [DataClassification("Location", "Home Address Updated", SensitivityRank.High)]
        public string Evadress { get; set; } = string.Empty;

        //deleteS
        public int Size { get; set; }

        //change
        [DataClassification("public", "Owner Name", SensitivityRank.High)]
        public string OwnerName { get; set; } = string.Empty;

        //dropp



        //add
        [DataClassification("public", "Year Built", SensitivityRank.Low)]
        public int YearBuilt { get; set; }
    }
}
