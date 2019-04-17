using System;
using Jaeger.Propagation;
using OpenTracing.Propagation;

namespace JDSF.Common.TraceExtensions
{
    public class JaegerExtractor<T>
    {
        public IFormat<T> Format { get; set; }

        public Extractor<T> Extractor { get; set; }
    }
}
