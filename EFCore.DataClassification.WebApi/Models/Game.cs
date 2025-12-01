using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Game {

        public int Id { get; set; }

        public string PublisherUnikeUnitID { get; set; }

    
        public string Title { get; set; }

        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }

        
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
