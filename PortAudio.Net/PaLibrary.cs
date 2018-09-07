using System;
using System.Runtime.InteropServices;
using PortAudio.Net;
using static PortAudio.Net.PaBindings;

using unsigned_long_t = System.UInt64;
using pa_host_api_index_t = System.Int32;

namespace PortAudio.Net
{
    public class PaLibrary : IDisposable
    {
        private bool disposed = false;

        public int Version => (int)Pa_GetVersion();

        public PaVersionInfo VersionInfo => Marshal.PtrToStructure<PaVersionInfo>(Pa_GetVersionInfo());

        public PaDeviceInfo DefaultOutputDevice => Marshal.PtrToStructure<PaDeviceInfo>(Pa_GetDeviceInfo(Pa_GetDefaultOutputDevice()));

        public int DeviceCount => Pa_GetDeviceCount();

        private class StreamCallbackDelegateContainer
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
        }

        public static PaLibrary Initialize()
        {
            PaErrorException.ThrowIfError(Pa_Initialize());
            return new PaLibrary();
        }

        public PaDeviceInfo GetDeviceInfo(int index)
        {
            if (index < 0 || index >= DeviceCount)
                throw new ArgumentOutOfRangeException("index");
            return Marshal.PtrToStructure<PaDeviceInfo>(Pa_GetDeviceInfo(index));
        }

        private unsafe static PaBuffer PaBufferBySampleFormat(PaSampleFormat sampleFormat, void* pointer, int length, int channels)
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
        
        // Note: userData object cannot be reconstituted from IntPtr but thunking delegate can curry the userData, bypassing PortAudio with better efficiency
        private _PaStreamCallback PaStreamCallbackThunk(
            PaStreamCallback streamCallback,
            PaSampleFormat inputSampleFormat, PaSampleFormat outputSampleFormat,
            int numInputChannels, int numOutputChannels, object userData)
        {
            unsafe
            {
                var container = new StreamCallbackDelegateContainer(
                    streamCallback,
                    inputSampleFormat, outputSampleFormat,
                    numInputChannels, numOutputChannels, userData);
                return new _PaStreamCallback(container.StreamCallback);
            }
        }
        
        public PaStream OpenStream(
            PaStreamParameters? inputParameters, PaStreamParameters? outputParameters,
            double sampleRate, int framesPerBuffer, PaStreamFlags streamFlags,
            PaStreamCallback streamCallback, object userData)
        {
            var streamCallbackThunk = PaStreamCallbackThunk(
                streamCallback,
                inputParameters.HasValue ? inputParameters.Value.sampleFormat : 0,
                outputParameters.HasValue ? outputParameters.Value.sampleFormat : 0,
                inputParameters.HasValue ? inputParameters.Value.channelCount : 0,
                outputParameters.HasValue ? outputParameters.Value.channelCount : 0,
                userData);
            var stream_callback_handle = GCHandle.Alloc(streamCallbackThunk);
            var user_data_handle = GCHandle.Alloc(userData);
            IntPtr stream;
            unsafe
            {
                PaStreamParameters inputParametersTemp, outputParametersTemp;
                IntPtr inputParametersPtr = new IntPtr(0);
                if (inputParameters.HasValue)
                {
                    inputParametersPtr = new IntPtr(&inputParametersTemp);
                    Marshal.StructureToPtr<PaStreamParameters>(inputParameters.Value, inputParametersPtr, false);
                }
                IntPtr outputParametersPtr = new IntPtr(0);
                if (outputParameters.HasValue)
                {
                    outputParametersPtr = new IntPtr(&outputParametersTemp);
                    Marshal.StructureToPtr<PaStreamParameters>(outputParameters.Value, outputParametersPtr, false);
                }
                PaErrorException.ThrowIfError(Pa_OpenStream(
                    new IntPtr(&stream),
                    inputParametersPtr, outputParametersPtr,
                    sampleRate, (unsigned_long_t)framesPerBuffer, streamFlags,
                    streamCallbackThunk, new IntPtr(0)));
            }
            return new PaStream(stream, stream_callback_handle, user_data_handle);
        }
        
        public PaStream OpenDefaultStream(
            int numInputChannels, int numOutputChannels, PaSampleFormat sampleFormat,
            double sampleRate, int framesPerBuffer, PaStreamFlags streamFlags,
            PaStreamCallback streamCallback, object userData)
        {
            var streamCallbackThunk = PaStreamCallbackThunk(
                streamCallback,
                sampleFormat, sampleFormat,
                numInputChannels, numOutputChannels, userData);
            var stream_callback_handle = GCHandle.Alloc(streamCallbackThunk);
            var user_data_handle = GCHandle.Alloc(userData);
            IntPtr stream;
            unsafe
            {
                PaErrorException.ThrowIfError(Pa_OpenDefaultStream(
                    new IntPtr(&stream),
                    numInputChannels, numOutputChannels, sampleFormat,
                    sampleRate, (unsigned_long_t)framesPerBuffer, streamFlags,
                    streamCallbackThunk, new IntPtr(0)));
            }
            return new PaStream(stream, stream_callback_handle, user_data_handle);
        }

        private void Dispose(bool disposing)
        {
            PaErrorException.ThrowIfError(Pa_Terminate());
        }

        public void Dispose()
        {
            lock (this)
            {
                if (!disposed)
                {
                    disposed = true;
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
            }
        }

        ~PaLibrary()
        {
            if (!disposed)
            {
                disposed = true;
                Dispose(false);
            }
        }
    }
}