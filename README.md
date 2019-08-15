# PowerShell Notebook

PowerShell Notebook is a console application for PowerShell to bring Jupyter Notebook-like user experience to PowerShell scripting.

This project is work-in-progress.

## Installation

```PowerShell
Install-Module psnotebook -Scope CurrentUser
```

## How to start

Invoke the `Start-Notebook` cmdlet in the PowerShell console.
As another way, execute `NotebookApp.exe` in the module directory (`$HOME\Documents\WindowsPowerShell\Modules\psnotebook` in the usual environment).

## Key Bindings

|Key stroke|Action|
| -------- | ---- |
|Ctrl+Up   |Move to the previous command line.|
|Ctrl+Down |Move to the next command line.|
|Ctrl+Enter|Execute the command line and move to the next command line.|
|F5        |Execute the command line and keep the cursor in the current command line.|
|Ctrl+F5   |Execute all command lines.|

The other key strokes work in the same way as the standard edit control.

## Helper cmdlets

|Cmdlet|Description|
| ---- | --------- |
|Start-Notebook|Start a new Notebook application.|
|New-WpfGrid|Create a WPF Grid object.|
|New-WpfImage|Create a WPF Image object.|
|Get-NotebookWindowDispatcher|Get a Dispatcher object that the Notebook's GUI exposes.|
|Invoke-WpfAction|Invoke a script block under the thread in which the Notebook application is running.|

## Using with OxyPlot CLI

To use [OxyPlot CLI](https://github.com/horker/oxyplotcli2) with this application, execute the following commands:

```PowerShell
Import-Module oxyplotcli
Set-OxyPlotViewDispatcher (Get-NotebookWindowDispatcher)
```

You should add the `-AsPlotView` parameter to each OxyPlot CLI cmdlet and pass it to the `New-WpfGrid` cmdlet.

```PowerShell
oxy.line -x 1,2,3 -y 1,2,3 -AsPlotView | New-WpfGrid
```

## License

This product is published under the MIT license.
