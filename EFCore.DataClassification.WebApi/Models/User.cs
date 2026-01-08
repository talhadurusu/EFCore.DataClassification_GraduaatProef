using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class User {

        public int Id { get; set; }

        // PHASE 2 - SMOKE TEST 1: Column rename (FullName -> UserName) + DC change (Medium -> Low)
        [DataClassification("Personal", "User Name", SensitivityRank.Low)]
        public string UserName { get; set; } = string.Empty;

        // PHASE 2 - SMOKE TEST 2: DC remove (Adress'ten DC kaldırıldı)
        public string Adress { get; set; } = string.Empty;

        // PHASE 2 - SMOKE TEST 3: AlterColumn nullable (Email: NOT NULL -> NULL) + DC change (High -> Medium)
        [DataClassification("Contact", "Email Address", SensitivityRank.Medium)]
        public string? Email { get; set; }

        // PHASE 2 - SMOKE TEST 4: DC same (PhoneNumber değişmedi)
        public string PhoneNumber { get; set; } = string.Empty;

        // PHASE 2 - SMOKE TEST 5: Column drop (Salary silindi)

        // PHASE 2 - SMOKE TEST 6: Column drop (LastLoginUtc silindi)

        // PHASE 2 - SMOKE TEST 7: Column rename (Status -> AccountStatus) + DC same
        [DataClassification("Internal", "User Status", SensitivityRank.Low)]
        public string AccountStatus { get; set; } = "Active";
        
        public ICollection<Game> Games { get; set; } = new List<Game>();

        // PHASE 2 - SMOKE TEST 8: DC change (AdminId: High -> Critical)
        [DataClassification("Confidential", "Admin Reference", SensitivityRank.Critical)]
        public int? AdminId { get; set; }
        
        public Admin? Admin { get; set; }

        // PHASE 2 - SMOKE TEST 9: Column add + DC
        [DataClassification("Security", "Last Password Change", SensitivityRank.Medium)]
        public DateTime? LastPasswordChangeUtc { get; set; }

        // PHASE 2 - SMOKE TEST 10: Column add without DC
        public bool IsActive { get; set; } = true;
    }
}
