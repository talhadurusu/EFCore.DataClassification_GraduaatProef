using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Game {

        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

   
        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

     
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

       
        public string? Studio { get; set; }

        
        public int ReleaseYear { get; set; }

        public bool IsMultiplayer { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }
    }
}
