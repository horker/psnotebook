# PowerShell Notebook

PowerShell Notebook is a console application for PowerShell to bring Jupyter Notebook-like user experience to PowerShell scripting.

This project is work-in-progress.

## Installation

```PowerShell
Install-Module psnotebook -Scope CurrentUser
```

## How to start

Invoke the `Start-Notebook` cmdlet in the PowerShell console.
As another way, execute `NotebookApp.exe` in the module directory (`$HOME\Documents\WindowsPowerShell\Module\psnotebook`).

## Keybindings

|Key stroke|Action|
| -------- | ---- |
|Ctrl+Up   |Move to the previous command line.|
|Ctrl+Down |Move to the next command line.|
|Ctrl+Enter|Execute the command line and move to the next command line.|
|F5        |Execute the command line and keep the cursor in the current commnad line.|
|Ctrl+F5   |Execute all command lines.|

The other key strokes work in the same way as the standard edit control.

## Using with OxyPlot CLI

To use with the [OxyPlot CLI](https://www.powershellgallery.com/packages/oxyplotcli) module, execute the following commands:

```PowerShell
Import-Module oxyplotcli
Set-OxyPlotViewDispatcher (Get-NotebookWindowDispatcher)
```

You should add the `-AsPlotView` parameter to each OxyPlot CLI cmdlet and pass it to the `Out-WpfGrid` cmdlet.

```PowerShell
oxy.line -x 1,2,3 -y 1,2,3 -AsPlotView | Out-WpfGrid
```

## License

This product is published under the MIT license.
