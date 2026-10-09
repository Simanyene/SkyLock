using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace SkyLock.Services;

public static class StyleResourceService
{
    public static Style Get(string key)
    {
        if (Application.Current is not null &&
            Application.Current.Resources.TryGetValue(
                key, out object value) &&
            value is Style style)
        {
            return style;
        }

        throw new KeyNotFoundException(
            $"Style '{key}' was not found in application resources.");
    }
}