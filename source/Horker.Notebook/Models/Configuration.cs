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
        public FontFamily FontFamily { get; set; } = new FontFamily("Consolas");

        public double FontSize { get; set; } = (96.0 / 72) * 11; // 11pt

        private int _consoleWidthInChars = 120;

        public int ConsoleWidthInChars
        {
            get => _consoleWidthInChars;
            set
            {
                _consoleWidthInChars = value;
                _consoleWidth = double.NaN;
            }
        }

        private double _consoleWidth = double.NaN;

        public double ConsoleWidth
        {
            get
            {
                if (double.IsNaN(_consoleWidth))
                    _consoleWidth = GetWidthBasedOnNumberOfCharacters(ConsoleWidthInChars);

                return _consoleWidth;
            }
        }

        // Crude adjustment for the vertical scrollbar
        public double CommandLineWidth => ConsoleWidth + 30;

        // Crude adjustment for the margins, paddings and borders
        public double WindowWidth => CommandLineWidth + 100;

        public double WindowHeight => 1000;

        public int MaxOutputHeightInLines { get; set; } = 30;

        public double MaxOutputHeight => FontSize * MaxOutputHeightInLines;

        private double _defaultWpfElementWidth = double.NaN;

        public double DefaultWpfElementWidth
        {
            get
            {
                if (double.IsNaN(_defaultWpfElementWidth))
                    return ConsoleWidth * .7;
                return _defaultWpfElementWidth;
            }
            set => _defaultWpfElementWidth = value;
        }

        private double _defaultWpfElementHeight = double.NaN;

        public double DefaultWpfElementHeight
        {
            get
            {
                if (double.IsNaN(_defaultWpfElementHeight))
                    return MaxOutputHeight * .7;
                return _defaultWpfElementHeight;
            }
            set => _defaultWpfElementHeight = value;
        }

        public bool IgnoreBackgroundColor => true;

        public bool InlineCompletion = true;

        // Helper methods

        private double GetWidthBasedOnNumberOfCharacters(int charCount)
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
