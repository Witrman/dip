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
using System.Windows.Resources;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace Media_Editor_1._0._2
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class PhotoWindow : Window
    {
        private double onFirst, zoomKoeficient, brushSizeDigit, opacitiValue = 1;
        private SolidColorBrush colorButtonSelected = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
            colorButtonNotSelected = new SolidColorBrush(Color.FromRgb(82, 83, 88)),
            color;
        private String buttonSelectedString;
        private Point prev;
        private Canvas brushCanvas; 
        private Rectangle rect = new Rectangle(); 
        private bool isPaint = false;
        ScaleTransform st = new ScaleTransform();

        public PhotoWindow()
        {
            InitializeComponent();
            onFirst = canvas.Height; 
            canvas.LayoutTransform = st;
            buttonSelected(pointButton);
        }
        //Открытие файла
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
        //Закрытие программы
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //зумирование канваса
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (onFirst == 0) return;
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

        //Инициализация кисти
        private void setBrush()
        {
            try
            {
                color = new SolidColorBrush(colorPickerBrush.SelectedColor.Value);
                double sizeBrush = 0;
                Double.TryParse(brushSize.Text, out sizeBrush);
                if (sizeBrush <= 150)
                {
                    brushSizeDigit = sizeBrush;
                }
                else
                {
                    brushSize.Text = "150";
                    brushSizeDigit = 150;
                }
            }
            catch (InvalidCastException e)
            {
                brushSize.Text = "150";
            }

        }
        //Ввод размера кисти только цифрами
        private void brushSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            double sizeBrush = 0;
            Double.TryParse(brushSize.Text, out sizeBrush);

            if (!Char.IsDigit(e.Text, 0))
            {
                if (sizeBrush >= 150) brushSize.Text = sizeBrush.ToString();
                e.Handled = true;
                setBrush();
            }
        }
        private void brushSize_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double sizeBrush = 0;
            Double.TryParse(brushSize.Text, out sizeBrush);

            if (e.Delta > 0 && sizeBrush < 150)
                sizeBrush += 1;

            else if (e.Delta < 0)
                sizeBrush -= 1;

            brushSize.Text = sizeBrush.ToString();

        }


        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            setBrush();
            switch (buttonSelectedString)
            {

                case "pointButton":
                    break;
                case "brushButton":
                    buttonBrushMouseDown(sender, e);
                    break;
                case "colorFullButton":
                    buttonFullColor(sender, e);
                    break;
                case "rectButton":
                    rectButtonDown(sender, e);
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
                    break;
                case "brushButton":
                    buttonBrushMouseMove(sender, e);
                    break;
                case "colorFullButton":
                    break;
                case "rectButton":
                    rectButtonMove(sender, e);
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

                    break;
                case "brushButton":
                    buttonBrushMouseUp(sender, e);
                    break;
                case "colorFullButton":
                    break;
                case "rectButton":
                    rectButtonUp(sender, e);
                    break;
                default:
                    break;
            }
        }


        private void pointButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            buttonSelected(pointButton);
        } 
        private void brushButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Cross;
            buttonSelected(brushButton);
        } 
        private void colorFullButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            buttonSelected(colorFullButton);
        }
        private void rectButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Cross;
            buttonSelected(rectButton);
        }

        private void buttonSelected(Button selectButon)
        {    
            pointButton.Background = colorButtonNotSelected;
            brushButton.Background = colorButtonNotSelected;
            colorFullButton.Background = colorButtonNotSelected;
            rectButton.Background = colorButtonNotSelected;
            selectButon.Background = colorButtonSelected;
            buttonSelectedString = selectButon.Name;
        }

        private void buttonFullColor(object sender, MouseButtonEventArgs e)
        {
            try
            {
                setBrush();
                if (buttonSelectedString == "colorFullButton" &&
                    sender.GetType().Name.ToString() == "Rectangle")
                {
                    Rectangle rect = (Rectangle)sender;
                    rect.Fill = color;
                }
            }
            catch (InvalidOperationException ex)
            {
            }
        }
      
        private void rectButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    prev = Mouse.GetPosition(canvas); 
                }
            }
            catch (InvalidOperationException ex)
            {
            }
        }
        private void rectButtonMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    canvas.Children.Remove(rect);
                    Point point = e.GetPosition(canvas);
                    double x = point.X - prev.X;
                    double y = point.Y - prev.Y;
                    rect = new Rectangle
                    {
                        Width = Math.Abs(point.X - prev.X),
                        Height = Math.Abs(point.Y - prev.Y),
                        Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                        Opacity = 0.2
                    };
                    rect.SetValue(Canvas.LeftProperty, (prev.X <= point.X) ? prev.X : point.X);
                    rect.SetValue(Canvas.TopProperty, (prev.Y <= point.Y) ? prev.Y : point.Y); 
                    rect.MouseUp += new MouseButtonEventHandler(rectButtonUp);
                    canvas.Children.Add(rect);
                }
            }
            catch (ArgumentException ex)
            {
            }
        }
        private void rectButtonUp(object sender, MouseEventArgs e)
        { 
            try
            {
                canvas.Children.Remove(rect);
                Point point = e.GetPosition(canvas);  
                double x = point.X - prev.X;
                double y = point.Y - prev.Y; 
                var rectangle = new Rectangle { Width = Math.Abs(point.X - prev.X), Height = Math.Abs(point.Y - prev.Y), Fill = color };
                rectangle.SetValue(Canvas.LeftProperty, (prev.X <= point.X)? prev.X:point.X );
                rectangle.SetValue(Canvas.TopProperty, (prev.Y <= point.Y) ? prev.Y : point.Y);
                rectangle.MouseDown += new MouseButtonEventHandler(buttonFullColor); 
                rectangle.Opacity = opacitiValue;
                canvas.Children.Add(rectangle);
            }
            catch (ArgumentException ex)
            {
            }
        }
         
        private void liderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            opacitiValue = sliderOpacity.Value / 100;
        }
     
        private void buttonBrushMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isPaint) return;
                brushCanvas = new Canvas();
                brushCanvas.Width = canvas.Width;
                brushCanvas.Height = canvas.Height;
                brushCanvas.Opacity = opacitiValue;
                canvas.Children.Add(brushCanvas);
                isPaint = true;
                prev = Mouse.GetPosition(canvas);
                var dot = new Ellipse { Width = brushSizeDigit, Height = brushSizeDigit, Fill = color };
                dot.SetValue(Canvas.LeftProperty, prev.X - brushSizeDigit / 2);
                dot.SetValue(Canvas.TopProperty, prev.Y - brushSizeDigit / 2);
                brushCanvas.Children.Add(dot);
            }
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
                    StrokeThickness = brushSizeDigit,
                    X1 = prev.X,
                    Y1 = prev.Y,
                    X2 = point.X,
                    Y2 = point.Y,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                prev = point;
                brushCanvas.Children.Add(line);
            } 
        }
        private void buttonBrushMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            { 
                isPaint = false;
            }
            catch {
                System.Windows.MessageBox.Show("isError");
            }
        }
    }
}
