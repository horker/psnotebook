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

        // Crude adjustment for the vertical scrollbar
        public static double CommandLineWidth => ConsoleWidth + 30;

        // Crude adjustment for the margins, paddings and borders
        public static double WindowWidth => CommandLineWidth + 100;

        public static int MaxOutputHeightInLines { get; set; } = 30;

        public static double MaxOutputHeight => FontSize * MaxOutputHeightInLines;

        public static double DefaultWpfElementWidth { get; set; } = ConsoleWidth * .7;

        public static double DefaultWpfElementHeight { get; set; } = MaxOutputHeight * .9;

        public static bool IgnoreBackgroundColor => true;

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
