using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }

        // CASE: High sensitivity (PII)
        [DataClassification("Confidential", "Admin First Name", SensitivityRank.High)]
        public string FirstName { get; set; } = string.Empty;

        // CASE: High sensitivity (PII)
        [DataClassification("Confidential", "Admin Last Name", SensitivityRank.High)]
        public string LastName { get; set; } = string.Empty;

        // CASE: Nullable + classified (nullable olunca da metadata eklenebilmeli)
        [DataClassification("Confidential", "Admin Email", SensitivityRank.High)]
        public string? Email { get; set; }

        // CASE: Phone should be string (formatting/+32/leading zero)
       
        public string PhoneNo { get; set; } = string.Empty;

        // CASE: Medium sensitivity (unique-ish identifier, ama her zaman High olmak zorunda değil)
        [DataClassification("Confidential", "Admin Inscription Number", SensitivityRank.Medium)]
        public string InscriptionNumber { get; set; } = string.Empty;

        // CASE: Low/None sensitivity examples
        // (Bu alanlar classification farkını göstermek için güzel demo olur)
        [DataClassification("Internal", "Favorite Author", SensitivityRank.Low)]
        public string FavoriteBookAuthor { get; set; } = string.Empty;

        // CASE: Classified but optional + lower rank
        [DataClassification("Internal", "Admin Notes", SensitivityRank.Low)]
        public string? Notes { get; set; }

        // CASE: Unclassified field (library bunu görmez, migration'a satır düşmez)
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // CASE: Relationship / FK classification senaryosu
        // (Users tablosunda AdminId gibi bir alan zaten var; burada ters navigation)
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
