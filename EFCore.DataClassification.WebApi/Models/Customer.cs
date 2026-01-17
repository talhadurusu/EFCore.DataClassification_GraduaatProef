using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Customer {
        public int Id { get; set; }

        [DataClassification("Contact", "Customer Full Name", SensitivityRank.Medium)]
        public string FullName { get; set; } = string.Empty;

        [DataClassification("Contact", "Email Address", SensitivityRank.High)]
        public string? Email { get; set; }

        [DataClassification("Address", "Mailing Address", SensitivityRank.None)]
        public string Address { get; set; } = string.Empty;

        [DataClassification("Contact", "Phone Number", SensitivityRank.High)]
        public string? PhoneNumber { get; set; }
    }
}











