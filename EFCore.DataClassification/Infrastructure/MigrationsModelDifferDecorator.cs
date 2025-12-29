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

        // 4) RENAME COLUMN -> old remove + new create
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
                        ops.Insert(i + 1, new RemoveDataClassificationOperation {
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
                    ops.Insert(i + 1, new RemoveDataClassificationOperation {
                        Schema = schema,
                        Table = alter.Table,
                        Column = alter.Name
                    });
                    i++;
                } else if (sHas && tHas && (sLabel != tLabel || sInfo != tInfo || sRank != tRank)) {
                    ops.Insert(i + 1, new RemoveDataClassificationOperation {
                        Schema = schema,
                        Table = alter.Table,
                        Column = alter.Name
                    });

                    ops.Insert(i + 2, new CreateDataClassificationOperation {
                        Schema = schema,
                        Table = alter.Table,
                        Column = alter.Name,
                        Label = tLabel,
                        InformationType = tInfo,
                        Rank = tRank
                    });

                    i += 2;
                }
            }
        }

        return ops;
    }
}
