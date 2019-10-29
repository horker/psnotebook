#!Notebook v1

#!CommandLine
# Note: This session uses the ImageMagick tool set.
#!CommandLine
cd $env:TEMP
#!CommandLine
$wallpapers = dir -file C:\Windows\web\Wallpaper\Theme2
$wallpapers.Name
#!CommandLine
$wallpapers | foreach { New-WpfImage $_.FullName } | New-WpfGrid
#!CommandLine
$wallpapers | foreach {
    $newFile = $_ -replace ".jpg$", "_gray.jpg"
    "$_ => $newFile"
    convert -type grayscale $_.fullname $newFile
}
#!CommandLine
dir *_gray.jpg | foreach { New-WpfImage $_.FullName } | New-WpfGrid
#!CommandLine
rm *_gray.jpg
#!CommandLine
