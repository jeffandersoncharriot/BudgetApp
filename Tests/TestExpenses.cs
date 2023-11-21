using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using Budget;
using static BudgetCodeTests.TestConstants;
using System.Text.RegularExpressions;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestExpenses
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void New_NewDatabaseIsEmpty(bool newDb) {
            // Arrange
            CreateAndConnectToNewDb();

            // Act
            _ = new Expenses(Database.dbConnection!, newDb);

            // Assert
            using var cmd = Database.dbConnection!.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM expenses";
            Assert.Equal(0L, cmd.ExecuteScalar());
        }

        // ========================================================================

        [Fact]
        public void New_UsesExistingData() {
            // Arrange
            CopyAndConnectToMessyDb();
            
            // Act
            _ = new Expenses(Database.dbConnection!, newDb: false);
            
            // Assert
            using var cmd = Database.dbConnection!.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM expenses";
            // expenses table should still have its existing data:
            Assert.Equal((long)numberOfExpensesInFile, cmd.ExecuteScalar());
        }

        // ========================================================================

        [Fact]
        public void New_NewDbClearsExistingData() {
            // Arrange
            CopyAndConnectToMessyDb();
            
            // Act
            _ = new Expenses(Database.dbConnection!, newDb: true);
            
            // Assert 
            using var cmd = Database.dbConnection!.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM expenses";
            // expenses table should be empty:
            Assert.Equal(0L, cmd.ExecuteScalar());
        }

        // ========================================================================

        #region UpdateProperties

        [Theory]
        [InlineData(-15, 3 /*Food; Type: Expense (Negative)*/)]
        [InlineData(-30, 15 /*Savings; Type: Savings (Negative)*/)]
        [InlineData(30, 9 /*Credit Card; Type: Credit (Positive)*/)]
        [InlineData(15, 16 /*Income; Type: Income (Positive)*/)]
        [InlineData(0, 3 /*Food; Type: Expense (Negative)*/)]
        [InlineData(0, 15 /*Savings; Type: Savings (Negative)*/)]
        [InlineData(0, 9 /*Credit Card; Type: Credit (Positive)*/)]
        [InlineData(0, 16 /*Income; Type: Income (Positive)*/)]
        public void UpdateProperties(double newAmount, int newCatId) {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new(Database.dbConnection!, newDb: false);

            int id = 3;
            DateTime newDate = new(2022, 12, 12);
            string newDescr = "Falafel";

            // Act
            expenses.UpdateProperties(id, newDate, newCatId, newAmount, newDescr);

            // Assert 
            using var cmd = Database.dbConnection!.CreateCommand();

            cmd.CommandText =
                "SELECT Date, CategoryId, Amount, Description"
                + " FROM expenses"
                + $" WHERE id = {id}";

            var reader = cmd.ExecuteReader();
            _ = reader.Read();

            Assert.Equal(newDate.ToString("yyyy-MM-dd"), reader.GetString(0));
            Assert.Equal(newCatId, reader.GetInt32(1));
            Assert.Equal(newAmount, reader.GetDouble(2));
            Assert.Equal(newDescr, reader.GetString(3));
        }

        [Theory]
        [InlineData(15, 3 /*Food; Type: Expense (Negative)*/)]
        [InlineData(30, 15 /*Savings; Type: Savings (Negative)*/)]
        public void UpdateProperties_FailsOnPositiveExpenseInNegativeCategory(double amount, int category) {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            
            // Act
            Exception exception = Record.Exception(
                () => expenses.UpdateProperties(3, DateTime.Now, category, amount, "Test"));

            // Assert

            // Ensure correct exception
            _ = Assert.IsAssignableFrom<ArgumentException>(exception);
            Assert.Matches(
                new Regex("positive expense .* negative categor", RegexOptions.IgnoreCase),
                exception.Message);

            // Ensure nothing has changed
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expensesList.Count;
            Assert.Equal(numberOfExpensesInFile, sizeOfList);
            Assert.Equal(maxIDInExpenseFile, expensesList[sizeOfList - 1].Id);
        }

        [Theory]
        [InlineData(-30, 9 /*Credit Card; Type: Credit (Positive)*/)]
        [InlineData(-15, 16 /*Income; Type: Income (Positive)*/)]
        public void UpdateProperties_FailsOnNegativeExpenseInPositiveCategory(double amount, int category) {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            
            // Act
            Exception exception = Record.Exception(
                () => expenses.UpdateProperties(3, DateTime.Now, category, amount, "Test"));

            // Assert

            // Ensure correct exception
            _ = Assert.IsAssignableFrom<ArgumentException>(exception);
            Assert.Matches(
                new Regex("negative expense .* positive categor", RegexOptions.IgnoreCase),
                exception.Message);

            // Ensure nothing has changed
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expensesList.Count;
            Assert.Equal(numberOfExpensesInFile, sizeOfList);
            Assert.Equal(maxIDInExpenseFile, expensesList[sizeOfList - 1].Id);
        }

        [Fact]
        public void UpdateProperties_FailsOnBadForeignKey() {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new(Database.dbConnection!, newDb: false);

            // Act
            // -1 is an invalid Category Id:
            Exception exception = Record.Exception(
                () => expenses.UpdateProperties(1, DateTime.Now, -1, 0, ""));

            // Assert 
            _ = Assert.IsAssignableFrom<ArgumentException>(exception);
            Assert.Matches(
                new Regex("category does not exist", RegexOptions.IgnoreCase),
                exception.Message);
        }

        [Fact]
        public void UpdateProperties_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => expenses.UpdateProperties(1, DateTime.Now, 1, 0, ""));

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        #endregion

        // ========================================================================

        #region Add

        [Theory]
        [InlineData(-15, 3 /*Food; Type: Expense (Negative)*/)]
        [InlineData(-30, 15 /*Savings; Type: Savings (Negative)*/)]
        [InlineData(30, 9 /*Credit Card; Type: Credit (Positive)*/)]
        [InlineData(15, 16 /*Income; Type: Income (Positive)*/)]
        [InlineData(0, 3 /*Food; Type: Expense (Negative)*/)]
        [InlineData(0, 15 /*Savings; Type: Savings (Negative)*/)]
        [InlineData(0, 9 /*Credit Card; Type: Credit (Positive)*/)]
        [InlineData(0, 16 /*Income; Type: Income (Positive)*/)]
        public void ExpensesMethod_Add(double amount, int category)
        {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            
            // Act
            string desc = "New Expense";
            DateTime date = new DateTime(1900, 2, 3);
            expenses.Add(date, category, amount, desc);
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expensesList.Count;
            
            // Assert
            Assert.Equal(numberOfExpensesInFile + 1, sizeOfList);
            Assert.Equal(maxIDInExpenseFile + 1, expensesList[sizeOfList - 1].Id);
            Assert.Equal(desc, expensesList[sizeOfList - 1].Description);
            Assert.Equal(amount, expensesList[sizeOfList - 1].Amount);
            Assert.Equal(category, expensesList[sizeOfList - 1].Category);
            Assert.Equal(date, expensesList[sizeOfList - 1].Date);

        }

        [Theory]
        [InlineData(15, 3 /*Food; Type: Expense (Negative)*/)]
        [InlineData(30, 15 /*Savings; Type: Savings (Negative)*/)]
        public void Add_FailsOnPositiveExpenseInNegativeCategory(double amount, int category) {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            
            // Act
            Exception exception = Record.Exception(
                () => expenses.Add(DateTime.Now, category, amount, "Test"));

            // Assert

            // Ensure correct exception
            _ = Assert.IsAssignableFrom<ArgumentException>(exception);
            Assert.Matches(
                new Regex("positive expense .* negative categor", RegexOptions.IgnoreCase),
                exception.Message);

            // Ensure nothing has changed
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expensesList.Count;
            Assert.Equal(numberOfExpensesInFile, sizeOfList);
            Assert.Equal(maxIDInExpenseFile, expensesList[sizeOfList - 1].Id);
        }

        [Theory]
        [InlineData(-30, 9 /*Credit Card; Type: Credit (Positive)*/)]
        [InlineData(-15, 16 /*Income; Type: Income (Positive)*/)]
        public void Add_FailsOnNegativeExpenseInPositiveCategory(double amount, int category) {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            
            // Act
            Exception exception = Record.Exception(
                () => expenses.Add(DateTime.Now, category, amount, "Test"));

            // Assert

            // Ensure correct exception
            _ = Assert.IsAssignableFrom<ArgumentException>(exception);
            Assert.Matches(
                new Regex("negative expense .* positive categor", RegexOptions.IgnoreCase),
                exception.Message);

            // Ensure nothing has changed
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expensesList.Count;
            Assert.Equal(numberOfExpensesInFile, sizeOfList);
            Assert.Equal(maxIDInExpenseFile, expensesList[sizeOfList - 1].Id);
        }

        [Fact]
        public void Add_FailsOnBadForeignKey() {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new(Database.dbConnection!, newDb: false);

            // Act
            // -1 is an invalid Category Id:
            Exception exception = Record.Exception(
                () => expenses.Add(DateTime.Now, -1, 0, ""));

            // Assert 
            _ = Assert.IsAssignableFrom<ArgumentException>(exception);
            Assert.Matches(
                new Regex("category does not exist", RegexOptions.IgnoreCase),
                exception.Message);
        }

        [Fact]
        public void Add_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => expenses.Add(DateTime.Now, 1, 0, ""));

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        #endregion

        // ========================================================================

        #region Delete

        [Fact]
        public void ExpensesMethod_Delete()
        {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            int idToDelete = 3;
            
            // Act
            expenses.Delete(idToDelete);
            List<Expense> expensesList = expenses.List();
            int sizeOfList = expensesList.Count;
            
            // Assert
            Assert.Equal(numberOfExpensesInFile - 1, sizeOfList);
            Assert.False(expensesList.Exists(e => e.Id == idToDelete), "correct expense item deleted");

        }

        [Fact]
        public void ExpensesMethod_Delete_InvalidIDDoesntCrash()
        {
            // Arrange
            CopyAndConnectToMessyDb();
            int IdToDelete = 1006;
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            int sizeOfList = expenses.List().Count;
            
            // Act
            try
            {
                expenses.Delete(IdToDelete);
                Assert.Equal(sizeOfList, expenses.List().Count);
            }

            // Assert
            catch
            {
                Assert.True(false, "Invalid ID causes Delete to break");
            }
        }

        [Fact]
        public void Delete_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception( () => expenses.Delete(3) );

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        #endregion

        // ========================================================================

        #region List

        [Fact]
        public void ExpensesMethod_List_ReturnsListOfExpenses()
        {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            
            // Act
            List<Expense> list = expenses.List();
            
            // Assert
            Assert.Equal(numberOfExpensesInFile, list.Count);

        }

        [Fact]
        public void ExpensesMethod_List_ValidateCorrectDataWasRead()
        {
            // Arrange
            CopyAndConnectToMessyDb();
            
            // Act
            Expenses expenses = new Expenses(Database.dbConnection!, false);
            List<Expense> list = expenses.List();
            Expense firstExpense = list[0];
            
            // Assert
            Assert.Equal(numberOfExpensesInFile, list.Count);
            Assert.Equal(firstExpenseInFile.Id, firstExpense.Id);
            Assert.Equal(firstExpenseInFile.Description, firstExpense.Description);
            Assert.Equal(firstExpenseInFile.Amount, firstExpense.Amount);
            Assert.Equal(firstExpenseInFile.Category, firstExpense.Category);
            Assert.Equal(firstExpenseInFile.Date, firstExpense.Date);

        }

        [Fact]
        public void List_FailsOnClosedConnection() {
            // Arrange
            CopyAndConnectToMessyDb();
            Expenses expenses = new(Database.dbConnection!, newDb: false);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception( () => expenses.List() );

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

        #endregion

    }
}
