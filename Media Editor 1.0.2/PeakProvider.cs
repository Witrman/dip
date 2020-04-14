using NAudio.Wave;

namespace Media_Editor_1._0._2
{ 
        public abstract class PeakProvider : IPeakProvider
        {
            protected ISampleProvider Provider { get; private set; }
            protected int SamplesPerPeak { get; private set; }
            protected float[] ReadBuffer { get; private set; }

            public void Init(ISampleProvider provider, int samplesPerPeak)
            {
                Provider = provider;
                SamplesPerPeak = samplesPerPeak;
                ReadBuffer = new float[samplesPerPeak];
            }

            public abstract PeakInfo GetNextPeak();
        } 
}