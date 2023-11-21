using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget;
using System.IO;
using System.Data.Common;


namespace BudgetCodeTests
{
    public class TestConstants
    {

        private static Expense expense1 = new Expense(1, new DateTime(2018, 1, 10), 10, 12, "hat (on credit)");
        private static BudgetItem budgetItem1 = new BudgetItem
        {
            CategoryID = expense1.Category,
            ExpenseID = expense1.Id,
            Amount = expense1.Amount
        };

        private static Expense expense2 = new Expense(2, new DateTime(2018, 1, 11), 9, -10, "hat (on credit)");
        private static BudgetItem budgetItem2 = new BudgetItem
        {
            CategoryID = expense2.Category,
            ExpenseID = expense2.Id,
            Amount = expense2.Amount
        };


        private static BudgetItem budgetItem3 = new BudgetItem
        {
            CategoryID = 10,
            ExpenseID = 3,
            Amount = 15
        };

        private static Expense expense4 = new Expense(4, new DateTime(2020, 1, 10), 9, -15, "scarf (on credit)");
        private static BudgetItem budgetItem4 = new BudgetItem
        {
            CategoryID = expense4.Category,
            ExpenseID = expense4.Id,
            Amount = expense4.Amount
        };


        private static Expense expense5 = new Expense(5, new DateTime(2020, 1, 11), 14, 45, "McDonalds");
        private static BudgetItem budgetItem5 = new BudgetItem
        {
            CategoryID = expense5.Category,
            ExpenseID = expense5.Id,
            Amount = expense5.Amount
        };

        private static Expense expense7 = new Expense(7, new DateTime(2020, 1, 12), 14, 25, "Wendys");
        private static BudgetItem budgetItem7 = new BudgetItem
        {
            CategoryID = expense7.Category,
            ExpenseID = expense7.Id,
            Amount = expense7.Amount
        };


        public static int numberOfCategoriesInFile = 17;
        public static String testDBInputFile = "testDBInput.db";
        public static int maxIDInCategoryInFile = 17;
        public static Category firstCategoryInFile = new Category(1, "Utilities", Category.CategoryType.Expense);
        public static int CategoryIDWithSaveType = 15;

        public static readonly Category[] defaultCategories = {
            new(1, "Utilities", Category.CategoryType.Expense),
            new(2, "Rent", Category.CategoryType.Expense),
            new(3, "Food", Category.CategoryType.Expense),
            new(4, "Entertainment", Category.CategoryType.Expense),
            new(5, "Education", Category.CategoryType.Expense),
            new(6, "Miscellaneous", Category.CategoryType.Expense),
            new(7, "Medical Expenses", Category.CategoryType.Expense),
            new(8, "Vacation", Category.CategoryType.Expense),
            new(9, "Credit Card", Category.CategoryType.Credit),
            new(10, "Clothes", Category.CategoryType.Expense),
            new(11, "Gifts", Category.CategoryType.Expense),
            new(12, "Insurance", Category.CategoryType.Expense),
            new(13, "Transportation", Category.CategoryType.Expense),
            new(14, "Eating Out", Category.CategoryType.Expense),
            new(15, "Savings", Category.CategoryType.Savings),
            new(16, "Income", Category.CategoryType.Income)
        };

        public class CategoryComparer : IEqualityComparer<Category> {
            // Checking equality of two Category objects is useful for
            // assertions
            public bool Equals(Category? x, Category? y) {
                if (x is null || y is null) return false;

                return x.Id == y.Id
                    && x.Description == y.Description
                    && x.Type == y.Type;
            }

            public int GetHashCode(Category o) {
                throw new NotImplementedException();
            }
        }

        public static readonly CategoryComparer categoryComparer = new();

        public static int numberOfExpensesInFile = 6;
        public static int maxIDInExpenseFile = 7;
        public static Expense firstExpenseInFile { get { return expense1; } }

        public static string newDbPath => $"{GetSolutionDir()}newDB.db";

        public static string messyDbPath => $"{GetSolutionDir()}messy.db";

        public static string testFolderPath => $"{GetSolutionDir()}testFolder";

        public static List<Expense> filteredbyCat14()
        {
            List<Expense> filtered = new List<Expense>();
            filtered.Add(expense5);
            return filtered;
        }
        public static double filteredbyCat9Total = expense2.Amount + expense4.Amount;
        public static List<Expense> filteredbyCat9()
        {
            List<Expense> filtered = new List<Expense>();
            filtered.Add(expense2);
            filtered.Add(expense4);
            return filtered;
        }
        public static List<Expense> filteredbyYear2018AndCategory10()
        {
            List<Expense> filtered = new List<Expense>();
            filtered.Add(expense1);
            return filtered;
        }

        public static List<Expense> filteredbyYear2018()
        {
            List<Expense> filtered = new List<Expense>();
            filtered.Add(expense1);
            filtered.Add(expense2);
            return filtered;
        }


        // LIST EXPENSES BY MONTH
        public static int budgetItemsByMonth_MaxRecords = 3;
        public static BudgetItemsByMonth budgetItemsByMonth_FirstRecord = getBudgetItemsBy2018_01()[0];
        public static int budgetItemsByMonth_FilteredByCat9_number = 2;
        public static BudgetItemsByMonth budgetItemsByMonth_FirstRecord_FilteredCat9 = getBudgetItemsBy2018_01_filteredByCat9()[0];
        public static int budgetItemsByMonth_2018_FilteredByCat9_number = 1;


        public static List<BudgetItemsByMonth> getBudgetItemsBy2018_01()
        {
            List<BudgetItemsByMonth> list = new List<BudgetItemsByMonth>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem1);
            budgetItems.Add(budgetItem2);


            list.Add(new BudgetItemsByMonth
            {
                Month = "2018/01",
                Details = budgetItems,
                Total = budgetItem1.Amount + budgetItem2.Amount
            });
            return list;
        }

        public static List<BudgetItemsByMonth> getBudgetItemsBy2018_01_filteredByCat9()
        {
            List<BudgetItemsByMonth> list = new List<BudgetItemsByMonth>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem2);

            list.Add(new BudgetItemsByMonth
            {
                Month = "2018/01",
                Details = budgetItems,
                Total = budgetItem2.Amount
            });
            return list;
        }



        // LIST EXPENSES BY CATEGORY
        public static int budgetItemsByCategory_MaxRecords = 3;
        public static BudgetItemsByCategory budgetItemsByCategory_FirstRecord = getBudgetItemsByCategoryCat10()[0];
        public static int budgetItemsByCategory_FilteredByCat10_number = 2;
        public static int budgetItemsByCategory14 = 1;
        public static int budgetItemsByCategory20 = 0;


        public static List<BudgetItemsByCategory> getBudgetItemsByCategoryCat10()
        {
            List<BudgetItemsByCategory> list = new List<BudgetItemsByCategory>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem1);
            budgetItems.Add(budgetItem3);


            list.Add(new BudgetItemsByCategory
            {
                Category = "Clothes",
                Details = budgetItems,
                Total = budgetItem1.Amount+ budgetItem3.Amount
            });
            return list;
        }

        public static List<BudgetItemsByCategory> getBudgetItemsByCategory2018_Cat9()
        {
            List<BudgetItemsByCategory> list = new List<BudgetItemsByCategory>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem2);

            list.Add(new BudgetItemsByCategory
            {
                Category = "Credit Card",
                Details = budgetItems,
                Total = budgetItem2.Amount
            });
            return list;
        }

        public static List<BudgetItemsByCategory> getBudgetItemsByCategory2018()
        {
            List<BudgetItemsByCategory> list = new List<BudgetItemsByCategory>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem1);

            list.Add(new BudgetItemsByCategory
            {
                Category = "Clothes",
                Details = budgetItems,
                Total = budgetItem1.Amount
            });


            budgetItems = new List<BudgetItem>();
            budgetItems.Add(budgetItem2);

            list.Add(new BudgetItemsByCategory
            {
                Category = "Credit Card",
                Details = budgetItems,
                Total = budgetItem2.Amount
            });
            return list;
        }




        // LIST EXPENSES BY CATEGORY AND MONTH
        public static int budgetItemsByCategoryAndMonth_MaxRecords = 3; // 3 months

        public static Dictionary<string, object> getBudgetItemsByCategoryAndMonthFirstRecord()
        {
            List<BudgetItem> budgetItems;

            Dictionary<string, object> dict = new Dictionary<string, object> {
                { "Month","2018/01" },{"Total", budgetItem1.Amount+budgetItem2.Amount }  };


            budgetItems = new List<BudgetItem>();
            budgetItems.Add(budgetItem1);

            dict.Add("details:Clothes", budgetItems);
            dict.Add("Clothes", budgetItem1.Amount);


            budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem2);

            dict.Add("details:Credit Card", budgetItems);
            dict.Add("Credit Card", budgetItem2.Amount);



            return dict;
        }

        public static Dictionary<string, object> getBudgetItemsByCategoryAndMonthTotalsRecord()
        {
            Dictionary<string, object> dict = new Dictionary<string, object> {
                { "Month","TOTALS" }  };
            dict.Add("Clothes", budgetItem1.Amount + budgetItem3.Amount);
            dict.Add("Credit Card", budgetItem4.Amount + budgetItem2.Amount);
            dict.Add("Eating Out", budgetItem5.Amount + budgetItem7.Amount);

            return dict;
        }

        public static List<Dictionary<string,object>> getBudgetItemsByCategoryAndMonthCat10()
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            List<BudgetItem> budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem1);

            list.Add(new Dictionary<string, object> {
                {"Month","2018/01" },
                { "Clothes",budgetItem1.Amount},
                {"details:Clothes",budgetItems },
                }
            );

            budgetItems = new List<BudgetItem>();

            budgetItems.Add(budgetItem3);

            list.Add(new Dictionary<string, object> {
                {"Month","2019/01" },
                { "Clothes",budgetItem3.Amount},
                {"details:Clothes",budgetItems },
                }
            );

            list.Add(new Dictionary<string, object> {
                {"Month","TOTALS" },
                { "Clothes",budgetItem1.Amount + budgetItem3.Amount},
                }
            );

            return list;
        }

 
        public static List<Dictionary<string,object>> getBudgetItemsByCategoryAndMonth2020()
        {
            List< Dictionary<string, object> > list = new List<Dictionary<string, object>>();

            list.Add(new Dictionary<string, object> {
                {"Month","2020/01" },
                { "Credit Card",budgetItem4.Amount},
                {"details:Credit Card",new List<BudgetItem>{budgetItem4 } },
                {"Eating Out",budgetItem5.Amount + budgetItem7.Amount },
                {"details:Eating Out", new List<BudgetItem>{budgetItem5, budgetItem7} }
                }
            );

           list.Add(new Dictionary<string, object> {
                {"Month","TOTALS" },
                { "Credit Card",budgetItem4.Amount},
                { "Eating Out",budgetItem5.Amount + budgetItem7.Amount},
                }
            );



            return list;
        }

        static public String GetSolutionDir()
        {

            // this is valid for C# .Net Foundation (not for C# .Net Core)
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\"));
        }

        // source taken from: https://www.dotnetperls.com/file-equals

        static public bool FileEquals(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length)
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        static public bool FileSameSize(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            return (file1.Length == file2.Length);
        }

        /// <summary>
        /// Copies the test input database to a "messy" copy at
        /// <see cref="messyDbPath"/>.
        /// </summary>
        public static void CopyMessyDb() {
            string folder = GetSolutionDir();
            string goodDB = $"{folder}{testDBInputFile}";
            File.Copy(goodDB, messyDbPath, overwrite: true);
        }

        /// <summary>
        /// Makes a "messy" copy of the test input database and opens it with
        /// <see cref="Database.existingDatabase(string)"/>.
        /// </summary>
        public static void CopyAndConnectToMessyDb() {
            Database.CloseDatabaseAndReleaseFile();
            CopyMessyDb();
            Database.existingDatabase(messyDbPath);
        }

        /// <summary>
        /// Creates and opens a new database at <see cref="newDbPath"/> using
        /// <see cref="Database.newDatabase(string)"/> and returns a connection
        /// to it.
        /// </summary>
        public static void CreateAndConnectToNewDb() {
            Database.newDatabase(newDbPath);
        }
    }
}




