using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;
using System.Data.SQLite;
using static BudgetCodeTests.TestConstants;
using static Budget.Category;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestCategories
    {
        public int numberOfCategoriesInFile = TestConstants.numberOfCategoriesInFile;
        public String testInputFile = TestConstants.testDBInputFile;
        public int maxIDInCategoryInFile = TestConstants.maxIDInCategoryInFile;
        Category firstCategoryInFile = TestConstants.firstCategoryInFile;
        int IDWithSaveType = TestConstants.CategoryIDWithSaveType;

        // Category descriptions that should be considered equivalent,
        // for use with [ClassData]
        class EquivalentDescriptions : TheoryData<string, string> {
            public EquivalentDescriptions() {
                Add("Food", "Food");
                Add("Food", " Food");
                Add("Food", "Food ");
                Add("Food", "Food\t");
                Add("Credit Card", "Credit Card");
                Add("Credit Card", "Credit  Card");
                Add("Credit Card", "Credit\tCard");
                // \u3000 is CJK space:
                Add("Credit Card", "Credit\u3000\tCard");
                // Same letter encoded differently by unicode:
                Add("\u00C5", "\u0041\u030A");
            }
        }

        // ========================================================================

        [Fact]
        public void CategoriesObject_New()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, true);

            // Assert 
            Assert.IsType<Categories>(categories);
        }

        // ========================================================================

        [Fact]
        public void CategoriesObject_New_CreatesDefaultCategories()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, true);

            // Assert 
            Assert.False(categories.List().Count == 0, "Non zero categories");

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_ReadFromDatabase_ValidateCorrectDataWasRead()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String existingDB = $"{folder}\\{TestConstants.testDBInputFile}";
            Database.existingDatabase(existingDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, false);
            List<Category> list = categories.List();
            Category firstCategory = list[0];

            // Assert
            Assert.Equal(numberOfCategoriesInFile, list.Count);
            Assert.Equal(firstCategoryInFile.Id, firstCategory.Id);
            Assert.Equal(firstCategoryInFile.Description, firstCategory.Description);

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_List_ReturnsListOfCategories()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFile}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);

            // Act
            List<Category> list = categories.List();

            // Assert
            Assert.Equal(numberOfCategoriesInFile, list.Count);

        }


        // ========================================================================

        [Fact]
        public void CategoriesMethod_Add()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);
            string descr = "New Category";
            Category.CategoryType type = Category.CategoryType.Income;

            // Act
            categories.Add(descr, type);
            List<Category> categoriesList = categories.List();
            int sizeOfList = categories.List().Count;

            // Assert
            Assert.Equal(numberOfCategoriesInFile + 1, sizeOfList);
            Assert.Equal(descr, categoriesList[sizeOfList - 1].Description);

        }

        // ========================================================================

        [Fact]
        public void Add_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Categories categories = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => categories.Add("", Category.CategoryType.Income));

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        [Theory]
        [ClassData(typeof(EquivalentDescriptions))]
        public void Add_FailsWhenDescriptionIsTaken(string originalDesc, string incomingDesc) {
            // Arrange
            CreateAndConnectToNewDb();
            Categories categories = new(Database.dbConnection!, newDb: false);
            categories.Add(originalDesc, CategoryType.Expense);

            // Act
            var exception = Record.Exception(
                () => categories.Add(incomingDesc, CategoryType.Expense));

            // Assert
            _ = Assert.IsAssignableFrom<ArgumentException>(exception);
            Assert.Matches(
                new Regex("description.+already taken", RegexOptions.IgnoreCase),
                exception.Message);
        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_Delete()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);
            int IdToDelete = 3;

            // Act
            categories.Delete(IdToDelete);
            List<Category> categoriesList = categories.List();
            int sizeOfList = categoriesList.Count;

            // Assert
            Assert.Equal(numberOfCategoriesInFile - 1, sizeOfList);
            Assert.False(categoriesList.Exists(e => e.Id == IdToDelete), "correct Category item deleted");

        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_Delete_InvalidIDDoesntCrash()
        {
            // Arrange
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messyDB";
            System.IO.File.Copy(goodDB, messyDB, true);
            Database.existingDatabase(messyDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);
            int IdToDelete = 9999;
            int sizeOfList = categories.List().Count;

            // Act
            try
            {
                categories.Delete(IdToDelete);
                Assert.Equal(sizeOfList, categories.List().Count);
            }

            // Assert
            catch
            {
                Assert.True(false, "Invalid ID causes Delete to break");
            }
        }

        // ========================================================================

        [Fact]
        public void Delete_FailsWhenReferencedByAnExpense() {
            // Arrange
            CopyAndConnectToMessyDb();
            Categories categories = new(Database.dbConnection!, newDb: false);

            // Act
            // Category #9 is referenced by some expenses
            Exception exception = Record.Exception( () => categories.Delete(9) );

            // Assert 
            _ = Assert.IsAssignableFrom<DbException>(exception);
            Assert.Matches(
                new Regex("foreign", RegexOptions.IgnoreCase),
                exception.Message);
        }

        // ========================================================================

        [Fact]
        public void Delete_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Categories categories = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => categories.Delete(1));

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_GetCategoryFromId()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\{TestConstants.testDBInputFile}";
            Database.existingDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, false);
            int catID = 15;

            // Act
            Category category = categories.GetCategoryFromId(catID);

            // Assert
            Assert.Equal(catID, category.Id);

        }

        // ========================================================================

        [Fact]
        public void GetCategoryFromId_FailsOnBadId() {
            // Arrange
            CopyAndConnectToMessyDb();
            Categories categories = new(Database.dbConnection!, newDb: false);

            // Act
            Exception exception = Record.Exception(
                () => categories.GetCategoryFromId(1001));

            // Assert
            _ = Assert.IsAssignableFrom<Exception>(exception);
            Assert.Matches(
                new Regex("cannot find", RegexOptions.IgnoreCase),
                exception.Message);
        }

        // ========================================================================

        [Fact]
        public void GetCategoryFromId_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Categories categories = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => categories.GetCategoryFromId(1));

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_SetCategoriesToDefaults()
        {

            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;

            // Act
            Categories categories = new Categories(conn, true);
            List<Category> originalList = categories.List();

            // modify list of categories
            categories.Delete(1);
            categories.Delete(2);
            categories.Delete(3);
            categories.Add("Another one ", Category.CategoryType.Credit);

            //"just double check that initial conditions are correct");
            Assert.NotEqual(originalList.Count, categories.List().Count);

            // Act
            categories.SetCategoriesToDefaults();

            // Assert
            Assert.Equal(originalList.Count, categories.List().Count);
            foreach (Category defaultCat in originalList)
            {
                Assert.True(categories.List().Exists(c => c.Description == defaultCat.Description && c.Type == defaultCat.Type));
            }

        }

        // ========================================================================

        [Fact]
        public void SetCategoriesToDefaults_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Categories categories = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => categories.SetCategoriesToDefaults());

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        // ========================================================================

        [Fact]
        public void CategoriesMethod_UpdateCategory()
        {
            // Arrange
            String folder = TestConstants.GetSolutionDir();
            String newDB = $"{folder}\\newDB.db";
            Database.newDatabase(newDB);
            SQLiteConnection conn = Database.dbConnection;
            Categories categories = new Categories(conn, true);
            String newDescr = "Presents";
            int id = 11;

            // Act
            categories.UpdateProperties(id, newDescr, Category.CategoryType.Income);
            Category category = categories.GetCategoryFromId(id);

            // Assert 
            Assert.Equal(newDescr, category.Description);
            Assert.Equal(Category.CategoryType.Income, category.Type);

        }

        // ========================================================================

        [Fact]
        public void UpdateProperties_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Categories categories = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => categories.UpdateProperties(1, "", Category.CategoryType.Income));

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        [Theory]
        [ClassData(typeof(EquivalentDescriptions))]
        public void UpdateProperties_FailsWhenDescriptionIsTaken(string originalDesc, string incomingDesc) {
            // Arrange
            CreateAndConnectToNewDb();
            Categories categories = new(Database.dbConnection!, newDb: false);
            categories.Add(originalDesc, CategoryType.Expense);
            categories.Add("PLACEHOLDER", CategoryType.Expense);

            // Act
            var exception = Record.Exception(
                () => categories.UpdateProperties(2, incomingDesc, CategoryType.Expense));

            // Assert
            _ = Assert.IsAssignableFrom<ArgumentException>(exception);
            Assert.Matches(
                new Regex("description.+already taken", RegexOptions.IgnoreCase),
                exception.Message);
        }

        [Fact]
        public void UpdateProperties_AllowsUnchangedDescription() {
            // Arrange
            string desc = "Hi";
            CreateAndConnectToNewDb();
            Categories categories = new(Database.dbConnection!, newDb: false);
            categories.Add("Hi", CategoryType.Expense);

            // Act
            // Should not throw:
            categories.UpdateProperties(1, desc, CategoryType.Credit);

            // Assert
        }
    }
}

