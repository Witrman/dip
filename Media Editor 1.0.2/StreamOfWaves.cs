using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Media_Editor_1._0._2
{
    public class StreamOfWaves
    {
        double pos = 0;
        List<StreamWave> waves = new List<StreamWave>();
        private WaveFormRenderer waveFormRenderer;
        private WaveFormRendererSettings settings;
        private PeakProvider provider;
        DispatcherTimer timer = new DispatcherTimer(); 


        public StreamOfWaves()
        {
            settings = new WaveFormRendererSettings();
            provider = new MaxPeakProvider();
            settings.TopHeight = 50;
            settings.BottomHeight = 50;
            settings.BackgroundColor = System.Drawing.Color.Transparent;
            waveFormRenderer = new WaveFormRenderer();
        }
        public System.Windows.Shapes.Rectangle Add(BlockAlignReductionStream stream, WaveOutEvent output, AudioFileReader streamTrek)
        {
            try
            {
                // waves.Add(new StreamWave(stream, output));
                settings.Width = Convert.ToInt32(streamTrek.TotalTime.TotalSeconds * 100);
                System.Windows.Shapes.Rectangle waveRect = RenderThreadFunc(streamTrek, provider, settings);
                waveRect.Width = streamTrek.TotalTime.TotalMilliseconds / 80;
                return waveRect;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }
        private System.Windows.Shapes.Rectangle RenderThreadFunc(AudioFileReader streamTrek, PeakProvider peakProvider, WaveFormRendererSettings settings)
        {
            try
            {
                System.Drawing.Image image;
                image = waveFormRenderer.Render(streamTrek, peakProvider, settings);
                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream memory = new MemoryStream())
                {
                    image.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle
                {
                    Height = 100,
                    Width = 2000
                };
                rectangle.Fill = new ImageBrush(bitmapImage);
                rectangle.HorizontalAlignment = HorizontalAlignment.Left;
                return rectangle;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }
        public void Remove(int index)
        {
            waves.RemoveAt(index);
        }
        public void Dispose()
        {
            foreach (var wave in waves)
            {
                wave.DisposeWave();
            }
        }
        public void TrimMp3(string inputPath, string outputPath, TimeSpan? begin, TimeSpan? end)
        {
            if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");

            using (var reader = new Mp3FileReader(inputPath))
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }
        }
        public void Play()
        {

            foreach (var wave in waves)
            {
                wave.Play();
            }
            //   timer.Tick += new EventHandler(plpl);
            //   timer.Interval = new TimeSpan(0, 0, 1);
            //   timer.Start();
        }

        public void plpl(object sender, EventArgs e)
        {

        }

        public void Stop()
        {
            foreach (var wave in waves)
            {
                wave.Pause();
            }

            //    timer.Stop();
        }

        public void ChangePos(int index, double pos)
        {
            waves[index].SetPos(pos);
        }



    }
}