using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Common;
using static Budget.Category;
using System.Data.SQLite;
using System.Data;
using System.Text.RegularExpressions;
using TheGregs;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: categories
    //        - Accesses category items,
    //        - Read / write to database
    //        - etc
    // ====================================================================
    /// <summary>Accesses a database's <see cref="Category"/> data.</summary>
    public class Categories
    {
        
        private DbConnection connection;

        // ====================================================================
        // Constructor
        // ====================================================================
        /// <summary>
        /// Creates an instance, either by reading data from a database or
        /// overwriting it and filling it with default data.
        /// </summary>
        /// <param name="connection">The database connection to use.</param>
        /// <param name="newDb">If true, overwrite the data in the database and
        /// replace it with default data according to
        /// <see cref="SetCategoriesToDefaults"/>. Otherwise, use the data in
        /// the database as-is.</param>
        /// <exception cref="DbException">
        /// Thrown when something goes wrong while trying to modify the
        /// database when <paramref name="newDb"/> is true.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <example>
        /// Usage example showcasing a default <see cref="Category"/>:
        /// <code><![CDATA[
        /// DbConnection connection = Database.dbConnection;
        /// Categories categories = new(connection, true);
        /// Console.WriteLine(categories.List()[0].Description);
        /// // Expected output:
        /// // Utilities
        /// ]]></code>
        /// </example>
        internal Categories(DbConnection connection, bool newDb)
        {
            this.connection = connection;


            if (newDb) {

                SetCategoriesToDefaults();
            }
        }

        // ====================================================================
        // get a specific category from the list where the id is the one specified
        // ====================================================================
        /// <summary>
        /// Returns a <see cref="Category"/> whose <see cref="Category.Id"/>
        /// matches <paramref name="i"/>.
        /// </summary>
        /// <param name="i">
        /// <see cref="Category.Id"/> of a <see cref="Category"/> to return.
        /// </param>
        /// <returns>A matching <see cref="Category"/>.</returns>
        /// <exception cref="DbException">
        /// Thrown when something goes wrong while trying to read the
        /// database.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when there is no matching element.
        /// </exception>
        /// <example>
        /// Example of getting a <see cref="Category"/> by its
        /// <see cref="Category.Id"/>:
        /// <code><![CDATA[
        /// Categories categories = new();
        /// Console.WriteLine(categories.GetCategoryFromId(1));
        /// // Expected output:
        /// // Utilities
        /// ]]></code>
        /// </example>
        /// 

        public Category GetCategoryFromId(int i)
        {

            using DbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Description, TypeId FROM categories WHERE Id = @id";
            
            DbParameter idParam = new SQLiteParameter("id", DbType.Int32);
            idParam.Value = i;
            _ = command.Parameters.Add(idParam);

            using DbDataReader rdr = command.ExecuteReader();
            if (!rdr.Read()) throw new Exception("Cannot find category with id " + i.ToString());
		    
            return new Category(rdr.GetInt32(0), rdr.GetString(1), (CategoryType)rdr.GetInt32(2));
        }


        // ====================================================================
        // set categories to default
        // ====================================================================
        /// <summary>
        /// Clears this collection and fills it with default data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that calling this in a database which contains any
        /// <see cref="Expense"/> objects will likely throw an exception due to
        /// violated foreign key constraints.
        /// </para>
        /// <para>
        /// Existing <see cref="Category.Id"/> values may be reused; Any such
        /// external values may afterward refer to the wrong elements inside
        /// this collection.
        /// </para>
        /// <para>
        /// Fills this collection with new <see cref="Category"/> objects set
        /// to the following:
        /// <list type="table">
        /// <listheader>
        ///     <term><see cref="Category.Id"/></term>
        ///     <term><see cref="Category.Description"/></term>
        ///     <term><see cref="Category.Type"/></term>
        /// </listheader>
        /// <item>
        ///     <term>1</term>
        ///     <term>"Utilities"</term>
        ///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>2</term>
		///     <term>"Rent"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
		/// </item>
        /// <item>
        ///     <term>3</term>
		///     <term>"Food"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>4</term>
		///     <term>"Entertainment"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>5</term>
		///     <term>"Education"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>6</term>
		///     <term>"Miscellaneous"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>7</term>
		///     <term>"Medical Expenses"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>8</term>
		///     <term>"Vacation"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>9</term>
		///     <term>"Credit Card"</term>
		///     <term><see cref="Category.CategoryType.Credit"/></term>
        /// </item>
        /// <item>
        ///     <term>10</term>
		///     <term>"Clothes"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>11</term>
		///     <term>"Gifts"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>12</term>
		///     <term>"Insurance"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>13</term>
		///     <term>"Transportation"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>14</term>
		///     <term>"Eating Out"</term>
		///     <term><see cref="Category.CategoryType.Expense"/></term>
        /// </item>
        /// <item>
        ///     <term>15</term>
		///     <term>"Savings"</term>
		///     <term><see cref="Category.CategoryType.Savings"/></term>
        /// </item>
        /// <item>
        ///     <term>16</term>
		///     <term>"Income"</term>
		///     <term><see cref="Category.CategoryType.Income"/></term>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="DbException">
        /// Thrown when something goes wrong while trying to modify the
        /// database.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <example>
        /// The following example erases custom data by resetting an instance
        /// to the defaults:
        /// <code><![CDATA[
        /// Categories categories = new();
        ///
        /// categories.Add("Hi", Category.CategoryType.Expense);
        /// Console.WriteLine(categories.List().Last().Description);
        ///
        /// categories.SetCategoriesToDefaults();
        /// Console.WriteLine(categories.List().Last().Description);
        ///
        /// // Expected output:
        /// // Hi
        /// // Income
        /// ]]></code>
        /// </example>
        public void SetCategoriesToDefaults()
        {
            // ---------------------------------------------------------------
            // reset any current categories,
            // ---------------------------------------------------------------
            using (DbCommand command = connection.CreateCommand()) {
                command.CommandText = "DELETE FROM categories";
                _ = command.ExecuteNonQuery();
            }

            // ---------------------------------------------------------------
            // Add Defaults
            // ---------------------------------------------------------------
            Add("Utilities", Category.CategoryType.Expense);
            Add("Rent", Category.CategoryType.Expense);
            Add("Food", Category.CategoryType.Expense);
            Add("Entertainment", Category.CategoryType.Expense);
            Add("Education", Category.CategoryType.Expense);
            Add("Miscellaneous", Category.CategoryType.Expense);
            Add("Medical Expenses", Category.CategoryType.Expense);
            Add("Vacation", Category.CategoryType.Expense);
            Add("Credit Card", Category.CategoryType.Credit);
            Add("Clothes", Category.CategoryType.Expense);
            Add("Gifts", Category.CategoryType.Expense);
            Add("Insurance", Category.CategoryType.Expense);
            Add("Transportation", Category.CategoryType.Expense);
            Add("Eating Out", Category.CategoryType.Expense);
            Add("Savings", Category.CategoryType.Savings);
            Add("Income", Category.CategoryType.Income);

        }

        // ====================================================================
        // Add category
        // ====================================================================
        /// <summary>
        /// Creates and adds a new <see cref="Category"/> to the database.
        /// </summary>
        /// <param name="desc">
        /// <see cref="Category.Description"/> of element to create.
        /// </param>
        /// <param name="type">
        /// <see cref="Category.Type"/> of element to create.
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
        /// The following example creates and adds a new
        /// <see cref="Category"/>.
        /// <code><![CDATA[
        /// HomeBudget hb = new("database.db", newDB: true);
        /// hb.categories.Add("Hi", Category.CategoryType.Expense);
        /// Console.WriteLine(hb.categories.List().Last().Description);
        ///
        /// // Expected output:
        /// // Hi
        /// ]]></code>
        /// </example>
        public void Add(String desc, Category.CategoryType type)
        {
            desc = AsUniqueDescription(desc);

            // Create command
            using DbCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO categories (Description, TypeID) VALUES (@desc, @type)";

            // Add parameters
             
            DbParameter descParam = new SQLiteParameter("desc",DbType.String);
            DbParameter typeParam = new SQLiteParameter("type",DbType.Int32);
            
            descParam.Value = desc;
            typeParam.Value = (int)type;

            _ = command.Parameters.Add(descParam);
            _ = command.Parameters.Add(typeParam);
            // Execute	
            _ = command.ExecuteNonQuery();

        }

        /// <summary>
        /// Updates <see cref="Category"/>'s information to db
        /// </summary>
        /// <remarks>
        /// Does not throw when <paramref name="id"/> does not refer to a
        /// valid entry.
        /// </remarks>
        /// <param name="id"><see cref="Category.Id"/> of the element to update.</param>
        /// <param name="newDesc">The new <see cref="Category.Description"/>.</param>
        /// <param name="newType">The new <see cref="Category.Type"/>.</param>
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
        /// <see cref="Category"/>:
        /// <code><![CDATA[
        /// HomeBudget hb = new("database.db", newDB: true);
        /// 
        /// hb.categories.Add("Bank",Category.CategoryType.Savings);
        /// 
        /// hb.categories.UpdateProperties(hb.categories.List().Last().Id,"Restaurant", Category.CategoryType.Expense);
        /// 
        /// Console.WriteLine(hb.categories.List().Last().Description);
        ///
        /// // Expected output:
        /// // Restaurant
        /// ]]></code>
        /// </example>
        public void UpdateProperties(int id,string newDesc, Category.CategoryType newType)
        {
            newDesc = AsUniqueDescription(newDesc, ignoredId: id);

            using DbCommand command = connection.CreateCommand();

            command.CommandText = "UPDATE categories " +
                "SET Description = @desc, TypeId = @type WHERE Id = @id";

           
            DbParameter descParam = new SQLiteParameter("desc", DbType.String);
            DbParameter typeParam = new SQLiteParameter("type", DbType.Int32);
            DbParameter idParam = new SQLiteParameter("id", DbType.Int32);

            idParam.Value = id;
            descParam.Value = newDesc;
            typeParam.Value = (int)newType;
            
            _ = command.Parameters.Add(idParam);
            _ = command.Parameters.Add(descParam);
            _ = command.Parameters.Add(typeParam);

            _ = command.ExecuteNonQuery();

        }


        // ====================================================================
        // Delete category
        // ====================================================================
        /// <summary>
        /// Removes an element based on its <see cref="Category.Id"/>.
        /// </summary>
        /// <remarks>
        /// Does not throw when <paramref name="id"/> does not refer to a
        /// valid entry.
        /// </remarks>
        /// <param name="Id">
        /// <see cref="Category.Id"/> of element to remove.
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
        /// The following example adds a new <see cref="Category"/>,
        /// then removes it based on its <see cref="Category.Id"/>.
        /// <code><![CDATA[
        /// 
        ///
        /// HomeBudget hb = new("database.db", newDB: true);
        /// 
        /// hb.categories.Add("Restaurant", Category.CategoryType.Expense);
        /// hb.categories.Add("Shop", Category.CategoryType.Expense);
        ///
        /// hb.categories.Delete(hb.categories.List().Last().Id);
        /// 
        /// Console.WriteLine(hb.categories.List().Last().Description);
        ///
        /// // Expected output:
        /// // Restaurant
        /// ]]></code>
        /// </example>
        public void Delete(int Id)
        {
            using DbCommand command = connection.CreateCommand();
            command.CommandText = "DELETE FROM categories WHERE Id = @id";

            DbParameter idParam = new SQLiteParameter("id", DbType.Int32);
            idParam.Value = Id;
            _ = command.Parameters.Add(idParam);

            _ = command.ExecuteNonQuery();

        }

        // ====================================================================
        // Return list of categories
        // Note:  make new copy of list, so user cannot modify what is part of
        //        this instance
        // ====================================================================
        /// <summary>
        /// Creates a new list of the <see cref="Category"/> elements in this
        /// collection.
        /// </summary>
        /// <remarks>Elements are in the order they were added.</remarks>
        /// <returns>A new list of this collection's elements.</returns>
        /// <exception cref="DbException">
        /// Thrown when something goes wrong while trying to read the database.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when trying to do an operation after this instance's
        /// database connection is closed.
        /// </exception>
        /// <example>
        /// The following lists the default <see cref="Category"/> objects'
        /// <see cref="Category.Description"/> values.
        /// <code><![CDATA[
        /// 
        /// HomeBudget hb = new("database.db", newDB: true);
        /// 
        /// 
        ///
        /// hb.categories.SetCategoriesToDefaults()
        /// 
        /// foreach (var category in hb.categories.List()) {
        /// 	Console.WriteLine(category.Description);
        /// }
        ///
        /// // Expected output:
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
        public List<Category> List()
        {
		
            using DbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Description, TypeId FROM categories ORDER BY Id";


            using DbDataReader rdr = command.ExecuteReader();
            List<Category> cats = new();
            while (rdr.Read())
            {
                cats.Add(new Category(rdr.GetInt32(0), rdr.GetString(1), (CategoryType)rdr.GetInt32(2)));
            }

            return cats;

        }

        /// <summary>
        /// Normalizes a <see cref="Category.Description"/> string and ensures
        /// that it is unique.
        /// </summary>
        /// <remarks>
        /// This makes similar strings identical so that there are no
        /// accidental duplicates, ex. <c>"Food"</c>, <c>" Food"</c>, and
        /// <c>"Food "</c>, or <c>"Big Shovels"</c> and <c>"Big  Shovels"</c>.
        /// </remarks>
        /// <param name="desc">
        /// The original description, which should no longer be used afterward.
        /// </param>
        /// <param name="ignoredId">
        /// If specified, a <see cref="Category.Id"/> to ignore when checking
        /// for uniqueness. Useful when updating a category, as otherwise the
        /// category would conflict with itself.
        /// </param>
        /// <returns>
        /// The normalized string; The original string should no longer be
        /// used.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the description is not unique.
        /// </exception>
        private string AsUniqueDescription(string desc, int ignoredId=-1) {
            // Remove initial and trailing whitespace:
            desc = desc.Trim();
            // Replace all groups of whitespace with a single space:
            desc = Regex.Replace(desc, @"\s+", " ");
            // Unicode can sometimes hide duplicates; this fixes that:
            desc = desc.Normalize();

            // Ensure description is unique

            var cmd = connection.CreateCommand(
                "SELECT COUNT(*)"
                + " FROM categories"
                + " WHERE Description = @desc AND Id != @ignoredId");

            cmd.SetParam("desc", desc);
            cmd.SetParam("ignoredId", ignoredId);

            long duplicates = (long)cmd.ExecuteScalar()!;

            if (duplicates > 0) {
                throw new ArgumentException($"Description \"{desc}\" is already taken.");
            }

            return desc;
        }

    }
}

