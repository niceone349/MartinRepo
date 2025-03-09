using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for UpdateItemPopUp.xaml
    /// </summary>
    public partial class UpdateItemPopUp : Window
    {
        public int ItemId { get; private set; }

        public UpdateItemPopUp(int itemId, string itemName, int quantity, int lowStock, string description)
        {
            InitializeComponent();


            ItemId = itemId;


            txtItemName.Text = itemName;
            txtQuantity.Text = quantity.ToString();
            txtLowStock.Text = lowStock.ToString();
            txtItemDescription.Text = description;
        }
        public event Action ItemUpdated;
        //update button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string updatedName = txtItemName.Text.Trim();
            string updatedDescription = txtItemDescription.Text.Trim();
            int updatedQuantity, updatedLowStock;

            if (string.IsNullOrEmpty(updatedName) ||
                string.IsNullOrEmpty(updatedDescription) ||
                !int.TryParse(txtQuantity.Text, out updatedQuantity) || updatedQuantity <= 0 ||
                !int.TryParse(txtLowStock.Text, out updatedLowStock) || updatedLowStock < 0)
            {
                MessageBox.Show("Please fill all fields correctly!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "Server=Niceone349\\SQLDATABASE;Database=Inventory System;Integrated Security=True;";

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
                    Item_Low_Indicator = @LowStock
                WHERE Item_ID = @ItemID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemName", updatedName);
                        cmd.Parameters.AddWithValue("@Description", updatedDescription);
                        cmd.Parameters.AddWithValue("@Quantity", updatedQuantity);
                        cmd.Parameters.AddWithValue("@LowStock", updatedLowStock);
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
        //cancel button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
