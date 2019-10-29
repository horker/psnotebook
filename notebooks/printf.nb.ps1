#!Notebook v1

#!CommandLine
$msvcrt = Add-Type -pass -name "lib3" -MemberDefinition @"
    [DllImport("msvcrt.dll", CharSet = CharSet.Ansi,
    CallingConvention = CallingConvention.Cdecl)]
    static public extern int printf(
        string format);

    [DllImport("kernel32.dll")]
    static public extern bool WriteFile(IntPtr hFile, byte [] lpBuffer,
        uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten,
        IntPtr lpOverlapped);

    [DllImport("msvcrt.dll", CharSet = CharSet.Ansi)]
    static public extern int _write(
        int fd,
        string buffer,
        int count
    );

    [DllImport("msvcrt.dll")]
    static public extern IntPtr _get_osfhandle(int fd);
"@
#!CommandLine
$handle = [Horker.Notebook.StandardOutputRedirector]::GetStdHandle(-11)
$handle
#!CommandLine
[Horker.Notebook.StandardOutputRedirector]::GetStdHandle(-12)
#!CommandLine
$written = [int]0
$msvcrt::WriteFile($handle, [byte[]]@([byte][char]'a', [byte][char]'b', [byte][char]'c', [byte][char]"`n"), 4, [ref]$written, [IntPtr]::Zero)
#!CommandLine
$written
#!CommandLine
$msvcrt::printf("good bye`n")
[console]::WriteLine("good night")
$msvcrt::printf("good bye")
[console]::WriteLine("good night 2")
#!CommandLine
$msvcrt::_write(1, "hello`n", 5)
#!CommandLine
[console]::Write("hello")
#[console]::Out.Flush()
#!CommandLine
$msvcrt::_get_osfhandle(1) 
#!CommandLine
$stdout = $host.PrivateData.Model.Stdout
#!CommandLine
$stdout.Writer.BaseStream.SafePipeHandle.DangerousGetHandle()
#!CommandLine









