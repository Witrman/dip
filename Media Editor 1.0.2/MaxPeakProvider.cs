using System.Linq;

namespace Media_Editor_1._0._2
{
    public class MaxPeakProvider : PeakProvider
    {
        public override PeakInfo GetNextPeak()
        {
            var samplesRead = Provider.Read(ReadBuffer, 0, ReadBuffer.Length);
            var max = (samplesRead == 0) ? 0 : ReadBuffer.Take(samplesRead).Max();
            var min = (samplesRead == 0) ? 0 : ReadBuffer.Take(samplesRead).Min();
            return new PeakInfo(min, max);
        }
    }
}