using NAudio.Wave;

namespace Media_Editor_1._0._2
{
    public class StreamWave
    {
        BlockAlignReductionStream stream;
        WaveOutEvent output;
        double position = 0;

        public StreamWave(BlockAlignReductionStream stream, WaveOutEvent output)
        {
            this.output = output;
            this.stream = stream; 
        }

        public long GetPosition()
        {
            return stream.Position;
        }

        public void SetPosition(long position)
        {
            stream.Position = position;
        }

        public void Play()
        {
            output.Play();
        }

        public void Pause()
        {
            output.Pause();

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