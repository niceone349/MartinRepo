using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for HomeEditDetails.xaml
    /// </summary>
    public partial class HomeEditDetails : Window
    {
        public HomeEditDetails()
        {
            InitializeComponent();
        }
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = txtUsername.Text.Trim();
            string passwordText = txtPassword.Password.Trim();

            if (newUsername == "Username" || string.IsNullOrWhiteSpace(newUsername) || string.IsNullOrWhiteSpace(passwordText))
            {
                MessageBox.Show("Please enter a valid username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(passwordText, out _))
            {
                MessageBox.Show("Password must be numbers only!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int hashedPassword = ConvertHashToInt(passwordText);

            if (UpdateUserDetails(newUsername, hashedPassword))
            {
                MessageBox.Show("Account details updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to update account details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int ConvertHashToInt(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                int startIndex = hashBytes.Length / 2 - 2;
                int hashedInt = BitConverter.ToInt32(hashBytes, startIndex);

                return Math.Abs(hashedInt);
            }
        }

        private bool UpdateUserDetails(string username, int hashedPassword)
        {
            string connectionString = Server.ConnString;
            string query = "UPDATE Admin SET Username = @Username, Password = @Password WHERE Admin_ID = 1";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Update canceled.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text == "Username")
            {
                txtUsername.Text = "";
                txtUsername.Foreground = Brushes.Black;
            }
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = "Username";
                txtUsername.Foreground = Brushes.Gray;
            }
        }

        private void txtPassword_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }
    }
}
