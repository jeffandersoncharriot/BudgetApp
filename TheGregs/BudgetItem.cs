using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ============================================================================
// (c) Sandy Bultena 2018
// * Released under the GNU General Public License
// ============================================================================

// TODO These should all be records
namespace Budget
{
    // ====================================================================
    // CLASS: BudgetItem
    //        A single budget item, includes Category and Expense
    // ====================================================================

    /// <summary>
    /// A single budget item, which includes data related to an
    /// <see cref="Budget.Expense"/> and its associated
    /// <see cref="Budget.Category"/>.
    /// </summary>
    /// <remarks>
    /// This object's data is set when it is created; It is not updated when
    /// either the source <see cref="Budget.Expense"/> or
    /// <see cref="Budget.Category"/> are modified.
    /// </remarks>
    public record BudgetItem
    {
        /// <summary>
        /// The source <see cref="Budget.Category.Id">Category.Id</see>.
        /// </summary>
        public int CategoryID { get; init; }

        /// <summary>
        /// The source <see cref="Budget.Expense.Id">Expense.Id</see>.
        /// </summary>
        public int ExpenseID { get; init; }

        /// <summary>
        /// The associated <see cref="Budget.Expense.Date">Expense.Date</see>.
        /// </summary>
        /// <remarks>
        /// Not updated when the source object's data is modified.
        /// </remarks>
        public DateTime Date { get; init; }

        /// <summary>
        /// The associated <see cref="Budget.Category.Description">
        /// Category.Description</see>.
        /// </summary>
        /// <remarks>
        /// Not updated when the source object's data is modified.
        /// </remarks>
        public String Category { get; init; }

        /// <summary>
        /// The associated <see
        /// cref="Budget.Expense.Description">Expense.Description</see>.
        /// </summary>
        /// <remarks>
        /// Not updated when the source object's data is modified.
        /// </remarks>
        public String ShortDescription { get; init;}

        /// <summary>
        /// This item's amount of money, equal to the value of the source
        /// <see cref="Budget.Expense.Amount">Expense.Amount</see>.
        /// </summary>
        /// <remarks>
        /// Not updated when the source object's data is modified.
        /// </remarks>
        public Double Amount { get; init; }

        /// <summary>
        /// Chronological running total of <see cref="Amount"/> for all items
        /// in the query results containing this object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that this is a <b>chronologically ascending</b> running total;
        /// This value is not ordered by index in the query results, but by
        /// <see cref="Date"/>.
        /// </para>
        /// <para>
        /// Not updated when the source object's data or the containing
        /// collection are modified.
        /// </para>
        /// </remarks>
        // Eww!
        public Double Balance { get; init; }
    }

    /// <summary>
    /// Stores a list of <see cref="BudgetItem"/> objects from a certain month
    /// of a certain year.
    /// </summary>
    public class BudgetItemsByMonth
    {
        /// <summary>
        /// The year and month of all items in <see cref="Details"/>, in this
        /// format: <c>yyyy/MM</c>
        /// </summary>
        public String Month { get; init; }

        /// <summary>
        /// The list of <see cref="BudgetItem"/> objects which all share the
        /// year and month stored in <see cref="Month"/>.
        /// </summary>
        // TODO Make into ImmutableList
        public List<BudgetItem> Details { get; init; }

        /// <summary>
        /// The sum of the
        /// <see cref="BudgetItem.Amount">BudgetItem.Amount</see> of all items
        /// in <see cref="Details"/>.
        /// </summary>
        public Double Total { get; init; }

        public override bool Equals(object? obj) {
            return obj is BudgetItemsByMonth o
                && Month == o.Month
                && Details.SequenceEqual(o.Details)
                && Total == o.Total;
        }
    }


    /// <summary>
    /// Stores a list of <see cref="BudgetItem"/> objects from a certain
    /// <see cref="BudgetItem.Category"/>.
    /// </summary>
    /// <remarks>
    /// Note that this class groups by <see
    /// cref="Category.Description">Category.Description</see>, not by <see
    /// cref="Category.Id">Category.Id</see>
    /// </remarks>
    public class BudgetItemsByCategory
    {
        /// <summary>
        /// The <see cref="BudgetItem.Category"/> of all items in
        /// <see cref="Details"/>.
        /// </summary>
        public String Category { get; init; }

        /// <summary>
        /// The list of <see cref="BudgetItem"/> objects which all share the
        /// <see cref="BudgetItem.Category"/> as stored in
        /// <see cref="Category"/>.
        /// </summary>
        // TODO Make into ImmutableList
        public List<BudgetItem> Details { get; init; }

        /// <summary>
        /// The sum of the
        /// <see cref="BudgetItem.Amount">BudgetItem.Amount</see> of all items
        /// in <see cref="Details"/>.
        /// </summary>
        public Double Total { get; init; }

        public override bool Equals(object? obj) {
            return obj is BudgetItemsByCategory o
                && Category == o.Category
                && Details.SequenceEqual(o.Details)
                && Total == o.Total;
        }
    }


}
