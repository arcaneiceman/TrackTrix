using DemoApp.CoreAudio.Common;
using System.Threading.Tasks;

namespace DemoApp.Services.Interfaces
{
    public interface IWaveFileWriter
    {
        Task Begin(string fileName, WaveFormat waveFormat);
        void Write(byte[] buffer, int bytesRecorded);
        void End();
    }
}
