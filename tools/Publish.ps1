$key = cat $PSScriptRoot\..\private\NugetApiKey.txt

Publish-Module -Path $PSScriptRoot\..\module\psnotebook -NugetApiKey $key -Verbose
