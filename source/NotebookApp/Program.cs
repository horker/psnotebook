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
    class Program
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int SetStdHandle(int nStdHandle, IntPtr hHandle);

        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_ERROR_HANDLE = -12;

        static void Main(string[] args)
        {
            var readStream = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            var writeStream = new AnonymousPipeClientStream(PipeDirection.Out, readStream.ClientSafePipeHandle);

            var handle = writeStream.SafePipeHandle.DangerousGetHandle();

            if (SetStdHandle(STD_OUTPUT_HANDLE, handle) == 0)
                throw new InvalidOperationException("SetStdHandle(STD_OUTPUT_HANDLE) failed");

            if (SetStdHandle(STD_ERROR_HANDLE, handle) == 0)
                throw new InvalidOperationException("SetStdHandle(STD_OUTPUT_HANDLE) failed");

            var p = new Process();
            var pi = p.StartInfo;
            pi.FileName = "Horker.Notebook.App.exe";
            pi.RedirectStandardOutput = true;
            pi.RedirectStandardError = true;
            pi.UseShellExecute = false;

            p.Start();
            p.WaitForExit();

            Environment.Exit(p.ExitCode);
        }
    }
}
