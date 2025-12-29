using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.DataClassification.Models {
    public enum SensitivityRank {
        None,
        Low,
        Medium,
        High,
        Critical
    }
    public sealed record DataClassificationMetadata(
       string Label,
       string InformationType,
       string Rank
   );
}
