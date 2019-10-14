rm $HOME\localpsrepo\psnotebook.*.nupkg

Publish-Module -path .\module\psnotebook\ -Repository LocalPSrepo -NuGetApiKey any
