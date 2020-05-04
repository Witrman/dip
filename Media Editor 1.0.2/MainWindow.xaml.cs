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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Media_Editor_1._0._2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
          Button_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try { 
            PhotoWindow photoWindow = new PhotoWindow();
            photoWindow.Show();
            this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try { 
            AudioWindow audioWindow = new AudioWindow();
            audioWindow.Show();
            this.Close();
        }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
}
    }
}
