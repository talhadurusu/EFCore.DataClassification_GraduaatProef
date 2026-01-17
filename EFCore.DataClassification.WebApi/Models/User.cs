using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class User {

        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        [DataClassification("Location", "Home Address", SensitivityRank.Low)]
        public string Adress { get; set; } = string.Empty;

        [DataClassification("Contact", "Email Address", SensitivityRank.High)]
        public string? Email { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        [DataClassification("Internal", "User Status", SensitivityRank.Low)]
        public string AccountStatus { get; set; } = "Active";
        
        public ICollection<Game> Games { get; set; } = new List<Game>();

        [DataClassification("Confidential", "Admin Reference", SensitivityRank.Critical)]
        public int AdminId { get; set; }
        
        public Admin? Admin { get; set; }

        [DataClassification("Security", "Last Password Change", SensitivityRank.Medium)]
        public DateTime? LastPasswordChangeUtc { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
