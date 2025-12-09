using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Home {


        public int Id {get; set;}
        [DataClassification("Private", "Home Address", SensitivityRank.Medium)]
        public string Address { get; set; } = string.Empty;

        [DataClassification("Public", "Home Size", SensitivityRank.Low)]
        public int Size { get; set; }



        [DataClassification("Confidential", "Owner Name", SensitivityRank.High)]
        public string OwnerName { get; set; } = string.Empty;
    }
}
