using System;
using Jaeger.Propagation;
using OpenTracing.Propagation;

namespace JDSF.Common.TraceExtensions
{
    public class JaegerInjector<T>
    { 
        public IFormat<T> Format { get; set; }

        public Injector<T> Injector { get; set; }
    }
}
