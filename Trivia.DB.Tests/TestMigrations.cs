using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using PeanutButter.Utils;

namespace Trivia.DB.Tests
{
    [TestFixture]
    public class TestMigrations
    {
        public class TempDB : IDisposable
        {
            public string DatabaseFile { get; private set; }
            public string ConnectionString { get; private set; }
            private static Semaphore _lock = new Semaphore(1, 1);

            public TempDB()
            {
                using (new AutoLocker(_lock))
                {
                    DatabaseFile = Path.GetTempFileName();
                    if (File.Exists(DatabaseFile))
                        File.Delete(DatabaseFile);
                    ConnectionString = String.Format("DataSource=\"{0}\";Password=\"{1}\"", DatabaseFile, "password");
                    using (var engine = new SqlCeEngine(ConnectionString))
                    {
                        engine.CreateDatabase();
                    }
                }
            }

            public void Dispose()
            {
                try
                {
                    File.Delete(DatabaseFile);
                }
                catch
                {
                }
            }
        }

        private DBMigrationsRunner<SqlServerCeProcessorFactory> CreateWith(string connectionString)
        {
            return new DBMigrationsRunner<SqlServerCeProcessorFactory>(connectionString);
        }

        [Test]
        public void RunAllMigrations_ShouldNotFail()
        {
            //---------------Set up test pack-------------------
            using (var db = new TempDB())
            {
                var runner = CreateWith(db.ConnectionString);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(runner.MigrateToLatest);

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void Migration_001_ShouldAddCategoryTable()
        {
            //---------------Set up test pack-------------------
            using (var disposer = new AutoDisposer())
            {
                //---------------Assert Precondition----------------
                var db = disposer.Add(new TempDB());
                var runner = CreateWith(db.ConnectionString);

                //---------------Execute Test ----------------------
                runner.MigrateToLatest();

                //---------------Test Result -----------------------
                var helper = new MigrationTestsHelper<SqlCeConnection>(db.ConnectionString);

                helper.ShouldHaveTable(DataConstants.Tables.Category.NAME);
                helper.ShouldHaveTable(DataConstants.Tables.Category.NAME)
                        .WithColumn(DataConstants.Tables.Category.Columns.CATEGORYID)
                            .WithType<Guid>()
                            .NotNullable()
                        .WithColumn(DataConstants.Tables.Category.Columns.NAME)
                            .WithType<string>()
                            .NotNullable()
                            .WithMaxLength(128)
                        .WithColumn(DataConstants.Tables.Category.Columns.CREATED)
                            .WithType<DateTime>()
                            .NotNullable()
                        .WithColumn(DataConstants.Tables.Category.Columns.ENABLED)
                            .WithType<bool>()
                            .NotNullable();
            }
        }

        [Test]
        public void Migration_002_ShouldAddQuestionTable()
        {
            using (var disposer = new AutoDisposer())
            {
                //---------------Set up test pack-------------------
                var db = disposer.Add(new TempDB());
                var runner = CreateWith(db.ConnectionString);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                runner.MigrateToLatest();

                //---------------Test Result -----------------------
                var helper = CreateHelperFor(db.ConnectionString);
                helper.ShouldHaveTable(DataConstants.Tables.Question.NAME)
                        .WithColumn(DataConstants.Tables.Question.Columns.QUESTIONID)
                            .WithType<Guid>()
                            .NotNullable()
                        .WithColumn(DataConstants.Tables.Question.Columns.QUESTION)
                            .WithType<string>()
                            .WithMaxLength(255)
                        .WithColumn(DataConstants.Tables.Question.Columns.ANSWER)
                            .WithType<string>()
                            .WithMaxLength(255);
            }
        }

        private MigrationTestsHelper<SqlCeConnection> CreateHelperFor(string connectionString)
        {
            return new MigrationTestsHelper<SqlCeConnection>(connectionString);
        }
    }

    public class MigrationTestsHelper
    {
        protected string Quote(string str)
        {
            return str.Replace("'", "''");
        }

    }

    public class MigrationTestsHelper<T>: MigrationTestsHelper, IDisposable where T: DbConnection, new()
    {
        public class MigrationTestsColumnInfo
        {
            private static Dictionary<Type, string> _typesLookup;
            private string _dbType;
            private string _name;
            private object _defaultValue;
            private int? _maxLength;
            private bool _isNullable;
            private MigrationTestsColumnHelper _parent;

            static MigrationTestsColumnInfo()
            {
                _typesLookup = new Dictionary<Type, string>();
                _typesLookup[typeof(Guid)] = "uniqueidentifier";
                _typesLookup[typeof(string)] = "nvarchar";
                _typesLookup[typeof(DateTime)] = "datetime";
                _typesLookup[typeof(bool)] = "bit";
            }

            public MigrationTestsColumnInfo(MigrationTestsColumnHelper parent, DbDataReader reader)
            {
                _parent = parent;
                _name = reader["COLUMN_NAME"].ToString();
                _dbType = reader["DATA_TYPE"].ToString();
                _defaultValue = reader["COLUMN_DEFAULT"];
                _maxLength = GrokMaxLengthFrom(reader); //["CHARACTER_MAXIMUM_LENGTH"];
                _isNullable = GrokNullableFrom(reader);
            }

            public MigrationTestsColumnInfo WithColumn(string name)
            {
                return _parent.WithColumn(name);
            }

            private int? GrokMaxLengthFrom(DbDataReader reader)
            {
                var dbVal = reader["CHARACTER_MAXIMUM_LENGTH"];
                return dbVal as int?;
            }

            private bool GrokNullableFrom(DbDataReader reader)
            {
                var dbVal = reader["IS_NULLABLE"].ToString().ToLower();
                switch (dbVal)
                {
                    case "yes":
                    case "true":
                    case "1":
                        return true;
                    default:
                        return false;
                }
            }

            public MigrationTestsColumnInfo WithType<ExpectedType>()
            {
                var expectedTypeName = DbTypeNameFor<ExpectedType>();
                Assert.AreEqual(expectedTypeName, _dbType, String.Join("", new[] {
                    "Expected column '",
                    _name,
                    "' to have type '",
                    expectedTypeName,
                    "' but was actually '",
                    _dbType,
                    "'" }) );
                return this;
            }

            public MigrationTestsColumnInfo WithMaxLength(int expectedLength)
            {
                var precursor = "Expected maximum length of '" + expectedLength + "' ";
                if (!_maxLength.HasValue)
                    Assert.Fail(precursor + "but none actually set");
                if (expectedLength != _maxLength.Value)
                    Assert.Fail(precursor + "but got '" + _maxLength.Value.ToString() + "' instead");
                return this;
            }

            private string DbTypeNameFor<ColumnType>()
            {
                return _typesLookup[typeof(ColumnType)];
            }

            public MigrationTestsColumnInfo NotNullable()
            {
                Assert.IsFalse(_isNullable, "Expected '" + _name + "' to be not nullable");
                return this;
            }
            public MigrationTestsColumnInfo Nullable()
            {
                Assert.IsTrue(_isNullable, "Expected '" + _name + "' to be not nullable");
                return this;
            }
        }

        public class MigrationTestsColumnHelper: MigrationTestsHelper
        {
            private DbConnection _connection;
            public string TableName { get; private set; }

            public MigrationTestsColumnHelper(DbConnection connection, string tableName)
            {
                this._connection = connection;
                this.TableName = tableName;
            }

            public MigrationTestsColumnInfo WithColumn(string withName)
            {
                using (var disposer = new AutoDisposer())
                {
                    var cmd = disposer.Add(_connection.CreateCommand());
                    cmd.CommandText = String.Format("select * from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '{0}' and COLUMN_NAME = '{1}';", 
                        Quote(TableName), Quote(withName));
                    var reader = disposer.Add(cmd.ExecuteReader());
                    Assert.IsTrue(reader.Read(), "Could not find column '" + withName + "' on table '" + TableName + "'");
                    return new MigrationTestsColumnInfo(this, reader);
                }
            }
        }

        T _connection;

        public MigrationTestsHelper(string connectionString)
        {
            _connection = (T)Activator.CreateInstance(typeof(T), connectionString);
            _connection.Open();
        }

        public MigrationTestsColumnHelper ShouldHaveTable(string withName)
        {
            using (var disposer = new AutoDisposer())
            {
                var cmd = _connection.CreateCommand();
                cmd.CommandText = String.Format("Select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{0}'", Quote(withName));
                var reader = disposer.Add(cmd.ExecuteReader());
                Assert.IsTrue(reader.Read(), "Table not found: " + withName);
            }
            return new MigrationTestsColumnHelper(_connection, withName);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
