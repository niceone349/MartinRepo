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

                    // Hardcoded sample values
                    string borrowerName = "Jon Fukiko";
                    DateTime borrowDate = DateTime.Now;
                    // Generate one Activity_ID for the entire transaction.
                    int activityID = GenerateActivityID();

                    // Insert one ActivityLog record for the transaction.
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

                    // Iterate over each row in the cart.
                    foreach (DataRow row in cartDataTable.Rows)
                    {
                        int itemID = Convert.ToInt32(row["Item_ID"]);

                        // Validate and parse quantity.
                        if (!int.TryParse(row["Item_Quantity"].ToString(), out int requestedQuantity))
                        {
                            MessageBox.Show("Invalid quantity for item: " + row["Item_Name"] +
                                ". Please enter a numeric value.", "Quantity Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (requestedQuantity < 0)
                        {
                            MessageBox.Show("Quantity for item: " + row["Item_Name"] + " cannot be negative.",
                                "Quantity Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Check current stock.
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
                                    MessageBox.Show("Insufficient stock for item: " + row["Item_Name"] +
                                        ". Requested: " + requestedQuantity + ", Available: " + currentStock,
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
                                MessageBox.Show("Item " + row["Item_Name"] + " not found in inventory.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        // Use the current nextBorrowedID, then increment for the next row.
                        int currentBorrowedID = nextBorrowedID;
                        nextBorrowedID++;

                        // Insert into BorrowedItems using the generated Borrowed_ID.
                        string insertQuery = @"
                    INSERT INTO BorrowedItems (Borrowed_ID, Borrower_Name, Item_ID, Borrow_Transaction_Date, Activity_ID) 
                    VALUES (@borrowedID, @borrowerName, @itemID, @transactionDate, @activityID)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@borrowedID", currentBorrowedID);
                            insertCmd.Parameters.AddWithValue("@borrowerName", borrowerName);
                            insertCmd.Parameters.AddWithValue("@itemID", itemID);
                            insertCmd.Parameters.AddWithValue("@transactionDate", borrowDate);
                            insertCmd.Parameters.AddWithValue("@activityID", activityID);
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

        }

        private void addToCartButton_Click(object sender, RoutedEventArgs e)
        {
            //Checks if there is a selected row
            if(tblEquipment.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to add to cart.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DataRowView selectedRowView = tblEquipment.SelectedItem as DataRowView;
            if(selectedRowView != null)
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
    }
}
