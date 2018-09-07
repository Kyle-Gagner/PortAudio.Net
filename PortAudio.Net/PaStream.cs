using System;
using System.Runtime.InteropServices;
using PortAudio.Net;
using static PortAudio.Net.PaBindings;

namespace PortAudio.Net
{

    public class PaStream : IDisposable
    {
        private bool disposed;
        private IntPtr stream;
        private GCHandle stream_callback_handle;
        private GCHandle user_data_handle;

        public bool IsActive
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

        public bool IsStopped
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

        internal PaStream(IntPtr stream, GCHandle streamCallbackHandle, GCHandle userDataHandle)
        {
            this.stream = stream;
            this.stream_callback_handle = streamCallbackHandle;
            this.user_data_handle = userDataHandle;
        }

        public void Start()
        {
            PaErrorException.ThrowIfError(Pa_StartStream(stream));
        }

        public void Stop()
        {
            PaErrorException.ThrowIfError(Pa_StopStream(stream));
        }

        public void Abort()
        {
            PaErrorException.ThrowIfError(Pa_AbortStream(stream));
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
