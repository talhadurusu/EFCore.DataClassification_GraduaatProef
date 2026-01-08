using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCore.DataClassification.WebApi.Models {
    public class Bike {
        public int Id { get; set; }

        // EDGE CASE: Classification kaldırıldı (Brand'dan)
        public string Brand { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;

        // EDGE CASE: Column silindi (Owner kaldırıldı - classification vardı)

        public int GearCount { get; set; }

        // EDGE CASE: Yeni column eklendi (classification ile) - Owner'dan farklı bir column
        [DataClassification("Confidential", "Bike Serial Number", SensitivityRank.Medium)]
        public string SerialNumber { get; set; } = string.Empty;

        // EDGE CASE: Yeni column eklendi (classification olmadan)
        public string? Color { get; set; }
    }
}
