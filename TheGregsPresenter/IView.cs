namespace Budget;

/// <summary>The home budget MVP view.</summary>
public interface IView {
    /// <summary>
    /// Refreshes the categories list.
    /// </summary>
    /// <param name="categories">The new list of categories</param>
    /// <example>
    /// This would be code in the presenter, assuming view and model initalized properly
    /// <code>
    /// try{
    ///     model.AddCategory("random");
    ///     view.ShowSuccess("Category successfully added");
    ///     view.RefreshCategories(model.categories.List());
    /// } catch(error e) {
    ///     view.ShowError(e.message);
    ///     return false;
    /// }
    /// </code>
    /// </example>
    public void RefreshCategories(List<Category> category);


    /// <summary>
    /// Tells the view that it should refresh its expense budget items.
    /// </summary>
    /// <example>
    /// This would be code in the presenter, assuming view and model initalized properly
    /// <code>
    /// try{
    ///     model.AddExpense(DateTime.Now, 4, 12.99, "Green Apples")
    ///     view.RefreshBudgetItems();
    /// } catch(error e) {
    ///     view.ShowError(e.message);
    ///     return false;
    /// }
    /// </code>
    /// </example>
    public void RefreshBudgetItems();
}
