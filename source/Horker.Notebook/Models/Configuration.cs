using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Horker.Notebook.Models
{
    public class Configuration
    {
        public static FontFamily FontFamily { get; set; } = new FontFamily("Consolas");

        public static double FontSize { get; set; } = 14;

        private static int _consoleWidthInChars = 96;

        public static int ConsoleWidthInChars
        {
            get => _consoleWidthInChars;
            set
            {
                _consoleWidthInChars = value;
                _consoleWidth = double.NaN;
            }
        }

        private static double _consoleWidth = double.NaN;

        public static double ConsoleWidth
        {
            get
            {
                if (double.IsNaN(_consoleWidth))
                    _consoleWidth = GetWidthBasedOnNumberOfCharacters(ConsoleWidthInChars);

                return _consoleWidth;
            }
        }

        public static double MaxOutputHeight => FontSize * 25;

        // Helper methods

        private static double GetWidthBasedOnNumberOfCharacters(int charCount)
        {
            var l = new Label()
            {
                FontFamily = FontFamily,
                FontSize = FontSize,
                Content = new string('m', charCount)
            };

            l.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return l.DesiredSize.Width;
        }
    }
}
