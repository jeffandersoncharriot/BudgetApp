using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using Budget;
using System.Dynamic;
using System.Text.RegularExpressions;
using static BudgetCodeTests.TestConstants;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestHomeBudget_GetBudgetDictionaryByCategoryAndMonth
    {
         //========================================================================
         //Get Expenses By Month Method tests
         //========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_NoStartEnd_NoFilter_VerifyNumberOfRecords()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);

            int maxRecords = TestConstants.budgetItemsByCategoryAndMonth_MaxRecords;
            Dictionary<string, object> firstRecord = TestConstants.getBudgetItemsByCategoryAndMonthFirstRecord();

            // Act
            List<Dictionary<string, object>> budgetItemsByCategoryAndMonth = homeBudget.GetBudgetDictionaryByCategoryAndMonth(null, null, false, 9);

            // Assert
            Assert.Equal(maxRecords + 1, budgetItemsByCategoryAndMonth.Count);

        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_NoStartEnd_NoFilter_VerifyFirstRecord()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);

            int maxRecords = TestConstants.budgetItemsByCategoryAndMonth_MaxRecords;
            Dictionary<string, object> firstRecord = TestConstants.getBudgetItemsByCategoryAndMonthFirstRecord();

            // Act
            List<Dictionary<string, object>> budgetItemsByCategoryAndMonth = homeBudget.GetBudgetDictionaryByCategoryAndMonth(null, null, false, 9);
            Dictionary<string, object> firstRecordTest = budgetItemsByCategoryAndMonth[0];

            // Assert
            Assert.True(AssertDictionaryForExpenseByCategoryAndMonthIsOK(firstRecord, firstRecordTest));

        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_NoStartEnd_NoFilter_VerifyTotalsRecord()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);

            int maxRecords = TestConstants.budgetItemsByCategoryAndMonth_MaxRecords;
            Dictionary<string, object> totalsRecord = TestConstants.getBudgetItemsByCategoryAndMonthTotalsRecord();

            // Act
            List<Dictionary<string, object>> budgetItemsByCategoryAndMonth = homeBudget.GetBudgetDictionaryByCategoryAndMonth(null, null, false, 9);
            Dictionary<string, object> totalsRecordTest = budgetItemsByCategoryAndMonth[budgetItemsByCategoryAndMonth.Count - 1];

            // Assert
            // ... loop over all key/value pairs 
            Assert.True(AssertDictionaryForExpenseByCategoryAndMonthIsOK(totalsRecord, totalsRecordTest), "Totals Record is Valid");

        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_NoStartEnd_FilterbyCategory()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);
            List<Dictionary<string, object>> expectedResults = TestConstants.getBudgetItemsByCategoryAndMonthCat10();

            // Act
            List<Dictionary<string, object>> gotResults = homeBudget.GetBudgetDictionaryByCategoryAndMonth(null, null, true, 10);

            // Assert
            Assert.Equal(expectedResults.Count, gotResults.Count);
            for (int record = 0; record < expectedResults.Count; record++)
            {
                Assert.True(AssertDictionaryForExpenseByCategoryAndMonthIsOK(expectedResults[record],
                    gotResults[record]), "Record:" + record + " is Valid");

            }
        }

        // ========================================================================

        [Fact]
        public void HomeBudgetMethod_GetBudgetDictionaryByCategoryAndMonth_2020()
        {
            // Arrange
            string folder = TestConstants.GetSolutionDir();
            String goodDB = $"{folder}\\{TestConstants.testDBInputFile}";
            String messyDB = $"{folder}\\messy.db";
            System.IO.File.Copy(goodDB, messyDB, true);
            HomeBudget homeBudget = new HomeBudget(messyDB, false);
            List<Dictionary<string, object>> expectedResults = TestConstants.getBudgetItemsByCategoryAndMonth2020();

            // Act
            // Modified: Made the dates tighter to test inclusive endpoints
            List<Dictionary<string, object>> gotResults = homeBudget.GetBudgetDictionaryByCategoryAndMonth(new DateTime(2020, 1, 10), new DateTime(2020, 1, 12), false, 10);

            // Assert
            Assert.Equal(expectedResults.Count, gotResults.Count);
            for (int record = 0; record < expectedResults.Count; record++)
            {
                Assert.True(AssertDictionaryForExpenseByCategoryAndMonthIsOK(expectedResults[record],
                    gotResults[record]), "Record:" + record + " is Valid");

            }
        }

        // ========================================================================

        [Fact]
        public void GetBudgetDictionaryByCategoryAndMonth_FailsOnClosedConnection()
        {
            // Arrange
            HomeBudget hb = new(newDbPath);
            Database.CloseDatabaseAndReleaseFile();

            // Act
            Exception exception = Record.Exception(
                () => hb.GetBudgetDictionaryByCategoryAndMonth(null,null,false,0));

            // Assert
            _ = Assert.IsAssignableFrom<InvalidOperationException>(exception);
            Assert.Matches(
                new Regex("not open", RegexOptions.IgnoreCase),
                exception.Message);
        }

         //========================================================================

         //-------------------------------------------------------
         //helpful functions, ... they are not tests
         //-------------------------------------------------------
        Boolean AssertDictionaryForExpenseByCategoryAndMonthIsOK(Dictionary<string, object> recordExpeted, Dictionary<string, object> recordGot)
        {
            try
            {
                foreach (var kvp in recordExpeted)
                {
                    String key = kvp.Key as String;
                    Object recordExpectedValue = kvp.Value;
                    Object recordGotValue = recordGot[key];


                    // ... validate the budget items
                    if (recordExpectedValue != null && recordExpectedValue.GetType() == typeof(List<BudgetItem>))
                    {
                        List<BudgetItem> expectedItems = recordExpectedValue as List<BudgetItem>;
                        List<BudgetItem> gotItems = recordGotValue as List<BudgetItem>;
                        for (int budgetItemNumber = 0; budgetItemNumber < expectedItems.Count; budgetItemNumber++)
                        {
                            Assert.Equal(expectedItems[budgetItemNumber].Amount, gotItems[budgetItemNumber].Amount);
                            Assert.Equal(expectedItems[budgetItemNumber].CategoryID, gotItems[budgetItemNumber].CategoryID);
                            Assert.Equal(expectedItems[budgetItemNumber].ExpenseID, gotItems[budgetItemNumber].ExpenseID);
                        }
                    }

                    // else ... validate the value for the specified key
                    else
                    {
                        Assert.Equal(recordExpectedValue, recordGotValue);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}

