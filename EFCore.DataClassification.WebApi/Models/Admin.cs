using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

       // [DataClassification("Confidential", "Email Address", SensitivityRank.Medium)]
        public string? Email { get; set; }

       // [DataClassification("Confidential", "Phone Number", SensitivityRank.Low)]
        public string Phoneno { get; set; } = string.Empty;

      //  [DataClassification("Confidential", "Inscription Number", SensitivityRank.High)]
        public string InscriptionNo { get; set; } = string.Empty;

      //  [DataClassification("Internal", "Notes about the admin", SensitivityRank.Low)]
        public string? Notes { get; set; }

       
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
