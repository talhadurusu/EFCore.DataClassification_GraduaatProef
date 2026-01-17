using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class User {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public string Surname { get; set; } = string.Empty;

        // TEST 1: Attribute-based classification
        [DataClassification("Location", "Home Address", SensitivityRank.Low)]
        public string Adress { get; set; } = string.Empty;

     
        [DataClassification("Confidential", "User Email", SensitivityRank.Medium)]
        public string? Email { get; set; }

        // TEST 2: Fluent API 
       
        public string PhoneNumber { get; set; } = string.Empty;

        [DataClassification("Confidential", "Financial Information", SensitivityRank.Medium)]
        public int? Salary { get; set; }

        
        public ICollection<Game> Games { get; set; } = new List<Game>();

        [DataClassification("Confidential", "Admin Reference", SensitivityRank.High)]
        public int AdminId { get; set; }
        
        public Admin? Admin { get; set; }

        [DataClassification("Security", "Last Password Change", SensitivityRank.Medium)]
        public DateTime? LastPasswordChangeUtc { get; set; }
    }
}
