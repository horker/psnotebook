task . Compile, Build, ImportDebug, Test

Set-StrictMode -Version 4

############################################################
# Settings
############################################################

$PROJECT_NAME = "psnotebook"

$SOURCE_PATH = "$PSScriptRoot\source\NotebookApp\bin"
$SCRIPT_PATH = "$PSScriptRoot\scripts"

$MODULE_PATH = "$PSScriptRoot\module\psnotebook"
$MODULE_PATH_DEBUG = "$PSScriptRoot\module\debug\psnotebook"

$SOLUTION_FILE = "$PSScriptRoot\source\notebook.sln"

$OBJECT_FILES = @(
    "Horker.Notebook.dll"
    "Horker.Notebook.pdb"
    "NotebookApp.exe"
    "NotebookApp.exe.config"
    "NotebookApp.pdb"
)

#$TEMPLATE_INPUT_PATH = "$PSScriptRoot\templates"
#$TEMPLATE_OUTPUT_PATH = "$PSScriptRoot\source\Horker.PSCNTK\Generated files"

#$HELP_INPUT =  "$SOURCE_PATH\bin\Release\Horker.Math.dll"
#$HELP_INTERM = "$SOURCE_PATH\bin\Release\Horker.Data.dll-Help.xml"
#$HELP_OUTPUT = "$MODULE_PATH\Horker.Data.dll-Help.xml"
#$HELPGEN = "$PSScriptRoot\vendor\XmlDoc2CmdletDoc.0.2.10\tools\XmlDoc2CmdletDoc.exe"

############################################################
# Helper cmdlets
############################################################

function New-Folder2 {
  param(
    [string]$Path
  )

  try {
    $null = New-Item -Type Directory $Path -EA Stop
    Write-Host -ForegroundColor DarkCyan "$Path created"
  }
  catch {
    Write-Host -ForegroundColor DarkYellow $_
  }
}

function Copy-Item2 {
  param(
    [string]$Source,
    [string]$Dest,
    [switch]$Recurse
  )

  try {
    Copy-Item $Source $Dest -EA Stop -Recurse:$Recurse -Force
    Write-Host -ForegroundColor DarkCyan "Copy from $Source to $Dest done"
  }
  catch {
    Write-Host -ForegroundColor DarkYellow $_
  }
}

function Remove-Item2 {
  param(
    [string]$Path
  )

  Resolve-Path $PATH | foreach {
    try {
      Remove-Item $_ -EA Stop -Recurse -Force
      Write-Host -ForegroundColor DarkCyan "$_ removed"
    }
    catch {
      Write-Host -ForegroundColor DarkYellow $_
    }
  }
}

############################################################
# Tasks
############################################################

task Compile {
  msbuild $SOLUTION_FILE /p:Configuration=Debug /nologo /v:minimal
  msbuild $SOLUTION_FILE /p:Configuration=Release /nologo /v:minimal
}

task Build {
  . {
    $ErrorActionPreference = "Continue"

    function Copy-ObjectFiles {
      param(
        [string]$ObjectPath,
        [string]$TargetPath
      )

      # Scripts
      New-Folder2 $TargetPath
      Copy-Item2 "$SCRIPT_PATH\*" $TargetPath

      # DLL files
      $OBJECT_FILES | foreach {
        $path = Join-Path $objectPath $_
        Copy-Item2 $path $targetPath
      }
    }

    Copy-ObjectFiles "$SOURCE_PATH\Release" $MODULE_PATH
    Copy-ObjectFiles "$SOURCE_PATH\Debug" $MODULE_PATH_DEBUG
  }
}

task Test Build, ImportDebug, {
  Invoke-Pester "$PSScriptRoot\tests"
}

task ImportDebug {
    Import-Module $MODULE_PATH_DEBUG -Force
}

task Clean {
  Remove-Item2 "$MODULE_PATH\*"
  Remove-Item2 "$MODULE_PATH_DEBUG\*"
}
