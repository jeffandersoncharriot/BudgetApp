using Budget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static BudgetCodeTests.TestConstants;

namespace BudgetCodeTests;

[Collection("Sequential")]
public class TestBudgetItems {
	[Fact]
	public void BudgetItemsByCategory_Equals_True() {
		// Arrange
		CopyMessyDb();
		HomeBudget budget = new(messyDbPath);
		var first = budget.GetBudgetItemsByCategory(null, null, false, 0);

		// Act
		var second = budget.GetBudgetItemsByCategory(null, null, false, 0);

		// Assert
		Assert.Equal(first, second);
	}

	[Fact]
	public void BudgetItemsByCategory_Equals_False() {
		// Arrange
		CopyMessyDb();
		HomeBudget budget = new(messyDbPath);
		var first = budget.GetBudgetItemsByCategory(null, null, false, 0);

		budget.expenses.Add(new(2020, 1, 1), 9, 15, "TestExpense");

		// Act
		var second = budget.GetBudgetItemsByCategory(null, null, false, 0);

		// Assert
		Assert.NotEqual(first, second);
	}

	[Fact]
	public void BudgetItemsByMonth_Equals_True() {
		// Arrange
		CopyMessyDb();
		HomeBudget budget = new(messyDbPath);
		var first = budget.GetBudgetItemsByMonth(null, null, false, 0);

		// Act
		var second = budget.GetBudgetItemsByMonth(null, null, false, 0);

		// Assert
		Assert.Equal(first, second);
	}

	[Fact]
	public void BudgetItemsByMonth_Equals_False() {
		// Arrange
		CopyMessyDb();
		HomeBudget budget = new(messyDbPath);
		var first = budget.GetBudgetItemsByMonth(null, null, false, 0);

		budget.expenses.Add(new(2020, 1, 1), 9, 15, "TestExpense");

		// Act
		var second = budget.GetBudgetItemsByMonth(null, null, false, 0);

		// Assert
		Assert.NotEqual(first, second);
	}
}
