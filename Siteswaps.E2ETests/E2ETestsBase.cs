using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Siteswaps.E2ETests;

[Category("E2E")]
public abstract class E2ETestsBase : PageTest
{
    protected string BaseUrl { get; private set; } = default!;
    protected string ExpertUi => BaseUrl + "/generator";

    [SetUp]
    public void ModuleInitialize()
    {
        BaseUrl =
            Environment.GetEnvironmentVariable("E2E_TEST_BASEURL")
            ?? throw new NotSupportedException("E2E_TEST_BASEURL is not set");
        Console.WriteLine($"Testing against: {BaseUrl}");
    }

    [ModuleInitializer]
    public static void ModuleInitializer()
    {
        if (Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("HEADED", "1");
        }
    }
}
