using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private double resizeOnWidth,resizeOnHeight, onFirst, zoomKoeficient, brushSizeDigit, opacitiValue = 1;
        private SolidColorBrush colorButtonSelected = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
            colorButtonNotSelected = new SolidColorBrush(Color.FromRgb(82, 83, 88)),
            color;
        private String buttonSelectedString = "",file="";
        private Point prev, pointToDrag;
        private Canvas brushCanvas;
        private Shape shape; 
        private RichTextBox richTextBox;
        private bool isPaint = false, resizeX = false, resizeY = false;
        ScaleTransform st = new ScaleTransform();

        public PhotoWindow()
        {
            InitializeComponent();
            onFirst = canvas.Height;
            canvas.LayoutTransform = st; 
            shape = new Line();
            zoomKoeficient = 550 / canvas.Height;
            sliderZoom.Value = 100;
            textFontBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            textFontBox.SelectedIndex = 0; 
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
                file = openFile.FileName;
                canvas.Background = new ImageBrush(source);
                canvas.Height = source.PixelHeight;
                canvas.Width = source.PixelWidth; 
                zoomKoeficient = 550 / canvas.Height;
                sliderZoom.Value = 100;
            }

        }
        //Сохранение //занимает много места y'hjjj,bn
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Изображение |*.png;*.jpeg;*.jpg;*.bmp";
            if (file.Length < 6) file = ".png";
           
            save.DefaultExt = file.Substring(file.IndexOf("."));
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
                file = save.FileName;
                save.Reset();
                canvas = null;
                if (File.Exists(file)) File.Delete(file);
                using (var fs = File.OpenWrite(file))
                {
                    BufferSave.Save(fs);
                    fs.Dispose();
                }
                  
                    BitmapImage source = new BitmapImage(new Uri(save.FileName));
                    canvas.Background = new ImageBrush(source);
                    canvas.Height = source.PixelHeight;
                    canvas.Width = source.PixelWidth; 
                sliderZoom.Value = 100;
            }
        }
        //Закрытие 
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        //зумирование 
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (onFirst == 0) return;
            double minZoom = (zoomKoeficient + (sliderZoom.Value / 100) - 1) * canvas.Height;
            if (minZoom < 250) return;
            double xc = (sliderZoom.Value / 100);
            st.ScaleX = zoomKoeficient + (sliderZoom.Value / 100) - 1;
            st.ScaleY = zoomKoeficient + (sliderZoom.Value / 100) - 1; 
            labelZoom.Content = sliderZoom.Value.ToString("#.##") + " %";
        }
        private void scrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                sliderZoom.Value += 10;

            else if (e.Delta < 0)
                sliderZoom.Value += 10;

        }
        private void sliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            opacitiValue = sliderOpacity.Value / 100;
            if (onFirst == 0) return;
            labelOpacity.Content = sliderOpacity.Value.ToString("#.##") + " %";
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
                    brushSize.Text = "10";
                }

                if (sizeBrush >= 1)
                {
                    brushSizeDigit = sizeBrush;
                }
                else
                {
                    brushSize.Text = "1";
                }
            }
            catch (InvalidCastException e)
            {
                brushSize.Text = "10";
            }

        }
        //Ввод размера кисти только цифрами
        private void brushSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            double sizeBrush = 0;
            Double.TryParse(brushSize.Text, out sizeBrush);

            if (!Char.IsDigit(e.Text, 0))
            { 
                if (sizeBrush <= 1) brushSize.Text = "1";
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
            setBrush();

        }
        private void fontSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            double sizeFont = 8;
            Double.TryParse(fontSize.Text, out sizeFont);

            if (!Char.IsDigit(e.Text, 0))
            {
                if (sizeFont <= 8) fontSize.Text = "8";
                if (sizeFont > 300) fontSize.Text = "300";
                e.Handled = true; 
            }
        }
        private void fontSize_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double sizeFont = 8;
            Double.TryParse(fontSize.Text, out sizeFont);

            if (e.Delta > 0 && sizeFont < 300)
                sizeFont += 1;

            else if (e.Delta < 0 && sizeFont >8)
                sizeFont -= 1; 
            fontSize.Text = sizeFont.ToString();
        }
        private void isTransformation_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var children = canvas.Children.OfType<UIElement>().ToList();
                foreach (UIElement element in children)
                {
                    if (element.GetType().Name.ToString() == "Canvas") continue;
                    element.MouseDown += new MouseButtonEventHandler(buttonPointMouseDown);
                    element.MouseMove += new MouseEventHandler(buttonPointMouseMove);
                    element.MouseUp += new MouseButtonEventHandler(buttonPointMouseUp); 
                }
                this.pointButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            catch (ArgumentException ex)
            {

            }
        }
        private void isTransformation_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var children = canvas.Children.OfType<UIElement>().ToList();
                foreach (UIElement element in children)
                {
                    if (element.GetType().Name.ToString() == "Canvas") continue;
                    element.MouseDown -= new MouseButtonEventHandler(buttonPointMouseDown);
                    element.MouseMove -= new MouseEventHandler(buttonPointMouseMove);
                    element.MouseUp -= new MouseButtonEventHandler(buttonPointMouseUp);
                    if (element.GetType().Name.ToString() != "RichTextBox") ((Shape)element).StrokeThickness = 0;
                    if (element.GetType().Name.ToString() == "RichTextBox") ((RichTextBox)element).BorderThickness = new Thickness(1);

                }
                cleanShapeCanvas();
            }
            catch (ArgumentException ex)
            {

            }
        }
        private void cleanShapeCanvas()
        {
            try
            {
                var children = canvas.Children.OfType<Shape>().ToList();
                Shape el = null;
                foreach (Shape element in children)
                {
                    if (element.Tag == "shapeTransform")
                    {
                        canvas.Children.Remove(element);
                    }
                    if (el != null && stringToCleanShapes(element) == stringToCleanShapes(el)) canvas.Children.Remove(element);
                    el = element;

                }
            }
            catch (ArgumentException ex)
            {

            }
        }
        private string stringToCleanShapes(Shape shape)
        {
            return "" + shape.Width + shape.Height + shape.ActualHeight + shape.ActualWidth + shape.Opacity;
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            setBrush();
            switch (buttonSelectedString)
            {
                case "pointButton":
                    buttonPointMouseDown(sender, e);
                    break;
                case "brushButton":
                    buttonBrushMouseDown(sender, e);
                    break;
                case "colorFullButton":
                    buttonClickEvent(sender, e);
                    break;
                case "rectButton":
                    rectButtonDown(sender, e);
                    break;
                case "ellipsButton":
                    ellipsButtonDown(sender, e);
                    break;
                case "lineButton":
                    buttonLineMouseDown(sender, e);
                    break;
                case "textButton":
                    buttonTextMouseDown(sender, e);
                    break;
                case "deleteButton":
                    buttonClickEvent(sender, e);
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
                    buttonPointMouseMove(sender, e);
                    break;
                case "brushButton":
                    buttonBrushMouseMove(sender, e);
                    break;
                case "colorFullButton":
                    break;
                case "rectButton":
                    rectButtonMove(sender, e);
                    break;
                case "ellipsButton":
                    ellipsButtonMove(sender, e);
                    break;
                case "lineButton":
                    buttonLineMouseMove(sender, e);
                    break;
                case "deleteButton":
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
                case "ellipsButton":
                    ellipsButtonUp(sender, e);
                    break;
                case "lineButton":
                    buttonLineMouseUp(sender, e);
                    break;
                case "deleteButton":
                    break;
                default:
                    break;
            }
        }

        private void scroll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            canvas_MouseDown(sender, e);
        }
        private void scroll_MouseMove(object sender, MouseEventArgs e)
        {
            canvas_MouseMove(sender, e);
        }
        private void scroll_MouseUp(object sender, MouseButtonEventArgs e)
        {
            canvas_MouseUp(sender, e);
        }

        private void pointButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            buttonSelected(pointButton);
        }
        private void brushButton_Click(object sender, RoutedEventArgs e)
        {
            setBrush();
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
        private void ellipsButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Cross;
            buttonSelected(ellipsButton);
        }
        private void lineButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Cross;
            buttonSelected(lineButton);
        }
        private void textButton_Click(object sender, RoutedEventArgs e)
        {
            scrollViewer.Cursor = Cursors.IBeam;
            buttonSelected(textButton);
        }
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {

            scrollViewer.Cursor = Cursors.Arrow;
            buttonSelected(deleteButton); 
        } 
        private void buttonSelected(Button selectButon)
        {
            if (buttonSelectedString != "") ((Button)this.FindName(buttonSelectedString)).Background = colorButtonNotSelected;
            cleanShapeCanvas(); 
            selectButon.Background = colorButtonSelected;
            buttonSelectedString = selectButon.Name;
            if (isTransformation.IsChecked.Value && buttonSelectedString != "pointButton") isTransformation.IsChecked = false;
        }

        private void buttonPointMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!isTransformation.IsChecked.Value) return;
                if (e.LeftButton == MouseButtonState.Pressed 
                    && sender.GetType().Name.ToString() != "Canvas"
                     && sender.GetType().Name.ToString() != "ScrollView")
                {
                    if (shape != null) shape.StrokeThickness = 0;
                    if (richTextBox != null) richTextBox.BorderThickness = new Thickness(1);
                    shape = null;
                    richTextBox = null;
                    isPaint = true;  
                    if (sender.GetType().Name.ToString() == "RichTextBox") {
                        richTextBox = (RichTextBox)sender; 
                        prev = new Point(Canvas.GetLeft(richTextBox), Canvas.GetTop(richTextBox)); 
                        resizeOnHeight = richTextBox.Height;
                        resizeOnWidth = richTextBox.Width;
                        richTextBox.BorderThickness = new Thickness(3);  
                    }
                    if (sender.GetType().Name.ToString() == "Rectangle"
                        || sender.GetType().Name.ToString() == "Ellipse") {
                        shape = (Shape)sender; 
                        prev = new Point(Canvas.GetLeft(shape), Canvas.GetTop(shape)); 
                        resizeOnHeight = shape.Height;
                        shape.StrokeThickness = 2;
                        shape.Stroke = new SolidColorBrush(Colors.AliceBlue);
                        resizeOnWidth = shape.Width;  
                    } 
                    pointToDrag = Mouse.GetPosition(canvas); 
                    if (scrollViewer.Cursor == Cursors.SizeWE || scrollViewer.Cursor == Cursors.SizeNWSE) resizeX = true;
                    if (scrollViewer.Cursor == Cursors.SizeNS || scrollViewer.Cursor == Cursors.SizeNWSE) resizeY = true; 

                }
            }
            catch (ArgumentException ex)
            {
            }
        }
        private void buttonPointMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!isTransformation.IsChecked.Value) return;
                bool resX = false, resY = false;
                double x1 = 0, y1 = 0;
                Point position=new Point(x1,y1);
                if (richTextBox != null) { x1 = richTextBox.Width;y1 = richTextBox.Height; position = e.GetPosition(richTextBox); }
                if (shape != null) { x1 = shape.Width; y1 = shape.Height; position = e.GetPosition(shape); }
                
                if (position.X >= x1- 30 && position.X <= x1 ) resX = true;
                if (position.Y >= y1 - 30 && position.Y <= y1) resY = true; 
                if (resX && resY) scrollViewer.Cursor = Cursors.SizeNWSE;
                else if (resX) scrollViewer.Cursor = Cursors.SizeWE;
                else if (resY) scrollViewer.Cursor = Cursors.SizeNS;
                else if (!resX || !resY) scrollViewer.Cursor = Cursors.Arrow; 
                if (e.LeftButton == MouseButtonState.Pressed && isPaint)
                { 
                    if (resizeX)
                    {
                        if (shape != null) shape.Width = resizeOnWidth + (e.GetPosition(canvas).X - pointToDrag.X); 
                        if (richTextBox != null) richTextBox.Width = resizeOnWidth + (e.GetPosition(canvas).X - pointToDrag.X);
                    }
                    if (resizeY)
                    {
                        if (shape != null) shape.Height = resizeOnHeight + (e.GetPosition(canvas).Y - pointToDrag.Y); 
                        if (richTextBox != null) richTextBox.Height = resizeOnHeight + (e.GetPosition(canvas).Y - pointToDrag.Y);
                    }
                    if (!resizeX && !resizeY)
                    {
                        double x = prev.X + (e.GetPosition(canvas).X - pointToDrag.X);
                        double y = prev.Y + (e.GetPosition(canvas).Y - pointToDrag.Y); 
                        if (shape != null) shape.SetValue(Canvas.LeftProperty, x);
                        if (shape != null) shape.SetValue(Canvas.TopProperty, y);
                        if (richTextBox != null) richTextBox.SetValue(Canvas.LeftProperty, x);
                        if (richTextBox != null) richTextBox.SetValue(Canvas.TopProperty, y);
                    }
                } else if (e.RightButton == MouseButtonState.Pressed) {
                    if (richTextBox != null) richTextBox.RenderTransform = new RotateTransform(e.GetPosition(canvas).Y - pointToDrag.Y, richTextBox.Width/2, richTextBox.Height/2);
                    if (shape != null) shape.RenderTransform = new RotateTransform(e.GetPosition(canvas).Y - pointToDrag.Y, shape.Width / 2, shape.Height / 2);
                }
                
            }
            catch (ArgumentException ex)
            {
            }
        }
        private void buttonPointMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                isPaint = false;
                resizeX = false;
                resizeY = false; 

            }
            catch (ArgumentException ex)
            {
            }
        }


        private void buttonBrushMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                setBrush();
                if (isPaint) return;
                canvas.Children.Remove(shape);
                brushCanvas = new Canvas();
                brushCanvas.Width = canvas.Width;
                brushCanvas.Height = canvas.Height;
                brushCanvas.Opacity = opacitiValue;
                brushCanvas.MouseDown += new MouseButtonEventHandler(buttonClickEvent);
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
            var point = Mouse.GetPosition(canvas);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!isPaint) return;
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
            else
            { 
                canvas.Children.Remove(shape);
                shape = new Ellipse
                {
                    Tag = "shapeTransform",
                    Width = brushSizeDigit,
                    Height = brushSizeDigit,
                    Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Opacity = 0.3
                };
                shape.SetValue(Canvas.LeftProperty, point.X - brushSizeDigit / 2);
                shape.SetValue(Canvas.TopProperty, point.Y - brushSizeDigit / 2);
                shape.MouseDown += new MouseButtonEventHandler(buttonBrushMouseDown);
                canvas.Children.Add(shape);

            }
        }
        private void buttonBrushMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {

                canvas.Children.Remove(shape);
                isPaint = false;
                cleanShapeCanvas();
            }
            catch
            {
                System.Windows.MessageBox.Show("isError");
            }
        }

        private void buttonClickEvent(object sender, MouseButtonEventArgs e)
        {
            try
            {
                setBrush();
                if (buttonSelectedString == "colorFullButton")
                {
                    switch (sender.GetType().Name.ToString())
                    {
                        case "Rectangle":
                            ((Rectangle)sender).Fill = color;
                            break;
                        case "Ellipse":
                            ((Ellipse)sender).Fill = color;
                            break;
                        case "Line":
                            ((Line)sender).Stroke = color;
                            break;

                    }
                }

                if (buttonSelectedString == "deleteButton")
                {
                    switch (sender.GetType().Name.ToString())
                    {
                        case "Rectangle":
                            canvas.Children.Remove((Rectangle)sender);
                            break;
                        case "Canvas":
                            canvas.Children.Remove((Canvas)sender);
                            break;
                        case "Ellipse":
                            canvas.Children.Remove((Ellipse)sender);
                            break;
                        case "Line":
                            canvas.Children.Remove((Line)sender);
                            break;
                        case "RichTextBox":
                            canvas.Children.Remove((RichTextBox)sender);
                            break;

                    }

                }

                if (buttonSelectedString == "pointButton")
                {
                    switch (sender.GetType().Name.ToString())
                    {
                        case "Rectangle":
                           // ((Rectangle)sender).Fill = color;
                            break;

                    }
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
                    shape = new Rectangle
                    {
                        Tag = "shapeTransform",
                        Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                        Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Opacity = 0.3
                    };
                    canvas.Children.Add(shape);
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
                    Point point = e.GetPosition(canvas);
                    double x = point.X - prev.X;
                    double y = point.Y - prev.Y;
                    shape.Width = Math.Abs(point.X - prev.X);
                    shape.Height = Math.Abs(point.Y - prev.Y);
                    shape.SetValue(Canvas.LeftProperty, (prev.X <= point.X) ? prev.X : point.X);
                    shape.SetValue(Canvas.TopProperty, (prev.Y <= point.Y) ? prev.Y : point.Y);
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
                canvas.Children.Remove(shape);
                Point point = e.GetPosition(canvas);
                double x = point.X - prev.X;
                double y = point.Y - prev.Y;
                var rectangle = new Rectangle { Width = shape.Width, Height = shape.Height, Fill = color };
                rectangle.SetValue(Canvas.LeftProperty, (prev.X <= point.X) ? prev.X : point.X);
                rectangle.SetValue(Canvas.TopProperty, (prev.Y <= point.Y) ? prev.Y : point.Y);
                rectangle.MouseDown += new MouseButtonEventHandler(buttonClickEvent);
                rectangle.Opacity = opacitiValue;
                canvas.Children.Add(rectangle);
                cleanShapeCanvas();
            }
            catch (ArgumentException ex)
            {
            }
        }

        private void ellipsButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    prev = Mouse.GetPosition(canvas);
                    shape = new Ellipse
                    {
                        Tag = "shapeTransform",
                        Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                        Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Opacity = 0.3
                    };
                    canvas.Children.Add(shape);
                }
            }
            catch (InvalidOperationException ex)
            {
            }
        }
        private void ellipsButtonMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point point = e.GetPosition(canvas);
                    double x = point.X - prev.X;
                    double y = point.Y - prev.Y;
                    shape.Width = Math.Abs(point.X - prev.X);
                    shape.Height = Math.Abs(point.Y - prev.Y);
                    shape.SetValue(Canvas.LeftProperty, (prev.X <= point.X) ? prev.X : point.X);
                    shape.SetValue(Canvas.TopProperty, (prev.Y <= point.Y) ? prev.Y : point.Y);
                }
            }
            catch (ArgumentException ex)
            {
            }
        }
        private void ellipsButtonUp(object sender, MouseEventArgs e)
        {
            try
            {
                canvas.Children.Remove(shape);
                Point point = e.GetPosition(canvas);
                double x = point.X - prev.X;
                double y = point.Y - prev.Y;
                var ellipsFn = new Ellipse { Width = shape.Width, Height = shape.Height, Fill = color };
                ellipsFn.SetValue(Canvas.LeftProperty, (prev.X <= point.X) ? prev.X : point.X);
                ellipsFn.SetValue(Canvas.TopProperty, (prev.Y <= point.Y) ? prev.Y : point.Y);
                ellipsFn.MouseDown += new MouseButtonEventHandler(buttonClickEvent);
                ellipsFn.Opacity = opacitiValue;
                canvas.Children.Add(ellipsFn);
                cleanShapeCanvas();
            }
            catch (ArgumentException ex)
            {
            }
        }


        private void buttonLineMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                setBrush();
                shape = new Line
                {
                    Tag = "shapeTransform",
                    Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Opacity = 0.3,
                    StrokeThickness = brushSizeDigit,
                    X1 = e.GetPosition(canvas).X,
                    Y1 = e.GetPosition(canvas).Y
                };
                canvas.Children.Add(shape);
                prev = Mouse.GetPosition(canvas);
            }
        }
        private void buttonLineMouseMove(object sender, MouseEventArgs e)
        {
            var point = Mouse.GetPosition(canvas);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ((Line)shape).X2 = point.X;
                ((Line)shape).Y2 = point.Y;
            }
        }
        private void buttonLineMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                canvas.Children.Remove(shape);
                Point point = e.GetPosition(canvas);
                var line = new Line
                {
                    Fill = color,
                    Stroke = color,
                    StrokeThickness = brushSizeDigit,
                    X1 = ((Line)shape).X1,
                    Y1 = ((Line)shape).Y1,
                    X2 = ((Line)shape).X2,
                    Y2 = ((Line)shape).Y2,
                };
                line.MouseDown += new MouseButtonEventHandler(buttonClickEvent);
                line.Opacity = opacitiValue;
                canvas.Children.Add(line);
                cleanShapeCanvas();
            }
            catch (ArgumentException ex)
            {
            }
        }

        private void buttonTextMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                prev = Mouse.GetPosition(canvas);
                RichTextBox text = new RichTextBox();
                text.AppendText("Введите текст");
                text.Background = new SolidColorBrush(Colors.Transparent);
                text.BorderBrush = new SolidColorBrush(Colors.Transparent);
                text.Document.TextAlignment = TextAlignment.Center;
                text.Width =  200;
                text.Height =   100; 
                Canvas.SetLeft(text, prev.X - text.Width / 2);
                Canvas.SetTop(text, prev.Y - text.Height / 2); 
                text.SelectionChanged += new RoutedEventHandler(setTextSetting);
                text.MouseMove += new MouseEventHandler(isTransform); 
                text.PreviewMouseDown += new MouseButtonEventHandler(buttonPointMouseDown);
                text.PreviewMouseMove += new MouseEventHandler(buttonPointMouseMove);
                text.PreviewMouseUp += new MouseButtonEventHandler(buttonPointMouseUp);
                setTextSetting(text, e);
                canvas.Children.Add(text);
            }
        }
        private void setTextSetting(object sender, RoutedEventArgs e)
        {
            try
            {
                RichTextBox text = (RichTextBox)sender;
                buttonClickEvent(sender, null); 
                TextRange textRange = new TextRange(text.Document.ContentStart, text.Document.ContentEnd);  
                text.Opacity = opacitiValue;
                text.FontFamily = (FontFamily)textFontBox.SelectedItem;
                text.FontSize =  Convert.ToDouble(fontSize.Text);
                text.FontStyle = (isItalic.IsChecked.Value) ? FontStyles.Italic : FontStyles.Normal;
                text.FontWeight = (isBold.IsChecked.Value) ? FontWeights.Bold : FontWeights.Normal; 
                text.Foreground = new SolidColorBrush(colorPickerFont.SelectedColor.Value);  
            }
            catch (Exception ex)
            {


            }
        }
        private void isTransform(object sender, RoutedEventArgs e)
        {
            try
            { 
                RichTextBox text = (RichTextBox)sender;

                if (isTransformation.IsChecked.Value)
                {
                      text.IsReadOnly = true;
                    Point position = Mouse.GetPosition(text);
                    bool resX = false, resY = false;
                    if (position.X >= text.Width - 30 && position.X <= text.Width) resX = true;
                    if (position.Y >= text.Height - 30 && position.Y <= text.Height) resY = true;
                    if (resX && resY) text.Cursor = Cursors.SizeNWSE;
                    else if (resX) text.Cursor = Cursors.SizeWE;
                    else if (resY) text.Cursor = Cursors.SizeNS;
                    else if (!resX || !resY) text.Cursor = Cursors.IBeam;

                }
                else
                {    
                    text.Cursor = Cursors.IBeam;
                    text.IsReadOnly = false; 
                }

            }
            catch (Exception ex)
            {


            }
        }
    }
}
