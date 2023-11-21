using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Budget;
using Microsoft.Win32;
using TheGregsPresenter;

namespace TheGregsWPF;
/// <summary>
/// Interaction logic for BudgetWindow.xaml
/// </summary>
public partial class BudgetWindow : Window, IView {
    Presenter? presenter;
    List<Window> childWindows = new();

    public ObservableCollection<Category> BudgetCategories { get; } = new();

    Config config;

    private ContextMenu rightClickContext;

    // WPF command for editing a thing bound to space key;
    // not sure I'm using this right, but it works!
    private static RoutedUICommand EditCommand = new(
        "Edit",
        "Edit",
        typeof(ExecutedRoutedEventHandler),
        new InputGestureCollection { new KeyGesture(Key.Space) });

    /// <summary>
    /// Creates a new window without loading a budget.
    /// </summary>
    public BudgetWindow()
    {
        InitializeComponent();
        budgetContainer.IsEnabled = false;

        // Load config from default appdata file:
        config = new();

        if (config.LastFile is not null)
        {
            var choice = MessageBox.Show(
                $"Would you like to continue working on your last opened budget ({config.LastFile})?",
                "Last Opened Budget",
                MessageBoxButton.YesNo);

            if (choice == MessageBoxResult.Yes)
            {
                OpenDatabase(config.LastFile, isNewDb: false);
            }
        }

        // Make clicking the datagrid focus it to make pasting easier:
        dataGridBudgetItems.MouseDown += (_, _) => {
            // Only take focus if this window is active;
            // this prevents stealing focus when, for example, double clicking
            // tries to focus another window instead:
            if (this.IsActive) dataGridBudgetItems.Focus();
        };

        Closing += (_, cancel) =>
        {
            if (!closeChildWindows()) cancel.Cancel = true;
        };

        // Setup context menu
        rightClickContext = new ContextMenu();
        dataGridBudgetItems.ContextMenu = rightClickContext;

        AddBudgetItemContextMenuCommand(
            ApplicationCommands.Copy,
            handleCopyBudgetItems,
            HasBudgetItemsSelected);

        AddBudgetItemContextMenuCommand(
            ApplicationCommands.Paste,
            handlePasteBudgetItems,
            () => Clipboard.ContainsText());

        AddBudgetItemContextMenuCommand(
            EditCommand,
            dataGridBudgetItems_RightClickEdit,
            HasBudgetItemsSelected);

        AddBudgetItemContextMenuCommand(
            ApplicationCommands.Delete,
            btnDelete_Click,
            HasBudgetItemsSelected);
    }

    /// <summary>
    /// Adds a command to the budget item datagrid's context menu.
    /// </summary>
    /// <param name="command">The command to add.</param>
    /// <param name="eventHandler">Called when the command is executed.</param>
    /// <param name="isEnabled">
    /// A function which returns true if the command should be enabled,
    /// otherwise false.
    /// </param>
    private void AddBudgetItemContextMenuCommand(
            ICommand command,
            ExecutedRoutedEventHandler eventHandler,
            Func<bool> isEnabled) {

        // Add item to context menu

        MenuItem item = new() { Command = command };
        rightClickContext.Items.Add(item);

        // Register command

        CommandBinding commandBinding = new(
            command,
            eventHandler,
            (_, e) => {
                e.CanExecute = isEnabled();
                e.Handled = true;
            });

        dataGridBudgetItems.CommandBindings.Add(commandBinding);
    }

    /// <summary>Adds a child window to this window.</summary>
    /// <remarks>
    /// Does not set the window's <see cref="Window.Owner"/>.
    /// </remarks>
    /// <param name="window">The window to add.</param>
    public void AddChildWindow(Window window)
    {
        // Check if window is already closed:
        if (!Application.Current.Windows.Cast<Window>().Contains(window)) return;

        childWindows.Add(window);
        window.Closed += (_,_) => childWindows.Remove(window);
    }

    private void SetStatusPath(string path)
    {
        txbDbPath.Text = path;
        string name = System.IO.Path.GetFileName(path);
        this.Title = name;
    }

    private void btnNewExpense_Click(object sender, RoutedEventArgs e)
    {
        new AddExpense(presenter!, this).Show();
    }

    private void btnNewCategory_Click(object sender, RoutedEventArgs e)
    {
        new AddCategory(presenter!, this).Show();
    }

    private void handleSummaryChanged(object sender, RoutedEventArgs e)
    {
        RefreshBudgetItems();
    }

    public void Log(string message)
    {
        txbLog.AppendText($"{message}\n");
        txbLog.ScrollToEnd();
    }

    public void RefreshCategories(List<Category> categories)
    {
        BudgetCategories.Clear();
        foreach(Category category in categories) BudgetCategories.Add(category);

        // Necessary for grouping by category & month at the same time:
        RefreshBudgetItems();
    }

    /// <summary>Adds a column to the BudgetItem DataGrid.</summary>
    /// <param name="header">The column's header.</param>
    /// <param name="propertyPath">
    /// The path of the property to get from each row's item.
    /// </param>
    /// <param name="stringFormat">
    /// Optional format string to use when displaying values.
    /// </param>
    /// <param name="alignment">
    /// The horizontal alignment to use for this column. Defaults to right
    /// alignment if <paramref cref="stringFormat"/> is "c", otherwise left.
    /// </param>
    private void AddColumn(
            string header,
            string propertyPath,
            string? stringFormat=null,
            HorizontalAlignment? alignment=null) {

        var binding = new Binding(propertyPath);
        if (stringFormat is not null) binding.StringFormat = stringFormat;

        var column = new DataGridTextColumn() {
            Header = header,
            Binding = binding,

            // Need to create a new style because the default one is sealed:
            ElementStyle = new()
        };

        // Choose default alignment
        if (alignment == null) {
            // Currency defaults to right-aligned
            if (stringFormat == "c") alignment = HorizontalAlignment.Right;
        }

        // Add alignment to style
        if (alignment != null) {
            column.ElementStyle.Setters.Add(
                new Setter(HorizontalAlignmentProperty, alignment));
        }

        dataGridBudgetItems.Columns.Add(column);
    }

    /// <summary>
    /// Alerts the user that an error occured.
    /// </summary>
    /// <param name="errorMessage">A message describing the error.</param>
    /// <example>
    /// This code shows an example which reports that an error occured when
    /// calling a presenter function:
    /// <code>
    /// try{
    ///     presenter.AddCategory("random", Category.CategoryType.Expense);
    ///     budgetWindow.ShowSuccess("Category successfully added");
    ///     budgetWindow.RefreshCategories(model.categories.List());
    /// } catch(error e) {
    ///     budgetWindow.ShowError(e.message);
    ///     return false;
    /// }
    /// </code>
    /// </example>
    public void ShowError(string errorMessage)
    {
        Log("Error: " + errorMessage);
        MessageBox.Show(errorMessage, "An error has occured", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Alerts the user that an operation completed successfully.
    /// </summary>
    /// <param name="message">A message to tell the user about the successful action</param>
    /// <example>
    /// This code shows an example which reports that a presenter function
    /// completed successfully:
    /// <code>
    /// try{
    ///     presenter.AddCategory("random", Category.CategoryType.Expense);
    ///     budgetWindow.ShowSuccess("Category successfully added");
    ///     budgetWindow.RefreshCategories(model.categories.List());
    /// } catch(error e) {
    ///     budgetWindow.ShowError(e.message);
    ///     return false;
    /// }
    /// </code>
    /// </example>
    public void ShowSuccess(string message)
    {
        Log("Success: " + message);
        MessageBox.Show(message, "Action successful", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SetBudgetItemControlsEnabled(bool isEnable)
    {
        btnDelete.IsEnabled = SearchBar.IsEnabled = isEnable;
    }

    public void RefreshBudgetItems()
    {
        dataGridBudgetItems.ItemsSource = null;
        dataGridBudgetItems.Columns.Clear();
        SetBudgetItemControlsEnabled(false);
        // Use this instead of "d" so that we can take the user's settings into
        // account. For example, I might use en-US but want yyyy-MM-dd:
        var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        int catID = 0;
        if ((Category)cmbCategories.SelectedItem is not null) catID = ((Category)cmbCategories.SelectedItem).Id;
        switch (summarizeByCategoryCheckbox.IsChecked, summarizeByMonthCheckbox.IsChecked) {
            case (false, false):

                // Set up columns
                AddColumn("Date", nameof(BudgetItem.Date), dateFormat);
                AddColumn("Category", nameof(BudgetItem.Category));
                AddColumn("Description", nameof(BudgetItem.ShortDescription));
                AddColumn("Amount", nameof(BudgetItem.Amount), "c");
                AddColumn("Balance", nameof(BudgetItem.Balance), "c");

                dataGridBudgetItems.ItemsSource = presenter!.GetBudgetItems(
                    datePickerStart.SelectedDate,
                    datePickerEnd.SelectedDate,
                    (bool)chckBoxFilter.IsChecked,
                    catID);
                SetBudgetItemControlsEnabled(true);
                break;

            case (true, false):

                // Set up columns
                AddColumn("Category", nameof(BudgetItemsByCategory.Category));
                AddColumn("Total", nameof(BudgetItemsByCategory.Total), "c");

                dataGridBudgetItems.ItemsSource = presenter!.GetBudgetItemsByCategory(
                    datePickerStart.SelectedDate,
                    datePickerEnd.SelectedDate,
                    (bool)chckBoxFilter.IsChecked,
                    catID);
                break;

            case (false,true):

                // Set up columns
                AddColumn("Month", nameof(BudgetItemsByMonth.Month));
                AddColumn("Total", nameof(BudgetItemsByMonth.Total), "c");

                dataGridBudgetItems.ItemsSource = presenter!.GetBudgetItemsByMonth(
                    datePickerStart.SelectedDate,
                    datePickerEnd.SelectedDate,
                    (bool)chckBoxFilter.IsChecked,
                    catID);

                break;

            case (true, true):

                // Set up columns
                AddColumn("Month", "[Month]");
                foreach (var category in BudgetCategories.OrderBy(e => e.Description)) {
                    AddColumn(category.Description, $"[{category.Description}]", "c");
                }
                AddColumn("Total", "[Total]", "c");

                dataGridBudgetItems.ItemsSource = presenter!.GetBudgetDictionaryByCategoryAndMonth(
                    datePickerStart.SelectedDate,
                    datePickerEnd.SelectedDate,
                    (bool)chckBoxFilter.IsChecked,
                    catID);

                break;
        }
    }

    private void dataGridBudgetItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
       DataGrid cell = (DataGrid)sender;

       // Can only edit expenses, not summary groups (wacky three-valued logic)
       if (false != (
            summarizeByCategoryCheckbox.IsChecked
            | summarizeByMonthCheckbox.IsChecked)) return;
       // Can only edit when an expense is selected
       if (cell.SelectedItem is null) return;

        EditSelectedExpenses();
    }

    private void dataGridBudgetItems_RightClickEdit(object sender, RoutedEventArgs e)
    {
        EditSelectedExpenses();
    }

    private void handleCopyBudgetItems(object sender, RoutedEventArgs e) {
        string data = presenter.SerializeBudgetItems(dataGridBudgetItems.SelectedItems.Cast<BudgetItem>());
        Clipboard.SetText(data);
        dataGridBudgetItems.Focus();
        e.Handled = true;
    }

    private void handlePasteBudgetItems(object sender, RoutedEventArgs e) {
        try {
            Log(presenter.AddSerializedBudgetItems(Clipboard.GetText()));
            dataGridBudgetItems.Focus();
            e.Handled = true;
        } catch (JsonException error) {
            ShowError("Could not paste because of invalid data: " + error.Message);
        } catch (Exception error) {
            ShowError(error.Message);
        }
    }

    private void EditSelectedExpenses()
    {
        var budgetItems = dataGridBudgetItems.SelectedItems;

        foreach (BudgetItem budgetItem in budgetItems) {
            AddExpense AddExpenseWindow = new(presenter, this, budgetItem);

            // Prevent overlapping:
            AddExpenseWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            AddExpenseWindow.Show();
        }
    }


    /// <summary>
    /// Deletes selected expenses
    /// if none selected, throws error message
    /// confirms if user wants to delete selected expenses
    /// </summary>
    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var budgetItems = dataGridBudgetItems
                .SelectedItems
                .Cast<BudgetItem>()
                .ToArray();

            if (budgetItems.Length == 0) throw new Exception("No budget item selected");

            if (budgetItems.Length > 1)
            {

                var dialogResult = MessageBox.Show($"Are you sure you want to delete these selected expenses?", $"Delete expenses", MessageBoxButton.YesNo);
                if (dialogResult is MessageBoxResult.No) return;

                foreach(BudgetItem item in budgetItems) Log(presenter!.RemoveExpense(item.ExpenseID));

            }
            else
            {
                BudgetItem item = budgetItems[0];
                int id = item.ExpenseID;
                String desc = item.ShortDescription;

                var dialogResult = MessageBox.Show($"Are you sure you want to delete this expense? \"{desc}\"", $"Delete expense: {desc}", MessageBoxButton.YesNo);
                if (dialogResult is MessageBoxResult.No) return;

                Log(presenter.RemoveExpense(id));
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    /// <summary>
    /// True if the datagrid has selected items and those items are all
    /// <see cref="BudgetItem"/>s.
    /// </summary>
    private bool HasBudgetItemsSelected() {
        return dataGridBudgetItems.SelectedItems.Count > 0
            && dataGridBudgetItems
                .SelectedItems
                .Cast<object>()
                .All(e => e is BudgetItem);
    }

    private bool closeChildWindows()
    {
        // Try to close all child windows:
        while (childWindows.Count > 0)
        {
            Window temp = childWindows[0];

            temp.Show();
            temp.Focus();
            childWindows[0].Close();

            // If closing the window didn't work (probably unsaved
            // changes), abort:
            if (childWindows.Count > 0 && temp == childWindows[0]) return false;

        }

        return true;
    }

    private void SetEnableSearchNavButtons(bool isEnable)
    {
        btnNext.IsEnabled = btnPrevious.IsEnabled = isEnable;
    }
    private void txbSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (String.IsNullOrEmpty(txbSearch.Text))
        {
            SetEnableSearchNavButtons(false);
            return;
        }
        SetEnableSearchNavButtons(true);

        int index = dataGridBudgetItems.SelectedIndex;
        // So that when the user erases letters or adds, it doesn't jump to the
        // next match
        if (index != -1 && SearchMatch(index)) return;
        Search(index, isForwardSearch: true);
    }

    private void btnNext_Click(object sender, RoutedEventArgs e)
    {
        int index = dataGridBudgetItems.SelectedIndex;
        if (!Search(index, isForwardSearch: true))  ShowNotFoundMessage();
    }

    private void btnPrevious_Click(object sender, RoutedEventArgs e)
    {
        int index = dataGridBudgetItems.SelectedIndex;
        if(!Search(index, isForwardSearch: false)) ShowNotFoundMessage();
    }
    
    private void ShowNotFoundMessage()
    {
         MessageBox.Show($"No expenses found with name {txbSearch.Text}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // Searches the list from the current index
    private bool Search(int index, bool isForwardSearch)
    {
        // Checks which direction the search should go from
        int searchDirection = isForwardSearch ? 1 : -1;

        // Uses <= for when starting index is -1, as well as to allow looping
        // through the entire list and returning to where we started:
        for (int offset = 1; offset <= dataGridBudgetItems.Items.Count; offset++)
        {
            int itemIndex = WrapAroundBudgetItemIndex(index + (offset * searchDirection));
            if (SearchMatch(itemIndex))
            {
                dataGridBudgetItems.SelectedIndex = itemIndex;
                dataGridBudgetItems.ScrollIntoView(dataGridBudgetItems.SelectedItem);
                return true;
            }
        }
        return false;
    }

    // Searches the element at the index to see if it matches the typed in 
    private bool SearchMatch(int index)
    {
        return (dataGridBudgetItems.Items.GetItemAt(index) as BudgetItem).ShortDescription.ToLower().Contains(txbSearch.Text.ToLower());
    }

    // Basically a circular array converter
    private int WrapAroundBudgetItemIndex(int index)
    {
        int itemCount = dataGridBudgetItems.Items.Count;
        return (index % itemCount + itemCount) % itemCount;
    }

    /// <summary>
    /// Opens a given budget database.
    /// </summary>
    /// <param name="path">The budget's filepath.</param>
    /// <param name="isNewDb">
    /// True if should overwrite any existing data in the file with default
    /// data. Ignored if file does not already exist.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown if something goes wrong while opening the database.
    /// </exception>
    private void OpenDatabase(string path, bool isNewDb)
    {

        if(!closeChildWindows()) return;

        config.LastFile = path;
        SetStatusPath(path);
        presenter = new Presenter(path, isNewDb, this);
        presenter.RequestCategoriesRefresh();
        SetEnableSearchNavButtons(false);
        RefreshBudgetItems();
        cmbCategories.ItemsSource = BudgetCategories;
        budgetContainer.IsEnabled = true;

        Log($"Successfully {(isNewDb ? "created new" : "opened existing")} database: {path}");
    }

    private void btnSelectDatabase_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Database Files|*.db";

        if (openFileDialog.ShowDialog() == true)
        {
            OpenDatabase(openFileDialog.FileName, isNewDb: false);
        }
    }

    private void btnNewDatabase_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Database Files|*.db";
        if (saveFileDialog.ShowDialog() == true)
        {
            OpenDatabase(saveFileDialog.FileName, isNewDb: true);
        }
    }
}
