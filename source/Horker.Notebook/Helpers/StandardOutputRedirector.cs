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
        private bool _flushing;

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

            var handle = _writeStream.SafePipeHandle.DangerousGetHandle();

            // Update C-runtime file descriptors.
            // Note: We need to duplicate file handles for stdout and stderr respectively and
            // call SetStdHandle() to update Win32API-level stdout and stderr; fortunately,
            // _dup2() does both internally on behalf of us.

            var fd = _open_osfhandle(handle, _O_WRONLY | _O_TEXT);
            if (fd == -1)
                throw new InvalidOperationException("_open_osfhandle() failed");

            if (_dup2(fd, 1) == -1)
                throw new InvalidOperationException("_dup2(1) failed");

            fd = _open_osfhandle(handle, _O_WRONLY | _O_TEXT);
            if (_dup2(fd, 2) == -1)
                throw new InvalidOperationException("_dup2(2) failed");

            // Update .NET console output.

            Console.SetOut(_writer);
        }

        public void StartToRead(Action<string> action)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var line = _reader.ReadLine();

                    if (_flushing &&
                        (line.Length == 0 ||
                         (line.Length == 1 && line[0] == '\n') ||
                         (line.Length == 2 && line[0] == '\r' && line[1] == '\n')))
                    {
                        _flushing = false;
                        continue;
                    }
                    action.Invoke(line);
                }
            });
        }

        public void FlushRemnants()
        {
            _flushall();
            _writer.Flush();

            // To force to flush remnant buffer, write an empty newline.
            _flushing = true;
            _writer.WriteLine();
            _writer.Flush();
        }

        public void Close()
        {
            _readStream.Close();
            _reader.Close();
        }
    }
}
