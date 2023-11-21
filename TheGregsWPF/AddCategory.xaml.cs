using Budget;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AddCategory.xaml
    /// </summary>
    public partial class AddCategory : Window
    {
        private readonly Presenter _presenter;
        private readonly BudgetWindow _budgetWindow;

        private bool btnAddCategoryWasClicked;

        private string initialDesc;
        private Category.CategoryType? initialCatType;


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
        /// <example>
        /// <code>
        /// AddCategory addCategoryWindow = new AddCategory(presenter);
        /// addCategoryWindow.Show();
        /// </code>
        /// </example>
        public AddCategory(Presenter presenter, BudgetWindow budgetWindow)
        {
            InitializeComponent();
            _presenter = presenter;
            _budgetWindow = budgetWindow;
            budgetWindow.AddChildWindow(this);
            cmbCategoryTypes.ItemsSource = Enum.GetValues(typeof(Category.CategoryType));
            cmbCategoryTypes.SelectedIndex = 0;
            btnAddCategoryWasClicked = false;
            SetInitialFields();
        }

        [MemberNotNull(nameof(initialDesc))]
        private void SetInitialFields()
        {
            initialDesc = txbCategory.Text;
            initialCatType = (Category.CategoryType)cmbCategoryTypes.SelectedItem;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var type = (Category.CategoryType)cmbCategoryTypes.SelectedItem;
                string successMessage = _presenter.AddCategory(txbCategory.Text, type);

                _budgetWindow.Log(successMessage);
                _budgetWindow.ShowSuccess(successMessage);

                btnAddCategoryWasClicked = true;
                this.Close();
            }
            catch(Exception ex)
            {
                _budgetWindow.ShowError(ex.Message);
            }
        }
        // Checks if theres data added
        private bool UnsavedChanges()
        {
            if (btnAddCategoryWasClicked) return false;
            if (initialCatType != (Category.CategoryType)cmbCategoryTypes.SelectedItem
                || initialDesc != txbCategory.Text) return true;

            return false;
        }
        // Confirms if user wants to close if there's data
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (UnsavedChanges())
            {
                MessageBoxResult warning = MessageBox.Show("You have made some changes, Are you sure you want to close this window?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (warning == MessageBoxResult.No) e.Cancel = true;
            }
        }
    }
}
