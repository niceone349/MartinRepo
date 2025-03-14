using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for InventoryReport.xaml
    /// </summary>
    public partial class InventoryReport : Page
    {
        private string connectionString = Server.ConnString;
        public InventoryReport()
        {
            InitializeComponent();
        }

        private void Export_Button_Click_1(object sender, RoutedEventArgs e)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            string defaultFileName = $"Inventory Report - {timestamp}.pdf";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save PDF Report",
                FileName = defaultFileName 
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                ExportToPDF(filePath);
            }
        }
     

        private void ExportToPDF(string filePath)
        {
            try
            {
         
                DataTable dataTable = GetAvailableItems();

                if (dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to export.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Document doc = new Document(PageSize.A4.Rotate(), 50, 50, 50, 50); 
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                
                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("Available Items Report")
                {
                    Alignment = Element.ALIGN_CENTER
                };
                title.Font.SetStyle(Font.BOLD);
                doc.Add(title);
                doc.Add(new iTextSharp.text.Paragraph("\n"));

                
                PdfPTable table = new PdfPTable(dataTable.Columns.Count)
                {
                    WidthPercentage = 100
                };

                
                float[] columnWidths = new float[dataTable.Columns.Count];
                for (int i = 0; i < columnWidths.Length; i++)
                {
                    columnWidths[i] = 1f; 
                }
                table.SetWidths(columnWidths);

                
                foreach (DataColumn column in dataTable.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName))
                    {
                        BackgroundColor = new BaseColor(200, 200, 200),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                
                foreach (DataRow row in dataTable.Rows)
                {
                    foreach (object item in row.ItemArray)
                    {
                        PdfPCell dataCell = new PdfPCell(new Phrase(item.ToString()))
                        {
                            Padding = 5,
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            NoWrap = false 
                        };
                        table.AddCell(dataCell);
                    }
                }

                doc.Add(table);
                doc.Close();
                MessageBox.Show("PDF Exported Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataTable GetAvailableItems()
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM AvailableItems";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dataTable;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM AvailableItems";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                
                    Data_Grid1.ItemsSource = dt.DefaultView;
                    Dispatcher.Invoke(() =>
                    {
                        if (Data_Grid1.Columns.Count > 0)
                        {
                            Data_Grid1.Columns[0].Visibility = Visibility.Hidden;
                        }
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
