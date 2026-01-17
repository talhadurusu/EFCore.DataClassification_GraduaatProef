using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public abstract class PersonBase {
        public int Id { get; set; }

        [DataClassification("Personal", "Full Name", SensitivityRank.Medium)]
        public string FullName { get; set; } = string.Empty;

        public ContactInfo Contact { get; set; } = new();
    }
}
