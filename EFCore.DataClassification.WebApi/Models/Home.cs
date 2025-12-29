using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Home {


        public int Id {get; set;}

        [DataClassification("Location", "Home Address", SensitivityRank.Medium)]
        public string Address { get; set; } = string.Empty;

        public int Size { get; set; }



        public string OwnerName { get; set; } = string.Empty;
    }
}
