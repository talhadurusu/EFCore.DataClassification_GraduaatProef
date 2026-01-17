using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }
        
   
       public  int Age { get; set; }
        [DataClassification("Contact", "Admin Email", SensitivityRank.Medium)]
        public string? Email { get; set; }
        
        [DataClassification("Confidential", "Admin Key", SensitivityRank.High)]
        public int Adminkey { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
