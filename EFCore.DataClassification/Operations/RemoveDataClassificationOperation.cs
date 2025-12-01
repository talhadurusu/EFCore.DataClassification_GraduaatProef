using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.DataClassification.Operations {
    public sealed class RemoveDataClassificationOperation : MigrationOperation {
        public string Table { get; set; } = default!;
        public string? Schema { get; set; }
        public string Column { get; set; } = default!;
    }
}
