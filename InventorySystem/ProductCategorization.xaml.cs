using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for ProductCategorization.xaml
    /// </summary>
    public partial class ProductCategorization : Page
    {
        private readonly string connectionString = Server.ConnString;

        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public ProductCategorization()
        {
            InitializeComponent();
            DataContext = this; 
            LoadCategories();
        }
        private void LoadCategories()
        {
            Categories.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Category_Name FROM Categories ORDER BY Category_Name;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categories.Add(reader["Category_Name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lists_of_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lists_of_category.SelectedItem != null)
            {
                string selectedCategory = lists_of_category.SelectedItem.ToString().ToUpper(); 


                selectedCategoryTextBlock.Text = selectedCategory;
                selectedCategoryTextBlock.FontSize = 26; 


                LoadItemsForSelectedCategory(selectedCategory);
            }
        }

        public void LoadItemsForSelectedCategory(string selectedCategory)
        {
            Items.Clear();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT i.Item_ID, i.Item_Name, i.Item_Description, i.Category_ID
                FROM AvailableItems i
                INNER JOIN Categories c ON i.Category_ID = c.Category_ID
                WHERE c.Category_Name = @CategoryName;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Items.Add(new Item
                                {
                                    ItemID = Convert.ToInt32(reader["Item_ID"]),
                                    ItemName = reader["Item_Name"].ToString(),
                                    Description = reader["Item_Description"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading items: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void add_category_Click(object sender, RoutedEventArgs e)
        {
            AddCategoryWindow addCategoryWindow = new AddCategoryWindow();
            addCategoryWindow.ShowDialog();
            if (addCategoryWindow.CategoryAdded)
            {
                LoadCategories();
            }
        }

        private void remove_category_Click(object sender, RoutedEventArgs e)
        {
            if (lists_of_category.SelectedItem != null)
            {
                string selectedCategory = lists_of_category.SelectedItem.ToString();

                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the category '{selectedCategory}'? All associated items will also be removed.",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();

                            // First, delete related records from ExperimentItems
                            string deleteExperimentItemsQuery = @"
                        DELETE FROM ExperimentItems 
                        WHERE Item_ID IN (
                            SELECT Item_ID FROM AvailableItems 
                            WHERE Category_ID = (SELECT Category_ID FROM Categories WHERE Category_Name = @CategoryName)
                        );";

                            using (SqlCommand deleteExperimentItemsCmd = new SqlCommand(deleteExperimentItemsQuery, conn))
                            {
                                deleteExperimentItemsCmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                                deleteExperimentItemsCmd.ExecuteNonQuery();
                            }

                            // Then, delete items from AvailableItems
                            string deleteAvailableItemsQuery = @"
                        DELETE FROM AvailableItems 
                        WHERE Category_ID = (SELECT Category_ID FROM Categories WHERE Category_Name = @CategoryName);";

                            using (SqlCommand deleteAvailableItemsCmd = new SqlCommand(deleteAvailableItemsQuery, conn))
                            {
                                deleteAvailableItemsCmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                                deleteAvailableItemsCmd.ExecuteNonQuery();
                            }

                            // Finally, delete the category itself
                            string deleteCategoryQuery = @"
                        DELETE FROM Categories 
                        WHERE Category_Name = @CategoryName;";

                            using (SqlCommand deleteCategoryCmd = new SqlCommand(deleteCategoryQuery, conn))
                            {
                                deleteCategoryCmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                                deleteCategoryCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Category and all related data deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadCategories();
                            Items.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error removing category: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a category to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

       

    }
}
