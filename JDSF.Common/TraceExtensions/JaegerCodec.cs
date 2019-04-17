using System;
using Jaeger.Propagation;
using OpenTracing.Propagation;

namespace JDSF.Common.TraceExtensions
{
    public class JaegerCodec<T>  
    {
        public IFormat<T> Format { get; set; }

        public Codec<T> Codec { get; set; }
    }
}
