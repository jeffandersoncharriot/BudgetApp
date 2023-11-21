using Budget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TheGregs;
using static BudgetCodeTests.TestConstants;

namespace Tests
{
    public class TestView : IView
    {
        public bool calledRefreshCategories;
        public bool calledRefreshBudgetItems;
        public List<Category>? categories;
        public void RefreshCategories(List<Category> categories)
        {
            calledRefreshCategories = true;
            this.categories = categories;
        }
        public void RefreshBudgetItems()
        {
            calledRefreshBudgetItems = true;
        }
    }

    [Collection("Sequential")]
    public class TestPresenter
    {
        public static TheoryData<DateTime?, DateTime?, bool, int> FilterTestcases = new() {
            // No filters
            { null, null, false, 0 },

            // Dates only
            { new(2019, 1, 10), null, false, 0 },
            { null, new(2019, 1, 10), false, 0 },
            { new(2018, 01, 11), new(2020, 01, 11), false, 0 },

            // Filter flag off but category provided (should be ignored)
            { null, null, false, 9 },

            // Filter by category
            { null, null, true, 10 },

            // Filter by date and category
            { new(2018, 01, 11), new(2019, 01, 10), true, 9 },
        };

        [Fact]
        public void Constructor()
        {
            // Arrange
            TestView view = new TestView();

            // Act
            Presenter p = new Presenter(newDbPath, true, view);

            // Assert
            Assert.IsType<Presenter>(p);
        }

        [Theory]
        [InlineData("Hello", Category.CategoryType.Income)]
        [InlineData("Steve", Category.CategoryType.Credit)]
        [InlineData("Feline", Category.CategoryType.Savings)]
        [InlineData("I am a category", Category.CategoryType.Expense)]
        public void AddCategory_AddsDataToDatabase(string desc, Category.CategoryType type)
        {
            // Arrange
            TestView view = new();
            Presenter p = new(newDbPath, true, view);

            // Act
            string success = p.AddCategory(desc, type);

            // Assert
            Assert.NotEmpty(success);

            HomeBudget budget = new(newDbPath, newDB: false);
            Assert.Contains(
                budget.categories.List(),
                cat => cat.Description == desc && cat.Type == type);
        }

        [Fact]
        public void AddingAValidCategoryRefreshesTheViewCategory()
        {
            // Arrange 
            TestView view = new TestView();
            Presenter p = new Presenter(newDbPath, true, view);
            view.calledRefreshCategories = false;

            string desc = "NewCategory";
            var type = Category.CategoryType.Credit;

            // Act
            string success = p.AddCategory(desc, type);

            // Assert
            Assert.NotEmpty(success);
            
            Assert.True(view.calledRefreshCategories);
            Assert.Contains(
                view.categories,
                cat => cat.Description == desc && cat.Type == type);
        }

        [Fact]
        public void AddingAValidExpenseRefreshesTheViewExpense()
        {
            // Arrange 
            TestView view = new TestView();
            Presenter p = new Presenter(newDbPath, true, view);
            view.calledRefreshBudgetItems = false;

            DateTime date = new(2012, 4, 20);
            int catId = 2;
            int amount = -23;
            string desc = "NewExpense";

            // Act
            string success = p.AddExpense(date, catId, amount, desc);

            // Assert
            Assert.NotEmpty(success);
            
            Assert.True(view.calledRefreshBudgetItems);
            Assert.Contains(
                p.GetBudgetItems(null,null,false,0),
                exp => exp.ShortDescription == desc
                    && exp.CategoryID == catId
                    && exp.ExpenseID == 1
                    && exp.Date == date
                    && exp.Amount == amount); 
        }

        [Fact]
        public void UpdateAValidExpenseRefreshesTheViewExpense()
        {
            // Arrange 
            CopyMessyDb();

            TestView view = new TestView();
            Presenter p = new Presenter(messyDbPath, false, view);
            view.calledRefreshBudgetItems = false;

            int id = 1;
            DateTime date = new(2012, 4, 20);
            int catId = 3;
            int amount = -23;
            string desc = "NewExpense";

            // Act
            string success = p.UpdateExpense(id, date, catId, amount, desc);

            // Assert
            Assert.NotEmpty(success);
            Assert.True(view.calledRefreshBudgetItems);
            Assert.Contains(
                p.GetBudgetItems(null, null, false, 0),
                exp => exp.ShortDescription == desc
                    && exp.CategoryID == catId
                    && exp.ExpenseID == id
                    && exp.Date == date
                    && exp.Amount == amount) ;
        }

        [Fact]
        public void TestAddingExpenseToDatabase()
        {
            // Arrange
            TestView view = new();
            Presenter p = new(newDbPath, true, view);

            DateTime date = new(2012, 4, 20);
            int catId = 2;
            int amount = -23;
            string desc = "NewExpense";

            // Act
            string success = p.AddExpense(date, catId, amount, desc);

            // Assert
            Assert.NotEmpty(success);
            
            HomeBudget budget = new(newDbPath, newDB: false);
            Assert.Contains(
                budget.expenses.List(),
                exp => exp.Description == desc
                    && exp.Category == catId
                    && exp.Id == 1
                    && exp.Date == date
                    && exp.Amount == amount);
        }

        [Fact]
        public void TestUpdateExpenseToDatabase()
        {
            // Arrange
            CopyMessyDb();
            TestView view = new();
            Presenter p = new(messyDbPath, false, view);

            int id = 1;
            DateTime date = new(2012, 4, 20);
            int catId = 2;
            int amount = -23;
            string desc = "NewExpense";

            // Act
            string success = p.UpdateExpense(id,date,catId,amount,desc);

            // Assert
            Assert.NotEmpty(success);

            HomeBudget budget = new(messyDbPath, newDB: false);
            Assert.Contains(
                budget.expenses.List(),
                exp => exp.Description == desc
                    && exp.Category == catId
                    && exp.Id == id
                    && exp.Date == date
                    && exp.Amount == amount);
        }

        [Theory]
        // Invalid category ID
        [InlineData(1001, 0)]
        // Invalid amount (postive amount in negative category)
        [InlineData(1, 25)]
        public void UpdateANonValidExpenseThrowsError(int catId, int amount) {
            // Arrange
            CopyMessyDb();
            TestView view = new TestView();
            Presenter p = new Presenter(messyDbPath, false, view);

            // Act
            // Positive amount in negative category:
            Exception? exception = Record.Exception(() => p.UpdateExpense(1, new(2020, 01, 01), catId, amount, "Test"));

            // Assert
            Assert.NotNull(exception);
        }

        [Theory]
        [MemberData(nameof(FilterTestcases))]
        public void GetBudgetItems_FiltersMatchModel(
                DateTime? start, 
                DateTime? end,
                bool filterFlag,
                int categoryId) {

            // Arrange
            CopyMessyDb();

            HomeBudget model = new(messyDbPath);
            var modelItems = model.GetBudgetItems(start, end, filterFlag, categoryId);

            Presenter presenter = new(messyDbPath, false, new TestView());

            // Act
            var presenterItems = presenter.GetBudgetItems(start, end, filterFlag, categoryId);

            // Assert
            Assert.Equal(modelItems, presenterItems);
        }

        [Theory]
        [MemberData(nameof(FilterTestcases))]
        public void GetBudgetItemsByCategory_FiltersMatchModel(
                DateTime? start,
                DateTime? end,
                bool filterFlag,
                int categoryId) {

            // Arrange
            CopyMessyDb();

            HomeBudget model = new(messyDbPath);
            var modelGroups = model.GetBudgetItemsByCategory(start, end, filterFlag, categoryId);

            Presenter presenter = new(messyDbPath, false, new TestView());

            // Act
            var presenterGroups = presenter.GetBudgetItemsByCategory(start, end, filterFlag, categoryId);

            // Assert
            Assert.Equal(modelGroups, presenterGroups);
        }

        [Theory]
        [MemberData(nameof(FilterTestcases))]
        public void GetBudgetItemsByMonth_FiltersMatchModel(
                DateTime? start,
                DateTime? end,
                bool filterFlag,
                int categoryId) {

            // Arrange
            CopyMessyDb();

            HomeBudget model = new(messyDbPath);
            var modelGroups = model.GetBudgetItemsByMonth(start, end, filterFlag, categoryId);

            Presenter presenter = new(messyDbPath, false, new TestView());

            // Act
            var presenterGroups = presenter.GetBudgetItemsByMonth(start, end, filterFlag, categoryId);

            // Assert
            Assert.Equal(modelGroups, presenterGroups);
        }
        [Fact]
        public void RemovingAValidExpenseRefreshesTheViewExpense()
        {
            // Arrange 
            int id = 1;
            CopyMessyDb();

            TestView view = new TestView();
            Presenter p = new Presenter(messyDbPath, false, view);
            view.calledRefreshBudgetItems = false;

            // Sanity check (make sure expense already exists)
            Assert.Contains(
                p.GetBudgetItems(null, null, false, 0),
                e => e.ExpenseID == id);

            // Act
            string success = p.RemoveExpense(id);

            // Assert
            Assert.NotEmpty(success);
            Assert.True(view.calledRefreshBudgetItems);
            Assert.DoesNotContain(
                p.GetBudgetItems(null,null,false,0),
                exp => exp.ExpenseID == id);
        }

        [Fact]
        public void TestRemovingExpenseToDatabase()
        {
            // Arrange
            int id = 1;
            CopyMessyDb();

            // Sanity check (make sure expense already exists)
            HomeBudget beforeBudget = new(messyDbPath, newDB: false);
            Assert.Contains(beforeBudget.expenses.List(), e => e.Id == id);

            TestView view = new();
            Presenter p = new(messyDbPath, false, view);

            // Act
            string success = p.RemoveExpense(id);

            // Assert
            Assert.NotEmpty(success);

            HomeBudget budget = new(messyDbPath, newDB: false);
            Assert.DoesNotContain(
                budget.expenses.List(),
                exp => exp.Id == id);
        }

        [Fact]
        public void RemovingANonValidExpenseThrowsError()
        {
            // Arrange 
            CopyMessyDb();
            TestView view = new TestView();
            Presenter p = new Presenter(messyDbPath, false, view);

            // Act
            var exception = Record.Exception(() => p.RemoveExpense(1006));

            // Assert
            Assert.NotNull(exception);
        }

        [Theory]
        [MemberData(nameof(FilterTestcases))]
        public void GetBudgetDictionaryByCategoryAndMonth_FiltersMatchModel(
                DateTime? start,
                DateTime? end,
                bool filterFlag,
                int categoryId) {

            // Arrange
            CopyMessyDb();

            HomeBudget model = new(messyDbPath);
            var modelGroups = model.GetBudgetDictionaryByCategoryAndMonth(start, end, filterFlag, categoryId);

            Presenter presenter = new(messyDbPath, false, new TestView());

            // Act
            var presenterGroups = presenter.GetBudgetDictionaryByCategoryAndMonth(start, end, filterFlag, categoryId);

            // Assert
            Assert.Equal(modelGroups, presenterGroups);
        }

        [Fact]
        public void AddingANonValidCategoryThrowsError()
        {
            // Arrange 
            TestView view = new TestView();
            Presenter p = new Presenter(newDbPath, true, view);

            // Act
            // Conflict with existing category description:
            Exception? exception = Record.Exception(() => p.AddCategory("Utilities", Category.CategoryType.Expense));

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void AddingANonValidExpenseThrowsError()
        {
            // Arrange 
            TestView view = new TestView();
            Presenter p = new Presenter(newDbPath, true, view);

            // Act
            // Positive amount in negative category:
            Exception? exception = Record.Exception(() => p.AddExpense(new(2012, 4, 20), 2, 23, "Utilities"));

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void AddingAValidCategoryReturnsSuccess()
        {
            // Arrange 
            TestView view = new TestView();
            Presenter p = new Presenter(newDbPath, true, view);

            // Act
            string success = p.AddCategory("dfvdfsvsdfv", Category.CategoryType.Expense);

            // Assert
            Assert.NotEmpty(success);
        }

        [Fact]
        public void AddingAValidExpenseReturnsSuccess()
        {
            // Arrange 
            TestView view = new TestView();
            Presenter p = new Presenter(newDbPath, true, view);

            // Act
            string success = p.AddExpense(new(2012, 4, 20), 2, -23, "Utilities");

            // Assert
            Assert.NotEmpty(success);
        }

        #region Copy-Pasting
        [Fact]
        public void SerializeBudgetItems_SerializesMultipleExpenses() {
            // Arrange
            Presenter p = new(newDbPath, true, new TestView());

            p.AddCategory("TestCat", Category.CategoryType.Credit);

            string expectedJson = "[{\"Amount\":-23,\"Date\":\"2011-11-11T00:00:00\",\"ExpenseDescription\":\"TestExpense1\",\"CategoryDescription\":\"Entertainment\",\"CategoryType\":2},{\"Amount\":16,\"Date\":\"2013-04-15T00:00:00\",\"ExpenseDescription\":\"TestExpense2\",\"CategoryDescription\":\"TestCat\",\"CategoryType\":3}]";

            p.AddExpense(new(2011, 11, 11), 4, -23, "TestExpense1");
            p.AddExpense(new(2013, 4, 15), 17, 16, "TestExpense2");

            // Act
            string json = p.SerializeBudgetItems(p.GetBudgetItems(null, null, false, 0));

            // Assert
            Assert.Equal(expectedJson, json);
        }

        [Fact]
        public void AddSerializedBudgetItems_MultipleExpensesWithNewCategory() {
            // Arrange
            TestView view = new();
            Presenter p = new(newDbPath, true, view);

            var expectedItems = new List<BudgetItem> {
                new() {
                    Amount = -23,
                    Balance = -23,
                    Category = "Entertainment",
                    CategoryID = 4,
                    Date = new(2011, 11, 11),
                    ExpenseID = 1,
                    ShortDescription = "TestExpense1"},
                new() {
                    Amount = 16,
                    Balance = -7,
                    Category = "TestCat",
                    CategoryID = 17,
                    Date = new(2013, 4, 15),
                    ExpenseID = 2,
                    ShortDescription = "TestExpense2"},
            };

            var expectedNewCat = new Category(
                17, "TestCat", Category.CategoryType.Credit);

            string json = "[{\"Amount\":-23,\"Date\":\"2011-11-11T00:00:00\",\"ExpenseDescription\":\"TestExpense1\",\"CategoryDescription\":\"Entertainment\",\"CategoryType\":2},{\"Amount\":16,\"Date\":\"2013-04-15T00:00:00\",\"ExpenseDescription\":\"TestExpense2\",\"CategoryDescription\":\"TestCat\",\"CategoryType\":3}]";

            // Act
            string res = p.AddSerializedBudgetItems(json);

            // Assert

            // Check that result message is accurate
            // Don't check ends of words since they might change
            // ("expense(s)", or "category/categories"):
            Assert.Contains("2 expense", res);
            Assert.Contains("1 categor", res);

            Assert.Equal(expectedItems, p.GetBudgetItems(null, null, false, 0));
            Assert.Contains(expectedNewCat, view.categories);
        }

        
        [Fact]
        public void AddSerializedBudgetItems_NonFatalError_AddsSurvivingExpenses() {
            // Arrange
            TestView view = new();
            Presenter p = new(newDbPath, true, view);

            var expectedItems = new List<BudgetItem> {
                new() {
                    Amount = 16,
                    Balance = 16,
                    Category = "TestCat",
                    CategoryID = 17,
                    Date = new(2013, 4, 15),
                    ExpenseID = 1,
                    ShortDescription = "TestExpense2"},
            };

            var expectedNewCat = new Category(
                17, "TestCat", Category.CategoryType.Credit);

            // The first expense's category conflicts with the existing one;
            // The second expense's amount has the wrong sign;
            // The third expense should still be added:
            string json = "[{\"Amount\":-23,\"Date\":\"2011-11-11T00:00:00\",\"ExpenseDescription\":\"TestExpense1\",\"CategoryDescription\":\"Entertainment\",\"CategoryType\":1},{\"Amount\":23,\"Date\":\"2011-11-11T00:00:00\",\"ExpenseDescription\":\"TestExpense1\",\"CategoryDescription\":\"Entertainment\",\"CategoryType\":2},{\"Amount\":16,\"Date\":\"2013-04-15T00:00:00\",\"ExpenseDescription\":\"TestExpense2\",\"CategoryDescription\":\"TestCat\",\"CategoryType\":3}]";

            // Act
            var err = Record.Exception(() => p.AddSerializedBudgetItems(json));

            // Assert
            Assert.IsAssignableFrom<Exception>(err);

            // Check that result message is accurate
            var resFirstLine = err.Message.Split(Environment.NewLine, 2)[0];
            Assert.Contains("1 expense", resFirstLine);
            Assert.Contains("1 categor", resFirstLine);
            // First expense error:
            Assert.Contains("conflict", err.Message);
            // Second expense error:
            Assert.Contains("negative", err.Message);

            Assert.True(view.calledRefreshBudgetItems);
            Assert.Equal(expectedItems, p.GetBudgetItems(null, null, false, 0));
            Assert.Contains(expectedNewCat, view.categories);
        }

        [Fact]
        public void AddSerializedBudgetItems_FailsWithMissingFields() {
            // Arrange
            TestView view = new();
            Presenter p = new(newDbPath, true, view);

            // Act
            var err = Record.Exception(() => p.AddSerializedBudgetItems("[{}]"));

            // Assert
            Assert.IsAssignableFrom<Exception>(err);
            Assert.Contains("Expense with a missing description on missing date with missing amount in missing category (missing category type)", err.Message);
            Assert.Empty(p.GetBudgetItems(null, null, false, 0));
        }

        [Fact]
        public void AddSerializedBudgetItems_FailsWithNullJson() {
            // Arrange
            TestView view = new();
            Presenter p = new(newDbPath, true, view);

            // Act
            var err = Record.Exception(() => p.AddSerializedBudgetItems("null"));

            // Assert
            Assert.IsAssignableFrom<JsonException>(err);
            Assert.Contains("null", err.Message);
            Assert.Empty(p.GetBudgetItems(null, null, false, 0));
        }

        [Fact]
        public void AddSerializedBudgetItems_FailsWithInvalidJson() {
            // Arrange
            TestView view = new();
            Presenter p = new(newDbPath, true, view);

            // Act
            var err = Record.Exception(() => p.AddSerializedBudgetItems("hi"));

            // Assert
            Assert.IsAssignableFrom<JsonException>(err);
            Assert.Empty(p.GetBudgetItems(null, null, false, 0));
        }
        #endregion
    }
}
