// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;

namespace Autofac.Extensions.DependencyInjection.Test;

public static class TestCulture
{
    public static void With(CultureInfo culture, Action test)
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        var originalUICulture = Thread.CurrentThread.CurrentUICulture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.CurrentCulture.ClearCachedData();
        CultureInfo.CurrentUICulture.ClearCachedData();
        try
        {
            test?.Invoke();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUICulture;
            CultureInfo.CurrentCulture.ClearCachedData();
            CultureInfo.CurrentUICulture.ClearCachedData();
        }
    }
}
