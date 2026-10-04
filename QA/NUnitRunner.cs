using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;

public static class NUnitRunner
{
    public static int Main(string[] args)
    {
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(Assembly.GetExecutingAssembly(), new Dictionary<string, object>());
        var result = runner.Run(TestListener.NULL, TestFilter.Empty);
        File.WriteAllText(args[0], result.ToXml(true).OuterXml);
        Console.WriteLine("NUnit pure C# tests: " + result.PassCount + " passed, " + result.FailCount + " failed, " + result.SkipCount + " skipped.");
        if (result.FailCount > 0) Console.WriteLine(result.ToXml(true).OuterXml);
        return result.FailCount == 0 ? 0 : 1;
    }
}
