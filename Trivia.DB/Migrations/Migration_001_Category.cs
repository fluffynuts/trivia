using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using _Category = Trivia.DB.DataConstants.Tables.Category;
using _Columns = Trivia.DB.DataConstants.Tables.Category.Columns;

namespace Trivia.DB.Migrations
{
    [Migration(1)]
    public class Migration_001_Category: Migration
    {
        public override void Up()
        {
            Create.Table(DataConstants.Tables.Category.NAME)
                    .WithColumn(_Columns.CATEGORYID).AsGuid().NotNullable()
                    .WithColumn(_Columns.NAME).AsString(128).NotNullable()
                    .WithDefaultColumns();
        }

        public override void Down()
        {
        }
    }
}
