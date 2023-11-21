using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;
using static BudgetCodeTests.TestConstants;

namespace BudgetCodeTests {
    [Collection("Sequential")]
    public class TestHomeBudget {

        public TestHomeBudget() {
            // Run before every test
            Database.CloseDatabaseAndReleaseFile();
        }

        // ========================================================================

        [Fact]
        public void Properties_AreReadonly() {
            Assert.False( typeof(HomeBudget).GetProperty(nameof(HomeBudget.FileName  ))!.CanWrite );
            Assert.False( typeof(HomeBudget).GetProperty(nameof(HomeBudget.DirName   ))!.CanWrite );
            Assert.False( typeof(HomeBudget).GetProperty(nameof(HomeBudget.PathName  ))!.CanWrite );
            Assert.False( typeof(HomeBudget).GetProperty(nameof(HomeBudget.categories))!.CanWrite );
            Assert.False( typeof(HomeBudget).GetProperty(nameof(HomeBudget.expenses  ))!.CanWrite );
        }

        // ========================================================================

        #region Constructor opens connections

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void New_NewFileOpensConnection(bool newDb) {
            // Arrange
            File.Delete(newDbPath);

            // Act
            _ = new HomeBudget(newDbPath, newDb);

            // Assert
            Assert.NotNull(Database.dbConnection);
            Assert.Equal(newDbPath, Database.dbConnection!.FileName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void New_ExistingFileOpensConnection(bool newDb) {
            // Arrange
            CopyMessyDb();

            // Act
            _ = new HomeBudget(messyDbPath, newDb);

            // Assert
            Assert.NotNull(Database.dbConnection);
            Assert.Equal(messyDbPath, Database.dbConnection!.FileName);
        }

        #endregion

        // ========================================================================

        #region Constructor sets path properties

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void New_NewFileSetsPathProperties(bool newDb) {
            // Arrange
            File.Delete(newDbPath);

            // Act
            HomeBudget hb = new(newDbPath, newDb);

            // Assert
            Assert.Equal(Path.GetFileName(newDbPath), hb.FileName);
            Assert.Equal(Path.GetDirectoryName(newDbPath), hb.DirName);
            Assert.Equal(Path.GetFullPath(newDbPath), hb.PathName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void New_ExistingFileSetsPathProperties(bool newDb) {
            // Arrange
            CopyMessyDb();

            // Act
            HomeBudget hb = new(messyDbPath, newDb);

            // Assert
            Assert.Equal(Path.GetFileName(messyDbPath), hb.FileName);
            Assert.Equal(Path.GetDirectoryName(messyDbPath), hb.DirName);
            Assert.Equal(Path.GetFullPath(messyDbPath), hb.PathName);
        }

        #endregion

        // ========================================================================

        #region Constructor newDB parameter

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void New_NewFileHasDefaultCategories(bool newDb) {
            // Arrange
            File.Delete(newDbPath);

            // Act
            HomeBudget hb = new(newDbPath, newDb);

            // Assert
            Assert.Equal(defaultCategories, hb.categories.List(), categoryComparer);
        }

        [Fact]
        public void New_NewDbClearsExistingExpenses() {
            // Arrange
            CopyMessyDb();

            // Act
            _ = new HomeBudget(messyDbPath, newDB: true);

            // Assert
            using var cmd = Database.dbConnection!.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM expenses";
            Assert.Equal(0L, cmd.ExecuteScalar());
        }

        [Fact]
        public void New_NewDbResetsExistingCategoriesToDefaults() {
            // Arrange
            CopyMessyDb();

            // Act
            HomeBudget hb = new(messyDbPath, newDB: true);

            // Assert
            Assert.Equal(defaultCategories, hb.categories.List(), categoryComparer);
        }

        [Fact]
        public void New_UsesExistingData() {
            // Arrange
            CopyMessyDb();

            // Act
            _ = new HomeBudget(messyDbPath, newDB: false);

            // Assert
            // Ensure the original data is still present
            // (without also testing Expenses and Categories)
            using var cmd = Database.dbConnection!.CreateCommand();

            cmd.CommandText = "SELECT COUNT(*) FROM expenses";
            Assert.Equal((long)numberOfExpensesInFile, cmd.ExecuteScalar());
            cmd.CommandText = "SELECT COUNT(*) FROM categories";
            Assert.Equal((long)numberOfCategoriesInFile, cmd.ExecuteScalar());

            cmd.CommandText = "SELECT Description FROM expenses ORDER BY Id";
            Assert.Equal(cmd.ExecuteScalar(), firstExpenseInFile.Description);
            cmd.CommandText = "SELECT Description FROM categories ORDER BY Id";
            Assert.Equal(cmd.ExecuteScalar(), firstCategoryInFile.Description);
        }

        #endregion

        // ========================================================================

        #region Constructor fail cases

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void New_FailsWithFolder(bool newDb) {
            // Arrange
            _ = Directory.CreateDirectory(testFolderPath);

            // Act
            Exception? exception = Record.Exception(() => new HomeBudget(testFolderPath, newDb));

            // Assert
            Assert.NotNull(exception);
        }

        #endregion
    }
}

