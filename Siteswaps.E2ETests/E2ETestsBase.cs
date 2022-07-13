using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Siteswaps.E2ETests;

[Category("E2E")]
public abstract class E2ETestsBase : PageTest
{
    protected static string BaseUrl { get; private set; } = "https://siteswaps.netlify.app/";

    [ModuleInitializer]
    public static void ModuleInitialize()
    {
        var baseUrl = Environment.GetEnvironmentVariable("E2E_TEST_BASEURL");
        if (baseUrl != null)
        {
            BaseUrl = baseUrl;
        }
        
        if (Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("HEADED","1");
        }
    }
}