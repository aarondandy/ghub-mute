using System;

namespace GHubMute
{
    public struct LogiColor
    {
        public static LogiColor[] ParseMultiple(string argumentValue)
        {
            var colors = argumentValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return ParseMultiple(colors);
        }

        public static LogiColor[] ParseMultiple(string[] argumentValues) =>
            Array.ConvertAll(argumentValues, c => new LogiColor(c));

        public static LogiColor Black = new LogiColor();

        public LogiColor(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        public LogiColor(string value)
        {
            Red = 0;
            Green = 0;
            Blue = 0;

            var components = value.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (components.Length > 0)
            {
                byte.TryParse(components[0], out Red);
                if (components.Length > 1)
                {
                    byte.TryParse(components[1], out Green);
                    if (components.Length > 2)
                    {
                        byte.TryParse(components[2], out Blue);
                    }
                }
            }
        }

        public readonly byte Red;
        public readonly byte Green;
        public readonly byte Blue;
    }
}
