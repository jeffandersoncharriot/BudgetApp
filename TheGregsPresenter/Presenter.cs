using System.Text;
using System.Text.Json;
using TheGregsPresenter;

namespace Budget;

/// <summary>The home budget MVP presenter.</summary>
public class Presenter {
    HomeBudget model;
    IView view;

    /// <summary>
    /// Creates a new presenter for a given budget file and linked to a given
    /// view.
    /// </summary>
    /// <param name="filePath">The budget's filepath.</param>
    /// <param name="isNewDb">
    /// True if should overwrite any existing data in the file with default
    /// data. Ignored if file does not already exist.
    /// </param>
    /// <param name="view">The MVP view to link to.</param>
    /// <exception cref="Exception">
    /// Thrown when something goes wrong while opening the model.
    /// </exception>
    public Presenter(string filePath,bool isNewDb, IView view) {
        this.model = new HomeBudget(filePath, isNewDb);
        this.view = view;
    }

    /// <summary>
    /// Adds a category to the budget.
    /// </summary>
    /// <param name="categoryName">Name of the category to add</param>
    /// <param name="type">The type of the category to add</param>
    /// <returns>A success message if successful</returns>
    /// <exception cref="Exception">Thrown when the operation fails</exception>
    /// <example>
    /// <code>
    /// presenter.AddCategory("Random",Category.CategoryType.Income);
    /// </code>
    /// </example>
    public string AddCategory(string categoryName, Category.CategoryType type)
    {
        model.categories.Add(categoryName, type);
        view.RefreshCategories(model.categories.List());
        return $"\"{categoryName}\" added succesfully with type {type}";
    }

    /// <summary>
    /// Adds an expense to the budget
    /// </summary>
    /// <param name="date">date of the expense</param>
    /// <param name="category"> category type of the expense </param>
    /// <param name="amount"> expense amount</param>
    /// <param name="description"> description of the expense</param>
    /// <returns>A success message if successful</returns>
    /// <exception cref="Exception">Thrown when the operation fails</exception>
    /// <example>
    /// <code>
    /// presenter.AddExpense(new DateTime(2015, 12, 25),2,-23,"NewExpense");
    /// </code>
    /// </example>
    public string AddExpense(DateTime date, int category, Double amount, String description)
    { 
        model.expenses.Add(date,category,amount,description);
        view.RefreshBudgetItems();
        return $"Expense \"{description}\" on {date.ToString("d")} added succesfully with amount {amount}";
    }

    /// <summary>
    /// Updates an expense of the budget
    /// </summary>
    /// <param name="id">id of the expense to update</param>
    /// <param name="date">new date</param>
    /// <param name="category">new category type</param>
    /// <param name="amount">new expense amount</param>
    /// <param name="description">new description</param>
    /// <returns>A success message if successful</returns>
    /// <exception cref="Exception">Thrown when the operation fails</exception>
    /// <example>
    /// <code>
    /// presenter.UpdateExpense(1, new DateTime(2015, 12, 25),2,-23,"UpdatedExpense");
    /// </code>
    /// </example>
    public string UpdateExpense(int id, DateTime date, int category, Double amount, String description)
    {
        model.expenses.UpdateProperties(id,date, category, amount, description);
        view.RefreshBudgetItems();
        return $"Expense \"{description}\" on {date.ToString("d")} updated successfully";
    }

    /// <summary>
    /// Removes an expense from the budget.
    /// </summary>
    /// <param name="id"> id of the expense</param>
    /// <returns>A success message if successful</returns>
    /// <exception cref="Exception">Thrown when the operation fails</exception>
    /// <example>
    /// <code>
    ///  presenter.RemoveExpense(2);
    /// </code>
    /// </example>
    public string RemoveExpense(int id)
    {
        if (!model.expenses.List().Any(exp => exp.Id == id)) throw new Exception("Expense not found");

        model.expenses.Delete(id);
        
        view.RefreshBudgetItems();

        return $"Expense successfully deleted";
    }
    /// <summary>
    /// Makes this presenter call <see cref="IView.RefreshCategories"/> to
    /// refresh the view's categories.
    /// </summary>
    public void RequestCategoriesRefresh()
    {
        view.RefreshCategories(model.categories.List());
    }

    /// <summary>
    /// Returns a list of filtered <see cref="BudgetItem"/> objects
    /// representing this budget's expenses.
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
    /// <returns>The filtered items as a new list.</returns>
    /// <example>
    /// <code>
    /// presenter.GetBudgetItems(new DateTime(2015,06,05),new DateTime(2018,07,05),true,2);
    /// </code>
    /// </example>
    public List<BudgetItem> GetBudgetItems(DateTime? Start,DateTime? End,bool FilterFlag, int CategoryID) {
        return model.GetBudgetItems(Start,End,FilterFlag,CategoryID);
    }

    /// <summary>
    /// Returns filtered <see cref="BudgetItem"/> objects representing this
    /// budget's expenses grouped by <see cref="BudgetItem.Category"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// presenter.GetBudgetItemsByCategory(new DateTime(2015,06,05),new DateTime(2018,07,05),true,2);
    /// </code>
    /// </example>
    /// <inheritdoc cref="HomeBudget.GetBudgetItemsByCategory(DateTime?, DateTime?, bool, int)"/>
    public List<BudgetItemsByCategory> GetBudgetItemsByCategory(
            DateTime? Start,
            DateTime? End,
            bool FilterFlag,
            int CategoryID) {

        return model.GetBudgetItemsByCategory(Start, End, FilterFlag, CategoryID);
    }

    /// <summary>
    /// Returns filtered <see cref="BudgetItem"/> objects representing this
    /// budget's expenses grouped by <see cref="BudgetItem.Date"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// presenter.GetBudgetItemsByMonth(new DateTime(2015,06,05),new DateTime(2018,07,05),true,2);
    /// </code>
    /// </example>
    /// <inheritdoc cref="HomeBudget.GetBudgetItemsByMonth(DateTime?, DateTime?, bool, int)"/>
    public List<BudgetItemsByMonth> GetBudgetItemsByMonth(
            DateTime? Start,
            DateTime? End,
            bool FilterFlag,
            int CategoryID)
    {

        return model.GetBudgetItemsByMonth(Start, End, FilterFlag, CategoryID);
    }

    /// <summary>
    /// Returns filtered <see cref="BudgetItem"/> objects representing this
    /// budgets's expenses grouped by <see cref="BudgetItem.Category"/> and
    /// month.
    /// </summary>
    /// <example>
    /// <code>
    /// presenter.GetBudgetDictionaryByCategoryAndMonth(new DateTime(2015,06,05),new DateTime(2018,07,05),true,2);
    /// </code>
    /// </example>
    /// <inheritdoc cref="HomeBudget.GetBudgetDictionaryByCategoryAndMonth(DateTime?, DateTime?, bool, int)"/>
    public List<Dictionary<string, object>> GetBudgetDictionaryByCategoryAndMonth(
            DateTime? Start,
            DateTime? End,
            bool FilterFlag,
            int CategoryID) {

        return model.GetBudgetDictionaryByCategoryAndMonth(Start, End, FilterFlag, CategoryID);
    }

    /// <summary>
    /// Serializes budget items into a portable string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The portable string contains all information necessary to reproduce
    /// <paramref name="items"/> in another database. This includes both
    /// <see cref="Expense"/> and <see cref="Category"/> data.
    /// </para>
    /// <para>
    /// To add serialized items to a budget, see
    /// <see cref="AddSerializedBudgetItems(string)"/>.
    /// </para>
    /// </remarks>
    /// <param name="items">The budget items to serialize.</param>
    /// <returns><paramref name="items"/> serialized into a string.</returns>
    /// <exception cref="Exception">
    /// Thrown when a budget item's <see cref="BudgetItem.CategoryID"/> could
    /// not be found in the budget, or when the model throws an exception.
    /// </exception>
    public string SerializeBudgetItems(IEnumerable<BudgetItem> items) {
        var serializableItems =
            from item in items
            let category = model.categories.GetCategoryFromId(item.CategoryID)
            select new SerializableBudgetItem(
                item.Amount,
                item.Date,
                item.ShortDescription,
                item.Category,
                category.Type);

        return JsonSerializer.Serialize(serializableItems);
    }

    /// <summary>
    /// Adds serialized budget items from
    /// <see cref="SerializeBudgetItems(IEnumerable{BudgetItem})"/> to this
    /// budget.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Any missing <see cref="Category"/> data is added to the budget.
    /// </para>
    /// <para>
    /// Tries to add as many items as possible. All items which failed to be
    /// added are reported in the result.
    /// </para>
    /// <para>
    /// If there is a conflict with an
    /// existing <see cref="Category"/> (same
    /// <see cref="Category.Description"/> but different
    /// <see cref="Category.Type"/>), that <see cref="Category"/> and its
    /// associated items are skipped.
	/// </para>
    /// </remarks>
    /// <param name="serializedItems">
    /// Serialized budget items from
    /// <see cref="SerializeBudgetItems(IEnumerable{BudgetItem})"/>
    /// </param>
    /// <returns>
    /// A string describing the results of the operation, including all
    /// successes and failures.
    /// </returns>
    /// <exception cref="JsonException">
    /// Thrown when the JSON data is unrecoverably invalid.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when there were any items that were not added successfully.
    /// Other items may have been added, this is only thrown when there is at
    /// least one failure. Also thrown if the model throws an exception.
    /// </exception>
    public string AddSerializedBudgetItems(string serializedItems) {
        var items = JsonSerializer
            .Deserialize<ICollection<SerializableBudgetItem>>(serializedItems);

        if (items is null) throw new JsonException("Data was null.");

        // Records success messages:
        StringBuilder successReport = new();
        // We try to add as many items as possible;
        // record descriptions of non-fatal errors and throw them afterward:
        StringBuilder errorReport = new();

        List<Category> originalCats = model.categories.List();

        // Add any categories that don't already exist
        int numCategoriesAdded = 0;
        List<SerializableBudgetItem> categoryConflictItems = new();

        foreach (var item in items) {
            // Skip item if its JSON was invalid, it will be reported when
            // adding expenses.
            if (!item.IsValid) continue;

            Category? existingCat = originalCats.Find(cat => cat.Description == item.CategoryDescription);

            if (existingCat == null) {
                // Category doesn't exist, create it
                try {
                    model.categories.Add(
                        item.CategoryDescription,
                        item.CategoryType.Value);

                    ++numCategoriesAdded;
                    successReport.AppendLine($"Added category \"{item.CategoryDescription}\" ({item.CategoryType.Value}).");
                } catch (Exception e) {
                    _ = errorReport.AppendLine($"Failed to add category \"{item.CategoryDescription}\" -- {e.Message}");
                }
            } else if (existingCat is not null
                    && existingCat.Type != item.CategoryType) {

                // Conflicting category
                _ = errorReport.AppendLine($"A category named \"{item.CategoryDescription}\" was found but it has the wrong type (tried to add one with type \"{item.CategoryType}\", but the existing one is \"{existingCat.Type}\").");
                categoryConflictItems.Add(item);
            }
        }

        // Refresh the view's categories
        List<Category> cats = originalCats;
        if (numCategoriesAdded > 0) {
            cats = model.categories.List();
            view.RefreshCategories(cats);
        }

        // Add expenses
        int numExpensesAdded = 0;
        foreach (var item in items) {
            // Report broken JSON:
            if (!item.IsValid) {
                _ = errorReport.AppendLine($"Failed to add invalid expense {item} -- Missing data");
                continue;
            }

            // Skip if the expense's category had a conflict:
            if (categoryConflictItems.Contains(item)) {
                _ = errorReport.AppendLine($"Failed to add expense {item} -- Category had a conflict.");
                continue;
            }

            Category cat = cats.Find(
                cat => cat.Description == item.CategoryDescription)!;

            try {
                model.expenses.Add(
                    item.Date.Value,
                    cat.Id,
                    item.Amount.Value,
                    item.ExpenseDescription);

                ++numExpensesAdded;
                successReport.AppendLine($"Added expense {item}.");
            } catch (Exception e) {
                _ = errorReport.AppendLine($"Failed to add expense {item} -- {e.Message}");
            }
        }

        // Refresh view
        if (numCategoriesAdded > 0) view.RefreshCategories(cats);
        if (numExpensesAdded > 0) view.RefreshBudgetItems();

        // Report results
        string resultMessage = $"Added {numExpensesAdded} expenses (and {numCategoriesAdded} categories) {(errorReport.Length > 0 ? "with errors!" : "successfully!")}"
            + Environment.NewLine
            + successReport
            + errorReport;

        if (errorReport.Length > 0) {
            throw new Exception(resultMessage);
        }
        return resultMessage;
    }
}
