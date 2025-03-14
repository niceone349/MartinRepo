using System;
using System.Collections.Generic;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Main.Content = new Home();
        }
        // product management page
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new ProductManagements();
        }
        // equipment tempalte apge
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Main.Content = new EquipmentTemplate();
        }
        //product categorization
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Main.Content = new ProductCategorization();
        }
        //inventory reporting
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Main.Content = new InventoryReport();
        }
        //Home 
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Main.Content = new Home();
        }
        //activity Log
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Main.Content = new ActivityLog();
        }
    }
}
