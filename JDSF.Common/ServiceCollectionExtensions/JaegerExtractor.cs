using System;
using Jaeger.Propagation;
using OpenTracing.Propagation;

namespace OpenTracingDemo.Common.ServiceCollectionExtensions
{
    public class JaegerExtractor<T>
    {
        public IFormat<T> Format { get; set; }

        public Extractor<T> Extractor { get; set; }
    }
}
