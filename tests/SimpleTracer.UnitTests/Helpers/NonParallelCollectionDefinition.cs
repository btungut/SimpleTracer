using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SimpleTracer.UnitTests
{
    [CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
    public class NonParallelCollectionDefinition
    {
    }
}
