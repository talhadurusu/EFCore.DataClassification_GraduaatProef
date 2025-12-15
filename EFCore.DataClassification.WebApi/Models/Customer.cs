using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Customer {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [DataClassification("Contact", "Email Address", SensitivityRank.High)]
        public string Email { get; set; } = string.Empty;

        [DataClassification("Address", "Mailing Address", SensitivityRank.None)]
        public string Address { get; set; } = string.Empty;
    }
}

