using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.DataClassification.Constants {
    internal static class DataClassificationConstants {
        // EF Core metadata içinde bu verileri hangi isimle saklayacağız
        public const string AnnotationPrefix = "DataClassification:";
        public const string Label = AnnotationPrefix + "Label";
        public const string InformationType = AnnotationPrefix + "InformationType";
        public const string Rank = AnnotationPrefix + "Rank";
    }
}
