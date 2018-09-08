using System;
using System.Runtime.InteropServices;

using unsigned_long_t = System.UInt64;

namespace PortAudio.Net
{
    internal class StreamCallbackDelegateContainer
    {
        private PaStreamCallback streamCallback;
        private PaSampleFormat inputSampleFormat;
        private PaSampleFormat outputSampleFormat;
        int numInputChannels;
        int numOutputChannels;
        object userData;

        public unsafe StreamCallbackDelegateContainer (
            PaStreamCallback streamCallback,
            PaSampleFormat inputSampleFormat, PaSampleFormat outputSampleFormat,
            int numInputChannels, int numOutputChannels, object userData)
        {
            this.streamCallback = streamCallback;
            this.inputSampleFormat = inputSampleFormat;
            this.outputSampleFormat = outputSampleFormat;
            this.numInputChannels = numInputChannels;
            this.numOutputChannels = numOutputChannels;
            this.userData = userData;
        }

        public unsafe PaStreamCallbackResult  StreamCallback(
            void* input, void* output,
            unsigned_long_t frameCount, IntPtr timeInfo,
            PaStreamCallbackFlags statusFlags, IntPtr garbage)
        {
            return streamCallback(
                PaBufferBySampleFormat(inputSampleFormat, input, (int)frameCount, numInputChannels),
                PaBufferBySampleFormat(outputSampleFormat, output, (int)frameCount, numOutputChannels),
                (int)frameCount, Marshal.PtrToStructure<PaStreamCallbackTimeInfo>(timeInfo),
                statusFlags, userData);
        }

        private unsafe PaBuffer PaBufferBySampleFormat(PaSampleFormat sampleFormat, void* pointer, int length, int channels)
        {
            switch (sampleFormat & ~PaSampleFormat.paCustomFormat)
            {
                case 0:
                    return null;
                case PaSampleFormat.paFloat32:
                    return new PaBuffer<float>(pointer, length, channels);
                case PaSampleFormat.paInt32:
                    return new PaBuffer<Int32>(pointer, length, channels);
                case PaSampleFormat.paInt16:
                    return new PaBuffer<Int16>(pointer, length, channels);
                case PaSampleFormat.paInt8:
                    return new PaBuffer<SByte>(pointer, length, channels);
                case PaSampleFormat.paUInt8:
                    return new PaBuffer<Byte>(pointer, length, channels);
                default:
                    return new PaBuffer(pointer);
            }
        }
    }
}