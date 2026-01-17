using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class ContactInfo {
        [DataClassification("Contact", "Email Address", SensitivityRank.High)]
        public string Email { get; set; } = string.Empty;

        [DataClassification("Contact", "Phone Number", SensitivityRank.Medium)]
        public string Phone { get; set; } = string.Empty;
    }
}
