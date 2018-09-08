using System;
using System.Numerics;
using PortAudio.Net;

namespace PortAudio.Net.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine($"Using PortAudio version:\n  {PaLibrary.VersionInfo.versionText}");
            Console.WriteLine();

            using (var paLibrary = PaLibrary.Initialize())
            {
                PrintDevices(paLibrary);
                
                double sampleRate = 44100;

                var outIndex = ChooseDevice(paLibrary);

                var outputParameters = new PaStreamParameters()
                    {
                        device = outIndex,
                        channelCount = 1,
                        sampleFormat = PaSampleFormat.paFloat32,
                        suggestedLatency = paLibrary.GetDeviceInfo(outIndex).Value.defaultHighOutputLatency,
                        hostApiSpecificStreamInfo = IntPtr.Zero
                    };
                
                var callbackData = new SineCallbackData()
                    {
                        osc = new Complex(1, 0),
                        delta = Complex.Exp(new Complex(0, 440 * 2 * Math.PI / sampleRate))
                    };
                
                using (var stream = paLibrary.OpenStream(
                    null, outputParameters,
                    sampleRate, 512, PaStreamFlags.paNoFlag,
                    SineCallback, callbackData))
                {
                    stream.StartStream();
                    Console.WriteLine("Press any key to stop...");
                    Console.ReadKey();
                    stream.StopStream();
                }
            }
        }

        static int ChooseDevice(PaLibrary paLibrary)
        {
            int index;
            while (true)
            {
                Console.Write($"Select Device (0 - {paLibrary.DeviceCount - 1}): ");
                if (!int.TryParse(Console.ReadLine(), out index))
                    continue;
                if (index >= 0 && index < paLibrary.DeviceCount)
                    return index;
            }
        }

        static void PrintDevices(PaLibrary paLibrary)
        {
            var deviceCount = paLibrary.DeviceCount;
            string[,] data = new string[deviceCount + 1, 6];
            data[0, 0] = "#";
            data[0, 1] = "#in";
            data[0, 2] = "#out";
            data[0, 3] = "type";
            data[0, 4] = "api";
            data[0, 5] = "name";
            for (int n = 0; n < deviceCount; n++)
            {
                var row = n + 1;
                var deviceInfo = paLibrary.GetDeviceInfo(n).Value;
                var hostApiInfo = paLibrary.GetHostApiInfo(deviceInfo.hostApi).Value;
                data[row, 0] = n.ToString();
                data[row, 1] = deviceInfo.maxInputChannels.ToString();
                data[row, 2] = deviceInfo.maxOutputChannels.ToString();
                data[row, 3] = hostApiInfo.type.ToString();
                data[row, 4] = hostApiInfo.name;
                data[row, 5] = deviceInfo.name;
            }
            int[] widths = new int[6];
            for (int row = 0; row < data.GetLength(0); row++)
                for (int col = 0; col < data.GetLength(1); col++)
                    if (data[row, col].Length > widths[col])
                        widths[col] = data[row, col].Length;
            var sep = " ";
            var width = 0;
            for (int col = 0; col < widths.Length; col++) width += widths[col];
            width += sep.Length * (widths.Length - 1);
            var head = " Devices ";
            head = new String('=', (width - head.Length) / 2) + head;
            head += new String('=', width - head.Length);
            Console.WriteLine(head);
            for (int row = 0; row < data.GetLength(0); row++)
            {
                for (int col = 0; col < data.GetLength(1); col++)
                {
                    var pad = new String(' ', widths[col] + 1 - data[row, col].Length);
                    pad += col == data.GetLength(1) - 1 ? "\n" : " ";
                    Console.Write(data[row, col] + pad);
                }
                if (row == 0)
                    Console.WriteLine(new String('=', width));
            }
            Console.WriteLine();
        }

        class SineCallbackData
        {
            public Complex osc { get; set; }
            public Complex delta { get; set; }
        }

        private static PaStreamCallbackResult SineCallback(
            PaBuffer input, PaBuffer output,
            int frameCount, PaStreamCallbackTimeInfo timeInfo,
            PaStreamCallbackFlags statusFlags, object userData)
        {
            var data = (SineCallbackData)userData;
            var outBuffer = (PaBuffer<float>)output;
            var outSpan = outBuffer.Span;
            for (int n = 0; n < frameCount; n++)
            {
                outSpan[n] = 0.1f * (float)data.osc.Real;
                data.osc *= data.delta;;
            }
            data.osc *= (3 - (data.osc.Real * data.osc.Real + data.osc.Imaginary * data.osc.Imaginary)) / 2;
            return PaStreamCallbackResult.paContinue;
        }
    }
}
