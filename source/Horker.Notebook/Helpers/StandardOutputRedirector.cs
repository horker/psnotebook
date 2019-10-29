using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Horker.Notebook
{
    public class StandardOutputRedirector
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int SetStdHandle(int nStdHandle, IntPtr hHandle);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("msvcrt.dll")]
        public static extern int _dup2(int fd1, int fd2);

        [DllImport("msvcrt.dll")]
        public static extern int _open_osfhandle(IntPtr osfhandle, int flags);

        [DllImport("msvcrt.dll")]
        public static extern IntPtr _get_osfhandle(int fd);

        [DllImport("msvcrt.dll")]
        public static extern IntPtr freopen(string FileName, string Mode, IntPtr Stream);

        [DllImport("msvcrt.dll")]
        public static extern int _flushall();

        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_ERROR_HANDLE = -12;

        public const int _O_RDONLY = 0;
        public const int _O_WRONLY = 1;
        public const int _O_RDWR = 2;
        public const int _O_ACCMODE = (_O_RDONLY | _O_WRONLY | _O_RDWR);
        public const int _O_APPEND = 0x0008;
        public const int _O_TEXT = 0x4000;
        public const int _O_BINARY = 0x8000;
        public const int _O_RAW = _O_BINARY;
        public const int _O_WTEXT = 0x10000;
        public const int _O_U16TEXT = 0x20000;
        public const int _O_U8TEXT = 0x40000;

        private AnonymousPipeServerStream _readStream;
        private AnonymousPipeClientStream _writeStream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private StringBuilder _buffer;

        public StreamReader Reader => _reader;
        public StreamWriter Writer => _writer;

        public StandardOutputRedirector()
        {
            _readStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            _writeStream = new AnonymousPipeClientStream(PipeDirection.Out, _readStream.ClientSafePipeHandle);

            var encoding = Console.OutputEncoding;
            _reader = new StreamReader(_readStream, encoding);
            _writer = new StreamWriter(_writeStream, encoding);
            _writer.AutoFlush = true;

            _buffer = new StringBuilder();

            var handle = _writeStream.SafePipeHandle.DangerousGetHandle();

            // Update standard output

            var fd = _open_osfhandle(handle, _O_WRONLY | _O_TEXT);

            if (fd == -1)
                throw new InvalidOperationException("_open_osfhandle() failed");

            if (_dup2(fd, 1) == -1)
                throw new InvalidOperationException("_dup2(1) failed");

            if (SetStdHandle(STD_OUTPUT_HANDLE, _get_osfhandle(fd)) == 0)
                throw new InvalidOperationException("SetStdHandle(STD_OUTPUT_HANDLE) failed");

            Console.SetOut(_writer);

            // Update standard error[:w

            fd = _open_osfhandle(handle, _O_WRONLY | _O_TEXT);

            if (fd == -1)
                throw new InvalidOperationException("_open_osfhandle() failed");

            if (_dup2(fd, 2) == -1)
                throw new InvalidOperationException("_dup2(2) failed");

            if (SetStdHandle(STD_ERROR_HANDLE, _get_osfhandle(fd)) == 0)
                throw new InvalidOperationException("SetStdHandle(STD_OUTPUT_HANDLE) failed");

            Console.SetError(_writer);
        }

        public void StartToRead(Action<string> action)
        {
            Task.Run(() =>
            {
                var buffer = new char[1];
                var lastCh = '\0';
                while (true)
                {
                    var count = _reader.Read(buffer, 0, 1);
                    Debug.Assert(count == 1);

                    var ch = buffer[0];
                    if (ch == '\n')
                    {
                        if (lastCh == '\r')
                            _buffer.Remove(_buffer.Length - 1, 1);
                        action.Invoke(_buffer.ToString());
                        _buffer.Clear();
                    }
                    else if (ch == '\0')
                    {
                        if (_buffer.Length > 0)
                        {
                            action.Invoke(_buffer.ToString());
                            _buffer.Clear();
                        }
                    }
                    else
                    {
                        _buffer.Append(ch);
                    }

                    lastCh = ch;
                }
            });
        }

        public void Flush()
        {
            _flushall();
            _writer.Flush();
            _writer.Write('\0');
        }
    }
}
