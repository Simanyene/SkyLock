using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace SkyLock.Services
{
    [ContentProperty(nameof(Key))]
    public class TranslateExtensionService : IMarkupExtension<BindingBase>
    {
        public string Key { get; set; } = "";

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            return new Binding(
                path: $"[{Key}]",
                mode: BindingMode.OneWay,
                source: LocalizationService.Instance);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}