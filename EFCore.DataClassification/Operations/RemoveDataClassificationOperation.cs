using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.DataClassification.Operations {
    public sealed class RemoveDataClassificationOperation : MigrationOperation {
        public string Table { get; set; } = default!;
        public string? Schema { get; set; }
        public string Column { get; set; } = default!;
    }
}
