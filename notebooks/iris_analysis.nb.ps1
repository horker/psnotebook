#!Notebook v1

#!CommandLine
cd $home\work\oxyplotcli
#!CommandLine
$iris = import-csv .\datasets\r\iris.csv
#!CommandLine
$iris | select -f 5 | ft
#!CommandLine
$iris | oxy.scat -xname Petal.Length -yname Petal.Width -groupname Species -AsPlotView
#!CommandLine
