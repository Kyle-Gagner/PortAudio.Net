using System;
using System.Runtime.InteropServices;

namespace PortAudio.Net
{
    public class PaBuffer : IDisposable
    {
        private bool owning;
        
        private bool disposed = false;

        private object lockObject = new object();

        public IntPtr Pointer { get; }

        public int Frames { get; }

        public int Channels { get; }

        private PaBuffer(int channels, int frames)
        {
            Channels = channels;
            Frames = frames;
        }

        public PaBuffer(int size, int channels, int frames)
        {
            Pointer = Marshal.AllocHGlobal(size);
            owning = true;
        }

        public PaBuffer(IntPtr pointer, int channels, int frames) : this(channels, frames)
        {
            Pointer = pointer;
            owning = false;
        }

        private void Dispose(bool disposing)
        {
            Marshal.FreeHGlobal(Pointer);
        }

        public void Dispose()
        {
            if (owning)
            {
                lock (lockObject)
                {
                    if (!disposed)
                    {
                        disposed = true;
                        Dispose(true);
                        GC.SuppressFinalize(this);
                    }
                }
            }
            else
            {
                GC.SuppressFinalize(this);
            }
        }

        ~PaBuffer()
        {
            if (owning)
            {
                if (!disposed)
                {
                    disposed = true;
                    Dispose(false);
                }
            }
        }
    }

    public class PaBuffer<T>: PaBuffer where T: unmanaged
    {
        public PaBuffer(int channels, int frames) :
            base(channels * frames * Marshal.SizeOf(typeof(T)), channels, frames) {}

        public PaBuffer(IntPtr pointer, int channels, int frames) : base(pointer, channels, frames) {}

        public Span<T> Span
        {
            get
            {
                unsafe
                {
                    return new Span<T>(Pointer.ToPointer(), Channels * Frames);
                }
            }
        }
    }

    public class PaNonInterleavedBuffer<T> : PaBuffer where T: unmanaged
    {
        private PaBuffer<T>[] channelBuffers;

        public PaNonInterleavedBuffer(int channels, int frames) :
            base(channels * Marshal.SizeOf(typeof(T)), channels, frames)
        {
            channelBuffers = new PaBuffer<T>[Channels];
            unsafe
            {
                IntPtr* ptr = (IntPtr*)Pointer.ToPointer();
                for (int channelIndex = 0; channelIndex < Channels; channelIndex++)
                {
                    var channelBuffer = new PaBuffer<T>(1, Frames);
                    channelBuffers[channelIndex] = channelBuffer;
                    ptr[channelIndex] = channelBuffer.Pointer;
                }
            }
        }

        public unsafe PaNonInterleavedBuffer(IntPtr pointer, int channels, int frames) : base(pointer, channels, frames)
        {
            channelBuffers = new PaBuffer<T>[Channels];
            unsafe
            {
                IntPtr* ptr = (IntPtr*)Pointer.ToPointer();
                for (int channelIndex = 0; channelIndex < Channels; channelIndex++)
                    channelBuffers[channelIndex] = new PaBuffer<T>(ptr[channelIndex], channels, frames);
            }
        }

        public PaBuffer<T> GetChannel(int channelIndex)
        {
            if (channelIndex < 0 || channelIndex >= Channels)
                throw new ArgumentException(
                    "Channel indices must be between 0 and Channels - 1 inclusive.",
                    nameof(channelIndex));
            return channelBuffers[channelIndex];
        }
    }
}