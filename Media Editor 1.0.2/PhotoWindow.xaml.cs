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
        private double zoomKoeficient;
        private SolidColorBrush colorButtonSelected = new SolidColorBrush(Color.FromRgb(42, 42, 42));
        private SolidColorBrush colorButtonNotSelected = new SolidColorBrush(Color.FromRgb(82, 83, 88));
        private String buttonSelectedString;
        private Point prev;
        private SolidColorBrush color = new SolidColorBrush(Colors.Black); 
        private bool isPaint = false;
        private const int SIZE = 20; 
        ScaleTransform st = new ScaleTransform();
        private const int SHIFT = SIZE / 2;

        public PhotoWindow()
        {
            InitializeComponent(); 
            statHeight = canvas.Height;
            statWidht = canvas.Width;
            canvas.LayoutTransform = st; 

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Изображение |*.png;*.jpeg;*.jpg;*.bmp";
            openFile.Title = "Открытие файла";
            openFile.FilterIndex = 1;
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == true)
            {
                canvas.Children.Clear();
                BitmapImage source = new BitmapImage(new Uri(openFile.FileName));
                canvas.Background = new ImageBrush(source);
                canvas.Height = source.PixelHeight;
                canvas.Width = source.PixelWidth;
                zoomKoeficient = 550 / canvas.Height;
                sliderZoom.Value = 100; 
            }

        }

        //Сохранение //занимает много места
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Изображение |*.png;*.jpeg;*.jpg;*.bmp";
            save.Title = "Сохранение файла";
            if (save.ShowDialog() == true)
            {
                st.ScaleX = 1;
                st.ScaleY = 1;
                var rtb = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96d, 96d, PixelFormats.Default);
                canvas.Measure(new Size((int)canvas.Width, (int)canvas.Height));
                canvas.Arrange(new Rect(new Size((int)canvas.Width, (int)canvas.Height)));
                rtb.Render(canvas);

                PngBitmapEncoder BufferSave = new PngBitmapEncoder();
                BufferSave.Frames.Add((BitmapFrame.Create(rtb)));
                using (var fs = System.IO.File.OpenWrite(save.FileName))
                {
                    BufferSave.Save(fs);
                }
                sliderZoom.Value = 100;
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (statHeight == 0) return;
            double minZoom = (zoomKoeficient + (sliderZoom.Value / 100) - 1) * canvas.Height;
            if (minZoom < 250) return;
            double xc = (sliderZoom.Value / 100);
            st.ScaleX = zoomKoeficient + (sliderZoom.Value / 100) - 1;
            st.ScaleY = zoomKoeficient + (sliderZoom.Value / 100) - 1;
            lbl.Content = canvas.Height + "  " + zoomKoeficient + "  " + st.ScaleX + "   " +
            (canvas.Height * zoomKoeficient) + "    " + minZoom + "    " + xc; 
        }


        private void scrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                sliderZoom.Value += 10;

            else if (e.Delta < 0)
                sliderZoom.Value += 10;

        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (buttonSelectedString)
            {
                case "pointButton":
                    buttonBrushMouseDown(sender, e);
                    break;
                case "brushButton":
                    buttonBrushMouseDown(sender, e);
                    break;
                case "colorFullButton":

                    break;
                default:
                    break;
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (buttonSelectedString)
            {
                case "pointButton":

                    buttonBrushMouseUp(sender, e);
                    break;
                case "brushButton":
                    buttonBrushMouseUp(sender, e);
                    break;
                case "colorFullButton":
                    break;
                default:
                    break;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            switch (buttonSelectedString)
            {
                case "pointButton":
                    buttonBrushMouseMovwwe(sender, e);
                    break;
                case "brushButton":
                    buttonBrushMouseMove(sender, e);
                    break;
                case "colorFullButton":
                    break;
                default:
                    break;
            }
        }
        

        private void pointButton_Click(object sender, RoutedEventArgs e)
        {
            buttonSelected(pointButton);
        }

        private void brushButton_Click(object sender, RoutedEventArgs e)
        {
            buttonSelected(brushButton);
        }

        private void colorFullButton_Click(object sender, RoutedEventArgs e)
        {
            buttonSelected(colorFullButton);
        }


        private void buttonSelected(Button selectButon)
        {
            pointButton.Background = colorButtonNotSelected;
            brushButton.Background = colorButtonNotSelected;
            colorFullButton.Background = colorButtonNotSelected;
            selectButon.Background = colorButtonSelected;
            buttonSelectedString = selectButon.Name;
        }


        private void buttonBrushMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isPaint) return;
                isPaint = true;
                prev = Mouse.GetPosition(canvas);
                var dot = new Ellipse { Width = SIZE, Height = SIZE, Fill = color };
                dot.SetValue(Canvas.LeftProperty, prev.X - SHIFT);
                dot.SetValue(Canvas.TopProperty, prev.Y - SHIFT);
                canvas.Children.Add(dot);
            }
        }

        private void buttonBrushMouseUp(object sender, MouseButtonEventArgs e)
        {
            isPaint = false;
        }

        private void buttonBrushMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!isPaint) return;
                var point = Mouse.GetPosition(canvas);
                Pen pen = new Pen(new SolidColorBrush(Color.FromRgb(255, 255, 255)), 20);
                var line = new Line
                {

                    Stroke = color,
                    StrokeThickness = SIZE,
                    X1 = prev.X,
                    Y1 = prev.Y,
                    X2 = point.X,
                    Y2 = point.Y,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                prev = point;
                canvas.Children.Add(line); 
            }
            else
            {
                buttonBrushMouseUp(sender, null);
            }
        }
        //проверить потом работоспособность
        private void buttonBrushMouseMovwwe(object sender, MouseEventArgs e)
        {
            if (!isPaint) return;
            var point = Mouse.GetPosition(canvas);
            Pen pen = new Pen(new SolidColorBrush(Color.FromRgb(255, 255, 255)), 20);
            var line = new Line
            {

                Stroke = color,
                StrokeThickness = SIZE,
                X1 = prev.X,
                Y1 = prev.Y,
                X2 = point.X,
                Y2 = point.Y,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            prev = point;
            canvas.Children.Add(line);
            Ellipse elips = new Ellipse(); 
            elips.Fill = color;
            elips.Width = elips.Height = SIZE;
            Canvas.SetLeft(elips, point.X - SIZE / 2);
            Canvas.SetTop(elips, point.Y - SIZE / 2);
            canvas.Children.Add(elips);
        }


    }
}
