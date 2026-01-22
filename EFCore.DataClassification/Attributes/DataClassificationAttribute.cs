using EFCore.DataClassification.Models;
using System;

namespace EFCore.DataClassification.Attributes {

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DataClassificationAttribute : Attribute {
        public string Label { get; }            
        public string InformationType { get; }  
        public SensitivityRank Rank { get; }    

        public DataClassificationAttribute(string label, string informationType, SensitivityRank rank) {
            Label = label;
            InformationType = informationType;
            Rank = rank;
        }
    }
}
