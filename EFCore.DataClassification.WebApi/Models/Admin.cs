using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }
        
        [DataClassification("Confidential", "Admin Naam", SensitivityRank.Medium)]
        public string Name { get; set; } = string.Empty;
        
       
        public string Email { get; set; } = string.Empty;
        
        [DataClassification("Highly Confidential", "Admin Sleutel", SensitivityRank.Critical)]
        public int Adminkey { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
