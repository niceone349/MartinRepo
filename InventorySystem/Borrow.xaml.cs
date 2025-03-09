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


                    //HardCode for Borrower Sample kasi d pa naimplement HAHAHA
                    string borrowerName = "Jon Fukiko";
                    int activityID = 12345;
                    int borrowedID = 10;
                    DateTime borrowDate = DateTime.Now;
                    // Iterate over each row in the cartDataTable.
                    foreach (DataRow row in cartDataTable.Rows)
                    {
                        int itemID = Convert.ToInt32(row["Item_ID"]);
                        int requestedQuantity = Convert.ToInt32(row["Item_Quantity"]);

                        //Exception Handling if requestedQuantity isn't a number or is less than 0
                        if (!int.TryParse(row["Item_Quantity"].ToString(), out requestedQuantity))
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

                        // Query the database for the current available stock for this item.
                        string selectQuery = "SELECT Item_Quantity FROM AvailableItems WHERE Item_ID = @itemID";
                        SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                        selectCmd.Parameters.AddWithValue("@itemID", itemID);

                        object result = selectCmd.ExecuteScalar();
                        if (result != null)
                        {
                            int currentStock = Convert.ToInt32(result);
                            // If the requested quantity is more than the current stock, show an error and exit.
                            if (requestedQuantity > currentStock)
                            {
                                MessageBox.Show("Insufficient stock for item: " + row["Item_Name"] +
                                    ". Requested: " + requestedQuantity + ", Available: " + currentStock,
                                    "Stock Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }


                            //If stock is enough
                            if(requestedQuantity <= currentStock)
                            {
                                int finalStock = currentStock - requestedQuantity;
                                string updateQuery = "UPDATE AvailableItems SET Item_Quantity = @finalStock WHERE Item_ID = @itemID";
                                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                                updateCmd.Parameters.AddWithValue("@finalStock", finalStock);
                                updateCmd.Parameters.AddWithValue("@itemID", itemID);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Item " + row["Item_Name"] + " not found in inventory.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        /*
                        //Insert Info to Borrowed Table
                        //NEEDS WORK. ANDAMI CONFLICT NAKAKAINIS
                        string insertQuery = @"
                        INSERT INTO BorrowedItems (Borrowed_ID, Borrower_Name, Item_ID, Borrow_Transaction_Date, Activity_ID) 
                        VALUES (@borrowedID, @borrowerName, @itemID, @transactionDate, @activityID)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@borrowedID", borrowedID);
                            insertCmd.Parameters.AddWithValue("@borrowerName", borrowerName);
                            insertCmd.Parameters.AddWithValue("@itemID", itemID);
                            insertCmd.Parameters.AddWithValue("@transactionDate", borrowDate);
                            insertCmd.Parameters.AddWithValue("@activityID", activityID);
                            insertCmd.ExecuteNonQuery();
                        }
                        */

                    }
               

                    // If all rows pass the check, proceed with the checkout process.
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
                DataRow newRow = cartDataTable.NewRow();
                newRow["Item_ID"] = selectedRowView["Item_ID"];
                newRow["Item_Name"] = selectedRowView["Item_Name"];
                newRow["Item_Description"] = selectedRowView["Item_Description"];
                newRow["Item_Quantity"] = selectedRowView["Item_Quantity"];
                cartDataTable.Rows.Add(newRow);
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
