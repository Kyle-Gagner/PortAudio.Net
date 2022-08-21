using System;
using System.Runtime.InteropServices;

using unsigned_long_t = System.UInt64;

namespace PortAudio.Net
{
    internal class StreamFinishedCallbackContainer
    {
        private PaStreamFinishedCallback callbackProvider;
        private object userData;

        public _PaStreamFinishedCallback Callback { get; }

        public StreamFinishedCallbackContainer(PaStreamFinishedCallback callbackProvider, object userData)
        {
            this.callbackProvider = callbackProvider;
            this.userData = userData;
            this.Callback = new _PaStreamFinishedCallback(CallbackMethod);
        }

        public unsafe void CallbackMethod(IntPtr garbage)
        {
            callbackProvider(userData);
        }
    }
}