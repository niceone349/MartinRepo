using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
    /// Interaction logic for AddItemPopUp.xaml
    /// </summary>
    public partial class AddItemPopUp : Window
    {
        public AddItemPopUp()
        {
            InitializeComponent();
        }
        private void txtItemName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtItemName.Text == "Name")
            {
                txtItemName.Clear();
            }
        }

        private void txtItemName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                txtItemName.Text = "Name";
            }
        }

        private void txtQuantity_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtQuantity.Text == "Quantity")
            {
                txtQuantity.Clear();
            }
        }

        private void txtQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                txtQuantity.Text = "Quantity";
            }
        }
        private void txtItemDescription_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtItemDescription.Text == "Description")
            {
                txtItemDescription.Clear();
            }
        }

        private void txtItemDescription_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtItemDescription.Text))
            {
                txtItemDescription.Text = "Description";
            }
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();

            if (string.IsNullOrWhiteSpace(txtItemDescription.Text))
            {
                txtItemDescription.Text = "Description";
            }
            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                txtQuantity.Text = "Quantity";
            }
            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                txtItemName.Text = "Name";
            }
            if (string.IsNullOrWhiteSpace(txtLowStock.Text))
            {
                txtLowStock.Text = "Low Stock";
            }
        }
        //cancel button here
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            LoadTemplates();
        }
        private void LoadCategories()
        {
            string connectionString = "Server=Niceone349\\SQLDATABASE;Database=Inventory System;Integrated Security=True;";
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

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private async void LoadTemplates()
        {
            string connectionString = "Server=Niceone349\\SQLDATABASE;Database=Inventory System;Integrated Security=True;";

            string query = @"
                        SELECT e.Template_ID, e.Template_Name, e.Template_Description, c.Category_Name, e.Template_Category
                        FROM EquipmentTemplates e
                        LEFT JOIN AvailableItems a ON e.Item_ID = a.Item_ID
                        LEFT JOIN Categories c ON a.Category_ID = c.Category_ID";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbTemplate.ItemsSource = dt.DefaultView;
                    cmbTemplate.DisplayMemberPath = "Template_Name";
                    cmbTemplate.SelectedValuePath = "Template_ID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading templates: " + ex.Message);
            }
        }


        private void cmbTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTemplate.SelectedItem is DataRowView row)
            {
                Dispatcher.Invoke(() =>
                {
                    txtItemName.Text = row["Template_Name"].ToString();
                    cmbCategory.Text = row["Template_Category"].ToString();
                    txtItemDescription.Text = row["Template_Description"].ToString();
                    txtQuantity.Text = "1";
                    txtLowStock.Text = "1";
                });
            }
        }
        public event Action ItemAdded;
        //ADD BUTTON HERE
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            string itemName = txtItemName.Text.Trim();
            string category = cmbCategory.Text.Trim();
            string description = txtItemDescription.Text.Trim();
            int quantity, lowStock;


            if (string.IsNullOrEmpty(itemName) ||
                string.IsNullOrEmpty(category) ||
                string.IsNullOrEmpty(description) ||
                !int.TryParse(txtQuantity.Text, out quantity) || quantity <= 0 ||
                !int.TryParse(txtLowStock.Text, out lowStock) || lowStock < 0)
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


                    string categoryQuery = "SELECT Category_ID FROM Categories WHERE Category_Name = @Category";
                    int categoryId = -1;

                    using (SqlCommand categoryCmd = new SqlCommand(categoryQuery, conn))
                    {
                        categoryCmd.Parameters.AddWithValue("@Category", category);
                        object categoryResult = categoryCmd.ExecuteScalar();

                        if (categoryResult != null)
                            categoryId = Convert.ToInt32(categoryResult);
                    }

                    if (categoryId == -1)
                    {
                        MessageBox.Show("Error: Selected category does not exist!", "Invalid Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }


                    string checkQuery = @"
                                SELECT Item_ID, Item_Quantity FROM AvailableItems 
                                WHERE Item_Name = @ItemName AND Item_Description = @Description";

                    int existingItemId = -1;
                    int existingQuantity = 0;

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@ItemName", itemName);
                        checkCmd.Parameters.AddWithValue("@Description", description);

                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existingItemId = Convert.ToInt32(reader["Item_ID"]);
                                existingQuantity = Convert.ToInt32(reader["Item_Quantity"]);
                            }
                        }
                    }

                    if (existingItemId != -1)
                    {
                        string updateQuery = "UPDATE AvailableItems SET Item_Quantity = @NewQuantity WHERE Item_ID = @ItemID";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@NewQuantity", existingQuantity + quantity);
                            updateCmd.Parameters.AddWithValue("@ItemID", existingItemId);
                            updateCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Quantity updated!", "Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {

                        string insertQuery = @"
                                    INSERT INTO AvailableItems (Item_Name, Item_Description, Category_ID, Item_Quantity, Item_Low_Indicator) 
                                    VALUES (@ItemName, @Description, @CategoryID, @Quantity, @LowStock)";

                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@ItemName", itemName);
                            insertCmd.Parameters.AddWithValue("@Description", description);
                            insertCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                            insertCmd.Parameters.AddWithValue("@Quantity", quantity);
                            insertCmd.Parameters.AddWithValue("@LowStock", lowStock);
                            insertCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("New item added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                ItemAdded?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void txtLowStock_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtLowStock.Text == "Low Stock")
            {
                txtLowStock.Clear();
            }
        }

        private void txtLowStock_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLowStock.Text))
            {
                txtLowStock.Text = "Low Stock";
            }
        }

    }
}
