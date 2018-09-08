using System;
using System.Runtime.InteropServices;
using PortAudio.Net;
using static PortAudio.Net.PaBindings;

using pa_time_t = System.Double;
using signed_long_t = System.Int64;

namespace PortAudio.Net
{

    public class PaStream : IDisposable
    {
        private bool disposed;
        private IntPtr stream;
        private GCHandle stream_callback_handle;
        private GCHandle user_data_handle;

        public void StartStream()
        {
            PaErrorException.ThrowIfError(Pa_StartStream(stream));
        }

        public void StopStream()
        {
            PaErrorException.ThrowIfError(Pa_StopStream(stream));
        }

        public void AbortStream()
        {
            PaErrorException.ThrowIfError(Pa_AbortStream(stream));
        }

        public bool IsStreamStopped
        {
            get
            {
                var code = Pa_IsStreamStopped(stream);
                switch (code)
                {
                    case 0:
                        return false;
                    case 1:
                        return true;
                    default:
                        PaErrorException.ThrowIfError(code);
                        break;
                }
                return false;
            }
        }

        public bool IsStreamActive
        {
            get
            {
                var code = Pa_IsStreamActive(stream);
                switch (code)
                {
                    case 0:
                        return false;
                    case 1:
                        return true;
                    default:
                        PaErrorException.ThrowIfError(code);
                        break;
                }
                return false;
            }
        }

        public PaStreamInfo? StreamInfo
        {
            get
            {
                var ptr = Pa_GetStreamInfo(stream);
                if (ptr == IntPtr.Zero)
                    return null;
                return Marshal.PtrToStructure<PaStreamInfo>(ptr);
            }
        }

        public double GetStreamCpuLoad() => Pa_GetStreamCpuLoad(stream);

        public pa_time_t GetStreamTime() => Pa_GetStreamTime(stream);

        public signed_long_t GetStreamReadAvailable() => Pa_GetStreamReadAvailable(stream);

        public signed_long_t GetStreamWriteAvailable() => Pa_GetStreamWriteAvailable(stream);

        internal PaStream(IntPtr stream, GCHandle streamCallbackHandle, GCHandle userDataHandle)
        {
            this.stream = stream;
            this.stream_callback_handle = streamCallbackHandle;
            this.user_data_handle = userDataHandle;
        }        

        private void Dispose(bool disposing)
        {
            unsafe
            {
                stream_callback_handle.Free();
                user_data_handle.Free();
                PaErrorException.ThrowIfError(Pa_CloseStream(stream));
            }
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

        ~PaStream()
        {
            if (!disposed)
            {
                disposed = true;
                Dispose(false);
            }
        }
    }
}
