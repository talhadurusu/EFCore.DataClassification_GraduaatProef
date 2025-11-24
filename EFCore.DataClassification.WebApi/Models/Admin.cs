using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }
        public string Name { get; set; }


        [DataClassification("Private", "Contact Info", SensitivityRank.Critical)]
        public string Email { get; set; }

        [DataClassification("Private", "Security Info", SensitivityRank.Critical)]
        public int Adminkey { get; set; }
    }
}
