﻿using Microsoft.Win32;
using System;
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
        private double resizeOnWidth, resizeOnHeight, onFirst, zoomKoeficient, brushSizeDigit, opacitiValue = 1;
        private SolidColorBrush colorButtonSelected = new SolidColorBrush(Color.FromRgb(185, 185, 185)),
            colorButtonNotSelected = new SolidColorBrush(Color.FromRgb(112, 112, 112)),
            color = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
            fontColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private System.Windows.Forms.ColorDialog brushColorDialog = new System.Windows.Forms.ColorDialog(),
            fontColorDialog = new System.Windows.Forms.ColorDialog();
        private String buttonSelectedString = "", file = "";
        private Point prev, pointToDrag;
        private Canvas brushCanvas;
        private Shape shape;
        private RichTextBox richTextBox;
        private bool isPaint = false, resizeX = false, resizeY = false;
        private ScaleTransform st = new ScaleTransform();
        private Key key;

        public PhotoWindow()
        {
            try
            {
                InitializeComponent();
                shape = new Line();
                textFontBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
                textFontBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void MenuItemCreate(object sender, RoutedEventArgs e)
        {

            Create create = new Create();
            if (create.ShowDialog() == true)
            {
                if (canvas.Children.Count >= 2)
                    if (MessageBox.Show("Вы уверены? Поле рисования не пустое.", "Вы уверены",
                      MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No) return;
                createCanvas(create.HeightInt, create.WidthInt);
            }

        }

        private void createCanvas(int height, int width)
        {
            try
            {
                canvas.Children.Clear();
                canvas.Height = height;
                canvas.Width = width;
                canvas.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                onFirst = canvas.Height;
                lblName.Content = canvas.Height + " x " + canvas.Width;
                zoomKoeficient = 550 / canvas.Height;
                canvas.LayoutTransform = st;
                sliderZoom.Value++;
                sliderZoom.Value--;
                buttonSelected(pointButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Открытие файла
        private void MenuItemOpen(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Filter = "Изображение |*.png;*.jpeg;*.jpg;*.bmp";
                openFile.Title = "Открытие файла";
                openFile.FilterIndex = 1;
                openFile.RestoreDirectory = true;
                if (openFile.ShowDialog() == true)
                {
                    canvas.Children.Clear();
                    file = openFile.FileName.ToString();
                    BitmapImage source = (BitmapImage)BitmapFromUri(new Uri(openFile.FileName));
                    Rectangle rect = new Rectangle
                    {
                        Fill = new ImageBrush(source),
                        Height = source.PixelHeight,
                        Width = source.PixelWidth,
                        Tag = "PhotoRectangle"
                    };
                    rect.MouseDown += new MouseButtonEventHandler(buttonClickEvent);
                    canvas.Children.Add(rect); 
                    rect.SetValue(Canvas.LeftProperty, 0.0);
                    rect.SetValue(Canvas.TopProperty, 0.0);
                    canvas.Height = source.PixelHeight;
                    canvas.Width = source.PixelWidth;
                    zoomKoeficient = 550 / canvas.Height;
                    lblName.Content = " " + openFile.FileName + "    " + canvas.Height + " x " + canvas.Width;
                }

                sliderZoom.Value++;
                sliderZoom.Value--;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;

        }

        //Сохранение  
        private void MenuItemSaveAs(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Изображение |*.png;*.jpeg;*.jpg;*.bmp";
                if (file.Length < 6) file = ".png";

                save.DefaultExt = ".png";
                save.Title = "Сохранение файла";
                if (save.ShowDialog() == true)
                {
                    saveFile(save.FileName);
                    file = save.FileName;
                }

            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuItemSave(object sender, RoutedEventArgs e)
        {
            if (file != "") saveFile(file);
            else MenuItemSaveAs(sender, e);
        }

        private void saveFile(string fileSave)
        {
            try
            {
                st.ScaleX = 1;
                st.ScaleY = 1;
                var rtb = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96d, 96d, PixelFormats.Default);
                canvas.Measure(new Size((int)canvas.Width, (int)canvas.Height));
                canvas.Arrange(new Rect(new Size((int)canvas.Width, (int)canvas.Height)));
                rtb.Render(canvas);
                JpegBitmapEncoder BufferSave = new JpegBitmapEncoder();
                BufferSave.Frames.Add(BitmapFrame.Create(rtb));
                if (File.Exists(fileSave)) File.Delete(fileSave);
                using (var fs = File.OpenWrite(fileSave))
                {
                    BufferSave.Save(fs);
                    fs.Dispose();
                }

                sliderZoom.Value++;
                sliderZoom.Value--;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (canvas.Height!=0.0)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Сохранить изображение перед выходом?", "Выход",
                     MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

                if (messageBoxResult == MessageBoxResult.Cancel) e.Cancel = true;
                else if (messageBoxResult == MessageBoxResult.Yes) MenuItemSave(sender, null);
            }
        }


        private void CanvasClear(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
        }

        private void MenuItemAddImage(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Filter = "Изображение |*.png;*.jpeg;*.jpg;*.bmp";
                openFile.Title = "Открытие файла";
                openFile.FilterIndex = 1;
                openFile.RestoreDirectory = true;

                if (openFile.ShowDialog() == true)
                {
                    BitmapImage source = (BitmapImage)BitmapFromUri(new Uri(openFile.FileName));
                    Rectangle rect = new Rectangle
                    {
                        Fill = new ImageBrush(source),
                        Height = source.PixelHeight,
                        Width = source.PixelWidth,
                        Tag = "PhotoRectangle"
                    };
                    rect.MouseDown += new MouseButtonEventHandler(buttonClickEvent);
                    canvas.Children.Add(rect);
                    rect.SetValue(Canvas.LeftProperty, 15.0);
                    rect.SetValue(Canvas.TopProperty, 15.0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void reSizeCanvas(object sender, RoutedEventArgs e)
        {
            try
            {
                Change change = new Change();
                change.labelSize.Content = "Ширина - " + canvas.Width + ", Высота - " + canvas.Height;
                change.HeightBox.Text = canvas.Height.ToString();
                change.WidthBox.Text = canvas.Width.ToString();
                if (change.ShowDialog() == true) {
                    canvas.Height = change.NewHeight;
                    canvas.Width = change.NewWidth;
                    zoomKoeficient = 550 / canvas.Height;
                    lblName.Content = " " + file + "    " + canvas.Height + " x " + canvas.Width;
                }

                sliderZoom.Value++;
                sliderZoom.Value--;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void ZoomSliderValueChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (onFirst == 0) return;
            double minZoom = (zoomKoeficient + (sliderZoom.Value / 100) - 1) * canvas.Height;
            if (minZoom < 250) return;
            double xc = (sliderZoom.Value / 100);
            st.ScaleX = zoomKoeficient + (sliderZoom.Value / 100) - 1;
            st.ScaleY = zoomKoeficient + (sliderZoom.Value / 100) - 1;
            labelZoom.Content = sliderZoom.Value.ToString("#.##") + " %";

        }

        private void zoomMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (key == Key.LeftCtrl)
            {
                if (e.Delta > 0)
                {
                    sliderZoom.Value += 10;
                    scrollViewer.LineDown();
                    scrollViewer.LineDown();
                    scrollViewer.LineDown();
                }
                else if (e.Delta < 0)
                {
                    sliderZoom.Value -= 10;
                    scrollViewer.LineUp();
                    scrollViewer.LineUp();
                    scrollViewer.LineUp();
                }
            }
            else if (key == Key.LeftShift)
            {
                if (e.Delta > 0)
                {
                    scrollViewer.LineDown();
                    scrollViewer.LineDown();
                    scrollViewer.LineDown();
                }
                else if (e.Delta < 0)
                {
                    scrollViewer.LineUp();
                    scrollViewer.LineUp();
                    scrollViewer.LineUp();
                }
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + e.Delta);
            }
            if (buttonSelectedString == "brushButton")
            {
                if (e.Delta > 0)
                {
                    scrollViewer.LineUp();
                    scrollViewer.LineUp();
                    scrollViewer.LineUp();
                }
                else if (e.Delta < 0)
                {
                    scrollViewer.LineDown();
                    scrollViewer.LineDown();
                    scrollViewer.LineDown();
                }
            }
        }

        private void sliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            opacitiValue = sliderOpacity.Value / 100;
            if (onFirst == 0) return;
            labelOpacity.Content = sliderOpacity.Value.ToString("#.##") + " %";
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            key = e.Key;
            switch (key)
            {
                case Key.P:
                    scrollViewer.Cursor = Cursors.Arrow;
                    buttonSelected(pointButton);
                    break;
                case Key.B:
                    setBrush();
                    scrollViewer.Cursor = Cursors.Cross;
                    buttonSelected(brushButton);
                    break;
                case Key.C:
                    scrollViewer.Cursor = Cursors.Arrow;
                    buttonSelected(colorFullButton);
                    break;
                case Key.R:
                    scrollViewer.Cursor = Cursors.Cross;
                    buttonSelected(rectButton);
                    break;
                case Key.E:
                    scrollViewer.Cursor = Cursors.Cross;
                    buttonSelected(ellipsButton);
                    break;
                case Key.L:
                    scrollViewer.Cursor = Cursors.Cross;
                    buttonSelected(lineButton);
                    break;
                case Key.T:
                    scrollViewer.Cursor = Cursors.IBeam;
                    buttonSelected(textButton);
                    break;
                case Key.D:
                    scrollViewer.Cursor = Cursors.Arrow;
                    buttonSelected(deleteButton);
                    break;
                case Key.Space:
                    scrollViewer.Cursor = Cursors.ScrollAll; 
                    break;
                default:
                    break;
            }


        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            key = Key.U;
            switch (buttonSelectedString)
            {
                case "pointButton":
                    scrollViewer.Cursor = Cursors.Arrow;
                    break;
                case "brushButton":
                    scrollViewer.Cursor = Cursors.Cross;
                    break;
                case "colorFullButton":
                    scrollViewer.Cursor = Cursors.Arrow;
                    break;
                case "rectButton":
                    scrollViewer.Cursor = Cursors.Cross;
                    break;
                case "ellipsButton":
                    scrollViewer.Cursor = Cursors.Cross;
                    break;
                case "lineButton":
                    scrollViewer.Cursor = Cursors.Cross;
                    break;
                case "textButton":
                    scrollViewer.Cursor = Cursors.IBeam;
                    break;
                case "deleteButton":
                    scrollViewer.Cursor = Cursors.Arrow;
                    break;
                default:
                    break;
            }
        }
        private void setBrush()
        {
            try
            {
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
        private void colorBrushPicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            brushColorDialog.ShowDialog();
            System.Drawing.Color color1 = brushColorDialog.Color;
            Color colorс = Colors.Black;
            colorс.A = color1.A;
            colorс.R = color1.R;
            colorс.G = color1.G;
            colorс.B = color1.B;
            color = new SolidColorBrush(colorс);
            colorBrushPicker.Fill = color;
        }
        private void colorFontPicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            fontColorDialog.ShowDialog();
            System.Drawing.Color color1 = fontColorDialog.Color;
            Color colorс = Colors.Black;
            colorс.A = color1.A;
            colorс.R = color1.R;
            colorс.G = color1.G;
            colorс.B = color1.B;
            fontColor = new SolidColorBrush(colorс);
            colorFontPicker.Fill = fontColor;
        }
        private void brushSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                if (richTextBox != null)
                {
                    richTextBox.SelectAll();
                    richTextBox.Focus();
                }
                e.Handled = true;
            }
        }
        private void fontSize_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double sizeFont = 8;
            Double.TryParse(fontSize.Text, out sizeFont);

            if (e.Delta > 0 && sizeFont < 300)
                sizeFont += 1;

            else if (e.Delta < 0 && sizeFont > 8)
                sizeFont -= 1;
            fontSize.Text = sizeFont.ToString();
            if (richTextBox != null)
            {
                richTextBox.SelectAll();
                richTextBox.Focus();
            }
        }
        private void textFontBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (richTextBox != null)
            {
                richTextBox.SelectAll();
                richTextBox.Focus();
            }
        }
        private void colorPickerFont_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (richTextBox != null)
            {
                richTextBox.SelectAll();
                richTextBox.Focus();
            }
        }
        private void isBold_Checked(object sender, RoutedEventArgs e)
        {
            if (richTextBox != null)
            {
                richTextBox.SelectAll();
                richTextBox.Focus();
            }
        }
        private void isBold_Unchecked(object sender, RoutedEventArgs e)
        {
            if (richTextBox != null)
            {
                richTextBox.SelectAll();
                richTextBox.Focus();
            }
        }
        private void isItalic_Unchecked(object sender, RoutedEventArgs e)
        {
            if (richTextBox != null)
            {
                richTextBox.SelectAll();
                richTextBox.Focus();
            }
        }
        private void isItalic_Checked(object sender, RoutedEventArgs e)
        {
            if (richTextBox != null)
            {
                richTextBox.SelectAll();
                richTextBox.Focus();
            }
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void cleanShapeCanvas()
        {
            try
            {
                var children = canvas.Children.OfType<Shape>().ToList();
                Shape el = null;
                shape = null;
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private string stringToCleanShapes(Shape shape)
        {
            return "" + shape.Width + shape.Height + shape.ActualHeight + shape.ActualWidth + shape.Opacity;
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas.Height == 0) return;
            if (key == Key.Space)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    prev = Mouse.GetPosition(canvas);
                }
            }
            else
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
        }
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas.Height == 0) return;
            if (key == Key.Space)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + (prev.Y - Mouse.GetPosition(canvas).Y));
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (prev.X - Mouse.GetPosition(canvas).X));
                }
            }
            else
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
        }
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (canvas.Height == 0) return;
            if (key == Key.Space)
            {

            }
            else
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
            if (buttonSelectedString != "") ((Button)this.FindName(buttonSelectedString)).BorderBrush = colorButtonNotSelected;
            cleanShapeCanvas();
            selectButon.BorderBrush = colorButtonSelected;
            buttonSelectedString = selectButon.Name;
            if (buttonSelectedString == "pointButton") isTransformation.IsChecked = true;
            if (isTransformation.IsChecked.Value && buttonSelectedString != "pointButton") isTransformation.IsChecked = false;
            scrollViewer.Focus();
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
                            if (((Rectangle)sender).Tag == "PhotoRectangle") return;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                    if (sender.GetType().Name.ToString() == "RichTextBox")
                    {
                        richTextBox = (RichTextBox)sender;
                        prev = new Point(Canvas.GetLeft(richTextBox), Canvas.GetTop(richTextBox));
                        resizeOnHeight = richTextBox.Height;
                        resizeOnWidth = richTextBox.Width;
                        richTextBox.BorderThickness = new Thickness(3);
                    }
                    if (sender.GetType().Name.ToString() == "Rectangle"
                        || sender.GetType().Name.ToString() == "Ellipse")
                    {
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void buttonPointMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!isTransformation.IsChecked.Value) return;
                bool resX = false, resY = false;
                double x1 = 0, y1 = 0;
                Point position = new Point(0.0, 0.0);
                if (shape != null) { x1 = shape.Width; y1 = shape.Height; position = e.GetPosition(shape); }
                if (richTextBox != null) { x1 = richTextBox.Width; y1 = richTextBox.Height; position = e.GetPosition(richTextBox); }

                if (position.X >= x1 - 30 && position.X < x1 && position.Y > 0 && position.Y < y1) resX = true;
                if (position.Y >= y1 - 30 && position.Y < y1 && position.X > 0 && position.X < x1) resY = true;
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
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    if (richTextBox != null) richTextBox.RenderTransform = new RotateTransform((e.GetPosition(canvas).X - pointToDrag.X) / 4, richTextBox.Width / 2, richTextBox.Height / 2);
                    if (shape != null) shape.RenderTransform = new RotateTransform((e.GetPosition(canvas).X - pointToDrag.X)/4, shape.Width /2 , shape.Height / 2);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void buttonBrushMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (key == Key.Space) return;
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void buttonBrushMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (key == Key.Space) return;
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
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeLineJoin = PenLineJoin.Round
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
                    shape.MouseWheel += new MouseWheelEventHandler(zoomMouseWheel);
                    shape.SetValue(Canvas.LeftProperty, point.X - brushSizeDigit / 2);
                    shape.SetValue(Canvas.TopProperty, point.Y - brushSizeDigit / 2);
                    shape.MouseDown += new MouseButtonEventHandler(buttonBrushMouseDown);
                    canvas.Children.Add(shape);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void rectButtonUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (shape == null) return;
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ellipsButtonUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (shape == null) return;
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void buttonLineMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void buttonLineMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var point = Mouse.GetPosition(canvas);
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    ((Line)shape).X2 = point.X;
                    ((Line)shape).Y2 = point.Y;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void buttonLineMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (shape == null) return;
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonTextMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    prev = Mouse.GetPosition(canvas);
                    RichTextBox text = new RichTextBox();
                    text.AppendText("Введите текст");
                    text.Background = new SolidColorBrush(Colors.Transparent);
                    text.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    text.Document.TextAlignment = TextAlignment.Center;
                    text.Width = 200;
                    text.Height = 100;
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                text.FontSize = Convert.ToDouble(fontSize.Text);
                text.FontStyle = (isItalic.IsChecked.Value) ? FontStyles.Italic : FontStyles.Normal;
                text.FontWeight = (isBold.IsChecked.Value) ? FontWeights.Bold : FontWeights.Normal;
                text.Foreground = fontColor;
                richTextBox = text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                MessageBox.Show(ex.Message);
            }
        }
    }
}
