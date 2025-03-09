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
    /// Interaction logic for UpdateTemplatePopUp.xaml
    /// </summary>
    public partial class UpdateTemplatePopUp : Window
    {
        public int TemplateId { get; private set; }
        public UpdateTemplatePopUp(int templateId, string templateName, string templateCategory, string description)
        {
            InitializeComponent();
            TemplateId = templateId;

            LoadCategories();  

            txtTemplateName.Text = templateName;
            txtTemplateDescription.Text = description;

            cmbCategory.Text = templateCategory;

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
        public event Action TemplateUpdated;
        //update button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string updatedName = txtTemplateName.Text.Trim();
            string updatedDescription = txtTemplateDescription.Text.Trim();

            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Please select a valid category!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int updatedCategoryId = Convert.ToInt32(cmbCategory.SelectedValue);

            if (string.IsNullOrEmpty(updatedName) || string.IsNullOrEmpty(updatedDescription))
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
                    string updateQuery = @"
                    UPDATE EquipmentTemplates 
                    SET Template_Name = @TemplateName, 
                        Template_Description = @Description,
                        Category_ID = @CategoryID
                    WHERE Template_ID = @TemplateID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TemplateName", updatedName);
                        cmd.Parameters.AddWithValue("@Description", updatedDescription);
                        cmd.Parameters.AddWithValue("@CategoryID", updatedCategoryId);
                        cmd.Parameters.AddWithValue("@TemplateID", TemplateId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Template updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No template was updated. Please check if the Template ID exists.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                TemplateUpdated?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //cancel button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
