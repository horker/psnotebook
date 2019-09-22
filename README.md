# PowerShell Notebook

PowerShell Notebook is a console host application for PowerShell to bring Jupyter Notebook-like user experience to PowerShell scripting.

The code editor adopts [AvalonEdit](http://avalonedit.net) and provides code completion, syntax highlighting and automatic indentation.

<img src="https://github.com/horker/psnotebook/blob/master/docs/images/screenshot.png" width=40%>
<img src="https://github.com/horker/psnotebook/blob/master/docs/images/screenshot2.png" width=40%>

## Installation

This module is published in the [PowreShell Gallery](https://www.powershellgallery.com/packages/psnotebook)

```PowerShell
Install-Module psnotebook
```

## Getting started

Import the module and invoke the `Start-Notebook` cmdlet in the PowerShell console.

```PowerShell
Import-Module psnotebook
Start-Notebook
```

As another way, execute `NotebookApp.exe` in the module directory (`$HOME\Documents\WindowsPowerShell\Modules\psnotebook` in the usual environment).

## Key Bindings

|Key stroke |Action|
| --------- | ---- |
|Ctrl+Up    |Move to the previous command line.|
|Ctrl+Down  |Move to the next command line.|
|Ctrl+Left  |Move to the beginning of the command line.|
|Ctrl+Right |Move to the end of the command line.|
|Enter      |Insert a line break and automatic indentation. (Editor mode)|
|Shift+Enter|Execute the command line and move to the next command line. (Editor mode)|
|Ctrl+Enter |Execute the command line and keep the cursor in the current command line. (Editor mode)|
|F5         |Execute the command line and keep the cursor in the current command line.|
|Ctrl+F5    |Execute all command lines in sequence.|
|Ctrl+Q     |Show the context menu to operate command line cells.|
|Esc        |Cancel the ongoing execution.|

The other key strokes work in the same way as the standard edit control.

### Editor mode/Shell mode

By default, the application is in the 'Editor mode'. In this mode, the Enter key will insert a line break and entering the Ctrl+Enter and Shift+Enter keys will execute the command line.

When this mode is set to off ('Shell mode'), the Enter key will immediately execute the command. The Ctrl+Enter and Shift+Enter keys will insert a line break.

You can set the Editor mode on and off in the 'Edit' menu or the context menu.

## Context menu

By right-clicking on the cell index or enter Ctrl+Q in the code editor, the context menu will open.

Several commands to operate command line cells are available in the context menu.

## Helper cmdlets

|Cmdlet|Description|
| ---- | --------- |
|Start-Notebook|Start a new Notebook application.|
|New-WpfGrid|Create a WPF Grid object.|
|New-WpfImage|Create a WPF Image object.|
|Get-NotebookWindowDispatcher|Get a Dispatcher object that the Notebook's GUI exposes.|
|Invoke-WpfAction|Invoke a script block under the thread in which the Notebook application is running.|

## Limitationas

The following limitations exist in the current version:

- Keyboard input. The following operations do not work: `Read-Host`, `$ErrorActionPreference='Inquire'`, a console debugger (`Set-PSBreakpoint`) and native applications that perform any console I/O operations except writing to standard and error outputs.
- Terminal escape sequences.
- Any customization is not available for now.

## Notes

### Using with OxyPlot CLI

To use [OxyPlot CLI](https://github.com/horker/oxyplotcli2) with this application, execute the following commands:

```PowerShell
Import-Module oxyplotcli
Set-OxyPlotViewDispatcher (Get-NotebookWindowDispatcher)
```

You should add the `-AsPlotView` parameter to each OxyPlot CLI cmdlet to show charts inline.

```PowerShell
oxy.line -x 1,2,3 -y 1,2,3 -AsPlotView
```

## License

This software is published under the MIT license.

The AvalonEdit library that this software using is subject to its own license. Refer to the [project site](http://avalonedit.net).
