using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }

        [DataClassification("Confidential", "Admin First Name", SensitivityRank.High)]
        public string FirstName { get; set; } = string.Empty;

        [DataClassification("Confidential", "Admin Last Name", SensitivityRank.High)]
        public string LastName { get; set; } = string.Empty;

        [DataClassification("Confidential", "Admin Email", SensitivityRank.High)]
        public string? Email { get; set; }

        public string PhoneNo { get; set; } = string.Empty;

        [DataClassification("Confidential", "Admin Inscription Number", SensitivityRank.Medium)]
        public string InscriptionNumber { get; set; } = string.Empty;

        [DataClassification("Internal", "Favorite Author", SensitivityRank.Low)]
        public string FavoriteBookAuthor { get; set; } = string.Empty;

        [DataClassification("Internal", "Admin Notes", SensitivityRank.Low)]
        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
