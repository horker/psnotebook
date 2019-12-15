using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Horker.Notebook
{
    public class HostRawUserInterface : PSHostRawUserInterface
    {
        public HostRawUserInterface()
        { }

        public void InitializeConfiguration(Models.Configuration config)
        {
            _config = config;

            _bufferSize = new Size(_config.ConsoleWidthInChars, 80);
            _windowSize = new Size(_config.ConsoleWidthInChars, 80);
            _maxWindowSize = new Size(_config.ConsoleWidthInChars, 80);
            _maxPhysicalWindowSize = new Size(_config.ConsoleWidthInChars, 80);
        }

        private Models.Configuration _config;

        private ConsoleColor _foregroundColor = ConsoleColor.Black;

        public override ConsoleColor ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                _foregroundColor = value;
                _foregroundBrush = ViewModels.ConsoleColorToBrushConverter.GetBrush(value);
            }
        }

        private Brush _foregroundBrush = Brushes.Black;

        public Brush ForegroundBrush => _foregroundBrush;

        private ConsoleColor _backgroundColor = ConsoleColor.White;

        public override ConsoleColor BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        private Brush _backgroundBrush = Brushes.DarkBlue;

        public Brush BackgroundBrush
        {
            get
            {
                if (_config.IgnoreBackgroundColor)
                    return null;
                return _backgroundBrush;
            }
        }

        private Coordinates _cursorPosition = new Coordinates(0, 0);

        public override Coordinates CursorPosition
        {
            get => _cursorPosition;
            set => _cursorPosition = value;
        }

        private Coordinates _windowPosition = new Coordinates(0, 0);

        public override Coordinates WindowPosition
        {
            get => _windowPosition;
            set => _windowPosition = value;
        }

        private int _cursorSize = 1;

        public override int CursorSize
        {
            get => _cursorSize;
            set => _cursorSize = value;
        }

        private Size _bufferSize;

        public override Size BufferSize
        {
            get => _bufferSize;
            set => _bufferSize = value;
        }

        private Size _windowSize;

        public override Size WindowSize
        {
            get => _windowSize;
            set => _windowSize = value;
        }

        private Size _maxWindowSize;

        public override Size MaxWindowSize => _maxWindowSize;

        private Size _maxPhysicalWindowSize;

        public override Size MaxPhysicalWindowSize => _maxPhysicalWindowSize;

        public override bool KeyAvailable => false;

        private string _windowTitle;

        public override string WindowTitle
        {
            get => _windowTitle;
            set => _windowTitle = value;
        }

        public override void FlushInputBuffer()
        {
            throw new NotImplementedException();
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            // do nothing
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }
    }
}
