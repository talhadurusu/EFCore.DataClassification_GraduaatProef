using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }

        [DataClassification("Normal", "Admin First Name", SensitivityRank.High)]
        public string FirstName { get; set; } = string.Empty;

      [DataClassification("Confidential", " Last Name", SensitivityRank.High)]
        public string LastName { get; set; } = string.Empty;

        [DataClassification("Confidential", "Admin Email", SensitivityRank.Critical)]
        public string? Email { get; set; }

        public string PhoneNo { get; set; } = string.Empty;

        [DataClassification("Confidential", "Admin Inscription Number", SensitivityRank.Medium)]
        public string InscriptionNo { get; set; } = string.Empty;

        

        [DataClassification("Internal", "Admin Notes", SensitivityRank.Low)]
        public string? Notes { get; set; }

        [DataClassification("Normal", "Admin Created At", SensitivityRank.Low)]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
