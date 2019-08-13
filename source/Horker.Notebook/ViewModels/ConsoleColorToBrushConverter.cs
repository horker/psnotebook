using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Horker.ViewModels
{
    public  class ConsoleColorBrush
    {
        public ConsoleColor ConsoleColor { get; internal set; }
        public Brush Brush { get; internal set; }
    }

    public class ConsoleColorToBrushConverter
    {
        private static ConsoleColorBrush[] _definitions =
        {
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.Black, Brush = Brushes.Black }, // 0, 0, 0
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.DarkBlue, Brush = Brushes.DarkBlue }, // 0, 0, 128
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.DarkGreen, Brush = Brushes.DarkGreen }, // 0, 128, 0
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.DarkCyan, Brush = Brushes.DarkCyan }, // 0, 128, 128
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.DarkRed, Brush = Brushes.DarkRed }, // 128, 0, 0
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.DarkMagenta, Brush = Brushes.DarkMagenta }, // 1, 36, 86
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.DarkYellow, Brush = Brushes.DarkKhaki }, // 238, 237, 240
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.Gray, Brush = Brushes.Gray }, // 192, 192, 192
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.DarkGray, Brush = Brushes.DarkGray }, // 128, 128, 128
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.Blue, Brush = Brushes.Blue }, // 0, 0, 255
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.Green, Brush = Brushes.Green }, // 0, 255, 0
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.Cyan, Brush = Brushes.Cyan }, // 0, 255, 255
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.Red, Brush = Brushes.Red }, // 255, 0, 0
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.Magenta, Brush = Brushes.Magenta }, // 255, 0, 255
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.Yellow, Brush = Brushes.Yellow }, // 255, 255, 0
            new ConsoleColorBrush() { ConsoleColor = ConsoleColor.White, Brush = Brushes.White } // 255, 255, 255
        };

        private static Dictionary<ConsoleColor, Brush> _map;

        static ConsoleColorToBrushConverter()
        {
            _map = new Dictionary<ConsoleColor, Brush>();

            foreach (var def in _definitions)
                _map.Add(def.ConsoleColor, def.Brush);
        }

        public static Brush GetBrush(ConsoleColor color)
        {
            return _map[color];
        }
    }
}
