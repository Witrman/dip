using NAudio.Wave;
using System;
using System.Windows.Threading;

namespace Media_Editor_1._0._2
{
    public class StreamWave
    {
        BlockAlignReductionStream stream;
        WaveOutEvent output; 
        DispatcherTimer timer = new DispatcherTimer();
        double pos = 0, posOnTrek = 0,timeKoef = 0;

        public StreamWave(BlockAlignReductionStream stream, WaveOutEvent output)
        {
            this.output = output;
            this.stream = stream;
            timer.Tick += new EventHandler(player);
            timer.Interval = new TimeSpan(0, 0, 0, 1);
        }

        public long GetPosition()
        {
            return stream.Position;
        }

        public void SetPosition(long position)
        {
            stream.Position = position;
        }

        public double GetPos()
        {
            return pos;
        }

        public void SetPos(double pos)
        {
            this.pos = pos; 
        }

        public void Play()
        { 
            timer.Start(); 
        }
        public void player(object sender, EventArgs e)
        {
            posOnTrek++;
            if (posOnTrek >= pos) {
                if (output.PlaybackState != PlaybackState.Playing) output.Play(); 
            } 
        }
        public void Pause()
        {
            timer.Stop();
            output.Pause(); 
        }

        public void Stop() {
            timer.Stop();
            output.Stop();
        }

        public void DisposeWave()
        {
            if (output != null)
            {
                if (output.PlaybackState == NAudio.Wave.PlaybackState.Playing) output.Stop();
                output.Dispose();
                output = null;
            }
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }


    }
}