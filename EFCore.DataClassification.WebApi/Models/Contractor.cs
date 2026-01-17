using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Contractor : PersonBase {
        [DataClassification("Employment", "Agency Name", SensitivityRank.Medium)]
        public string AgencyName { get; set; } = string.Empty;
    }
}
