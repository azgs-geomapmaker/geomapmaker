using System;
using System.Windows.Media;

namespace Geomapmaker._helpers
{
    public static class ColorConverter
    {
        // Convert Color to RGB
        public static string ColorToRGB(Color? color)
        {
            if (color == null)
            {
                return null;
            }

            // GeMS: (1) each RGB color value is integer between 0 and 255; (2) values are left - padded with zeroes so that each consists of 3 digits; (3) values are separated by commas with no spaces(for example, nnn,nnn,nnn).

            return $"{color.Value.R:000},{color.Value.G:000},{color.Value.B:000}";
        }

        public static Color? RGBtoColor(string rgb)
        {
            // Null if the string is empty
            if (string.IsNullOrEmpty(rgb))
            {
                return null;
            }

            string[] strArray = { };

            if (rgb.Contains(","))
            {
                // Split by comma 
                strArray = rgb.Split(',');
            }
            else if (rgb.Contains(";"))
            {
                // Split by semicolon 
                strArray = rgb.Split(';');
            }

            // Color from RGB bytes
            return strArray.Length != 3 ? null : (Color?)Color.FromRgb(Convert.ToByte(strArray[0]), Convert.ToByte(strArray[1]), Convert.ToByte(strArray[2]));
        }

        // Convert RGB string to Hex
        public static string RGBtoHex(string rgb)
        {
            // Black if the string is empty
            if (string.IsNullOrEmpty(rgb))
            {
                return "#000000";
            }

            string[] strArray = { };

            if (rgb.Contains(","))
            {
                // Split by comma 
                strArray = rgb.Split(',');
            }
            else if (rgb.Contains(";"))
            {
                // Split by semicolon 
                strArray = rgb.Split(';');
            }

            if (strArray.Length != 3)
            {
                // Black as default
                return "#000000";
            }

            return Color.FromRgb(Convert.ToByte(strArray[0]), Convert.ToByte(strArray[1]), Convert.ToByte(strArray[2])).ToString();
        }
    }
}
