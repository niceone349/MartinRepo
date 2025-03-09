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
    /// Interaction logic for DeleteItemPopUp.xaml
    /// </summary>
    public partial class DeleteItemPopUp : Window
    {
        private string connectionString = "Server=Niceone349\\SQLDATABASE;Database=Inventory System;Integrated Security=True;";
        public int ItemId { get; private set; }
        private int currentQuantity;

        public DeleteItemPopUp(int itemId, string itemName, int quantity)
        {
            InitializeComponent();
            ItemId = itemId;
            currentQuantity = quantity;

            txtItemName.Text = itemName;
            txtQuantity.Text = quantity.ToString();
        }

        //cancel button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public event Action ItemDeleted;
        //delete button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtQuantity.Text, out int quantityToDelete) || quantityToDelete <= 0)
            {
                MessageBox.Show("Please enter a valid quantity to delete.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool deleteAll = chkDeleteAll.IsChecked == true;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query;

                    if (deleteAll || quantityToDelete >= currentQuantity)
                    {
                        query = "DELETE FROM AvailableItems WHERE Item_ID = @ItemId";
                    }
                    else
                    {
                        query = "UPDATE AvailableItems SET Item_Quantity = Item_Quantity - @Quantity WHERE Item_ID = @ItemId";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemId", ItemId);
                        if (!deleteAll) cmd.Parameters.AddWithValue("@Quantity", quantityToDelete);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Item deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ItemDeleted?.Invoke();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void chkDeleteAll_Checked(object sender, RoutedEventArgs e)
        {
            txtQuantity.Text = currentQuantity.ToString();
            txtQuantity.IsEnabled = false;
        }

        private void chkDeleteAll_Unchecked(object sender, RoutedEventArgs e)
        {
            txtQuantity.IsEnabled = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}
