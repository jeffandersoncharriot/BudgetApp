using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Data.Common;
using System.Data;
using System.Data.SQLite;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: expenses
    //        - A collection of expense items,
    //        - Read / write to file
    //        - etc
    // ====================================================================
    /// <summary>A collection of <see cref="Expense"/> objects.</summary>
    public class Expenses {
        private DbConnection connection;
        


        // ====================================================================
        // Constructor
        // ====================================================================
        /// <summary>
        /// Creates an instance which uses data from a database.
        /// </summary>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="newDb">If true, empty any existing data in the
        /// database. Otherwise, use the data in the database as-is.</param>
        /// <exception cref="DbException">
        /// Thrown when there is a database-related error.
        /// </exception>
        /// <example>
        /// New (and thus empty) database usage example:
        /// <code><![CDATA[
        /// Database.newDatabase(Console.ReadLine());
        /// Expenses expenses = new(Database.dbConnection, newDb: false);
        /// Console.WriteLine(expenses.List().Count);
        ///
        /// // Expected output:
        /// // 0
        /// ]]></code>
        /// Emptying an existing database:
        /// <code><![CDATA[
        /// Database.existingDatabase(Console.ReadLine());
        /// Expenses expenses = new(Database.dbConnection, newDb: true);
        /// Console.WriteLine(expenses.List().Count);
        ///
        /// // Expected output:
        /// // 0
        /// ]]></code>
        /// </example>
        internal Expenses(DbConnection connection, bool newDb) {
            this.connection = connection;

            if (newDb) {
                using DbCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM expenses";
                _ = command.ExecuteNonQuery();
            }
        }



        // ====================================================================
        // Add expense
        // ====================================================================

        /// <summary>
        /// Creates and adds a new <see cref="Expense"/> to the database.
        /// </summary>
        /// <remarks>
        /// The new <see cref="Expense"/>'s <see cref="Expense.Id"/> is
        /// guaranteed to be unique relative to all currently contained
        /// elements.
        /// </remarks>
        /// <param name="date">
        /// <see cref="Expense.Date"/> of element to create.
        /// </param>
        /// <param name="category">
        /// <see cref="Expense.Category"/> of element to create.
        /// </param>
        /// <param name="amount">
        /// <see cref="Expense.Amount"/> of element to create.
        /// </param>
        /// <param name="description">
        /// <see cref="Expense.Description"/> of element to create.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="category"/> does not refer to an
        /// existing category or when <paramref name="amount"/>'s sign
        /// conflicts with the category's sign (positive/negative expense in
        /// negative/positive category).
        /// </exception>
        /// <exception cref="DbException">
        /// Thrown when something goes wrong while trying to modify the
        /// database.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <example>
        /// The following example adds a new <see cref="Expense"/>.
        /// <code><![CDATA[
        /// 
        /// HomeBudget hb = new("database.db", newDB: true);
        /// hb.expenses.Add(new DateTime(2011, 4, 1), 5, 451, "Mustard");
        /// Console.WriteLine(hb.expenses.List().Last().Description);
        ///
        /// // Expected output:
        /// // Mustard
        /// ]]></code>
        /// </example>
        public void Add(DateTime date, int category, Double amount, String description)
        {
            // Ensure category exists and amount matches sign
            EnsureAmountMatchesCategorySign(amount, category);

            // Add expense
            using DbCommand command = connection.CreateCommand(
                "INSERT INTO expenses (Date,Amount,Description,CategoryID) VALUES (@date,@amount,@desc,@categoryID)");

            command.SetParam("date", date.ToString("yyyy-MM-dd"));
            command.SetParam("categoryID", category);
            command.SetParam("amount", amount);
            command.SetParam("desc", description);

            _ = command.ExecuteNonQuery();

        }

        /// <summary>
        /// Updates the data for an <see cref="Expense"/>.
        /// </summary>
        /// <remarks>
        /// Does not throw when <paramref name="id"/> does not refer to a
        /// valid entry.
        /// </remarks>
        /// <param name="id"><see cref="Expense.Id"/> of the element to update.</param>
        /// <param name="newDate">The new <see cref="Expense.Date"/>.</param>
        /// <param name="newCatId">The new <see cref="Expense.Category"/>.</param>
        /// <param name="newAmount">The new <see cref="Expense.Amount"/>.</param>
        /// <param name="newDesc">The new <see cref="Expense.Description"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="newCatId"/> does not refer to an
        /// existing category or when <paramref name="newAmount"/>'s sign
        /// conflicts with the category's sign (positive/negative expense in
        /// negative/positive category).
        /// </exception>
        /// <exception cref="DbException">
        /// Thrown when something goes wrong while trying to modify the
        /// database.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <example>
        /// The following updates the properties of the last
        /// <see cref="Expense"/>:
        /// <code><![CDATA[
        /// HomeBudget hb = new("database.db", newDB: true);
        ///
        /// hb.expenses.Add(
        /// 	new DateTime(1994, 12, 23),
        /// 	2, // Rent
        /// 	45.10,
        /// 	"Fizzwaffle");
        ///
        /// Console.WriteLine(hb.categories.List().Last().Description);
        ///
        /// hb.expenses.UpdateProperties(
        /// 	hb.categories.List().Last().Id,
        /// 	new DateTime(1994, 9, 23),
        /// 	3, // Food
        /// 	4.51,
        /// 	"Falafel");
        ///
        /// Console.WriteLine(hb.categories.List().Last().Description);
        ///
        /// // Expected output:
        /// // Fizzwaffle
        /// // Falafel
        /// ]]></code>
        /// </example>
        public void UpdateProperties(
                int id,
                DateTime newDate,
                int newCatId,
                double newAmount,
                string newDesc) {

            // Ensure category exists and amount matches sign
            EnsureAmountMatchesCategorySign(newAmount, newCatId);

            // Update category
            using DbCommand command = connection.CreateCommand(
                "UPDATE expenses"
                + " SET Date = @date, Amount = @amount,"
                + " Description = @desc, CategoryId = @categoryId"
                + " WHERE Id = @id");

            command.SetParam("date", newDate.ToString("yyyy-MM-dd"));
            command.SetParam("amount", newAmount);
            command.SetParam("desc", newDesc);
            command.SetParam("categoryId", newCatId);
            command.SetParam("id", id);

            _ = command.ExecuteNonQuery();
        }

        // ====================================================================
        // Delete expense
        // ====================================================================
        /// <summary>
        /// Removes an element based on its <see cref="Expense.Id"/>.
        /// </summary>
        /// <remarks>
        /// Does not throw when <paramref name="id"/> does not refer to a
        /// valid entry.
        /// </remarks>
        /// <param name="Id">
        /// <see cref="Expense.Id"/> of element to remove.
        /// </param>
        /// <exception cref="DbException">
        /// Thrown when something goes wrong while trying to modify the
        /// database.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <example>
        /// The following example adds two new <see cref="Expense"/> objects,
        /// then removes the last one based on its <see cref="Expense.Id"/>.
        /// <code><![CDATA[
        /// 
        /// HomeBudget hb = new("database.db", newDB: true);
        /// 
        /// hb.expenses.Add(new DateTime(2011, 4, 1), 5, 451, "Ketchup");
        /// hb.expenses.Add(new DateTime(2011, 4, 1), 5, 451, "Mustard");
        /// 
        /// Console.WriteLine(hb.expenses.List().Last().Description);
        /// hb.expenses.Delete(hb.expenses.List().Last().Id);
        /// Console.WriteLine(hb.expenses.List().Last().Description);
        ///
        /// // Expected output:
        /// // Mustard
        /// // Ketchup
        /// ]]></code>
        /// </example>
        public void Delete(int Id)
        {

            using DbCommand command = connection.CreateCommand("DELETE FROM expenses WHERE Id = @id");

            command.SetParam("id", Id);

            _ = command.ExecuteNonQuery();

        }

        // ====================================================================
        // Return list of expenses
        // ====================================================================
        /// <summary>
        /// Creates a new list representing the <see cref="Expense"/> data in
        /// the database.
        /// </summary>
        /// <remarks>Elements are in the order they were added.</remarks>
        /// <returns>A new list of this collection's elements.</returns>
        /// <exception cref="DbException">
        /// Thrown when something goes wrong while trying to read from the
        /// database.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <example>
        /// The following example adds two new <see cref="Expense"/> objects,
        /// then prints them by iterating through a list.
        /// <code><![CDATA[
        /// HomeBudget hb = new("database.db", newDB: true);
        /// 
        /// hb.expenses.Add(new DateTime(2011, 4, 1), 5, 451, "Ketchup");
        /// hb.expenses.Add(new DateTime(2011, 4, 1), 5, 451, "Mustard");
        /// 
        /// List<Expense> expensesList = hb.expenses.List();
        /// 
        /// foreach (Expense expense in expensesList) {
        /// 	Console.WriteLine(expense.Description);
        /// }
        /// 
        /// // Expected output:
        /// // Ketchup
        /// // Mustard
        /// ]]></code>
        /// </example>
        public List<Expense> List()
        {
            int idIndex = 0;
            int dateIndex = 1;
            int amountIndex = 2;
            int descIndex = 3;
            int categoryIdIndex = 4;



            using DbCommand command = connection.CreateCommand(
                "SELECT Id, Date, Amount, Description, CategoryID"
                + " FROM expenses"
                + " ORDER BY Id");

            using DbDataReader reader = command.ExecuteReader();
            List<Expense> expenses = new List<Expense>();

            while (reader.Read())
            {
                expenses.Add(new Expense(
                    reader.GetInt32(idIndex),
                    reader.GetDateTime(dateIndex),
                    reader.GetInt32(categoryIdIndex),
                    reader.GetDouble(amountIndex),
                    reader.GetString(descIndex)));
            }

            return expenses;
        }

        /// <summary>
        /// Ensures that a category exists and that an amount matches that
        /// category's sign (positive amount for positive category, negative
        /// for negative).
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when category doesn't exist or when sign doesn't match.
        /// </exception>
        private void EnsureAmountMatchesCategorySign(double amount, int categoryId) {
            // Check if category is valid and get its type
            using DbCommand categoryTypeQuery = connection.CreateCommand(
                "SELECT TypeID FROM categories WHERE Id = @id");
            categoryTypeQuery.SetParam("id", categoryId);
            Category.CategoryType? categoryType = (Category.CategoryType?)(long?)categoryTypeQuery.ExecuteScalar();

            if (!categoryType.HasValue) throw new ArgumentException("Category does not exist.");

            // Check if expense amount has valid sign
            bool categoryIsPositive = Category.TypeIsPositive(categoryType.Value);
            if (categoryIsPositive && amount < 0) throw new ArgumentException("Negative expense amounts cannot go into positive categories.");
            if (!categoryIsPositive && amount > 0) throw new ArgumentException("Positive expense amounts cannot go into negative categories.");
        }
    }
}

