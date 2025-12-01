using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Admin {
        public int Id { get; set; }

        public string Name { get; set; }


 
        public string Email { get; set; }

        public int Adminkey { get; set; }
    }
}
