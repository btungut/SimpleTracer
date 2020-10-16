using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTracer
{
    public class PredefinedEventsCollector
    {
        internal readonly List<ISourceDefinition> SourceDefinitions;

        public readonly SystemNetHttpEvents SystemNetHttp;
        public readonly TplEvents Tpl;
        public readonly DotNetRuntimeEvents DotNetRuntime;

        internal PredefinedEventsCollector()
        {
            SourceDefinitions = new List<ISourceDefinition>();

            SystemNetHttp = new SystemNetHttpEvents(this);
            Tpl = new TplEvents(this);
            DotNetRuntime = new DotNetRuntimeEvents(this);
        }

        internal PredefinedEventsCollector Add(ISourceDefinition definition)
        {
            SourceDefinitions.Add(definition);
            return this;
        }
    }
}
