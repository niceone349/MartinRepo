using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for DeleteTemplatePopUp.xaml
    /// </summary>
    public partial class DeleteTemplatePopUp : Window
    {
        public int TemplateId { get; private set; }
        public DeleteTemplatePopUp(int templateID, string templateName)
        {
            InitializeComponent();
            TemplateId = templateID;


            txtTemplateName.Text = templateName;
   
        }
        public event Action TemplateDeleted;
        //Delete button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            string connectionString = Server.ConnString;
            string deleteQuery = "DELETE FROM EquipmentTemplates WHERE Template_ID = @TemplateID";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TemplateID", TemplateId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Template deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            TemplateDeleted?.Invoke(); 
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("No template was deleted. It may not exist.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
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
        }
    }
}
