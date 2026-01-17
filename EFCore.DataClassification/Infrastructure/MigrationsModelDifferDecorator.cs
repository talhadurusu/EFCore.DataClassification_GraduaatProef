using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.DataClassification.Infrastructure;

public sealed class MigrationsModelDifferDecorator : IMigrationsModelDiffer {
    private const string DefaultSchema = "dbo";
    private readonly IMigrationsModelDiffer _inner;

    public MigrationsModelDifferDecorator(IMigrationsModelDiffer inner)
        => _inner = inner;

    public bool HasDifferences(IRelationalModel? source, IRelationalModel? target)
        => _inner.HasDifferences(source, target);

    public IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target) {
        var ops = _inner.GetDifferences(source, target).ToList();

        var columnOps = new HashSet<(string schema, string table, string column)>(
            new SchemaTableColumnComparer());

        foreach (var op in ops) {
            switch (op) {
                case AddColumnOperation add:
                    columnOps.Add((NormalizeSchema(add.Schema), add.Table, add.Name));
                    break;
                case DropColumnOperation drop:
                    columnOps.Add((NormalizeSchema(drop.Schema), drop.Table, drop.Name));
                    break;
                case AlterColumnOperation alter:
                    columnOps.Add((NormalizeSchema(alter.Schema), alter.Table, alter.Name));
                    break;
                case RenameColumnOperation rename:
                    columnOps.Add((NormalizeSchema(rename.Schema), rename.Table, rename.Name));
                    columnOps.Add((NormalizeSchema(rename.Schema), rename.Table, rename.NewName));
                    break;
            }
        }

        // 0) CREATE TABLE -> create classifications for classified columns (target modelden)
        if (target is not null) {
            for (int i = 0; i < ops.Count; i++) {
                if (ops[i] is CreateTableOperation ct) {
                    var schema = ct.Schema ?? DefaultSchema;

                    foreach (var col in ct.Columns) {
                        if (DataClassificationModelLookup.TryGetTriplet(
                                target, schema, ct.Name, col.Name,
                                out var label, out var info, out var rank)) {
                            ops.Insert(i + 1, new CreateDataClassificationOperation {
                                Schema = schema,
                                Table = ct.Name,
                                Column = col.Name,
                                Label = label,
                                InformationType = info,
                                Rank = rank
                            });
                            i++;
                        }
                    }
                }
            }
        }

        // 0.5) DROP TABLE -> remove classifications for classified columns (source modelden) BEFORE drop
        if (source is not null) {
            for (int i = 0; i < ops.Count; i++) {
                if (ops[i] is DropTableOperation dt) {
                    var schema = dt.Schema ?? DefaultSchema;

                    var table = source.FindTable(dt.Name, schema);
                    if (table is null) continue;

                    foreach (var col in table.Columns) {
                        if (DataClassificationModelLookup.TryGetTriplet(
                                source, schema, dt.Name, col.Name,
                                out _, out _, out _)) {
                            ops.Insert(i, new RemoveDataClassificationOperation {
                                Schema = schema,
                                Table = dt.Name,
                                Column = col.Name
                            });
                            i++;
                        }
                    }
                }
            }
        }

        // 4) RENAME COLUMN -> old remove (before) + new create (after)
        if (source is not null && target is not null) {
            for (int i = 0; i < ops.Count; i++) {
                if (ops[i] is RenameColumnOperation rc) {
                    var schema = rc.Schema ?? DefaultSchema;

                    var oldHas = DataClassificationModelLookup.TryGetTriplet(
                        source, schema, rc.Table, rc.Name,
                        out _, out _, out _);

                    var newHas = DataClassificationModelLookup.TryGetTriplet(
                        target, schema, rc.Table, rc.NewName,
                        out var nLabel, out var nInfo, out var nRank);

                    if (oldHas) {
                        ops.Insert(i, new RemoveDataClassificationOperation {
                            Schema = schema,
                            Table = rc.Table,
                            Column = rc.Name
                        });
                        i++;
                    }

                    if (newHas) {
                        ops.Insert(i + 1, new CreateDataClassificationOperation {
                            Schema = schema,
                            Table = rc.Table,
                            Column = rc.NewName,
                            Label = nLabel,
                            InformationType = nInfo,
                            Rank = nRank
                        });
                        i++;
                    }
                }
            }
        }

        // 1) ADD COLUMN -> CREATE classification (target modelden)
        if (target is not null) {
            for (int i = 0; i < ops.Count; i++) {
                if (ops[i] is AddColumnOperation add) {
                    var schema = add.Schema ?? DefaultSchema;

                    if (DataClassificationModelLookup.TryGetTriplet(
                            target, schema, add.Table, add.Name,
                            out var label, out var info, out var rank)) {
                        ops.Insert(i + 1, new CreateDataClassificationOperation {
                            Schema = schema,
                            Table = add.Table,
                            Column = add.Name,
                            Label = label,
                            InformationType = info,
                            Rank = rank
                        });
                        i++;
                    }
                }
            }
        }

        // 2) DROP COLUMN -> REMOVE classification (source modelden) (drop'tan önce)
        if (source is not null) {
            for (int i = 0; i < ops.Count; i++) {
                if (ops[i] is DropColumnOperation drop) {
                    var schema = drop.Schema ?? DefaultSchema;

                    if (DataClassificationModelLookup.TryGetTriplet(
                            source, schema, drop.Table, drop.Name,
                            out _, out _, out _)) {
                        ops.Insert(i, new RemoveDataClassificationOperation {
                            Schema = schema,
                            Table = drop.Table,
                            Column = drop.Name
                        });
                        i++;
                    }
                }
            }
        }

        // 3) ALTER COLUMN -> compare source vs target
        if (source is not null && target is not null) {
            for (int i = 0; i < ops.Count; i++) {
                if (ops[i] is not AlterColumnOperation alter) continue;

                var schema = alter.Schema ?? DefaultSchema;

                var sHas = DataClassificationModelLookup.TryGetTriplet(
                    source, schema, alter.Table, alter.Name,
                    out var sLabel, out var sInfo, out var sRank);

                var tHas = DataClassificationModelLookup.TryGetTriplet(
                    target, schema, alter.Table, alter.Name,
                    out var tLabel, out var tInfo, out var tRank);

                if (!sHas && tHas) {
                    ops.Insert(i + 1, new CreateDataClassificationOperation {
                        Schema = schema,
                        Table = alter.Table,
                        Column = alter.Name,
                        Label = tLabel,
                        InformationType = tInfo,
                        Rank = tRank
                    });
                    i++;
                } else if (sHas && !tHas) {
                    ops.Insert(i, new RemoveDataClassificationOperation {
                        Schema = schema,
                        Table = alter.Table,
                        Column = alter.Name
                    });
                    i++;
                } else if (sHas && tHas && (sLabel != tLabel || sInfo != tInfo || sRank != tRank)) {
                    ops.Insert(i, new RemoveDataClassificationOperation {
                        Schema = schema,
                        Table = alter.Table,
                        Column = alter.Name
                    });

                    i++;
                    ops.Insert(i + 1, new CreateDataClassificationOperation {
                        Schema = schema,
                        Table = alter.Table,
                        Column = alter.Name,
                        Label = tLabel,
                        InformationType = tInfo,
                        Rank = tRank
                    });

                    i++;
                }
            }
        }

        // 5) Classification-only changes (no column operation) -> remove/create
        if (source is not null && target is not null) {
            foreach (var targetTable in target.Tables) {
                var schema = NormalizeSchema(targetTable.Schema);
                var sourceTable = FindTable(source, targetTable.Name, schema);
                if (sourceTable is null) continue;

                foreach (var targetColumn in targetTable.Columns) {
                    var columnName = targetColumn.Name;
                    if (columnOps.Contains((schema, targetTable.Name, columnName)))
                        continue;

                    var sHas = DataClassificationModelLookup.TryGetTriplet(
                        source, schema, targetTable.Name, columnName,
                        out var sLabel, out var sInfo, out var sRank);
                    var tHas = DataClassificationModelLookup.TryGetTriplet(
                        target, schema, targetTable.Name, columnName,
                        out var tLabel, out var tInfo, out var tRank);

                    if (!sHas && !tHas)
                        continue;

                    if (sHas && tHas && sLabel == tLabel && sInfo == tInfo && sRank == tRank)
                        continue;

                    if (sHas) {
                        ops.Add(new RemoveDataClassificationOperation {
                            Schema = schema,
                            Table = targetTable.Name,
                            Column = columnName
                        });
                    }

                    if (tHas) {
                        ops.Add(new CreateDataClassificationOperation {
                            Schema = schema,
                            Table = targetTable.Name,
                            Column = columnName,
                            Label = tLabel,
                            InformationType = tInfo,
                            Rank = tRank
                        });
                    }
                }
            }
        }

        EnsureOperationOrdering(ops);
        return ops;
    }

    private static string NormalizeSchema(string? schema)
        => string.IsNullOrWhiteSpace(schema) ? DefaultSchema : schema;

    private static ITable? FindTable(IRelationalModel model, string table, string schema) {
        var found = model.FindTable(table, schema);
        if (found is not null) return found;

        if (string.Equals(schema, DefaultSchema, StringComparison.OrdinalIgnoreCase)) {
            return model.FindTable(table, null);
        }

        if (string.IsNullOrWhiteSpace(schema)) {
            return model.FindTable(table, DefaultSchema);
        }

        return null;
    }

    private static void EnsureOperationOrdering(List<MigrationOperation> ops) {
        var changed = true;
        while (changed) {
            changed = false;

            for (var i = 0; i < ops.Count; i++) {
                if (ops[i] is RenameColumnOperation rename) {
                    var schema = rename.Schema;
                    var table = rename.Table;

                    var renameIdx = ops.IndexOf(rename);

                    var removeIdx = FindRemoveIndex(ops, schema, table, rename.Name);
                    if (removeIdx > renameIdx) {
                        var remove = ops[removeIdx];
                        ops.RemoveAt(removeIdx);
                        ops.Insert(renameIdx, remove);
                        changed = true;
                        break;
                    }

                    renameIdx = ops.IndexOf(rename);
                    var createIdx = FindCreateIndex(ops, schema, table, rename.NewName);
                    if (createIdx >= 0 && createIdx < renameIdx) {
                        var create = ops[createIdx];
                        ops.RemoveAt(createIdx);
                        ops.Insert(renameIdx + 1, create);
                        changed = true;
                        break;
                    }
                } else if (ops[i] is AlterColumnOperation alter) {
                    var schema = alter.Schema;
                    var table = alter.Table;

                    var alterIdx = ops.IndexOf(alter);
                    var removeIdx = FindRemoveIndex(ops, schema, table, alter.Name);
                    if (removeIdx > alterIdx) {
                        var remove = ops[removeIdx];
                        ops.RemoveAt(removeIdx);
                        ops.Insert(alterIdx, remove);
                        changed = true;
                        break;
                    }

                    alterIdx = ops.IndexOf(alter);
                    var createIdx = FindCreateIndex(ops, schema, table, alter.Name);
                    if (createIdx >= 0 && createIdx < alterIdx) {
                        var create = ops[createIdx];
                        ops.RemoveAt(createIdx);
                        ops.Insert(alterIdx + 1, create);
                        changed = true;
                        break;
                    }
                } else if (ops[i] is DropColumnOperation drop) {
                    var dropIdx = ops.IndexOf(drop);
                    var removeIdx = FindRemoveIndex(ops, drop.Schema, drop.Table, drop.Name);
                    if (removeIdx > dropIdx) {
                        var remove = ops[removeIdx];
                        ops.RemoveAt(removeIdx);
                        ops.Insert(dropIdx, remove);
                        changed = true;
                        break;
                    }
                } else if (ops[i] is AddColumnOperation add) {
                    var addIdx = ops.IndexOf(add);
                    var createIdx = FindCreateIndex(ops, add.Schema, add.Table, add.Name);
                    if (createIdx >= 0 && createIdx < addIdx) {
                        var create = ops[createIdx];
                        ops.RemoveAt(createIdx);
                        ops.Insert(addIdx + 1, create);
                        changed = true;
                        break;
                    }
                }
            }
        }
    }

    private static int FindRemoveIndex(List<MigrationOperation> ops, string? schema, string table, string column)
        => ops.FindIndex(op =>
            op is RemoveDataClassificationOperation remove
            && SchemaEquals(remove.Schema, schema)
            && string.Equals(remove.Table, table, StringComparison.OrdinalIgnoreCase)
            && string.Equals(remove.Column, column, StringComparison.OrdinalIgnoreCase));

    private static int FindCreateIndex(List<MigrationOperation> ops, string? schema, string table, string column)
        => ops.FindIndex(op =>
            op is CreateDataClassificationOperation create
            && SchemaEquals(create.Schema, schema)
            && string.Equals(create.Table, table, StringComparison.OrdinalIgnoreCase)
            && string.Equals(create.Column, column, StringComparison.OrdinalIgnoreCase));

    private static bool SchemaEquals(string? schema1, string? schema2) {
        var s1 = NormalizeSchema(schema1);
        var s2 = NormalizeSchema(schema2);
        return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class SchemaTableColumnComparer : IEqualityComparer<(string schema, string table, string column)> {
        public bool Equals((string schema, string table, string column) x, (string schema, string table, string column) y)
            => string.Equals(x.schema, y.schema, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.table, y.table, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.column, y.column, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string schema, string table, string column) obj) {
            var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.schema);
            hash = (hash * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.table);
            hash = (hash * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.column);
            return hash;
        }
    }
}
