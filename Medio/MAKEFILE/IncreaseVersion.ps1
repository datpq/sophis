$path = "D:\BUILDAGENT\_work\1\s\MediolanumVersion.cs"
$Current = (Get-Content $path)
$lines=$Current.Split("=")
$firstPart=$lines[2]
$versionFull=$lines[3].TrimEnd('";')
$lastIndex=$versionFull.LastIndexOf('.')
$secondPart=$versionFull.Substring(0,$lastIndex+1)
$lastVer=${env:TOOLKIT_VERSION}
$secondPart=$secondPart+$lastVer+'";'
$fullVersionString=$firstPart+'='+$secondPart;

$Current.SetValue($fullVersionString,2)
$Current | Out-File $path

$path = "D:\BUILDAGENT\_work\1\s\MediolanumVersion.h"
$Current = (Get-Content $path)
$lines=$Current.Split("#")
$firstLine=$lines[6]
$secondLine=$lines[8]
$lastIndex=$firstLine.LastIndexOf(' ')
$firstString='#'+$firstLine.Substring(0,$lastIndex+1)
$versionFull=$lines[6].Split(" ")[3]
$lastIndex=$versionFull.LastIndexOf(',')
$secondPart=$versionFull.Substring(0,$lastIndex+1)
$secondPart=$secondPart+$lastVer
$adjustedFirstLine=$firstString+$secondPart

$lastIndex=$secondLine.LastIndexOf(' ')
$firstString='#'+$secondLine.Substring(0,$lastIndex+1)
$versionFull=$secondLine.Substring($lastIndex+1).TrimEnd('"');
$lastIndex=$versionFull.LastIndexOf('.')
$updatedVersion=$versionFull.Substring(0,$lastIndex+1)+$lastVer+'"'
$adjustedSecondLine=$firstString+$updatedVersion

$Current.SetValue($adjustedFirstLine,3)
$Current.SetValue($adjustedSecondLine,4)
$Current | Out-File $path

$path = "D:\BUILDAGENT\_work\1\s\MediolanumReleaseNotes.txt"
$Current = (Get-Content $path)
$firstLine=$Current[0]
$versionString=$secondPart -replace ',', '.'
$adjustedNote= $firstLine -replace 'AUTO_VERSION', $versionString
$dateLine=$Current[2]
$currentDate=Get-Date -Format "MM/dd/yyyy"
$adjustedDateLine=$dateLine -replace 'AUTO_DATE', $currentDate
$Current.SetValue($adjustedNote,0)
$Current.SetValue($adjustedDateLine,2)
$Current | Out-File $path