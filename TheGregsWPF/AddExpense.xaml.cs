using Budget;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TheGregsWPF
{
    /// <summary>
    /// Interaction logic for AddExpense.xaml
    /// </summary>
    public partial class AddExpense : Window
    {
        private readonly Presenter _presenter;
        private readonly BudgetWindow budgetWindow;
        private readonly BudgetItem? budgetItem;
        private bool btnSaveWasClicked;

        private Category? initialCat;
        private string initialAmount;
        private string initialDesc;
        private DateTime? initialDate;

        /// <summary>
        /// Creates a new instance linked to a given presenter and
        /// <see cref="BudgetWindow"/>.
        /// </summary>
        /// <remarks>
        /// The new instance is added as a child window to
        /// <paramref name="budgetWindow"/> through
        /// <see cref="BudgetWindow.AddChildWindow(Window)"/>.
        /// </remarks>
        /// <param name="presenter">Presenter object to be used for linking view and model</param>
        /// <param name="budgetWindow">The budget's window.</param>
        /// <example>
        /// <code>
        /// private void btnNewExpense_Click(object sender, RoutedEventArgs e)
        /// {
        ///    AddExpense addExpenseWindow = new AddExpense(presenter, this);
        ///    addExpenseWindow.Show();
        /// }
        /// </code>
        /// </example>
        public AddExpense(Presenter presenter, BudgetWindow budgetWindow)
        {
            InitializeComponent();
            _presenter = presenter;
            this.budgetWindow = budgetWindow;
            budgetWindow.AddChildWindow(this);
            cmbCategories.ItemsSource = budgetWindow.BudgetCategories;
            btnSaveWasClicked = false;
            SetInitialFields();
        }

        /// <summary>
        /// Creates a new instance linked to a given presenter and
        /// <see cref="BudgetWindow"/> to update an expense.
        /// </summary>
        /// <remarks>
        /// The new instance is added as a child window to
        /// <paramref name="budgetWindow"/> through
        /// <see cref="BudgetWindow.AddChildWindow(Window)"/>.
        /// </remarks>
        /// <param name="presenter">Presenter object to be used for linking view and model</param>
        /// <param name="budgetWindow">The budget's window.</param>
        /// <param name="budgetItem">The budget item of the  expense to be updated</param>
        /// <example>
        /// <code>
        /// private void btnNewExpense_Click(object sender, RoutedEventArgs e)
        /// {
        ///    AddExpense addExpenseWindow = new AddExpense(presenter, this, budgetItem);
        ///    addExpenseWindow.Show();
        /// }
        /// </code>
        /// </example>
        public AddExpense(Presenter presenter, BudgetWindow budgetWindow, BudgetItem budgetItem) : this(presenter, budgetWindow)
        {
            txbTitle.Text = "Update Expense";
            btnSave.Content = "Update";
            this.budgetItem = budgetItem;
            btnSave.Click -= btnSave_Click;
            btnSave.Click += btnUpdate_Click;
            SetExpenseFields();
            SetInitialFields();
        }

        [MemberNotNull(new[] {
            nameof(initialDesc),
            nameof(initialAmount)})]
        private void SetInitialFields()
        {
            initialCat = this.cmbCategories.SelectedItem as Category;
            initialDesc = txbDesc.Text;
            initialAmount = txbAmount.Text;
            initialDate = txbDate.SelectedDate;

        }


        /// <summary>
        /// Saves all user input values to add to the expense to the database
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var category = this.cmbCategories.SelectedItem as Category;
                if (category is null) throw new Exception("No category chosen");
                var description = txbDesc.Text;

                if (!double.TryParse(txbAmount.Text, out double amount))
                    throw new Exception("Invalid or empty amount");

                if (txbDate.SelectedDate is null) throw new Exception("Date cannot be empty");
                DateTime date = txbDate.SelectedDate.Value;
             
                budgetWindow.Log(_presenter.AddExpense(date, category.Id, amount, description));
                btnSaveWasClicked = true;
                this.Close();
            }
            catch(Exception ex)
            {
                budgetWindow.ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Saves all user input values to update an expense of the database
        /// </summary>
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_presenter
                        .GetBudgetItems(null, null, false, 0)
                        .Exists(e => e.ExpenseID == budgetItem.ExpenseID)) {

                    budgetWindow.ShowError("Expense no longer exists!");
                    this.Close();
                    return;
                }

                var category = this.cmbCategories.SelectedItem as Category;
                if (category is null) throw new Exception("No category chosen");
                var description = txbDesc.Text;

                if (!double.TryParse(txbAmount.Text, out double amount))
                    throw new Exception("Invalid or empty amount");

                if (txbDate.SelectedDate is null) throw new Exception("Date cannot be empty");
                DateTime date = txbDate.SelectedDate.Value;

                budgetWindow.Log(
                    _presenter.UpdateExpense(budgetItem.ExpenseID, date, category.Id, amount, description)
                );
                btnSaveWasClicked = true;
                this.Close();
            }
            catch (Exception ex)
            {
                budgetWindow.ShowError(ex.Message);
            }
        }
        
        private void SetExpenseFields()
        {
            txbAmount.Text = budgetItem.Amount.ToString();
            txbDate.SelectedDate = budgetItem.Date;
            txbDesc.Text = budgetItem.ShortDescription;
            cmbCategories.SelectedValue = budgetWindow.BudgetCategories.First(category => category.Id == budgetItem.CategoryID );
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Opens New Category window when button is clicked
        /// </summary>
        private void btnNewCategory_Click(object sender, RoutedEventArgs e)
        {
            new AddCategory(_presenter, budgetWindow).Show();
        }
        
        // Checks if theres unsaved changes in the window
        private bool UnsavedChanges()
        {
            Category cat = cmbCategories.SelectedItem as Category;

            if (btnSaveWasClicked) return false;
            if (cat is not null  && initialCat is not null && initialCat.Id != cat.Id
                || txbDate.SelectedDate != initialDate
                || txbDesc.Text != initialDesc
                || txbAmount.Text != initialAmount) return true;

            return false;
        }

        // Confirms if user wants to close if theres some unsaved data in the window
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(UnsavedChanges())
            {
                MessageBoxResult warning = MessageBox.Show("You have some changes, Are you sure you want to close this window?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (warning == MessageBoxResult.No) e.Cancel = true;
            }
        }
    }
}
