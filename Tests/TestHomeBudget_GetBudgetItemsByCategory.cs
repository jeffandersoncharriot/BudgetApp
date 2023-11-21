using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;
using System.Text.RegularExpressions;
using static BudgetCodeTests.TestConstants;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestHomeBudget_GetBudgetItemsByCategory
    {
        // ========================================================================
        // Get Expenses By Month Method tests
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByCategory_NoStartEnd_NoFilter()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);
            int maxRecords = TestConstants.budgetItemsByCategory_MaxRecords;
            BudgetItemsByCategory firstRecord = TestConstants.budgetItemsByCategory_FirstRecord;

            // Act
            List<BudgetItemsByCategory> budgetItemsByCategory = homeBudget.GetBudgetItemsByCategory(null, null, false, 9);
            BudgetItemsByCategory firstRecordTest = budgetItemsByCategory[0];

            // Assert
            Assert.Equal(maxRecords, budgetItemsByCategory.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Category, firstRecordTest.Category);
            Assert.Equal(firstRecord.Total, firstRecordTest.Total);
            Assert.Equal(firstRecord.Details.Count, firstRecordTest.Details.Count);
            for (int record = 0; record < firstRecord.Details.Count; record++)
            {
                BudgetItem validItem = firstRecord.Details[record];
                BudgetItem testItem = firstRecordTest.Details[record];
                Assert.Equal(validItem.Amount, testItem.Amount);
                Assert.Equal(validItem.CategoryID, testItem.CategoryID);
                Assert.Equal(validItem.ExpenseID, testItem.ExpenseID);

            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByCategory_NoStartEnd_FilterbyCategory()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);
            int maxRecords14 = TestConstants.budgetItemsByCategory14;
            int maxRecords20 = TestConstants.budgetItemsByCategory20;

            // Act
            List<BudgetItemsByMonth> budgetItemsByCategory = homeBudget.GetBudgetItemsByMonth(null, null, true, 14);

            // Assert
            Assert.Equal(maxRecords14, budgetItemsByCategory.Count);


            // Act
            budgetItemsByCategory = homeBudget.GetBudgetItemsByMonth(null, null, true, 20);

            // Assert
            Assert.Equal(maxRecords20, budgetItemsByCategory.Count);

        }
        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByCategory_2018_filterDateAndCat9()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);
            List<BudgetItemsByCategory> validBudgetItemsByCategory = TestConstants.getBudgetItemsByCategory2018_Cat9();
            BudgetItemsByCategory firstRecord = validBudgetItemsByCategory[0];

            // Act
            List<BudgetItemsByCategory> budgetItemsByCategory = homeBudget.GetBudgetItemsByCategory(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31), true, 9);
            BudgetItemsByCategory firstRecordTest = budgetItemsByCategory[0];

            // Assert
            Assert.Equal(validBudgetItemsByCategory.Count, budgetItemsByCategory.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Category, firstRecordTest.Category);
            Assert.Equal(firstRecord.Total, firstRecordTest.Total);
            Assert.Equal(firstRecord.Details.Count, firstRecordTest.Details.Count);
            for (int record = 0; record < firstRecord.Details.Count; record++)
            {
                BudgetItem validItem = firstRecord.Details[record];
                BudgetItem testItem = firstRecordTest.Details[record];
                Assert.Equal(validItem.Amount, testItem.Amount);
                Assert.Equal(validItem.CategoryID, testItem.CategoryID);
                Assert.Equal(validItem.ExpenseID, testItem.ExpenseID);

            }
        }


        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetItemsByCategory_2018_filterDate()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);
            List<BudgetItemsByCategory> validBudgetItemsByCategory = TestConstants.getBudgetItemsByCategory2018();
            BudgetItemsByCategory firstRecord = validBudgetItemsByCategory[0];


            // Act
            // Modified: Made the dates tighter to test inclusive endpoints
            List<BudgetItemsByCategory> budgetItemsByCategory = homeBudget.GetBudgetItemsByCategory(new DateTime(2018, 1, 10), new DateTime(2018, 1, 11), false, 9);
            BudgetItemsByCategory firstRecordTest = budgetItemsByCategory[0];

            // Assert
            Assert.Equal(validBudgetItemsByCategory.Count, budgetItemsByCategory.Count);

            // verify 1st record
            Assert.Equal(firstRecord.Category, firstRecordTest.Category);
            Assert.Equal(firstRecord.Total, firstRecordTest.Total);
            Assert.Equal(firstRecord.Details.Count, firstRecordTest.Details.Count);
            for (int record = 0; record < firstRecord.Details.Count; record++)
            {
                BudgetItem validItem = firstRecord.Details[record];
                BudgetItem testItem = firstRecordTest.Details[record];
                Assert.Equal(validItem.Amount, testItem.Amount);
                Assert.Equal(validItem.CategoryID, testItem.CategoryID);
                Assert.Equal(validItem.ExpenseID, testItem.ExpenseID);

            }
        }

        // ========================================================================

        [Fact]
        public void GetBudgetItemsByCategory_FailsOnClosedConnection()
        {
            // Arrange
            HomeBudget hb = new(newDbPath);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => hb.GetBudgetItemsByCategory(null, null, false, 0));

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }
    }
}

