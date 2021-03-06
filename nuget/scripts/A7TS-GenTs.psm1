function A7TS-GenTs([string]$controllerFilter = "", [switch]$nobuild)
{
	try
	{
		$project = Get-Project
		if (-not $nobuild) { 
			Build-Project($project) 
			$DTE.ExecuteCommand("Debug.Start")
		}
		
		Start-Sleep -s 5
		
        $httpclient = New-Object "System.Net.WebClient"

		$url = Get-Url($project)
		$url+= "/tsgenerator.axd" + "?controllerFilter=" + $controllerFilter;
		
        try
		{
			$ts = $httpclient.DownloadString($url)
		}
		catch [Exception]
		{
			echo 'Error occurred downloading metadata from tsgenerator.axd.'
		}

		if (-not $nobuild) { $DTE.ExecuteCommand("Debug.StopDebugging") }
		
        if($ts -eq "{No Services Found}"){
            echo "No services were found."
        } else {        
            foreach($fileMeta in $ts.Split([string[]]@("<--- FILE DELIMITER ---><br />"), [StringSplitOptions]"RemoveEmptyEntries")){          
                GenerateFile($fileMeta); 
            }       
        }       
        
		echo 'TS file generation completed.'				

	}
	catch [Exception]
	{
		$exception = $_.Exception
		Write-Host $exception
	}
}


function Build-Project($project)
{
    $configuration = $DTE.Solution.SolutionBuild.ActiveConfiguration.Name

    $DTE.Solution.SolutionBuild.BuildProject($configuration, $project.UniqueName, $true)

    if ($DTE.Solution.SolutionBuild.LastBuildInfo)
    {
        throw 'The project ''' + $project.Name + ''' failed to build.'
    }
}



function Get-Url($project)
{
	return $project.Properties.Item("WebApplication.BrowseURL").Value	
}

function GenerateFile($fileMeta){

        $arrFileMeta = $fileMeta.Split([string[]]@("--++--<br />"), [StringSplitOptions]"RemoveEmptyEntries")
        $fileType = $arrFileMeta[0]
        $fileName = $arrFileMeta[1]
        $fileContent = $arrFileMeta[2]
        
        echo $("FileType " + $fileType)
        echo $("FileName " + $fileName)
        
        $scriptsSrcFolder = $project.projectitems.item('Scripts').ProjectItems.Item("Src")
        $modelsFolder = $scriptsSrcFolder.ProjectItems.Item("Models")
        $servicesFolder = $scriptsSrcFolder.ProjectItems.Item("Services")
        $fileFullName = $($fileName + ".ts")
        $path = [System.Io.Path]::Combine([System.Io.Path]::GetTempPath(), $fileFullName)  
        
        echo $('Generating file ' + $fileFullName)
                      
		[System.IO.File]::WriteAllText($path,$fileContent)
                        
		try
		{
            if($fileType -eq "Model"){
                $modelsFolder.ProjectItems.Item($fileFullName).Delete()
            } else {
                $servicesFolder.ProjectItems.Item($fileFullName).Delete()
            }
        
		}
		catch [Exception]
		{}
        
        if($fileType -eq "Model"){
               $modelsFolder.ProjectItems.AddFromFileCopy($path)
            } else {
                $servicesFolder.ProjectItems.AddFromFileCopy($path)
            }

}

Export-ModuleMember A7TS-GenTs