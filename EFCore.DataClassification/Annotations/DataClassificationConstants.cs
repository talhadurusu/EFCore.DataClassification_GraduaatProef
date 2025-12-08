using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.DataClassification.Annotations {
    /// <summary>
    /// Central constants for data classification features.
    /// Ensures consistent values across the application.
    /// </summary>
    public static class DataClassificationConstants {
        
        // ========================================
        // ANNOTATION NAMES
        // ========================================
        
        public const string AnnotationPrefix = "DataClassification:";
        
        public const string Label = AnnotationPrefix + "Label";
        
        public const string InformationType = AnnotationPrefix + "InformationType";
        
        public const string Rank = AnnotationPrefix + "Rank";


        public const int MaxLabelLength = 128;
        
        public const int MaxInformationTypeLength = 128;
        
        public const string DefaultSchema = "dbo";
        

       
        /// <summary>
        /// Valid sensitivity rank values (maps to SQL Server sensitivity classification ranks)
        /// </summary>
        public static readonly string[] AllowedRanks = 
        {
            "None",
            "Low",
            "Medium",
            "High",
            "Critical"
        };
         
        /// <summary>
        /// Validates if a rank value is allowed
        /// </summary>
        public static bool IsValidRank(string? rank) {
            if (string.IsNullOrWhiteSpace(rank))
                return true;
                
            return Array.Exists(AllowedRanks, r => 
                r.Equals(rank, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Gets allowed ranks as comma-separated string for error messages
        /// </summary>
        public static string GetAllowedRanksString() 
            => string.Join(", ", AllowedRanks);
    }
}
