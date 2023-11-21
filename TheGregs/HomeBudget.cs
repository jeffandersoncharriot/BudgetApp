using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Dynamic;
using System.Data.Common;
using System.Data;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================


namespace Budget
{
    /// <summary>
    /// Accesses a database's <see cref="Expenses"/> and
    /// <see cref="Categories"/>.
    /// </summary>
    /// <example>
    /// The following is an example of using this class.
    /// <code><![CDATA[
    /// // Creates a new database with default categories
    /// string filePath = Console.ReadLine();
    /// HomeBudget homeBudget = new(filePath, newDB: true);
    ///
    /// // Creating and adding a custom Category
    /// string categoryDescription = "Example Category";
    /// homeBudget.categories.Add(categoryDescription, Category.CategoryType.Expense);
    ///
    /// // Getting the added category
    /// Category customCategory = homeBudget.categories
    /// 	.List()
    /// 	.Find(category => category.Description == categoryDescription)!;
    ///
    /// // Adding a new expense to that category
    /// homeBudget.expenses.Add(new DateTime(1998, 1, 1), customCategory.Id, 45, "Old expense");
    /// homeBudget.expenses.Add(DateTime.Now, customCategory.Id, 1, "My sanity");
    ///
    /// // Getting data for all expenses at or after 2000-01-01
    /// foreach (BudgetItem expense in homeBudget.GetBudgetItems(
    /// 		new DateTime(2000, 1, 1),
    /// 		null,
    /// 		false,
    /// 		0)) {
    ///
    /// 	Console.WriteLine($"{expense.ShortDescription} in the category {expense.Category}.");
    /// }
    /// // Expected output:
    /// // My sanity in the category Example Category.
    /// ]]></code>
    /// </example>
    public class HomeBudget
    {
        private string _FileName;
        private string _DirName;
        private Categories _categories;
        private Expenses _expenses;
        private DbConnection connection;

        // ====================================================================
        // Properties
        // ===================================================================

        // Properties (location of files etc)

        /// <summary>
        /// File name of this instance's database file.
        /// </summary>
        public String FileName { get { return _FileName; } }

        /// <summary>
        /// Path of the directory containing this instance's database file.
        /// </summary>
        /// <remarks>
        /// Does not contain a trailing separator.
        /// </remarks>
        public String DirName { get { return _DirName; } }

        /// <summary>
        /// The full path of this instance's database file.
        /// </summary>
        public String PathName
        {
            get { return Path.GetFullPath(_DirName + "\\" + _FileName); }
        }

        // Properties (categories and expenses object)

        /// <summary>
        /// The categories associated with <see cref="expenses"/>.
        /// </summary>
        public Categories categories { get { return _categories; } }

        /// <summary>
        /// This budget's expenses.
        /// </summary>
        /// <remarks>
        /// Every element's <see cref="Expense.Category"/> refers to an object
        /// in <see cref="categories"/>.
        /// </remarks>
        public Expenses expenses { get { return _expenses; } }

        /// <summary>
        /// Creates an instance tied to a database, either by using existing
        /// data or overwriting any existing data with default data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When creating a new database, <see cref="expenses"/> is empty and
        /// <see cref="categories"/> is set according to
        /// <see cref="Categories.SetCategoriesToDefaults"/>.
        /// </para>
        /// <para>
        /// Changes <see cref="Database.dbConnection"/>, closing any existing
        /// connections.
        /// </para>
        /// </remarks>
        /// <param name="databaseFile">
        /// Path of the budget database. If this file does not exist, a new
        /// database is created with default data.
        /// </param>
        /// <param name="newDB">
        /// True if should create a new database even if
        /// <paramref name="databaseFile"/> points to an existing database
        /// (overwriting it), false if should use the existing database's data
        /// as-is. Ignored if file does not already exist. Defaults to false.
        /// </param>
        /// <exception cref="DbException">
        /// Thrown when there is a database-related error.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when there is a file-related error.
        /// </exception>
        /// <example>
        /// The following example shows how to create a new instance from an
        /// existing file.
        /// <code><![CDATA[
        /// string filePath = Console.ReadLine();
        /// HomeBudget original = new(filePath, newDB: true);
        /// 
        /// original.expenses.Add(new DateTime(2011, 4, 1), 2, 451, "Appartment Rent");
        /// original.SaveToFile(filePath);
        /// 
        /// HomeBudget loadedFromDatabase = new(filePath);
        /// 
        /// foreach (Expense expense in createdFromFile.expenses.List()) {
        /// 	Console.WriteLine(expense.Description);
        /// }
        /// // Expected output:
        /// // Appartment Rent
        /// ]]></code>
        /// The following example showcases the default data when creating a
        /// new file.
        /// <code><![CDATA[
        /// HomeBudget homeBudget = new("nonexistent_filepath.db", newDB: true);
        /// 
        /// foreach (Expense expense in homeBudget.expenses.List()) {
        /// 	Console.WriteLine("Blah");
        /// }
        /// // There should be no output because there are no expenses
        /// 
        /// foreach (Category category in homeBudget.categories.List()) {
        /// 	Console.WriteLine(category);
        /// }
        /// // Expected output (because of default categories):
        /// // Utilities
        /// // Rent
        /// // Food
        /// // Entertainment
        /// // Education
        /// // Miscellaneous
        /// // Medical Expenses
        /// // Vacation
        /// // Credit Card
        /// // Clothes
        /// // Gifts
        /// // Insurance
        /// // Transportation
        /// // Eating Out
        /// // Savings
        /// // Income
        /// ]]></code>
        /// </example>
        public HomeBudget(String databaseFile, bool newDB=false)
        {
            if (!newDB && File.Exists(databaseFile))
            {
                // if database exists, and user doesn't want a new database, open existing DB
                Database.existingDatabase(databaseFile);
            }
            else
            {
                // file did not exist, or user wants a new database, so open NEW DB
                Database.newDatabase(databaseFile);
                newDB = true;
            }

            // create the category object
            _categories = new Categories(Database.dbConnection!, newDB);

            // create the _expenses course
            _expenses = new Expenses(Database.dbConnection!, newDB);

            connection = Database.dbConnection!;
            _DirName = Path.GetDirectoryName(databaseFile)!;
            _FileName = Path.GetFileName(databaseFile);
        }

        #region GetList



        // ============================================================================
        // Get all expenses list
        // NOTE: VERY IMPORTANT... budget amount is the negative of the expense amount
        // Reasoning: an expense of $15 is -$15 from your bank account.
        // ============================================================================
        /// <summary>
        /// Returns a list of filtered <see cref="BudgetItem"/> objects
        /// representing this instance's <see cref="expenses"/>.
        /// </summary>
        /// <remarks>
        /// Returned items are ordered by date ascending.
        /// </remarks>
        /// <param name="Start">
        /// Earliest <see cref="BudgetItem.Date">BudgetItem.Date</see> included
        /// in results, inclusive. If null, defaults to 1900-01-01T00:00:00.
        /// </param>
        /// <param name="End">
        /// Latest <see cref="BudgetItem.Date">BudgetItem.Date</see> included
        /// in results, inclusive. If null, defaults to 2500-01-01T00:00:00.
        /// </param>
        /// <param name="FilterFlag">
        /// True if should filter results using <paramref name="CategoryID"/>,
        /// false otherwise.
        /// </param>
        /// <param name="CategoryID">
        /// If <paramref name="FilterFlag"/> is true, only include items whose
        /// <see cref="BudgetItem.CategoryID"/> equals this; Ignored otherwise.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <returns>The filtered items as a new list.</returns>
        /// <example>
        /// For all examples below, assume the budget file contains the
        /// following elements:
        ///
        /// <code>
        /// Cat_ID | Expense_ID | Date                  | Description              | Cost
        ///     10 |          1 | 1/10/2018 12:00:00 AM | Clothes hat (on credit)  |  10
        ///      9 |          2 | 1/11/2018 12:00:00 AM | Credit Card hat          | -10
        ///     10 |          3 | 1/10/2019 12:00:00 AM | Clothes scarf(on credit) |  15
        ///      9 |          4 | 1/10/2020 12:00:00 AM | Credit Card scarf        | -15
        ///     14 |          5 | 1/11/2020 12:00:00 AM | Eating Out McDonalds     |  45
        ///     14 |          7 | 1/12/2020 12:00:00 AM | Eating Out Wendys        |  25
        ///     14 |         10 |  2/1/2020 12:00:00 AM | Eating Out Pizza         |  33.33
        ///      9 |         13 | 2/10/2020 12:00:00 AM | Credit Card mittens      | -15
        ///      9 |         12 | 2/25/2020 12:00:00 AM | Credit Card Hat          | -25
        ///     14 |         11 | 2/27/2020 12:00:00 AM | Eating Out Pizza         |  33.33
        ///     14 |          9 | 7/11/2020 12:00:00 AM | Eating Out Cafeteria     |  11.11
        /// </code>
        ///
        /// <b>Getting a list of ALL budget items.</b>
        ///
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        ///
        /// // Get a list of all budget items
        /// var budgetItems = budget.GetBudgetItems(null, null, false, 0);
        ///
        /// // print important information
        /// foreach (var bi in budgetItems) {
        /// 	Console.WriteLine (
        /// 			String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 				bi.Date.ToString("yyyy/MMM/dd"),
        /// 				bi.ShortDescription,
        /// 				bi.Amount,
        /// 				bi.Balance));
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// 2018-Jan.-10 Clothes hat (on credit)    -$10.00      -$10.00
        /// 2018-Jan.-11 Credit Card hat             $10.00        $0.00
        /// 2019-Jan.-10 Clothes scarf(on credit)   -$15.00      -$15.00
        /// 2020-Jan.-10 Credit Card scarf           $15.00        $0.00
        /// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
        /// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
        /// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
        /// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
        /// 2020-Feb.-25 Credit Card Hat             $25.00      -$63.33
        /// 2020-Feb.-27 Eating Out Pizza           -$33.33      -$96.66
        /// 2020-Jul.-11 Eating Out Cafeteria       -$11.11     -$107.77
        /// </code>
        ///
        /// <b>Getting a list of all budget items WITHIN A RANGE OF DATETIMES.</b>
        /// 
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        /// 
        /// // Get a list of all budget items at or after 1/11/2020 12:00:00 AM and at or
        /// // before 2/10/2020 12:00:00 AM.
        /// // Notice that the start and end dates are inclusive, so "Eating Out McDonalds"
        /// // and "Credit Card mittens" don't get filtered out.
        /// var budgetItems = budget.GetBudgetItems(
        /// 	new DateTime(2020, 1, 11),
        /// 	new DateTime(2020, 2, 10),
        /// 	false,
        /// 	0);
        /// 
        /// // print important information
        /// foreach (var bi in budgetItems) {
        /// 	Console.WriteLine (
        /// 			String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 				bi.Date.ToString("yyyy/MMM/dd"),
        /// 				bi.ShortDescription,
        /// 				bi.Amount,
        /// 				bi.Balance));
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
        /// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
        /// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
        /// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
        /// </code>
        ///
        /// <b>Getting a list of all budget items IN A SPECIFIC CATEGORY.</b>
        ///
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        /// 
        /// // Get a list of all budget items in the category with ID 9.
        /// // When FilterFlag is true, only items matching CategoryID will be returned.
        /// // CategoryID is ignored when FilterFlag is false (this is why the previous
        /// // examples didn't filter everything out even if their Category wasn't 0).
        /// var budgetItems = budget.GetBudgetItems(null, null, true, 9);
        /// 
        /// // print important information
        /// foreach (var bi in budgetItems) {
        /// 	Console.WriteLine (
        /// 			String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 				bi.Date.ToString("yyyy/MMM/dd"),
        /// 				bi.ShortDescription,
        /// 				bi.Amount,
        /// 				bi.Balance));
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
        /// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
        /// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
        /// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
        /// </code>
        /// </example>
        // TODO Should return an ImmutableList.
        public List<BudgetItem> GetBudgetItems(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            // ------------------------------------------------------------------------
            // return joined list within time frame
            // ------------------------------------------------------------------------
            Start = Start ?? new DateTime(1900, 1, 1);
            End = End ?? new DateTime(2500, 1, 1);

            using DbCommand cmd = connection.CreateCommand();
            cmd.CommandText
                = "SELECT e.Id AS eId, Date, Amount, e.Description AS eDesc, c.Id AS cId, c.Description AS cDesc"
                + " FROM expenses e"
                + " JOIN categories c ON c.Id = CategoryId"
                // Not using parameters because these strings are clean:
                + $" WHERE e.Date BETWEEN '{Start.Value:yyyy-MM-dd}' AND '{End.Value:yyyy-MM-dd}'"
                + (FilterFlag ? $" AND CategoryId = {CategoryID}" : "")
                + " ORDER BY Date";

            // ------------------------------------------------------------------------
            // create a BudgetItem list with totals,
            // ------------------------------------------------------------------------
            List<BudgetItem> items = new List<BudgetItem>();
            Double total = 0;

            DbDataReader reader = cmd.ExecuteReader();

            // Get column indices:
            int eIdColumn = reader.GetOrdinal("eId"),
                dateColumn = reader.GetOrdinal("Date"),
                amountColumn = reader.GetOrdinal("Amount"),
                eDescColumn = reader.GetOrdinal("eDesc"),
                cIdColumn = reader.GetOrdinal("cId"),
                cDescColumn = reader.GetOrdinal("cDesc");

            while (reader.Read())
            {
                double amount = reader.GetDouble(amountColumn);

                // keep track of running totals
                total += amount;

                items.Add(new BudgetItem
                {
                    CategoryID = reader.GetInt32(cIdColumn),
                    ExpenseID = reader.GetInt32(eIdColumn),
                    ShortDescription = reader.GetString(eDescColumn),
                    Date = reader.GetDateTime(dateColumn),
                    Amount = amount,
                    Category = reader.GetString(cDescColumn),
                    Balance = total
                });
            }

            return items;
        }

        // ============================================================================
        // Group all expenses month by month (sorted by year/month)
        // returns a list of BudgetItemsByMonth which is 
        // "year/month", list of budget items, and total for that month
        // ============================================================================
        /// <summary>
        /// Returns filtered <see cref="BudgetItem"/> objects representing this
        /// instance's <see cref="expenses"/> grouped by month and year.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returned groups and the items inside them are ordered by date
        /// ascending.
        /// </para>
        /// <para>
        /// The <see cref="BudgetItem.Balance"/> values of all
        /// <see cref="BudgetItem"/> objects are relative to all returned
        /// items, not just the items in their specific months.
        /// </para>
        /// </remarks>
        /// <returns>A new list containing groups of items by month.</returns>
        /// <inheritdoc cref="GetBudgetItems(DateTime?, DateTime?, bool, int)"/>
        /// <example>
        /// For all examples below, assume the budget file contains the
        /// following elements:
        ///
        /// <code>
        /// Cat_ID | Expense_ID | Date                  | Description              | Cost
        ///     10 |          1 | 1/10/2018 12:00:00 AM | Clothes hat (on credit)  |  10
        ///      9 |          2 | 1/11/2018 12:00:00 AM | Credit Card hat          | -10
        ///     10 |          3 | 1/10/2019 12:00:00 AM | Clothes scarf(on credit) |  15
        ///      9 |          4 | 1/10/2020 12:00:00 AM | Credit Card scarf        | -15
        ///     14 |          5 | 1/11/2020 12:00:00 AM | Eating Out McDonalds     |  45
        ///     14 |          7 | 1/12/2020 12:00:00 AM | Eating Out Wendys        |  25
        ///     14 |         10 |  2/1/2020 12:00:00 AM | Eating Out Pizza         |  33.33
        ///      9 |         13 | 2/10/2020 12:00:00 AM | Credit Card mittens      | -15
        ///      9 |         12 | 2/25/2020 12:00:00 AM | Credit Card Hat          | -25
        ///     14 |         11 | 2/27/2020 12:00:00 AM | Eating Out Pizza         |  33.33
        ///     14 |          9 | 7/11/2020 12:00:00 AM | Eating Out Cafeteria     |  11.11
        /// </code>
        ///
        /// <b>Getting a list of ALL budget item groups.</b>
        ///
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        /// 
        /// // Get a list of all budget item groups
        /// var monthGroups = budget.GetBudgetItemsByMonth(null, null, false, 0);
        /// 
        /// // print important information
        /// foreach (var mg in monthGroups) {
        /// 	Console.WriteLine($"Month: {mg.Month}, Amount Total: {mg.Total}");
        /// 
        /// 	foreach (var budgetItem in mg.Details) {
        /// 		Console.WriteLine(
        /// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
        /// 					budgetItem.ShortDescription,
        /// 					budgetItem.Amount,
        /// 					budgetItem.Balance));
        /// 	}
        /// 
        /// 	Console.WriteLine();
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// Month: 2018/01, Amount Total: 0
        /// 2018-Jan.-10 Clothes hat (on credit)    -$10.00      -$10.00
        /// 2018-Jan.-11 Credit Card hat             $10.00        $0.00
        /// 
        /// Month: 2019/01, Amount Total: -15
        /// 2019-Jan.-10 Clothes scarf(on credit)   -$15.00      -$15.00
        /// 
        /// Month: 2020/01, Amount Total: -55
        /// 2020-Jan.-10 Credit Card scarf           $15.00        $0.00
        /// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
        /// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
        /// 
        /// Month: 2020/02, Amount Total: -26.659999999999997
        /// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
        /// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
        /// 2020-Feb.-25 Credit Card Hat             $25.00      -$63.33
        /// 2020-Feb.-27 Eating Out Pizza           -$33.33      -$96.66
        /// 
        /// Month: 2020/07, Amount Total: -11.11
        /// 2020-Jul.-11 Eating Out Cafeteria       -$11.11     -$107.77
        /// </code>
        ///
        /// <b>Getting a list of all budget item groups WITHIN A RANGE OF DATETIMES.</b>
        /// 
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        /// 
        /// // Get a list of all budget items at or after 1/11/2020 12:00:00 AM and at or
        /// // before 2/10/2020 12:00:00 AM grouped by month.
        /// // Notice that the start and end dates are inclusive, so "Eating Out McDonalds"
        /// // and "Credit Card mittens" don't get filtered out.
        /// var monthGroups = budget.GetBudgetItemsByMonth(
        /// 	new DateTime(2020, 1, 11),
        /// 	new DateTime(2020, 2, 10),
        /// 	false,
        /// 	0);
        /// 
        /// // print important information
        /// foreach (var mg in monthGroups) {
        /// 	Console.WriteLine($"Month: {mg.Month}, Amount Total: {mg.Total}");
        /// 
        /// 	foreach (var budgetItem in mg.Details) {
        /// 		Console.WriteLine(
        /// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
        /// 					budgetItem.ShortDescription,
        /// 					budgetItem.Amount,
        /// 					budgetItem.Balance));
        /// 	}
        /// 
        /// 	Console.WriteLine();
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// Month: 2020/01, Amount Total: -70
        /// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
        /// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
        /// 
        /// Month: 2020/02, Amount Total: -18.33
        /// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
        /// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
        /// </code>
        ///
        /// <b>Getting a list of all budget items IN A SPECIFIC CATEGORY.</b>
        ///
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        /// 
        /// // Get a list of all budget items in the category with ID 9 grouped by month.
        /// // When FilterFlag is true, only items matching CategoryID will be returned.
        /// // CategoryID is ignored when FilterFlag is false (this is why the previous
        /// // examples didn't filter everything out even if their Category wasn't 0).
        /// var monthGroups = budget.GetBudgetItemsByMonth(null, null, true, 9);
        /// 
        /// // print important information
        /// foreach (var mg in monthGroups) {
        /// 	Console.WriteLine($"Month: {mg.Month}, Amount Total: {mg.Total}");
        /// 
        /// 	foreach (var budgetItem in mg.Details) {
        /// 		Console.WriteLine(
        /// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
        /// 					budgetItem.ShortDescription,
        /// 					budgetItem.Amount,
        /// 					budgetItem.Balance));
        /// 	}
        /// 
        /// 	Console.WriteLine();
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// Month: 2018/01, Amount Total: 10
        /// 2018-Jan.-11 Credit Card hat             $10.00       $10.00
        /// 
        /// Month: 2020/01, Amount Total: 15
        /// 2020-Jan.-10 Credit Card scarf           $15.00       $25.00
        /// 
        /// Month: 2020/02, Amount Total: 40
        /// 2020-Feb.-10 Credit Card mittens         $15.00       $40.00
        /// 2020-Feb.-25 Credit Card Hat             $25.00       $65.00
        /// </code>
        /// </example>
        public List<BudgetItemsByMonth> GetBudgetItemsByMonth(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            // -----------------------------------------------------------------------
            // get all items first
            // -----------------------------------------------------------------------
            List<BudgetItem> items = GetBudgetItems(Start, End, FilterFlag, CategoryID);

            // -----------------------------------------------------------------------
            // Group by year/month
            // -----------------------------------------------------------------------
            var GroupedByMonth = items.GroupBy(c => c.Date.ToString("yyyy-MM"));

            // -----------------------------------------------------------------------
            // create new list
            // -----------------------------------------------------------------------
            // Prepare command to query each group's total
            var cmd = connection.CreateCommand(
                "SELECT SUM(amount)"
                + " FROM expenses"
                + " WHERE date BETWEEN @monthStart AND @monthEnd"
                + ( Start is null ? "" : " AND date >= @overallStart")
                + ( End is null ? "" : " AND date <= @overallEnd")
                + (FilterFlag ? $" AND CategoryId = {CategoryID}" : ""));

            // Set overall start and end dates
            if (Start.HasValue) cmd.SetParam("overallStart", Start.Value.ToString("yyyy-MM-dd"));
            if (End.HasValue) cmd.SetParam("overallEnd", End.Value.ToString("yyyy-MM-dd"));

            var summary = new List<BudgetItemsByMonth>();
            foreach (var MonthGroup in GroupedByMonth)
            {
                // query total for this month
                cmd.SetParam("monthStart", MonthGroup.Key + "-01");
                // Because we're using strings, we can always use 31:
                cmd.SetParam("monthEnd", MonthGroup.Key + "-31");

                double total = cmd.ExecuteScalar() as double? ?? 0.0;
                
                // create list of details
                var details = MonthGroup.ToList();

                // Add new BudgetItemsByMonth to our list
                summary.Add(new BudgetItemsByMonth
                {
                    // Replace '-' with '/' in Month:
                    Month = MonthGroup.Key[..4] + "/" + MonthGroup.Key[5..],
                    Details = details,
                    Total = total
                });
            }

            return summary;
        }

        // ============================================================================
        // Group all expenses by category (ordered by category name)
        // ============================================================================
        /// <summary>
        /// Returns filtered <see cref="BudgetItem"/> objects representing this
        /// instance's <see cref="expenses"/> grouped by <see
        /// cref="BudgetItem.Category"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returned groups are ordered by
        /// <see cref="Category.Description">Category.Description</see>
        /// ascending. Items inside groups are ordered by
        /// <see cref="BudgetItem.Date"/> ascending.
        /// </para>
        /// <para>
        /// The <see cref="BudgetItem.Balance"/> values of all
        /// <see cref="BudgetItem"/> objects are relative to all returned
        /// items, not just the items in their specific categories, and
        /// increases chronologically.
        /// </para>
        /// <para>
        /// Note that groups are by <see
        /// cref="Category.Description">Category.Description</see>, not by <see
        /// cref="Category.Id">Category.Id</see>
        /// </para>
        /// </remarks>
        /// <returns>
        /// A new list containing groups of items by category.
        /// </returns>
        /// <inheritdoc cref="GetBudgetItems(DateTime?, DateTime?, bool, int)"/>
		/// <example>
		/// For all examples below, assume the budget file contains the
		/// following elements:
		///
		/// <code>
		/// Cat_ID | Expense_ID | Date                  | Description              | Cost
		///     10 |          1 | 1/10/2018 12:00:00 AM | Clothes hat (on credit)  |  10
		///      9 |          2 | 1/11/2018 12:00:00 AM | Credit Card hat          | -10
		///     10 |          3 | 1/10/2019 12:00:00 AM | Clothes scarf(on credit) |  15
		///      9 |          4 | 1/10/2020 12:00:00 AM | Credit Card scarf        | -15
		///     14 |          5 | 1/11/2020 12:00:00 AM | Eating Out McDonalds     |  45
		///     14 |          7 | 1/12/2020 12:00:00 AM | Eating Out Wendys        |  25
		///     14 |         10 |  2/1/2020 12:00:00 AM | Eating Out Pizza         |  33.33
		///      9 |         13 | 2/10/2020 12:00:00 AM | Credit Card mittens      | -15
		///      9 |         12 | 2/25/2020 12:00:00 AM | Credit Card Hat          | -25
		///     14 |         11 | 2/27/2020 12:00:00 AM | Eating Out Pizza         |  33.33
		///     14 |          9 | 7/11/2020 12:00:00 AM | Eating Out Cafeteria     |  11.11
		/// </code>
		///
		/// <b>Getting a list of ALL budget item groups.</b>
		///
		/// <code><![CDATA[
		/// HomeBudget budget = new("database.db", newDB: false);
		/// 
		/// // Get a list of all budget item groups
		/// var categoryGroups = budget.GetBudgetItemsByCategory(null, null, false, 0);
		/// 
		/// // print important information
		/// foreach (var cg in categoryGroups) {
		/// 	Console.WriteLine($"Category: {cg.Category}, Amount Total: {cg.Total}");
		/// 
		/// 	foreach (var budgetItem in cg.Details) {
		/// 		Console.WriteLine(
		/// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
		/// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
		/// 					budgetItem.ShortDescription,
		/// 					budgetItem.Amount,
		/// 					budgetItem.Balance));
		/// 	}
		/// 
		/// 	Console.WriteLine();
		/// }
		/// ]]></code>
		///
		/// Sample output:
		/// <code>
		/// Category: Clothes, Amount Total: -25
		/// 2018-Jan.-10 Clothes hat (on credit)    -$10.00      -$10.00
		/// 2019-Jan.-10 Clothes scarf(on credit)   -$15.00      -$15.00
		/// 
		/// Category: Credit Card, Amount Total: 65
		/// 2018-Jan.-11 Credit Card hat             $10.00        $0.00
		/// 2020-Jan.-10 Credit Card scarf           $15.00        $0.00
		/// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
		/// 2020-Feb.-25 Credit Card Hat             $25.00      -$63.33
		/// 
		/// Category: Eating Out, Amount Total: -147.76999999999998
		/// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
		/// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
		/// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
		/// 2020-Feb.-27 Eating Out Pizza           -$33.33      -$96.66
		/// 2020-Jul.-11 Eating Out Cafeteria       -$11.11     -$107.77
		/// </code>
		///
		/// <b>Getting a list of all budget item groups WITHIN A RANGE OF DATETIMES.</b>
		/// 
		/// <code><![CDATA[
		/// HomeBudget budget = new("database.db", newDB: false);
		/// 
		/// // Get a list of all budget items at or after 1/11/2020 12:00:00 AM and at or
		/// // before 2/10/2020 12:00:00 AM grouped by category.
		/// // Notice that the start and end dates are inclusive, so "Eating Out McDonalds"
		/// // and "Credit Card mittens" don't get filtered out.
		/// var categoryGroups = budget.GetBudgetItemsByCategory(
		/// 	new DateTime(2020, 1, 11),
		/// 	new DateTime(2020, 2, 10),
		/// 	false,
		/// 	0);
		/// 
		/// // print important information
		/// foreach (var cg in categoryGroups) {
		/// 	Console.WriteLine($"Category: {cg.Category}, Amount Total: {cg.Total}");
		/// 
		/// 	foreach (var budgetItem in cg.Details) {
		/// 		Console.WriteLine(
		/// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
		/// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
		/// 					budgetItem.ShortDescription,
		/// 					budgetItem.Amount,
		/// 					budgetItem.Balance));
		/// 	}
		/// 
		/// 	Console.WriteLine();
		/// }
		/// ]]></code>
		///
		/// Sample output:
		/// <code>
		/// Category: Credit Card, Amount Total: 15
		/// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
		/// 
		/// Category: Eating Out, Amount Total: -103.33
		/// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
		/// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
		/// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
		/// </code>
		///
		/// <b>Getting a list of all budget items IN A SPECIFIC CATEGORY.</b>
		/// (note that this is typically useless)
		///
		/// <code><![CDATA[
		/// HomeBudget budget = new("database.db", newDB: false);
		/// 
		/// // Get a list of all budget items in the category with ID 9 grouped by category.
		/// // When FilterFlag is true, only items matching CategoryID will be returned.
		/// // CategoryID is ignored when FilterFlag is false (this is why the previous
		/// // examples didn't filter everything out even if their Category wasn't 0).
		/// var categoryGroups = budget.GetBudgetItemsByCategory(null, null, true, 9);
		/// 
		/// // print important information
		/// foreach (var cg in categoryGroups) {
		/// 	Console.WriteLine($"Category: {cg.Category}, Amount Total: {cg.Total}");
		/// 
		/// 	foreach (var budgetItem in cg.Details) {
		/// 		Console.WriteLine(
		/// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
		/// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
		/// 					budgetItem.ShortDescription,
		/// 					budgetItem.Amount,
		/// 					budgetItem.Balance));
		/// 	}
		/// 
		/// 	Console.WriteLine();
		/// }
		/// ]]></code>
		///
		/// Sample output:
		/// <code>
		/// Category: Credit Card, Amount Total: 65
		/// 2018-Jan.-11 Credit Card hat             $10.00       $10.00
		/// 2020-Jan.-10 Credit Card scarf           $15.00       $25.00
		/// 2020-Feb.-10 Credit Card mittens         $15.00       $40.00
		/// 2020-Feb.-25 Credit Card Hat             $25.00       $65.00
		/// </code>
		/// </example>
        // TODO Breaking change: Remove FilterFlag and CategoryID? Niche
        //      use-case in delegates?
        public List<BudgetItemsByCategory> GetBudgetItemsByCategory(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            // ------------------------------------------------------------------------
            // return joined list within time frame
            // ------------------------------------------------------------------------
            Start = Start ?? new DateTime(1900, 1, 1);
            End = End ?? new DateTime(2500, 1, 1);

            // -----------------------------------------------------------------------
            // get all items first
            // -----------------------------------------------------------------------
            List<BudgetItem> items = GetBudgetItems(Start, End, FilterFlag, CategoryID);

            // -----------------------------------------------------------------------
            // Group by Category
            // -----------------------------------------------------------------------
            var GroupedByCategory = items.GroupBy(c => c.Category);

            // -----------------------------------------------------------------------
            // create new list
            // -----------------------------------------------------------------------
            var summary = new List<BudgetItemsByCategory>();

            using DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = cmd.CommandText
                = "SELECT SUM(Amount) AS total, c.Description AS cDesc"
                + " FROM expenses e"
                + " JOIN categories c ON c.Id = CategoryId"
                // Not using parameters because these strings are clean:
                + $" WHERE e.Date BETWEEN '{Start.Value:yyyy-MM-dd}' AND '{End.Value:yyyy-MM-dd}'"
                + (FilterFlag ? $" AND CategoryId = {CategoryID}" : "")
                + " GROUP BY cDesc"
                + " ORDER BY cDesc";

            DbDataReader reader = cmd.ExecuteReader();

            // Get column indices:
            int categoryTotalColumn = reader.GetOrdinal("total");

            foreach (var CategoryGroup in GroupedByCategory.OrderBy(g => g.Key))
            {

                // calculate total for this category, and create list of details
                var details = new List<BudgetItem>();

                reader.Read();

                foreach (var item in CategoryGroup)
                {
                    details.Add(item);
                }

                // Add new BudgetItemsByCategory to our list
                summary.Add(new BudgetItemsByCategory
                    {
                        Category = CategoryGroup.Key,
                        Details = details,
                        Total = reader.GetDouble(categoryTotalColumn)
                    });

            }

            return summary;
        }


        // ============================================================================
        // Group all events by category and Month
        // creates a list of Dictionary objects (which are objects that contain key value pairs).
        // The list of Dictionary objects includes:
        //          one dictionary object per month with expenses,
        //          and one dictionary object for the category totals
        // 
        // Each per month dictionary object has the following key value pairs:
        //           "Month", <the year/month for that month as a string>
        //           "Total", <the total amount for that month as a double>
        //            and for each category for which there is an expense in the month:
        //             "details:category", a List<BudgetItem> of all items in that category for the month
        //             "category", the total amount for that category for this month
        //
        // The one dictionary for the category totals has the following key value pairs:
        //             "Month", the string "TOTALS"
        //             for each category for which there is an expense in ANY month:
        //             "category", the total for that category for all the months
        // ============================================================================
        /// <summary>
        /// Returns filtered <see cref="BudgetItem"/> objects representing this
        /// instance's <see cref="expenses"/> grouped by
        /// <see cref="BudgetItem.Category"/> and month.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Returns a list of dictionaries. This includes, in order:
        /// <list type="bullet">
        /// <item>
        /// One dictionary for each month which has expenses (months are per
        /// year, so January 2021 and January 2022 would be distinct). Each
        /// dictionary has the following entries:
        /// <list type="table">
        ///     <listheader>
        ///     <term># of entries</term>
        ///     <term>Key</term>
        ///     <term>Value</term>
        ///     </listheader>
        ///     <item>
        ///         <term>One.</term>
        ///         <term>"Month"</term>
        ///         <term>A string containing the year/month for the
        ///         dictionary in this format: <c>yyyy/MM</c></term>
        ///     </item>
        ///     <item>
        ///         <term>One.</term>
        ///         <term>"Total"</term>
        ///         <term>A double containing the total <see
        ///         cref="BudgetItem.Amount"/> for the
        ///         dictionary.</term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///         One per <see cref="Category"/> included in month.
        ///         </term>
        ///         <term>
        ///         <see cref="Category.Description">Category.Description</see>
        ///         </term>
        ///         <term>
        ///         The total <see cref="BudgetItem.Amount"/> for
        ///         all included items in the month whose
        ///         <see cref="BudgetItem.Category"/> matches the key.
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///         One per <see cref="Category"/> included in month.
        ///         </term>
        ///         <term>
        ///         "details:" +
        ///         <see cref="Category.Description">Category.Description</see>
        ///         </term>
        ///         <term>
        ///         A <see cref="List{T}">List</see>&lt;<see
        ///         cref="BudgetItem"/>&gt; of all included items whose
        ///         <see cref="BudgetItem.Category"/> matches the one in the
        ///         key.
        ///         </term>
        ///     </item>
        /// </list>
        /// </item>
        /// <item>
        /// A dictionary for the category totals with the following entries:
        /// <list type="table">
        ///     <listheader>
        ///     <term># of entries</term>
        ///     <term>Key</term>
        ///     <term>Value</term>
        ///     </listheader>
        ///     <item>
        ///         <term>One.</term>
        ///         <term>"Month"</term>
        ///         <term>The string "TOTALS".</term>
        ///     </item>
        ///     <item>
        ///         <term>
        ///         One per <see cref="Category"/> included in month.
        ///         </term>
        ///         <term>
        ///         <see cref="Category.Description">Category.Description</see>
        ///         </term>
        ///         <term>
        ///         The total <see cref="BudgetItem.Amount"/> for <b>all</b>
        ///         included items (in every month) whose
        ///         <see cref="BudgetItem.Category"/> matches the key.
        ///         </term>
        ///     </item>
        /// </list>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// When a key conflicts with a <see cref="Category"/>'s
        /// <see cref="Category.Description"/>, the value in that key is
        /// unspecified.
        /// </para>
        /// <para>
        /// Month dictionaries and the items within are in chronologically
        /// ascending order.
        /// </para>
        /// <para>
        /// The <see cref="BudgetItem.Balance"/> values of all
        /// <see cref="BudgetItem"/> objects are relative to all returned
        /// items, not just the items in their specific dictionaries, and
        /// increase chronologically.
        /// </para>
        /// </remarks>
        /// <returns>
        /// A new dictionary containing groups of items by month and category.
        /// </returns>
        /// <inheritdoc cref="GetBudgetItems(DateTime?, DateTime?, bool, int)"/>
        /// <example>
        /// For all examples below, assume the budget file contains the
        /// following elements:
        ///
        /// <code>
        /// Cat_ID | Expense_ID | Date                  | Description              | Cost
        ///     10 |          1 | 1/10/2018 12:00:00 AM | Clothes hat (on credit)  |  10
        ///      9 |          2 | 1/11/2018 12:00:00 AM | Credit Card hat          | -10
        ///     10 |          3 | 1/10/2019 12:00:00 AM | Clothes scarf(on credit) |  15
        ///      9 |          4 | 1/10/2020 12:00:00 AM | Credit Card scarf        | -15
        ///     14 |          5 | 1/11/2020 12:00:00 AM | Eating Out McDonalds     |  45
        ///     14 |          7 | 1/12/2020 12:00:00 AM | Eating Out Wendys        |  25
        ///     14 |         10 |  2/1/2020 12:00:00 AM | Eating Out Pizza         |  33.33
        ///      9 |         13 | 2/10/2020 12:00:00 AM | Credit Card mittens      | -15
        ///      9 |         12 | 2/25/2020 12:00:00 AM | Credit Card Hat          | -25
        ///     14 |         11 | 2/27/2020 12:00:00 AM | Eating Out Pizza         |  33.33
        ///     14 |          9 | 7/11/2020 12:00:00 AM | Eating Out Cafeteria     |  11.11
        /// </code>
        ///
        /// <b>Getting a list of ALL budget item groups.</b>
        ///
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        /// 
        /// // Get a list of all budget item groups
        /// var groupDictionaries = budget.GetBudgetDictionaryByCategoryAndMonth(null, null, false, 0);
        /// 
        /// // print important information from dictionaries other than the category totals
        /// // dictionary (which is the last in the list)
        /// foreach (var gd in groupDictionaries.SkipLast(1)) {
        /// 	Console.WriteLine($"Month: {gd["Month"]}");
        /// 
        /// 	foreach (var entry in gd
        /// 			.Where(entry => entry.Key.StartsWith("details:"))) {
        /// 
        /// 		// Goes from details to category total
        /// 		// ex. From "details:Income" to "Income":
        /// 		string keyWithoutPrefix = entry.Key.Substring("details:".Length);
        /// 
        /// 		Console.WriteLine($"Category Amount Total: {gd[keyWithoutPrefix]}");
        /// 		foreach (var budgetItem in (List<BudgetItem>)entry.Value) {
        /// 			Console.WriteLine(
        /// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
        /// 					budgetItem.ShortDescription,
        /// 					budgetItem.Amount,
        /// 					budgetItem.Balance));
        /// 		}
        /// 
        /// 		Console.WriteLine();
        /// 	}
        /// }
        /// 
        /// // print information in the category totals dictionary
        /// var categoryTotals = groupDictionaries[groupDictionaries.Count - 1];
        /// Console.WriteLine("Category totals");
        /// Console.WriteLine(categoryTotals["Month"]);
        /// foreach (var entry in categoryTotals
        /// 		.Where(entry => entry.Key != "Month")) {
        /// 
        /// 	Console.WriteLine($"{entry.Key}: {entry.Value}");
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// Month: 2018/01
        /// Category Amount Total: -10
        /// 2018-Jan.-10 Clothes hat (on credit)    -$10.00      -$10.00
        /// 
        /// Category Amount Total: 10
        /// 2018-Jan.-11 Credit Card hat             $10.00        $0.00
        /// 
        /// Month: 2019/01
        /// Category Amount Total: -15
        /// 2019-Jan.-10 Clothes scarf(on credit)   -$15.00      -$15.00
        /// 
        /// Month: 2020/01
        /// Category Amount Total: 15
        /// 2020-Jan.-10 Credit Card scarf           $15.00        $0.00
        /// 
        /// Category Amount Total: -70
        /// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
        /// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
        /// 
        /// Month: 2020/02
        /// Category Amount Total: 40
        /// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
        /// 2020-Feb.-25 Credit Card Hat             $25.00      -$63.33
        /// 
        /// Category Amount Total: -66.66
        /// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
        /// 2020-Feb.-27 Eating Out Pizza           -$33.33      -$96.66
        /// 
        /// Month: 2020/07
        /// Category Amount Total: -11.11
        /// 2020-Jul.-11 Eating Out Cafeteria       -$11.11     -$107.77
        /// 
        /// Category totals
        /// TOTALS
        /// Credit Card: 65
        /// Clothes: -25
        /// Eating Out: -147.76999999999998
        /// </code>
        ///
        /// <b>Getting a list of all budget item groups WITHIN A RANGE OF DATETIMES.</b>
        /// 
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        /// 
        /// // Get a list of all budget items at or after 1/11/2020 12:00:00 AM and at or
        /// // before 2/10/2020 12:00:00 AM grouped by month and category.
        /// // Notice that the start and end dates are inclusive, so "Eating Out McDonalds"
        /// // and "Credit Card mittens" don't get filtered out.
        /// var groupDictionaries = budget.GetBudgetDictionaryByCategoryAndMonth(
        /// 	new DateTime(2020, 1, 11),
        /// 	new DateTime(2020, 2, 10),
        /// 	false,
        /// 	0);
        /// 
        /// // print important information from dictionaries other than the category totals
        /// // dictionary (which is the last in the list)
        /// foreach (var gd in groupDictionaries.SkipLast(1)) {
        /// 	Console.WriteLine($"Month: {gd["Month"]}");
        /// 
        /// 	foreach (var entry in gd
        /// 			.Where(entry => entry.Key.StartsWith("details:"))) {
        /// 
        /// 		// Goes from details to category total
        /// 		// ex. From "details:Income" to "Income":
        /// 		string keyWithoutPrefix = entry.Key.Substring("details:".Length);
        /// 
        /// 		Console.WriteLine($"Category Amount Total: {gd[keyWithoutPrefix]}");
        /// 		foreach (var budgetItem in (List<BudgetItem>)entry.Value) {
        /// 			Console.WriteLine(
        /// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
        /// 					budgetItem.ShortDescription,
        /// 					budgetItem.Amount,
        /// 					budgetItem.Balance));
        /// 		}
        /// 
        /// 		Console.WriteLine();
        /// 	}
        /// }
        /// 
        /// // print information in the category totals dictionary
        /// var categoryTotals = groupDictionaries[groupDictionaries.Count - 1];
        /// Console.WriteLine("Category totals");
        /// Console.WriteLine(categoryTotals["Month"]);
        /// foreach (var entry in categoryTotals
        /// 		.Where(entry => entry.Key != "Month")) {
        /// 
        /// 	Console.WriteLine($"{entry.Key}: {entry.Value}");
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// Month: 2020/01
        /// Category Amount Total: -70
        /// 2020-Jan.-11 Eating Out McDonalds       -$45.00      -$45.00
        /// 2020-Jan.-12 Eating Out Wendys          -$25.00      -$70.00
        /// 
        /// Month: 2020/02
        /// Category Amount Total: 15
        /// 2020-Feb.-10 Credit Card mittens         $15.00      -$88.33
        /// 
        /// Category Amount Total: -33.33
        /// 2020-Feb.-01 Eating Out Pizza           -$33.33     -$103.33
        /// 
        /// Category totals
        /// TOTALS
        /// Credit Card: 15
        /// Eating Out: -103.33
        /// </code>
        ///
        /// <b>Getting a list of all budget items IN A SPECIFIC CATEGORY.</b>
        /// (note that this is typically useless)
        ///
        /// <code><![CDATA[
        /// HomeBudget budget = new("database.db", newDB: false);
        /// 
        /// // Get a list of all budget items in the category with ID 9 grouped by month
        /// // and category.
        /// // When FilterFlag is true, only items matching CategoryID will be returned.
        /// // CategoryID is ignored when FilterFlag is false (this is why the previous
        /// // examples didn't filter everything out even if their Category wasn't 0).
        /// var groupDictionaries = budget.GetBudgetDictionaryByCategoryAndMonth(null, null, true, 9);
        /// 
        /// // print important information from dictionaries other than the category totals
        /// // dictionary (which is the last in the list)
        /// foreach (var gd in groupDictionaries.SkipLast(1)) {
        /// 	Console.WriteLine($"Month: {gd["Month"]}");
        /// 
        /// 	foreach (var entry in gd
        /// 			.Where(entry => entry.Key.StartsWith("details:"))) {
        /// 
        /// 		// Goes from details to category total
        /// 		// ex. From "details:Income" to "Income":
        /// 		string keyWithoutPrefix = entry.Key.Substring("details:".Length);
        /// 
        /// 		Console.WriteLine($"Category Amount Total: {gd[keyWithoutPrefix]}");
        /// 		foreach (var budgetItem in (List<BudgetItem>)entry.Value) {
        /// 			Console.WriteLine(
        /// 				String.Format("{0} {1,-25} {2,8:C} {3,12:C}",
        /// 					budgetItem.Date.ToString("yyyy/MMM/dd"),
        /// 					budgetItem.ShortDescription,
        /// 					budgetItem.Amount,
        /// 					budgetItem.Balance));
        /// 		}
        /// 
        /// 		Console.WriteLine();
        /// 	}
        /// }
        /// 
        /// // print information in the category totals dictionary
        /// var categoryTotals = groupDictionaries[groupDictionaries.Count - 1];
        /// Console.WriteLine("Category totals");
        /// Console.WriteLine(categoryTotals["Month"]);
        /// foreach (var entry in categoryTotals
        /// 		.Where(entry => entry.Key != "Month")) {
        /// 
        /// 	Console.WriteLine($"{entry.Key}: {entry.Value}");
        /// }
        /// ]]></code>
        ///
        /// Sample output:
        /// <code>
        /// Month: 2018/01
        /// Category Amount Total: 10
        /// 2018-Jan.-11 Credit Card hat             $10.00       $10.00
        /// 
        /// Month: 2020/01
        /// Category Amount Total: 15
        /// 2020-Jan.-10 Credit Card scarf           $15.00       $25.00
        /// 
        /// Month: 2020/02
        /// Category Amount Total: 40
        /// 2020-Feb.-10 Credit Card mittens         $15.00       $40.00
        /// 2020-Feb.-25 Credit Card Hat             $25.00       $65.00
        /// 
        /// Category totals
        /// TOTALS
        /// Credit Card: 65
        /// </code>
        /// </example>
        // TODO Breaking change: Remove FilterFlag and CategoryID?
        public List<Dictionary<string,object>> GetBudgetDictionaryByCategoryAndMonth(DateTime? Start, DateTime? End, bool FilterFlag, int CategoryID)
        {
            // -----------------------------------------------------------------------
            // get all items by month 
            // -----------------------------------------------------------------------
            List<BudgetItemsByMonth> GroupedByMonth = GetBudgetItemsByMonth(Start, End, FilterFlag, CategoryID);

            // -----------------------------------------------------------------------
            // loop over each month
            // -----------------------------------------------------------------------
            var summary = new List<Dictionary<string, object>>();
            var totalsPerCategory = new Dictionary<String, Double>();

            foreach (var MonthGroup in GroupedByMonth)
            {
                // create record object for this month
                Dictionary<string, object> record = new Dictionary<string, object>();
                record["Month"] = MonthGroup.Month;
                record["Total"] = MonthGroup.Total;

                // break up the month details into categories
                var GroupedByCategory = MonthGroup.Details.GroupBy(c => c.Category);

                // -----------------------------------------------------------------------
                // loop over each category
                // -----------------------------------------------------------------------
                foreach (var CategoryGroup in GroupedByCategory.OrderBy(g => g.Key))
                {

                    // calculate totals for the cat/month, and create list of details
                    double total = 0;
                    var details = new List<BudgetItem>();

                    foreach (var item in CategoryGroup)
                    {
                        total = total + item.Amount;
                        details.Add(item);
                    }

                    // add new properties and values to our record object
                    record["details:" + CategoryGroup.Key] =  details;
                    record[CategoryGroup.Key] = total;

                    // keep track of totals for each category
                    if (totalsPerCategory.TryGetValue(CategoryGroup.Key, out Double CurrentCatTotal))
                    {
                        totalsPerCategory[CategoryGroup.Key] = CurrentCatTotal + total;
                    }
                    else
                    {
                        totalsPerCategory[CategoryGroup.Key] = total;
                    }
                }

                // add record to collection
                summary.Add(record);
            }
            // ---------------------------------------------------------------------------
            // add final record which is the totals for each category
            // ---------------------------------------------------------------------------
            Dictionary<string, object> totalsRecord = new Dictionary<string, object>();
            totalsRecord["Month"] = "TOTALS";

            foreach (var cat in categories.List())
            {
                try
                {
                    totalsRecord.Add(cat.Description, totalsPerCategory[cat.Description]);
                }
                catch { }
            }
            summary.Add(totalsRecord);


            return summary;
        }




        #endregion GetList

    }
}
