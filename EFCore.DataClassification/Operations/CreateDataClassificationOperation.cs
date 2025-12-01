using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.DataClassification.Operations {
    public sealed class CreateDataClassificationOperation :MigrationOperation {
        public string Table { get; set; } = default!;
        public string? Schema { get; set; }
        public string Column { get; set; } = default!;
        public string? Label { get; set; }
        public string? InformationType { get; set; }
        public string? Rank { get; set; }
        public string? PropertyDisplayName { get; set; }
    }
}