using System.Data;
using System.Data.SqlClient;
using System.Text;
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
    /// Interaction logic for ProductManagement.xaml
    /// </summary>
    public partial class ProductManagement : Window
    {
        public ProductManagement()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string connectionString = Server.ConnString;
            string query = @"
                    SELECT a.Item_ID, a.Item_Name, a.Item_Description, c.Category_Name, 
                           a.Item_Quantity, a.Item_Low_Indicator
                    FROM AvailableItems a
                    INNER JOIN Categories c ON a.Category_ID = c.Category_ID";



            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    DataGrid_Inventory.ItemsSource = dt.DefaultView;

                    Dispatcher.Invoke(() =>
                    {
                        if (DataGrid_Inventory.Columns.Count > 0)
                        {
                            DataGrid_Inventory.Columns[0].Visibility = Visibility.Collapsed;
                        }
                    }, System.Windows.Threading.DispatcherPriority.Background);

                    RenameDataGridColumns();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        //rename lang ng pangalan nung columns
        private void RenameDataGridColumns()
        {
            foreach (var column in DataGrid_Inventory.Columns)
            {
                switch (column.Header.ToString())
                {
                    case "Item_Name":
                        column.Header = "Product Name";
                        break;
                    case "Item_Description":
                        column.Header = "Description";
                        break;
                    case "Category_Name":
                        column.Header = "Category";
                        break;
                    case "Item_Quantity":
                        column.Header = "Available Stock";
                        break;
                    case "Item_Low_Indicator":
                        column.Header = "Low Stock Indicator";
                        break;
                }
            }
        }
        // sa pag select ng item sa datagrid then lalabas sa textblock
        private void DataGrid_Inventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGrid_Inventory.SelectedItem is DataRowView row)
            {
                txtSelectedItem.Text = row["Item_Name"].ToString();
            }
            else
            {
                txtSelectedItem.Text = "";
            }
        }
        //pang refresh nung data grid
        private void RefreshDataGrid()
        {
            string connectionString = Server.ConnString;
            string query = @"
                    SELECT a.Item_ID, a.Item_Name, a.Item_Description, c.Category_Name, 
                           a.Item_Quantity, a.Item_Low_Indicator
                    FROM AvailableItems a
                    INNER JOIN Categories c ON a.Category_ID = c.Category_ID";



            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    DataView defaultView = dt.DefaultView;

                    Dispatcher.Invoke(() =>
                    {
                        if (DataGrid_Inventory != null) 
                        {
                            DataGrid_Inventory.ItemsSource = defaultView; 

                            if (DataGrid_Inventory.Columns.Count > 0)
                            {
                                DataGrid_Inventory.Columns[0].Visibility = Visibility.Collapsed;
                            }

                            RenameDataGridColumns();
                        }
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        //add item button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddItemPopUp addItemWindow = new AddItemPopUp();
            addItemWindow.ItemAdded += RefreshDataGrid;
            addItemWindow.ShowDialog();

        }
        
        //Update item button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Inventory.SelectedItem is DataRowView selectedRow)
            {

                int itemId = selectedRow["Item_ID"] != DBNull.Value ? Convert.ToInt32(selectedRow["Item_ID"]) : 0;
                string itemName = selectedRow["Item_Name"]?.ToString() ?? "Unknown";
                int quantity = selectedRow["Item_Quantity"] != DBNull.Value ? Convert.ToInt32(selectedRow["Item_Quantity"]) : 0;
                int lowStock = selectedRow["Item_Low_Indicator"] != DBNull.Value ? Convert.ToInt32(selectedRow["Item_Low_Indicator"]) : 0;
                string description = selectedRow["Item_Description"]?.ToString() ?? "No Description";

                UpdateItemPopUp updateWindow = new UpdateItemPopUp(itemId, itemName, quantity, lowStock, description);
                updateWindow.ItemUpdated += RefreshDataGrid;
                updateWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select an item to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //Delete button
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Inventory.SelectedItem is DataRowView selectedRow)
            {

                int itemId = selectedRow["Item_ID"] != DBNull.Value ? Convert.ToInt32(selectedRow["Item_ID"]) : 0;
                string itemName = selectedRow["Item_Name"]?.ToString() ?? "Unknown";
                int quantity = selectedRow["Item_Quantity"] != DBNull.Value ? Convert.ToInt32(selectedRow["Item_Quantity"]) : 0;


                DeleteItemPopUp deleteItemWindow = new DeleteItemPopUp(itemId, itemName, quantity);
                deleteItemWindow.ItemDeleted += RefreshDataGrid;
                deleteItemWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select an item to Delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //search box click
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search Item")
            {
                txtSearch.Clear();
            }
        }
        //search box functionallity
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchText) || searchText == "search item")
            {
                RefreshDataGrid();
                return;
            }

            DataView dv = (DataGrid_Inventory.ItemsSource as DataView);
            if (dv != null)
            {
                dv.RowFilter = $"Item_Name LIKE '%{searchText}%' OR Item_Description LIKE '%{searchText}%'";
            }
        }
        //search box outside click
        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search Item";
            }
        }

        private void BorrowButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}