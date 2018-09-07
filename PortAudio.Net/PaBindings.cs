using System;
using System.Runtime.InteropServices;

using int_t = System.Int32;
using char_t = System.Byte;
using long_t = System.Int64;
using pa_device_index_t = System.Int32;
using pa_host_api_index_t = System.Int32;
using pa_time_t = System.Double;
using pa_sample_format_t = System.Int64;
using pa_stream_flags_t = System.Int64;
using pa_stream_callback_flags_t = System.Int64;
using enum_default_t = System.Int32;
using pa_error_t = System.Int32;
using unsigned_long_t = System.UInt64;
using signed_long_t = System.Int64;

namespace PortAudio.Net
{
    public enum PaHostApiTypeId : enum_default_t
    {
        paInDevelopment   =  0,
        paDirectSound     =  1,
        paMME             =  2,
        paASIO            =  3,
        paSoundManager    =  4,
        paCoreAudio       =  5,
        paOSS             =  7,
        paALSA            =  8,
        paAL              =  9,
        paBeOS            = 10,
        paWDMKS           = 11,
        paJACK            = 12,
        paWASAPI          = 13,
        paAudioScienceHPI = 14
    }

    public enum PaSampleFormat : pa_sample_format_t
    {
        paFloat32        = 0x00000001,
        paInt32          = 0x00000002,
        paInt24          = 0x00000004,
        paInt16          = 0x00000008,
        paInt8           = 0x00000010,
        paUInt8          = 0x00000020,
        paCustomFormat   = 0x00010000,
        paNonInterleaved = 0x80000000
    }

    public enum PaStreamCallbackResult : enum_default_t
    {
        paContinue = 0,
        paComplete = 1,
        paAbort    = 2
    }

    public enum PaStreamFlags : pa_stream_flags_t
    {
        paNoFlag                                = 0x00000000,
        paClipOff                               = 0x00000001,
        paDitherOff                             = 0x00000002,
        paNeverDropInput                        = 0x00000004,
        paPrimeOutputBuffersUsingStreamCallback = 0x00000008,
        paPlatformSpecificFlags                 = 0xFFFF0000
    }

    public enum PaStreamCallbackFlags : pa_stream_callback_flags_t
    {
        paInputUnderflow  = 0x00000001,
        paInputOverflow   = 0x00000002,
        paOutputUnderflow = 0x00000004,
        paOutputOverflow  = 0x00000008,
        paPrimingOutput   = 0x00000010
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaVersionInfo
    {
        public int_t versionMajor;
        public int_t versionMinor;
        public int_t versionSubMinor;
        [MarshalAs(UnmanagedType.LPStr)]
        public string versionControlRevision;
        [MarshalAs(UnmanagedType.LPStr)]
        public string versionText;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaHostApiInfo
    {
        public int_t structVersion;
        public PaHostApiTypeId type;
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;
        public int_t deviceCount;
        public pa_device_index_t defaultInputDevice;
        public pa_device_index_t defaultOutputDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaHostErrorInfo
    {
        public PaHostApiTypeId hostApiType;
        public long_t errorCode;
        [MarshalAs(UnmanagedType.LPStr)]
        public string errorText;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaDeviceInfo
    {
        public int_t structVersion;
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;
        public pa_host_api_index_t hostApi;
        public int maxInputChannels;
        public int maxOutputChannels;
        public pa_time_t defaultLowInputLatency;
        public pa_time_t defaultLowOutputLatency;
        public pa_time_t defaultHighInputLatency;
        public pa_time_t defaultHighOutputLatency;
        public double defaultSampleRate;

        public PaHostApiInfo hostApiInfo => Marshal.PtrToStructure<PaHostApiInfo>(PaBindings.Pa_GetHostApiInfo(hostApi));
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaStreamParameters
    {
        public pa_device_index_t device;
        public int channelCount;
        public PaSampleFormat sampleFormat;
        public pa_time_t suggestedLatency;
        public IntPtr hostApiSpecificStreamInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaStreamCallbackTimeInfo
    {
        public pa_time_t inputBufferAdcTime;
        public pa_time_t currentTime;
        public pa_time_t outputBufferDacTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PaStreamInfo
    {
        public int_t structVersion;
        public pa_time_t inputLatency;
        public pa_time_t outputLatency;
        public double sampleRate;
    }

    //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal unsafe delegate PaStreamCallbackResult _PaStreamCallback(
        void* input, void* output,
        unsigned_long_t frameCount, IntPtr timeInfo,
        PaStreamCallbackFlags statusFlags, IntPtr userData);
        
    internal delegate void _PaStreamFinishedCallback(IntPtr userData);

    public delegate PaStreamCallbackResult PaStreamCallback(
        PaBuffer input, PaBuffer output,
        int frameCount, PaStreamCallbackTimeInfo timeInfo,
        PaStreamCallbackFlags statusFlags, object userData);

    internal class PaBindings
    {
        [DllImport("libportaudio", EntryPoint = "Pa_GetVersion")]
        public static extern int_t Pa_GetVersion();

        [DllImport("libportaudio", EntryPoint = "Pa_GetVersionInfo")]
        public unsafe static extern IntPtr Pa_GetVersionInfo();

        [DllImport("libportaudio", EntryPoint = "Pa_GetErrorText")]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string Pa_GetErrorText(pa_error_t errorCode);

        [DllImport("libportaudio", EntryPoint = "Pa_Initialize")]
        public static extern pa_error_t Pa_Initialize();

        [DllImport("libportaudio", EntryPoint = "Pa_Terminate")]
        public static extern pa_error_t Pa_Terminate();

        [DllImport("libportaudio", EntryPoint = "Pa_GetHostApiCount")]
        public static extern pa_host_api_index_t Pa_GetHostApiCount();

        [DllImport("libportaudio", EntryPoint = "Pa_GetDefaultHostApi")]
        public static extern pa_host_api_index_t Pa_GetDefaultHostApi();

        [DllImport("libportaudio", EntryPoint = "Pa_GetHostApiInfo")]
        public static extern IntPtr Pa_GetHostApiInfo(pa_host_api_index_t hostApi);

        [DllImport("libportaudio", EntryPoint = "Pa_HostApiTypeIdToHostApiIndex")]
        public static extern pa_host_api_index_t Pa_HostApiTypeIdToHostApiIndex(PaHostApiTypeId type);

        [DllImport("libportaudio", EntryPoint = "Pa_HostApiDeviceIndexToDeviceIndex")]
        public static extern pa_device_index_t Pa_HostapiDeviceIndexToDeviceIndex(pa_host_api_index_t hostApi, pa_host_api_index_t hostApiDeviceIndex);

        [DllImport("libportaudio", EntryPoint = "Pa_GetLastHostErrorInfo")]
        public static extern IntPtr Pa_GetLastHostErrorInfo();

        [DllImport("libportaudio", EntryPoint = "Pa_GetDeviceCount")]
        public static extern pa_device_index_t Pa_GetDeviceCount();

        [DllImport("libportaudio", EntryPoint = "Pa_GetDefaultInputDevice")]
        public static extern pa_device_index_t Pa_GetDefaultInputDevice();

        [DllImport("libportaudio", EntryPoint = "Pa_GetDefaultOutputDevice")]
        public static extern pa_device_index_t Pa_GetDefaultOutputDevice();

        [DllImport("libportaudio", EntryPoint = "Pa_GetDeviceInfo")]
        public static extern IntPtr Pa_GetDeviceInfo(pa_device_index_t device);

        [DllImport("libportaudio", EntryPoint = "Pa_IsFormatSupported")]
        public static extern pa_error_t Pa_IsFormatSupported(IntPtr inputParameters, IntPtr outputParameters, double sampleRate);

        [DllImport("libportaudio", EntryPoint = "Pa_OpenStream")]
        public static extern pa_error_t Pa_OpenStream(
            IntPtr stream,
            IntPtr inputParameters, IntPtr outputParameters,
            double sampleRate, unsigned_long_t framesPerBuffer, PaStreamFlags streamFlags,
            _PaStreamCallback streamCallback, IntPtr userData);
        
        [DllImport("libportaudio", EntryPoint = "Pa_OpenDefaultStream")]
        public static extern pa_error_t Pa_OpenDefaultStream(
            IntPtr stream,
            int_t numInputChannels, int numOutputChannels, PaSampleFormat sampleFormat,
            double sampleRate, unsigned_long_t framesPerBuffer, PaStreamFlags streamFlags,
            _PaStreamCallback streamCallback, IntPtr userData);
        
        [DllImport("libportaudio", EntryPoint = "Pa_CloseStream")]
        public static extern pa_error_t Pa_CloseStream(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_SetStreamFinishedCallback")]
        public static extern pa_error_t Pa_SetStreamFinishedCallback(IntPtr stream, _PaStreamFinishedCallback streamFinishedCallback);

        [DllImport("libportaudio", EntryPoint = "Pa_StartStream")]
        public static extern pa_error_t Pa_StartStream(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_StopStream")]
        public static extern pa_error_t Pa_StopStream(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_AbortStream")]
        public static extern pa_error_t Pa_AbortStream(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_IsStreamStopped")]
        public static extern pa_error_t Pa_IsStreamStopped(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_IsStreamActive")]
        public static extern pa_error_t Pa_IsStreamActive(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_GetStreamInfo")]
        public static extern IntPtr Pa_GetStreamInfo(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_GetStreamTime")]
        public static extern pa_time_t Pa_GetStreamTime(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_GetStreamCpuLoad")]
        public static extern double Pa_GetStreamCpuLoad(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_ReadStream")]
        public static extern  pa_error_t Pa_ReadStream(IntPtr stream, IntPtr buffer, unsigned_long_t frames);

        [DllImport("libportaudio", EntryPoint = "Pa_WriteStream")]
        public  static extern pa_error_t Pa_WriteStream(IntPtr stream, IntPtr buffer, unsigned_long_t frames);

        [DllImport("libportaudio", EntryPoint = "Pa_GetStreamReadAvailable")]
        public static extern signed_long_t Pa_GetStreamReadAvailable(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_GetStreamWriteAvailable")]
        public static extern signed_long_t Pa_GetStreamWriteAvailable(IntPtr stream);

        [DllImport("libportaudio", EntryPoint = "Pa_GetSampleSize")]
        public static extern pa_error_t Pa_GetSampleSize(PaSampleFormat format);

        [DllImport("libportaudio", EntryPoint = "Pa_Sleep")]
        public static extern void Pa_Sleep(long_t msec);
    }
}