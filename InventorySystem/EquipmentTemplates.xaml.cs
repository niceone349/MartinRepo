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
    /// Interaction logic for EquipmentTemplates.xaml
    /// </summary>
    public partial class EquipmentTemplates : Window
    {
        public EquipmentTemplates()
        {
            InitializeComponent();
        }
        //add templatessss
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddTemplatePopUp addTemplateWindow = new AddTemplatePopUp();
            addTemplateWindow.TemplateAdded += RefreshDataGrid;
            addTemplateWindow.ShowDialog();
        }
        //load window para sa data grid
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string connectionString = Server.ConnString;
            string query = @"
                    SELECT e.Template_ID, e.Template_Name, e.Template_Description, c.Category_Name    
                    FROM EquipmentTemplates e
                    INNER JOIN Categories c ON e.Category_ID = c.Category_ID";



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
        //refresh ng datagrid para real time
        private void RefreshDataGrid()
        {
            string connectionString = Server.ConnString;
            string query = @"
                        SELECT e.Template_ID, e.Template_Name, e.Template_Description, c.Category_Name 
                        FROM EquipmentTemplates e
                        LEFT JOIN Categories c ON e.Category_ID = c.Category_ID";




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
        //renaming lang ng column
        private void RenameDataGridColumns()
        {
            foreach (var column in DataGrid_Inventory.Columns)
            {
                switch (column.Header.ToString())
                {
                    case "Template_Name":
                        column.Header = "Template Name";
                        break;
                    case "Template_Description":
                        column.Header = "Description";
                        break;
                    case "Category_Name":
                        column.Header = "Category";
                        break;
                }
            }
        }
        //Update button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Inventory.SelectedItem is DataRowView selectedRow)
            {

                int templateId = selectedRow["Template_ID"] != DBNull.Value ? Convert.ToInt32(selectedRow["Template_ID"]) : 0;
                string templateName = selectedRow["Template_Name"]?.ToString() ?? "Unknown";
                string templateCategory = selectedRow["Category_Name"] != DBNull.Value? selectedRow["Category_Name"].ToString() : "No Category"; 
                string description = selectedRow["Template_Description"]?.ToString() ?? "No Description";

                UpdateTemplatePopUp updatetemplateWindow = new UpdateTemplatePopUp(templateId, templateName, templateCategory, description);
                updatetemplateWindow.TemplateUpdated += RefreshDataGrid;
                updatetemplateWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select an item to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //delete item button
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (DataGrid_Inventory.SelectedItem is DataRowView selectedRow)
            {

                int templateId = selectedRow["Template_ID"] != DBNull.Value ? Convert.ToInt32(selectedRow["Template_ID"]) : 0;
                string templateName = selectedRow["Template_Name"]?.ToString() ?? "Unknown";
   

                DeleteTemplatePopUp deletetemplateWindow = new DeleteTemplatePopUp(templateId, templateName);
                deletetemplateWindow.TemplateDeleted += RefreshDataGrid;
                deletetemplateWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select an item to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //paglabas lang ulit sa textblock
        private void DataGrid_Inventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGrid_Inventory.SelectedItem is DataRowView row)
            {
                txtSelectedTemplate.Text = row["Template_Name"].ToString();
            }
            else
            {
                txtSelectedTemplate.Text = "";
            }
        }

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
                dv.RowFilter = $"Template_Name LIKE '%{searchText}%' OR Template_Description LIKE '%{searchText}%'";
            }
        }
        //searchbox click
        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text == "Search Item")
            {
                txtSearch.Clear();
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
    }
}
