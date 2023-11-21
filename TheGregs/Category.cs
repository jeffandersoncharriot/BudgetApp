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
    // CLASS: Category
    //        - An individual category for budget program
    //        - Valid category types: Income, Expense, Credit, Saving
    // ====================================================================
    /// <summary>
    /// A category for budget items (such as <see cref="Expense"/> objects).
    /// </summary>
    public record Category
    {
        // ====================================================================
        // Properties
        // ====================================================================
        /// <summary>This category's unique identifier.</summary>
        /// <remarks>
        /// Unique relative to the database this data comes from.
        /// </remarks>
        public int Id { get; }

        /// <summary>The name or short description of this category.</summary>
        public String Description { get; }

        /// <summary>The type of this category.</summary>
        public CategoryType Type { get; }

        /// <summary>
        /// The <see cref="Category.Type"/> of a <see cref="Category"/>.
        /// </summary>
        public enum CategoryType
        {
            /// <summary>Related to income.</summary>
            Income = 1,
            /// <summary>Related to expenses.</summary>
            Expense,
            /// <summary>Related to credit, such as credit cards.</summary>
            Credit,
            /// <summary>Related to savings.</summary>
            Savings
        };

        /// <summary>
        /// Returns true if a category type is for positive expenses (profit)
        /// or negative expenses (losses).
        /// </summary>
        public static bool TypeIsPositive(CategoryType type) {
            return type switch {
                CategoryType.Income => true,
                CategoryType.Expense => false,
                CategoryType.Credit => true,
                CategoryType.Savings => false,
            };
        }

        // ====================================================================
        // Constructor
        // ====================================================================
        /// <summary>Creates a new instance by providing its members.</summary>
        /// <param name="id">The <see cref="Id"/>.</param>
        /// <param name="description">The <see cref="Description"/>.</param>
        /// <param name="type">
        /// The <see cref="Type"/>. Defaults to
        /// <see cref="CategoryType.Expense"/>.
        /// </param>
        /// <example>
        /// The following example shows the creation of a new instance.
        /// <code><![CDATA[
        /// Category category = new(18, "Bananas", Category.CategoryType.Expense);
        /// ]]></code>
        /// </example>
        internal Category(int id, String description, CategoryType type = CategoryType.Expense)
        {
            this.Id = id;
            this.Description = description;
            this.Type = type;
        }

        // ====================================================================
        // Copy Constructor
        // ====================================================================
        /// <summary>
        /// Creates a deep copy of <paramref name="category"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Id"/> is also copied, which may have greater
        /// implications when it is expected to be unique within a collection.
        /// </remarks>
        /// <param name="category">The instance to copy.</param>
        /// <example>
        /// The following example shows how to copy an instance into a new
        /// instance.
        /// <code><![CDATA[
        /// Category category = new(18, "Bananas", Category.CategoryType.Expense);
        /// Category copy = new(category);
        /// Console.WriteLine($"Id: {copy.Id}, Description: {copy.Description}, Type: {copy.Type}");
        ///
        /// // Expected output:
        /// // Id: 18, Description: Bananas, Type: Expense
        /// ]]></code>
        /// </example>
        public Category(Category category)
        {
            this.Id = category.Id;;
            this.Description = category.Description;
            this.Type = category.Type;
        }
        // ====================================================================
        // String version of object
        // ====================================================================
        /// <summary>
        /// Returns this instance's <see cref="Description"/>.
        /// </summary>
        /// <returns>This instance's <see cref="Description"/>.</returns>
        /// <example>
        /// Usage example:
        /// <code><![CDATA[
        /// Category category = new(18, "Bananas", Category.CategoryType.Expense);
        /// Console.WriteLine(category);
        ///
        /// // Expected output:
        /// // Bananas
        /// ]]></code>
        /// </example>
        public override string ToString()
        {
            return Description;
        }

    }
}

