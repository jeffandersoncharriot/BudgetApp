using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Threading;
using System.Security.Cryptography;
using System.Data.Common;

// ===================================================================
// Very important notes:
// ... To keep everything working smoothly, you should always
//     dispose of EVERY SQLiteCommand even if you recycle a 
//     SQLiteCommand variable later on.
//     EXAMPLE:
//            Database.newDatabase(GetSolutionDir() + "\\" + filename);
//            var cmd = new SQLiteCommand(Database.dbConnection);
//            cmd.CommandText = "INSERT INTO categoryTypes(Description) VALUES('Whatever')";
//            cmd.ExecuteNonQuery();
//            cmd.Dispose();
//
// ... also dispose of reader objects
//
// ... by default, SQLite does not impose Foreign Key Restraints
//     so to add these constraints, connect to SQLite something like this:
//            string cs = $"Data Source=abc.sqlite; Foreign Keys=1";
//            var con = new SQLiteConnection(cs);
//
// ===================================================================


namespace Budget
{
    internal class Database
    {

        /// <summary>
        /// The active database connection, or null if there is none or the
        /// last attempt at loading one failed.
        /// </summary>
        public static SQLiteConnection? dbConnection { get { return _connection; } }
        private static SQLiteConnection? _connection;

        /// <summary>
        /// Creates and opens a connection to a database
        /// </summary>
        /// <param name="filename">Filepath of database</param>
        /// <exception cref="Exception">
        /// Thrown when an error occurs while connecting to the database or when the database cannot be created.\
        /// </exception>
        public static void newDatabase(string filename)
        {
            // https://web.archive.org/web/20190910153157/http://blog.tigrangasparian.com/2012/02/09/getting-started-with-sqlite-in-c-part-one/
            // https://zetcode.com/csharp/sqlite/

            // Closes any previous connection that might exist
            CloseDatabaseAndReleaseFile();

            // Creates a new database file at the specified location
            SQLiteConnection.CreateFile(filename);
            existingDatabase(filename);

            using var cmd = new SQLiteCommand(_connection);

            // expenses table
            cmd.CommandText = "CREATE TABLE expenses(Id INTEGER PRIMARY KEY, Date TEXT, Amount REAL, Description TEXT, CategoryId INTEGER references categories(Id));";
            cmd.ExecuteNonQuery();

            // categories table
            cmd.CommandText = "CREATE TABLE categories(Id INTEGER PRIMARY KEY, Description TEXT UNIQUE, TypeId REFERENCES categoryTypes(Id))";
            cmd.ExecuteNonQuery();

            // categoryTypes table, used as a FK for the tables above
            cmd.CommandText = "CREATE TABLE categoryTypes(Id INTEGER PRIMARY KEY, Description TEXT)";
            cmd.ExecuteNonQuery();

            // Add CategoryTypes
            DbParameter descParam = cmd.CreateParameter();
            cmd.CommandText = "INSERT OR IGNORE INTO categoryTypes (Description) VALUES (?)";
            _ = cmd.Parameters.Add(descParam);
            foreach (Category.CategoryType categoryType in Enum.GetValues(typeof(Category.CategoryType)))
            {
                // Set parameters
                descParam.Value = categoryType.ToString();
                // Execute
                _ = cmd.ExecuteNonQuery();
            }
        }


        /// <summary>Opens an existing database.</summary>
        /// <param name="filename">Filepath of database.</param>
        /// <exception cref="Exception">
        /// Thrown when an error occurs while connecting to the database.
        /// </exception>
        public static void existingDatabase(string filename)
        {

            CloseDatabaseAndReleaseFile();

            // Use an SQLiteConnectionStringBuilder to prevent injection
            // attacks:
            string connectionString = new SQLiteConnectionStringBuilder() {
                DataSource = filename,

                // This is *supposed* to throw when the database doesn't exist,
                // but all it seems to do is prevent creating a new db:
                FailIfMissing = true,

                // Ensure foreign key constraints are respected:
                ForeignKeys = true,

            }.ToString();

            BudgetFiles.VerifyReadFromFileName(filename);
            BudgetFiles.VerifyWriteToFileName(filename);
            _connection = new SQLiteConnection(connectionString);
            _connection.Open();
        }

       // ===================================================================
       // close existing database, wait for garbage collector to
       // release the lock before continuing
       // ===================================================================
        static public void CloseDatabaseAndReleaseFile()
        {
            if (Database.dbConnection != null)
            {
                // close the database connection
                Database.dbConnection.Close();
                _connection = null;


                // TODO It feels like we should be calling Dispose() instead?
                // wait for the garbage collector to remove the
                // lock from the database file
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }

}
