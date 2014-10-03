using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace Trivia.DB
{
    public static class MigrationExtensions
    {
        public static ICreateTableWithColumnSyntax WithDefaultColumns(this ICreateTableWithColumnSyntax root)
        {
            return root.WithColumn(DataConstants.Tables._CommonColumns.CREATED).AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                        .WithColumn(DataConstants.Tables._CommonColumns.LASTMODIFIED).AsDateTime().Nullable()
                        .WithColumn(DataConstants.Tables._CommonColumns.ENABLED).AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
}
