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
    /// Interaction logic for Borrow.xaml
    /// </summary>
    public partial class Borrow : Window
    {
        private DataTable equipmentDataTable;
        private DataTable cartDataTable;
        private DataTable experimentDataTable;

        public Borrow()
        {
            InitializeComponent();
            cartDataTable = new DataTable();
            cartDataTable.Columns.Add("Item_ID", typeof(int));
            cartDataTable.Columns.Add("Item_Name", typeof(string));
            cartDataTable.Columns.Add("Item_Description", typeof(string));
            cartDataTable.Columns.Add("Item_Quantity", typeof(int));
            tblBorrow.ItemsSource = cartDataTable.DefaultView;
        }

        private void LoadEquipmentData()
        {
            string connString = Server.ConnString;
            string query = "SELECT Item_ID, Item_Name, Item_Description, Item_Quantity FROM AvailableItems WHERE Item_Quantity > 0";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    equipmentDataTable = new DataTable();
                    adapter.Fill(equipmentDataTable);

                    tblEquipment.ItemsSource = equipmentDataTable.DefaultView;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error loading equipment: " + ex.Message);
                }
            }
        }

        private void LoadExperimentData(int id)
        {
            string connString = Server.ConnString;
            string query = @"
            SELECT 
                CAST(e.Item_ID AS INT) AS Item_ID, 
                e.Item_Name, 
                e.Item_Description, 
                ei.Quantity_Required
            FROM ExperimentItems ei
            JOIN AvailableItems e ON ei.Item_ID = e.Item_ID
            WHERE ei.Experiment_ID = @exp_id";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@exp_id", id);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        experimentDataTable = new DataTable();
                        adapter.Fill(experimentDataTable);
                        tblExperiment.ItemsSource = experimentDataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading experiment data: " + ex.Message);
                }
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEquipmentData();
        }

        private int GenerateActivityID()
        {
            DateTime baseDate = new DateTime(2020, 1, 1);
            // Total seconds since January 1, 2020.
            int activityID = (int)(DateTime.Now - baseDate).TotalSeconds;
            return activityID;
        }

        private int GetNextBorrowedID(SqlConnection conn)
        {
            string query = "SELECT ISNULL(MAX(Borrowed_ID), 0) + 1 FROM BorrowedItems";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            //Checks if there is a selected row
            if (tblBorrow.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DataRowView selectedRowView = tblBorrow.SelectedItem as DataRowView;
            if (tblBorrow.SelectedItem != null)
            {
                DataRow newRow = equipmentDataTable.NewRow();
                newRow["Item_ID"] = selectedRowView["Item_ID"];
                newRow["Item_Name"] = selectedRowView["Item_Name"];
                newRow["Item_Description"] = selectedRowView["Item_Description"];
                cartDataTable.Rows.Remove(selectedRowView.Row);
            }
        }

        private void checkoutButton_Click(object sender, RoutedEventArgs e)
        {
            string connString = Server.ConnString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    string borrowerName = "Jon"; // Replace with actual user input
                    DateTime borrowDate = DateTime.Now;
                    int activityID = GenerateActivityID();

                    // Insert into ActivityLog
                    string insertActivityQuery = @"
            INSERT INTO ActivityLog (Activity_ID, Action)
            VALUES (@activityID, @action)";
                    using (SqlCommand activityCmd = new SqlCommand(insertActivityQuery, conn))
                    {
                        activityCmd.Parameters.AddWithValue("@activityID", activityID);
                        activityCmd.Parameters.AddWithValue("@action", "BORROW EQUIPMENT");
                        activityCmd.ExecuteNonQuery();
                    }

                    // Get the next available Borrowed_ID
                    int nextBorrowedID = GetNextBorrowedID(conn);

                    foreach (DataRow row in cartDataTable.Rows)
                    {
                        int itemID = Convert.ToInt32(row["Item_ID"]);
                        string itemName = row["Item_Name"].ToString();
                        string itemDescription = row["Item_Description"].ToString();
                        int requestedQuantity = Convert.ToInt32(row["Item_Quantity"]);

                        // Retrieve Category_ID and Item_Low_Indicator from AvailableItems
                        string itemQuery = "SELECT Category_ID, Item_Low_Indicator FROM AvailableItems WHERE Item_ID = @itemID";
                        int categoryID;
                        bool itemLowIndicator;
                        using (SqlCommand itemCmd = new SqlCommand(itemQuery, conn))
                        {
                            itemCmd.Parameters.AddWithValue("@itemID", itemID);
                            using (SqlDataReader reader = itemCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    categoryID = reader.GetInt32(0);
                                    itemLowIndicator = reader.GetInt32(1) == 1;
                                }
                                else
                                {
                                    MessageBox.Show($"Item {itemName} not found in inventory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                        }

                        // Check current stock
                        string selectQuery = "SELECT Item_Quantity FROM AvailableItems WHERE Item_ID = @itemID";
                        using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                        {
                            selectCmd.Parameters.AddWithValue("@itemID", itemID);
                            object result = selectCmd.ExecuteScalar();
                            if (result != null)
                            {
                                int currentStock = Convert.ToInt32(result);
                                if (requestedQuantity > currentStock)
                                {
                                    MessageBox.Show($"Insufficient stock for {itemName}. Requested: {requestedQuantity}, Available: {currentStock}",
                                        "Stock Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                else
                                {
                                    int finalStock = currentStock - requestedQuantity;
                                    string updateQuery = "UPDATE AvailableItems SET Item_Quantity = @finalStock WHERE Item_ID = @itemID";
                                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                    {
                                        updateCmd.Parameters.AddWithValue("@finalStock", finalStock);
                                        updateCmd.Parameters.AddWithValue("@itemID", itemID);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Item {itemName} not found in inventory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        // Insert into BorrowedItems
                        string insertQuery = @"
                INSERT INTO BorrowedItems 
                (Borrowed_ID, Borrower_Name, Item_ID, Borrow_Transaction_Date, Activity_ID, Item_Name, Item_Description, Borrowed_Quantity, Category_ID, Item_Low_Indicator) 
                VALUES 
                (@borrowedID, @borrowerName, @itemID, @transactionDate, @activityID, @itemName, @itemDescription, @borrowedQuantity, @categoryID, @itemLowIndicator)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@borrowedID", nextBorrowedID++);
                            insertCmd.Parameters.AddWithValue("@borrowerName", borrowerName);
                            insertCmd.Parameters.AddWithValue("@itemID", itemID);
                            insertCmd.Parameters.AddWithValue("@transactionDate", borrowDate);
                            insertCmd.Parameters.AddWithValue("@activityID", activityID);
                            insertCmd.Parameters.AddWithValue("@itemName", itemName);
                            insertCmd.Parameters.AddWithValue("@itemDescription", itemDescription);
                            insertCmd.Parameters.AddWithValue("@borrowedQuantity", requestedQuantity);
                            insertCmd.Parameters.AddWithValue("@categoryID", categoryID);
                            insertCmd.Parameters.AddWithValue("@itemLowIndicator", itemLowIndicator);
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Checkout successful!");
                    cartDataTable.Rows.Clear();
                    LoadEquipmentData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during checkout: " + ex.Message);
                }
            }
        }


        private void browseExperimentButton_Click(object sender, RoutedEventArgs e)
        {
            experimentPanel.Visibility = Visibility.Visible;
            string connString = Server.ConnString;
            string query = "SELECT * FROM Experiments";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "t");
                    experimentComboBox.ItemsSource = ds.Tables["t"].DefaultView;
                    experimentComboBox.DisplayMemberPath = "Experiment_Name";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading experiments: " + ex.Message);
                }
            }

        }

        private void addToCartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Checks if there is a selected row
                if (tblEquipment.SelectedItem == null)
                {
                    MessageBox.Show("Please select an item to add to cart.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DataRowView selectedRowView = tblEquipment.SelectedItem as DataRowView;
                if (selectedRowView != null)
                {
                    int itemID = Convert.ToInt32(selectedRowView["Item_ID"]);
                    bool exists = cartDataTable.AsEnumerable().Any(row => row.Field<int>("Item_ID") == itemID);

                    //Checks if item is already in the cart.
                    if (exists)
                    {
                        MessageBox.Show("Item is already in the cart.", "Duplicate Item", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        DataRow newRow = cartDataTable.NewRow();
                        newRow["Item_ID"] = selectedRowView["Item_ID"];
                        newRow["Item_Name"] = selectedRowView["Item_Name"];
                        newRow["Item_Description"] = selectedRowView["Item_Description"];
                        newRow["Item_Quantity"] = selectedRowView["Item_Quantity"];
                        cartDataTable.Rows.Add(newRow);
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error in addToCart: " + ex.Message);
            }
            
            
        }

        private void searchEquipmentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (equipmentDataTable != null)
            {
                string filter = searchEquipmentTextBox.Text;

                DataView dv = equipmentDataTable.DefaultView;
                if (!string.IsNullOrEmpty(filter))
                {
                    dv.RowFilter = $"Item_Name LIKE '{filter}%'";
                }
                else
                {
                    dv.RowFilter = string.Empty;
                }
                tblEquipment.ItemsSource = dv;
            }
        }

        private void returnButton_Click(object sender, RoutedEventArgs e)
        {
            experimentPanel.Visibility = Visibility.Collapsed;
        }

        private void experimentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView selectedRow = experimentComboBox.SelectedItem as DataRowView;
            if(experimentComboBox.SelectedItem == null){
                return;
            }

            int experiment_id = Convert.ToInt32(selectedRow["Experiment_ID"]);
            LoadExperimentData(experiment_id);

        }

        private void experimentAddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure that experimentDataTable is loaded
            if (experimentDataTable == null || experimentDataTable.Rows.Count == 0)
            {
                MessageBox.Show("No experiment items to add.");
                return;
            }

            // Loop through each row in the experimentDataTable
            foreach (DataRow row in experimentDataTable.Rows)
            {
                int itemID = Convert.ToInt32(row["Item_ID"]);
                string itemName = row["Item_Name"].ToString();
                string itemDescription = row["Item_Description"].ToString();
                int quantityRequired = Convert.ToInt32(row["Quantity_Required"]);

                // Optional: Check if the item is already in the cart to avoid duplicates.
                bool exists = cartDataTable.AsEnumerable().Any(r => r.Field<int>("Item_ID") == itemID);
                if (exists)
                {
                    continue; // Skip duplicate items.
                }

                DataRow newRow = cartDataTable.NewRow();
                newRow["Item_ID"] = itemID;
                newRow["Item_Name"] = itemName;
                newRow["Item_Description"] = itemDescription;
                newRow["Item_Quantity"] = quantityRequired;

                cartDataTable.Rows.Add(newRow);
            }

            MessageBox.Show("All experiment items have been added to the cart.");
        }

    }
}
