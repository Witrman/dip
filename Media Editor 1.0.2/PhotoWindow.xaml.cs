using Microsoft.Win32;
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

namespace Media_Editor_1._0._2
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class PhotoWindow : Window
    {
        public PhotoWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog(); 
            openFile.Filter = "Изображение |*.png;*.jpeg;*.jpg;*.bmp";
            openFile.FilterIndex = 1;
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == true)
            {
                BitmapImage loadImage = new BitmapImage();
                loadImage.BeginInit();
                loadImage.UriSource = new Uri(openFile.FileName);
                loadImage.EndInit(); 
                photoImage.Source = loadImage;
                photoImage.Height = scrollViewer.Height;
                photoImage.Width = scrollViewer.Width;
            }

        }
         


        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        { 
            photoImage.Height = scrollViewer.Height * sliderZoom.Value / 100;
            photoImage.Width = scrollViewer.Width * sliderZoom.Value / 100; 
        }

        private void scrollViewer_PreviewKeyUp(object sender, KeyEventArgs e)
        {    
        }

        private void scrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {  
                if (e.Delta > 0)
                    sliderZoom.Value += 10;

                else if (e.Delta < 0)
                    sliderZoom.Value -= 10;
          
        }
    }
}
