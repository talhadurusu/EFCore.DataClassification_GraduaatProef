using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace EFCore.DataClassification.Extensions {
    public static class MigrationBuilderExtensions {
        public static OperationBuilder<CreateDataClassificationOperation> AddDataClassification(
            this MigrationBuilder migrationBuilder,
            string table,
            string column,
            string? schema = null,
            string? label = null,
            string? informationType = null,
            string? rank = null) {

            var operation = new CreateDataClassificationOperation {
                Table = table,
                Column = column,
                Schema = schema,
                Label = label,
                InformationType = informationType,
                Rank = rank,
            };

            migrationBuilder.Operations.Add(operation);
            return new OperationBuilder<CreateDataClassificationOperation>(operation);
        }

        public static OperationBuilder<RemoveDataClassificationOperation> DropDataClassification(
            this MigrationBuilder migrationBuilder,
            string table,
            string column,
            string? schema = null) {

            var operation = new RemoveDataClassificationOperation {
                Table = table,
                Column = column,
                Schema = schema
            };

            migrationBuilder.Operations.Add(operation);
            return new OperationBuilder<RemoveDataClassificationOperation>(operation);
        }
    }
}