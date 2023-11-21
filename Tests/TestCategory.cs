using System;
using Xunit;
using Budget;

namespace BudgetCodeTests
{
    [Collection("Sequential")]
    public class TestCategory
    {
        // ========================================================================

        [Fact]
        public void CategoryObject_New()
        {
            // Arrange
            string descr = "Clothing";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Credit;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.IsType<Category>(category);
            Assert.Equal(id, category.Id);
            Assert.Equal(descr, category.Description);
            Assert.Equal(type, category.Type);
        }

        [Fact]
        public void CategoryObject_PropertiesAreReadOnly()
        {
            // Arrange
            string descr = "Clothing";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Credit;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.IsType<Category>(category);
            Assert.True(typeof(Category).GetProperty("Id").CanWrite == false);
            Assert.True(typeof(Category).GetProperty("Description").CanWrite == false);
            Assert.True(typeof(Category).GetProperty("Type").CanWrite == false);
        }


        // ========================================================================

        [Fact]
        public void CategoryObject_New_WithDefaultType()
        {

            // Arrange
            string descr = "Clothing";
            int id = 42;
            Category.CategoryType defaultType = Category.CategoryType.Expense;

            // Act
            Category category = new Category(id, descr);

            // Assert 
            Assert.Equal(defaultType, category.Type);
        }

        // ========================================================================

        [Fact]
        public void CategoryObject_New_TypeIncome()
        {

            // Arrange
            string descr = "Work";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Income;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.Equal(type, category.Type);

        }

        // ========================================================================

        [Fact]
        public void CategoryObjectType_New_Expense()
        {

            // Arrange
            string descr = "Eating Out";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Expense;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.Equal(type, category.Type);

        }

        // ========================================================================

        [Fact]
        public void CategoryObject_New_TypeCredit()
        {

            // Arrange
            string descr = "MasterCard";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Credit;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.Equal(type, category.Type);

        }

        // ========================================================================

        [Fact]
        public void CategoryObject_New_TypeSavings()
        {

            // Arrange
            string descr = "For House";
            int id = 42;
            Category.CategoryType type = Category.CategoryType.Savings;

            // Act
            Category category = new Category(id, descr, type);

            // Assert 
            Assert.Equal(type, category.Type);

        }


        // ========================================================================

        [Fact]
        public void CategoryObject_ToString()
        {

            // Arrange
            string descr = "Eating Out";
            int id = 42;

            // Act
            Category category = new Category(id, descr);

            // Assert 
            Assert.Equal(descr, category.ToString());
        }

    }
}

