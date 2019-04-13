using System;
using JDSF.Common.Util;
using Xunit;
using Xunit.Abstractions;

namespace JDSF.Common.Test
{
    public class UnitTest1
    {

        private readonly ITestOutputHelper testOutput;

        public UnitTest1(ITestOutputHelper output)
        {
            testOutput = output;
        }

        [Fact]
        public void Test1()
        {
            testOutput.WriteLine(NetworkUtil.GetHostIp());

            testOutput.WriteLine(NetworkUtil.GetHostIp("10.12.209.43"));
        }
    }
}
