using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class User {

        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        // TEST 1: Attribute-based classification
        [DataClassification("Private", "Home Address", SensitivityRank.Medium)]
        public string Adress { get; set; }

     
        public string Email { get; set; }

        // TEST 2: Fluent API 
       
        public string PhoneNumber { get; set; }

        [DataClassification("Confidential", "Financial Information", SensitivityRank.High)]
        public int Salary { get; set; }

        
        public List<Game> Games { get; set; }

        [DataClassification("Confidential", "Admin Reference", SensitivityRank.High)]
        public int? AdminId { get; set; }
    }
}
