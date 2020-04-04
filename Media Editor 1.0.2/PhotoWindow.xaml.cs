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
        private double statHeight, statWidht;
        public PhotoWindow()
        {
            InitializeComponent();

            //временно
            statHeight = photoImage.Height;
            statWidht = photoImage.Width;

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Изображение |*.png;*.jpeg;*.jpg;*.bmp";
            openFile.FilterIndex = 1;
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == true)
            {
                photoImage.Source = new BitmapImage(new Uri(openFile.FileName));
                photoImage.Height = photoImage.Source.Height * (scrollViewer.ActualHeight / photoImage.Source.Height);
                photoImage.Width = photoImage.Source.Width * (scrollViewer.ActualHeight / photoImage.Source.Width);
                statHeight = photoImage.Height;
                statWidht = photoImage.Width;
            }

        }



        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (statWidht != 0 && statWidht != 0)
            {
                photoImage.Height = (statHeight * (0.1+sliderZoom.Value / 100));
                photoImage.Width = (statWidht * (0.1+sliderZoom.Value/ 100)); 
            }
        }


        private void scrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                sliderZoom.Value += 10;

            else if (e.Delta < 0)
                sliderZoom.Value += 10;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        { 
        }
    }
}
