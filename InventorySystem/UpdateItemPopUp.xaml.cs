using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Data;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for UpdateItemPopUp.xaml
    /// </summary>
    public partial class UpdateItemPopUp : Window
    {
        public int ItemId { get; private set; }

        public UpdateItemPopUp(int itemId, string itemName, int quantity, int lowStock, string description, string categoryName)
        {
            InitializeComponent();

            ItemId = itemId;

            txtItemName.Text = itemName;
            txtQuantity.Text = quantity.ToString();
            txtLowStock.Text = lowStock.ToString();
            txtItemDescription.Text = description;

            LoadCategories(categoryName);

        }

        private void LoadCategories(string currentCategoryName)
        {
            string connectionString = Server.ConnString;
            string query = "SELECT Category_ID, Category_Name FROM Categories";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbCategory.ItemsSource = dt.DefaultView;
                    cmbCategory.DisplayMemberPath = "Category_Name";
                    cmbCategory.SelectedValuePath = "Category_ID";

               
                    DataRowView defaultItem = dt.AsEnumerable()
                        .Where(row => row["Category_Name"].ToString() == currentCategoryName)
                        .Select(row => dt.DefaultView.Cast<DataRowView>().FirstOrDefault(r => r["Category_Name"].ToString() == currentCategoryName))
                        .FirstOrDefault();

                    if (defaultItem != null)
                    {
                        cmbCategory.SelectedItem = defaultItem;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        public event Action ItemUpdated;

        // Update button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string updatedName = txtItemName.Text.Trim();
            string updatedDescription = txtItemDescription.Text.Trim();
            int updatedQuantity, updatedLowStock;
            int updatedCategoryId = (int)cmbCategory.SelectedValue;

            if (string.IsNullOrEmpty(updatedName) ||
                string.IsNullOrEmpty(updatedDescription) ||
                !int.TryParse(txtQuantity.Text, out updatedQuantity) || updatedQuantity <= 0 ||
                !int.TryParse(txtLowStock.Text, out updatedLowStock) || updatedLowStock < 0 ||
                updatedCategoryId <= 0)
            {
                MessageBox.Show("Please fill all fields correctly!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = Server.ConnString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string updateQuery = @"
                    UPDATE AvailableItems 
                    SET Item_Name = @ItemName, 
                        Item_Description = @Description, 
                        Item_Quantity = @Quantity, 
                        Item_Low_Indicator = @LowStock,
                        Category_ID = @CategoryId
                    WHERE Item_ID = @ItemID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemName", updatedName);
                        cmd.Parameters.AddWithValue("@Description", updatedDescription);
                        cmd.Parameters.AddWithValue("@Quantity", updatedQuantity);
                        cmd.Parameters.AddWithValue("@LowStock", updatedLowStock);
                        cmd.Parameters.AddWithValue("@CategoryId", updatedCategoryId);
                        cmd.Parameters.AddWithValue("@ItemID", ItemId);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Item updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ItemUpdated?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Cancel button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
    }
}