using System;
using System.Runtime.InteropServices;

namespace PortAudio.Net
{
    public class PaBuffer : IDisposable
    {
        protected bool disposed = false;
        protected IntPtr pointer;
        protected bool hglobal = false;

        unsafe public PaBuffer(void* pointer)
        {
            this.pointer = new IntPtr(pointer);
        }

        public static explicit operator IntPtr(PaBuffer buffer) => buffer.pointer;

        public unsafe static explicit operator void*(PaBuffer buffer) => (void*)buffer.pointer.ToPointer();

        private void Dispose(bool disposing)
        {
            unsafe
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        public void Dispose()
        {
            if (hglobal)
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
            else
            {
                GC.SuppressFinalize(this);
            }
        }

        ~PaBuffer()
        {
            if (hglobal)
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
        private int length;
        int channels;
        private int size;

        public Span<T> Span
        {
            get
            {
                unsafe
                {
                    return new Span<T>(pointer.ToPointer(), (int)size);
                }
            }
        }

        public int Length => length;

        public int Channels => channels;

        public unsafe PaBuffer(int length, int channels) : base((void*)0)
        {
            var array = new T[length];
            this.length = length;
            unsafe
            {
                this.size = sizeof(T) * length * channels;
            }
            this.pointer = Marshal.AllocHGlobal(this.size);
            this.hglobal = true;
        }

        public unsafe PaBuffer(void* pointer, int length, int channels) : base(pointer)
        {
            this.length = length;
            this.channels = channels;
            this.size = sizeof(T) * length * channels;
        }

        public PaBuffer Slice(int start)
        {
            if (start > length)
                throw new ArgumentOutOfRangeException("start");
            unsafe
            {
                return new PaBuffer<T>((T*)pointer.ToPointer() + start, length - start, channels);
            }
        }

        public PaBuffer Slice(int start, int length)
        {
            if (start > this.length)
                throw new ArgumentOutOfRangeException("start");
            if (length > this.length - start)
                throw new ArgumentOutOfRangeException("length");
            unsafe
            {
                return new PaBuffer<T>((T*)pointer.ToPointer() + start, length - start, channels);
            }
        }

        public static unsafe explicit operator T*(PaBuffer<T> buffer) => (T*)buffer.pointer.ToPointer();
    }
}