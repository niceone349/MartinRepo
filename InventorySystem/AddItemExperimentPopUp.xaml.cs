using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AddItemExperimentPopUp.xaml
    /// </summary>
    public partial class AddItemExperimentPopUp : Window
    {
        private string _itemName;
        private string _category;
        private string _description;
        private int _quantity;
        private int _lowStock;

        public AddItemExperimentPopUp(string itemName, string category, string description, int quantity, int lowStock)
        {
            InitializeComponent();

            _itemName = itemName;
            _category = category;
            _description = description;
            _quantity = quantity;
            _lowStock = lowStock;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadExperiments();
        }

        private void LoadExperiments()
        {
            string connectionString = Server.ConnString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Experiment_ID, Experiment_Name FROM Experiments";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable experiments = new DataTable();
                        adapter.Fill(experiments);
                        experiments.Columns.Add(new DataColumn("IsSelected", typeof(bool)));
                        DataGrid_Inventory.ItemsSource = experiments.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //cancel button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            foreach (Window window in Application.Current.Windows)
            {
                if (window is AddItemPopUp)
                {
                    window.Activate();
                    break;
                }
            }
        }

        //add button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string connectionString = Server.ConnString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

           
                    string categoryQuery = "SELECT Category_ID FROM Categories WHERE Category_Name = @Category";
                    int categoryId = -1;

                    using (SqlCommand categoryCmd = new SqlCommand(categoryQuery, conn))
                    {
                        categoryCmd.Parameters.AddWithValue("@Category", _category);
                        object categoryResult = categoryCmd.ExecuteScalar();
                        if (categoryResult != null)
                            categoryId = Convert.ToInt32(categoryResult);
                    }

                    if (categoryId == -1)
                    {
                        MessageBox.Show("Error: Selected category does not exist!", "Invalid Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int itemId = -1;
                    string insertItemQuery = @"INSERT INTO AvailableItems (Item_Name, Item_Description, Category_ID, Item_Quantity, Item_Low_Indicator)
                                  OUTPUT INSERTED.Item_ID
                                  VALUES (@ItemName, @Description, @CategoryID, @Quantity, @LowStock)";

                    using (SqlCommand insertCmd = new SqlCommand(insertItemQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@ItemName", _itemName);
                        insertCmd.Parameters.AddWithValue("@Description", _description);
                        insertCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        insertCmd.Parameters.AddWithValue("@Quantity", _quantity);
                        insertCmd.Parameters.AddWithValue("@LowStock", _lowStock);
                        itemId = (int)insertCmd.ExecuteScalar();
                    }


                    var selectedExperiments = ((DataView)DataGrid_Inventory.ItemsSource).Table.AsEnumerable()
                        .Where(row => row.Field<bool?>("IsSelected") == true) 
                        .Select(row => row.Field<int>("Experiment_ID"));

              
                    foreach (var experimentId in selectedExperiments)
                    {
                        int quantityRequired = 0;

          
                        string getQuantityQuery = "SELECT Quantity_Required FROM ExperimentItems WHERE Experiment_ID = @ExperimentID";

                        using (SqlCommand getQtyCmd = new SqlCommand(getQuantityQuery, conn))
                        {
                            getQtyCmd.Parameters.AddWithValue("@ExperimentID", experimentId);
                            object result = getQtyCmd.ExecuteScalar();
                            if (result != null)
                            {
                                quantityRequired = Convert.ToInt32(result);
                            }
                        }

                        string insertExperimentQuery = "INSERT INTO ExperimentItems (Item_ID, Experiment_ID, Quantity_Required) VALUES (@ItemID, @ExperimentID, @QuantityRequired)";

                        using (SqlCommand insertExpCmd = new SqlCommand(insertExperimentQuery, conn))
                        {
                            insertExpCmd.Parameters.AddWithValue("@ItemID", itemId);
                            insertExpCmd.Parameters.AddWithValue("@ExperimentID", experimentId);
                            insertExpCmd.Parameters.AddWithValue("@QuantityRequired", quantityRequired);
                            insertExpCmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Item and associated experiments added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is AddItemPopUp)
                    {
                        window.Close();
                    }
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

    }
}
