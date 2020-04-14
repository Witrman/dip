using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Media_Editor_1._0._2
{
    /// <summary>
    /// Логика взаимодействия для AudioWindow.xaml
    /// </summary>
    public partial class AudioWindow : Window
    {
        private StreamOfWaves streamWaves;
        private Line lineOfTime;
        private ScaleTransform transform = new ScaleTransform();
        private Point pointToDrag, prev;
        private double timeKoef = 0.75;
        DispatcherTimer timer = new DispatcherTimer();
        public AudioWindow()
        {
            InitializeComponent();
            streamWaves = new StreamOfWaves();
            lineOfTime = new Line
            {
                X1 = 1,
                Y1 = 11,
                X2 = 1,
                Y2 = 2000,
                Stroke = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                StrokeThickness = 2,
            };
            canvas.Children.Add(lineOfTime);
            canvas.RenderTransform = transform;
            sliderOfTime.Width = canvas.Width + 10;
        }

        private void addFile(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Audio File (*.mp3;*.wav)|*.mp3;*.wav;";
                if (open.ShowDialog().Value == false) return;
                WaveStream pcm = new AudioFileReader(open.FileName);
                double millisecond = pcm.TotalTime.TotalMilliseconds / 80 ;
                BlockAlignReductionStream stream = new BlockAlignReductionStream(pcm);
                WaveOutEvent output = new WaveOutEvent();
                output.Init(stream);
                Rectangle rectangle = streamWaves.Add(stream, output, new AudioFileReader(open.FileName));
                rectangle.MouseDown += new MouseButtonEventHandler(waveMouseOnDown);
                rectangle.MouseMove += new MouseEventHandler(waveMouseOnMove);
                rectangle.MouseUp += new MouseButtonEventHandler(waveMouseOnUp);
                rectangle.SetValue(Canvas.TopProperty, (canvas.Children.Count - 2) * 100.0 + 20);
                if (canvas.Width < rectangle.Width) canvas.Width = rectangle.Width;
                canvas.Children.Add(rectangle);
                timeKoef = (canvas.Width / rectangle.Width * millisecond) / 100000;
                //streamTrek.TotalTime.TotalMilliseconds/80
                //lbl1.Content = pcm.TotalTime+"  " + pcm.TotalTime.TotalSeconds+ "  " + stream.Length+ "    " + pcm.TotalTime.Ticks ; 
                if (canvas.Children.Count * 100 - 80 >= canvas.Height) canvas.Height = (canvas.Children.Count - 2) * 100.0 + 20;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void streamPlayStop(object sender, RoutedEventArgs e)
        {
            try
            {
                streamWaves.Play();
                timer.Tick += new EventHandler(changePosition);
                timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
                timer.Start(); 

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void changePosition(object sender, EventArgs e)
        {
            sliderOfTime.Value += timeKoef;
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            streamWaves.Stop();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            streamWaves.Dispose();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.Height = scrollViewer.ActualHeight;
            canvas.Width = scrollViewer.ActualWidth;
            sliderOfTime.Width = canvas.Width + 10;
        }
        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            sliderOfTime.Width = canvas.Width + 10; 
        }
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            waveMouseOnDown(sender, e);
        }
        private void scrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            waveMouseOnDown(sender, e);
        }
        private void scrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            waveMouseOnMove(sender, e);
        }
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            waveMouseOnMove(sender, e);
        }
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {

            waveMouseOnUp(sender, e);
        }
        private void scrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            waveMouseOnUp(sender, e);
        }
        private void waveMouseOnDown(object sender, MouseEventArgs e)
        {
            if (sender.GetType().Name.ToString() != "Rectangle") return;
            Rectangle rect = (Rectangle)sender;
            pointToDrag = Mouse.GetPosition(canvas);
            prev = new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect));
        }
        private void waveMouseOnMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender.GetType().Name.ToString() != "Rectangle") return;
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Rectangle rect = (Rectangle)sender;
                    if (double.IsNaN(prev.X)) prev.X = 0.0;
                    double x = prev.X + (e.GetPosition(canvas).X - pointToDrag.X);
                    if (x < 0.0) x = 0.0;
                    rect.SetValue(Canvas.LeftProperty, x);
                    lbl1.Content = prev + "    " + pointToDrag + "    " + e.GetPosition(canvas);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void waveMouseOnUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender.GetType().Name.ToString() != "Rectangle") return;
                Rectangle rect = (Rectangle)sender;
                prev = new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect));
                if (double.IsNaN(prev.X)) prev.X = 0.0;
                if (prev.X + rect.Width > canvas.Width) canvas.Width = prev.X + rect.Width;
                streamWaves.ChangePos(canvas.Children.IndexOf(rect) - 2, prev.X);
                
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void sliderOfTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //   sliderOfTime.SelectionEnd = sliderOfTime.Value;
            lineOfTime.X1 = (canvas.Width * sliderOfTime.Value / 3) / 100;
            lineOfTime.X2 = (canvas.Width * sliderOfTime.Value / 3) / 100;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            transform.ScaleX += 0.1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            transform.ScaleX -= 0.1;
            //   canvas.Children[0].SetValue(LeftProperty, 200.0);
        }






    }
}
