using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public static class MaterialIcons
    {
        public const string Newspaper = "\uef5e";
        public const string DateRange = "\uef8c";
        public const string Visibility = "\ue8f4";
        public const string Hot = "\ue5db";

        public static FormattedString ToFontIcon(string icon, double size = 20, Color? color = null)
        {
            return new FormattedString
            {
                Spans =
            {
                new Span
                {
                    Text = icon,
                    FontFamily = "MaterialIcons",
                    FontSize = size,
                    TextColor = color ?? Colors.Gray
                }
            }
            };
        }
    }

    public class MaterialIconsExtension : IMarkupExtension<string>
    {
        public string Glyph { get; set; }

        public string ProvideValue(IServiceProvider serviceProvider)
        {
            var field = typeof(MaterialIcons).GetField(Glyph, BindingFlags.Public | BindingFlags.Static);
            return field?.GetValue(null) as string;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }
}
