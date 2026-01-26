using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class User {

        public int Id { get; set; }


        public string UserName { get; set; } = string.Empty;

        public string Adress { get; set; } = string.Empty;

     
        public string? Email { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

       
        public string AccountStatus { get; set; } = "Active";
        
        public ICollection<Game> Games { get; set; } = new List<Game>();


        public int? AdminId { get; set; }
        
        public Admin? Admin { get; set; }

        public DateTime? LastPasswordChangeUtc { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
