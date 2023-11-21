using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

namespace Budget
{
    // ====================================================================
    // CLASS: Expense
    //        - An individual expens for budget program
    // ====================================================================
    /// <summary>An individual expense (negative amount of money).</summary>
    public record Expense
    {
        // ====================================================================
        // Properties
        // ====================================================================
        /// <summary>This expense's unique identifier.</summary>
        /// <remarks>
        /// Unique relative to the database this data comes from.
        /// </remarks>
        public int Id { get; }

        /// <summary>The time at which this expense occurred.</summary>
        public DateTime Date { get;  }

        /// <summary>The amount of money expended. Represented in negative number.</summary>
        public Double Amount { get; }

        /// <summary>Describes this expense.</summary>
        public String Description { get; }

        /// <summary>
        /// The <see cref="Category.Id"/> of this expense's
        /// <see cref="Category"/>.
        /// </summary>
        public int Category { get; }

        // ====================================================================
        // Constructor
        //    NB: there is no verification the expense category exists in the
        //        categories object
        // ====================================================================
        /// <summary>Creates a new instance by providing its members.</summary>
        /// <remarks>
        /// Neither <paramref name="id"/> nor <paramref name="category"/> are
        /// verified because they are contextual; See <see cref="Id"/> and
        /// <see cref="Category"/>.
        /// </remarks>
        /// <param name="id">The <see cref="Id"/>.</param>
        /// <param name="date">The <see cref="Date"/>.</param>
        /// <param name="category">The <see cref="Category"/>.</param>
        /// <param name="amount">The <see cref="Amount"/>.</param>
        /// <param name="description">The <see cref="Description"/>.</param>
        /// <example>
        /// The following example shows the creation of a new instance.
        /// <code><![CDATA[
        /// // Note: category should refer to an ID in some Category collection (normally a Categories)
        /// Expense expense = new(18, new DateTime(2011, 4, 1), 5, 451, "Hi");
        /// ]]></code>
        /// </example>
        internal Expense(int id, DateTime date, int category, Double amount, String description)
        {
            this.Id = id;
            this.Date = date;
            this.Category = category;
            this.Amount = amount;
            this.Description = description;
        }

        // ====================================================================
        // Copy constructor - does a deep copy
        // ====================================================================
        /// <summary>Creates a deep copy of <paramref name="obj"/>.</summary>
        /// <remarks>
        /// <see cref="Id"/> is also copied, which may have greater
        /// implications when it is expected to be unique within a collection.
        /// </remarks>
        /// <param name="obj">The instance to copy.</param>
        /// <example>
        /// The following example shows how to copy an instance into a new
        /// instance.
        /// <code><![CDATA[
        /// // Note: category should refer to an ID in some Category collection (normally a Categories)
        /// Expense expense = new(18, new DateTime(2011, 4, 1), 5, 451, "Hi");
        /// Expense copy = new(expense);
        ///
        /// Console.WriteLine(
        ///     $"Id: {copy.Id}, Date: {copy.Date}, Category Id: {copy.Category},"
        ///     + $" Amount: {copy.Amount}, Description: {copy.Description}");
        ///
        /// // Expected output (date format may vary):
        /// // Id: 18, Date: 2011-04-01 00:00:00, Category Id: 5, Amount: 451, Description: Hi
        /// ]]></code>
        /// </example>
        public Expense (Expense obj)
        {
            this.Id = obj.Id;
            this.Date = obj.Date;
            this.Category = obj.Category;
            this.Amount = obj.Amount;
            this.Description = obj.Description;
           
        }
    }
}
