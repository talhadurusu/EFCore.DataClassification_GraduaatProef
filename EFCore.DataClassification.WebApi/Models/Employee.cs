using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Models;

namespace EFCore.DataClassification.WebApi.Models {
    public class Employee : PersonBase {
        [DataClassification("Employment", "Employee Code", SensitivityRank.Low)]
        public string EmployeeCode { get; set; } = string.Empty;

        [DataClassification("Financial", "Salary", SensitivityRank.High)]
        public decimal Salary { get; set; }
    }
}
