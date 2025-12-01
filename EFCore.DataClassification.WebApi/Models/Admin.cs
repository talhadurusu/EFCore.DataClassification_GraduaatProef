using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }
        [DataClassification("Confidential", "Admin Name", SensitivityRank.Medium)]
        public string Name { get; set; }
        [DataClassification("Confidential", "Email Address", SensitivityRank.High)]
        public string Email { get; set; }
        [DataClassification("Highly Confidential", "Admin Key", SensitivityRank.Critical)]
        public int Adminkey { get; set; }
    }
}
