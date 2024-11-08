function UpdateClientView
{
	# Updates clear case view using the parent of the solution file being built
	
	param($SolutionFile)
	
	$clientViewDir = Split-Path -Path $SolutionFile -Parent

    function Filter-CleartoolUpdate
    {
		begin
		{
			[int]$script:updatedFilesCount = 0
		}

		process
		{
			switch -Regex ($_)
			{
				'^\.+$'								{ $script:updatedFilesCount += $_.Length }
				'Processing dir "(.*)"\.'			{ Write-Progress -Activity "Updating view  '$clientViewDir'" `
																	 -Status "$script:updatedFilesCount files processed" `
																	 -CurrentOperation ('Updating {0}' -f $Matches[1]) }
				'Log has been written to "(.*)"\.'  { Get-Content $Matches[1] }  
			}
		}
    }
    
    cleartool.exe update -force -rename -ctime $clientViewDir  2>&1 | Filter-CleartoolUpdate
    Write-Progress -Activity "UpdateClientView" -Status 'done.' -Completed
    if($LASTEXITCODE -ne 0) {throw "Fail to update view  '$clientViewDir'."  }
}

function GetClientReleaseNote
{
	
	param($BuildFile)
	$Doc = New-Object System.Xml.XmlDocument  
	[xml]$Doc = Get-Content $BuildFile
	#echo "test"
	$pgs = $Doc.Project.PropertyGroup
	foreach ($pg in $pgs) {
	  if ($pg.ReleaseNote) {
	    $pg.ReleaseNote
		Write-Progress -Activity "GetClientReleaseNote" -Status 'done : $versionFile' -Completed
		return
	  }
	}
	echo "Cannot find ReleaseNote element in $BuildFile"
	throw 'Cannot find ReleaseNote element in $BuildFile'
	
}

function GetCurrentVersion
{	
	param($BuildFile,$clientViewDir)
	#$clientViewDir = Split-Path -Path $SolutionFile -Parent
	
	$versionFileName = GetClientReleaseNote -BuildFile $BuildFile
	$versionFile = Join-Path $clientViewDir $versionFileName
	
	$versionLine = Select-String $versionFile -pattern "Source control label:" -List
	$version = $versionLine.tostring().Split()[-1]
	$versionTrimmed = $version.Trim()
	Write-Progress -Activity "GetCurrentVersion" -Status 'Version is: $versionTrimmed' -CurrentOperation  "Version: $versionTrimmed"
	#$n = Read-Host "$versionFile | $versionLine | $version | $versionTrimmed"
	
	# check meets regex
	if( $versionTrimmed -match "[\S]+_[0-9]+.[0-9]+.[0-9]+.[0-9]+" )
	{
		$versionTrimmed
		Write-Progress -Activity "GetCurrentVersion" -Status 'done.' -Completed
		return
	}
	else
	{
		echo "$versionTrimmed is not a valid toolkit label"
		throw "$versionTrimmed is not a valid toolkit label"
	}
}

function CheckCurrentVersion
{
	param($BuildFile,$SolutionFile)
	$clientViewDir = Split-Path -Path $SolutionFile -Parent
	$versionLabel = GetCurrentVersion -clientViewDir $clientViewDir -BuildFile $BuildFile
	echo $versionLabel
}

function CCV
{
	param($BuildFile,$SolutionFile)
	echo "DEBUG VERSION"
	$clientViewDir = Split-Path -Path $SolutionFile -Parent
	echo "clientViewDir: " $clientViewDir
	
	echo "Loading build.proj XML..."
	$Doc = New-Object System.Xml.XmlDocument  
	[xml]$Doc = Get-Content $BuildFile
	echo "Loaded."
	
	echo "Searching for ReleaseNote element..."
	$pgs = $Doc.Project.PropertyGroup
	foreach ($pg in $pgs) {
	  if ($pg.ReleaseNote) {
		echo "Found!"
	    $versionFileName = $pg.ReleaseNote
		echo "versionFileName = " $versionFileName
		
		$versionFile = Join-Path $clientViewDir $versionFileName
		
		echo "versionFile = " $versionFile
		
		$versionLine = Select-String $versionFile -pattern "Source control label:" -List
		
		echo "versionLine = " $versionLine
		
		$version = $versionLine.tostring().Split()[-1]
		
		echo "version = " $version
		
		$versionTrimmed = $version.Trim()
		
		echo "versionTrimmed = " $versionTrimmed
		
		Write-Progress -Activity "GetCurrentVersion" -Status 'Version is: $versionTrimmed' -CurrentOperation  "Version: $versionTrimmed"
		#$n = Read-Host "$versionFile | $versionLine | $version | $versionTrimmed"
		
		# check meets regex
		if( $versionTrimmed -match "[\S]+_[0-9]+.[0-9]+.[0-9]+.[0-9]+" )
		{
			$versionTrimmed
			Write-Progress -Activity "GetCurrentVersion" -Status 'done.' -Completed
			return
		}
		else
		{
			echo "$versionTrimmed is not a valid toolkit label"
			throw "$versionTrimmed is not a valid toolkit label"
		}
		
		echo $versionLabel
		#Write-Progress -Activity "GetClientReleaseNote" -Status 'done : $versionFile' -Completed
		#return
	  }
	}
	
	echo "Cannot find ReleaseNote element in $BuildFile"
	throw 'Cannot find ReleaseNote element in $BuildFile'
	
	
}

function ApplyToolkitLabel
{
	param($BuildFile,$SolutionFile)
	$clientViewDir = Split-Path -Path $SolutionFile -Parent
	$versionLabel = GetCurrentVersion -clientViewDir $clientViewDir -BuildFile $BuildFile
	
	if ($versionLabel -eq "")
	{
		echo "Could not get a valid toolkit label"
		throw "Could not get a valid toolkit label"
	}
	
	function Filter-CleartoolUpdate
    {
		begin
		{
			[int]$script:updatedFilesCount = 0
		}

		process
		{
			switch -Regex ($_)
			{
				#'^\.+$'								{ $script:updatedFilesCount += $_.Length }
				'Created label "(.*)"\.'			{ Write-Progress -Activity "Applying label  '$versionLabel' on '$clientViewDir'" `
																	 -Status "$script:updatedFilesCount files processed" `
																	 -CurrentOperation ('Updating {0}' -f $Matches[1]) }
				#'Log has been written to "(.*)"\.'  { Get-Content $Matches[1] }  
			}
		}
    }
	
	$appLab = Read-Host "Apply label $versionLabel on $clientViewDir (y/n)?"
	if ($appLab -eq "y" -OR $appLab -eq "Y")
	{
		Write-Host "Creating and applying label $versionLabel..."
		
		cd $clientViewDir
		
		
		Write-Host "Creating label $versionLabel: cleartool mklbtype -nc $versionLabel"
		$mklb = cleartool mklbtype -nc $versionLabel
			
		Write-Host "Applying label $versionLabel : cleartool mklabel -r $versionLabel $clientViewDir\"
		$appLab = cleartool mklabel -r $versionLabel "$clientViewDir" | Filter-CleartoolUpdate
		
		echo "Applied label $versionLabel on $clientViewDir"
		Write-Progress -Activity "ApplyToolkitLabel" -Status 'done.' -Completed
		return
	}
	else
	{
		#Write-Host "Apply label cancelled by user"
		echo "Apply label cancelled by user"
		throw { "Apply label cancelled by user" }
	}
}

function ReapplyToolkitLabel
{
	
}

function DeprecatedMethodsCheck
{
	param($SolutionFile)
	$fileSystem = Split-Path -Path $SolutionFile -Parent
	
	$deprecatedFile = "D:\ToolkitProductBuilder\1_BUILD_view\VOBToolkitValue\MAKEFILE\DEPRECATED_METHODS\DeprecatedMethods.txt"
	
	$deprecatedStrings = Get-Content $deprecatedFile
	
	foreach ($deprecated in $deprecatedStrings)
	{
		$depLines = Get-ChildItem -Path $fileSystem -Include @("*.cpp","*.h","*.cs") -Recurse | Select-String -Pattern $deprecated | group path | select name, count
		if ($depLines)
		{
			$raiseError = "true"
			echo $("--- $deprecated -----------------")
			foreach ($d in $depLines)
			{
				#$toplevel = Split-Path -Path $fileSystem -leaf
				#$current = Split-Path -Path $d.name -Parent
				#$filePath = Split-Path -Path $d.name -leaf
				
				#while ($current -ne $toplevel)
				#{
				#	$filePath = Join-Path $current $filePath
				#	$current = Split-Path -Path $current -Parent
				#}
				
				#$folderPath = Split-Path -Path $d.name -Parent
				#$f = Split-Path $folderPath -leaf
				$n = Split-Path $d.name -leaf
				#$n = $filePath 
				#$n = $d.name | Resolve-Path -Path $fileSystem -relative
				$c = $d.count
				echo $("($c) $n")
			}
			echo " -------------------------------- "
		}		
	}
	
	if ($raiseError)
	{
		throw {"Deprecated methods found"}
	}
}

$exportOrder = @('UpdateClientView','CheckCurrentVersion','ApplyToolkitLabel','DeprecatedMethodsCheck','CCV')
export-modulemember -Variable exportOrder
export-modulemember -function UpdateClientView
export-modulemember -function CheckCurrentVersion
export-modulemember -function ApplyToolkitLabel
export-modulemember -function DeprecatedMethodsCheck
#export-modulemember -function CCV
#export-modulemember -function ReapplyToolkitLabel