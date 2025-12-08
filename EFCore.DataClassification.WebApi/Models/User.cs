using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class User {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Surname { get; set; } = string.Empty;

        // TEST 1: Attribute-based classification
        [DataClassification("Private", "Home Address", SensitivityRank.Medium)]
        public string Adress { get; set; } = string.Empty;

     
        public string Email { get; set; } = string.Empty;

        // TEST 2: Fluent API 
       
        public string PhoneNumber { get; set; } = string.Empty;

        [DataClassification("Confidential", "Financial Information", SensitivityRank.High)]
        public int Salary { get; set; }

        
        public ICollection<Game> Games { get; set; } = new List<Game>();

        [DataClassification("Confidential", "Admin Reference", SensitivityRank.High)]
        public int? AdminId { get; set; }
        
        public Admin? Admin { get; set; }
    }
}
