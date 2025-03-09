using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for AddTemplatePopUp.xaml
    /// </summary>
    public partial class AddTemplatePopUp : Window
    {
        public AddTemplatePopUp()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        //load categories para lumabas sa combobox
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
        public event Action TemplateAdded;
        //add button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string templateName = txtTemplateName.Text.Trim();
            string category = cmbCategory.Text.Trim();
            string description = txtTemplateDescription.Text.Trim();


            if (string.IsNullOrEmpty(templateName) ||
                string.IsNullOrEmpty(category) ||
                string.IsNullOrEmpty(description))
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
                                SELECT Template_ID FROM EquipmentTemplates 
                                WHERE Template_Name = @TemplateName AND Template_Description = @Description";

                    int existingTemplateId = -1;

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@TemplateName", templateName);
                        checkCmd.Parameters.AddWithValue("@Description", description);

                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                existingTemplateId = Convert.ToInt32(reader["Template_ID"]);
                            }
                        }
                    }
                    if (existingTemplateId != -1)
                    {
                        MessageBox.Show("Error: Template Already Exist", "Invalid Template", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {

                        string insertQuery = @"
                                    INSERT INTO EquipmentTemplates (Template_Name, Template_Description, Category_ID, Template_Category) 
                                    VALUES (@TemplateName, @Description, @CategoryID, @TemplateCategory)";

                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@TemplateName", templateName);
                            insertCmd.Parameters.AddWithValue("@Description", description);
                            insertCmd.Parameters.AddWithValue("@CategoryID", categoryId);
                            insertCmd.Parameters.AddWithValue("@TemplateCategory", category);
                            insertCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("New template added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                TemplateAdded?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtTemplateName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtTemplateName.Text == "Name")
            {
                txtTemplateName.Clear();
            }
        }

        private void txtTemplateName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTemplateName.Text))
            {
                txtTemplateName.Text = "Name";
            }
        }

        private void txtTemplateDescription_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtTemplateDescription.Text == "Description")
            {
                txtTemplateDescription.Clear();
            }
        }

        private void txtTemplateDescription_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTemplateDescription.Text))
            {
                txtTemplateDescription.Text = "Description";
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();

            if (string.IsNullOrWhiteSpace(txtTemplateDescription.Text))
            {
                txtTemplateDescription.Text = "Description";
            }
            if (string.IsNullOrWhiteSpace(txtTemplateName.Text))
            {
                txtTemplateName.Text = "Name";
            }
        }
        //cancel button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
