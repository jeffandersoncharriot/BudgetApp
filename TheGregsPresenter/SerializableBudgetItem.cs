using Budget;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TheGregsPresenter;

/// <summary>
/// Used for (de)serializing budget items as JSON.
/// See <see cref="Presenter.SerializeBudgetItems(IEnumerable{BudgetItem})"/>
/// and <see cref="Presenter.AddSerializedBudgetItems(string)"/>.
/// </summary>
internal record SerializableBudgetItem(
		double? Amount,
		DateTime? Date,
		string? ExpenseDescription,
		string? CategoryDescription,
		Category.CategoryType? CategoryType
	) {

	/// <summary>
	/// True if all required properties are present in this object.
	/// </summary>
	/// <remarks>
	/// Necessary because JSON before .NET 7 will ignore missing properties.
	/// </remarks>
	[JsonIgnore]
	[MemberNotNullWhen(true, new[] {
		nameof(Amount),
		nameof(Date),
		nameof(ExpenseDescription),
		nameof(CategoryDescription),
		nameof(CategoryType)})]
	public bool IsValid => Amount.HasValue
		&& Date.HasValue
		&& ExpenseDescription is not null
		&& CategoryDescription is not null
		&& CategoryType.HasValue;

	public override string ToString() {
		string expDesc = ExpenseDescription is not null
			? $"\"{ExpenseDescription}\""
			: "Expense with a missing description";

		var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
		string date = Date.HasValue
			? Date.Value.ToString(dateFormat)
			: "missing date";

		string amount = Amount is not null
			? $"amount {Amount}"
			: "missing amount";

		string catDesc = CategoryDescription is not null
			? $"category \"{CategoryDescription}\""
			: "missing category";

		string type = CategoryType.HasValue
			? $"{CategoryType}"
			: "missing category type";

		return $"{expDesc} on {date} with {amount} in {catDesc} ({type})";
	}
}
