$SQL="D:\BUILDAGENT\_work\1\binaries\SQL"
$ReleaseNotes="D:\BUILDAGENT\_work\1\binaries\ReleaseNotes"
$client = "D:\BUILDAGENT\_work\1\binaries\client"
$SophisMonitor = "D:\BUILDAGENT\_work\1\binaries\client\SophisMonitor"
$sharedApi = "D:\BUILDAGENT\_work\1\binaries\servers\sophis\shared\api"
$marketFIX = "D:\BUILDAGENT\_work\1\binaries\servers\sophis\Market\FIXGateway\bin"
$marketOrder = "D:\BUILDAGENT\_work\1\binaries\servers\sophis\Market\OrderAdapter\bin"
$marketRMA = "D:\BUILDAGENT\_work\1\binaries\servers\sophis\Market\RichMarketAdapter\bin"
$ETL = "D:\BUILDAGENT\_work\1\binaries\servers\sophis\ETL\bin"


Copy-Item "D:\BUILDAGENT\_work\1\s\MediolanumSQL_Base.sql" $(new-item -type directory -force $SQL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MediolanumSQL_Iterative.sql" $(new-item -type directory -force $SQL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MediolanumSQL_RollbackTKT.sql" $(new-item -type directory -force $SQL)
Copy-Item "D:\BUILDAGENT\_work\1\s\Medio.BackOffice.net\SQL\Medio.BackOffice.netScript.sql" $(new-item -type directory -force $SQL)
Copy-Item "D:\BUILDAGENT\_work\1\s\Medio.BackOffice.net\SQL\Rollback_Medio.BackOffice.netScript.sql" $(new-item -type directory -force $SQL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_GUI\SQL\MEDIO_UserRight_ExpiredFX_Script.sql" $(new-item -type directory -force $SQL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_GUI\SQL\Rollback_MEDIO_UserRight_ExpiredFX_Script.sql" $(new-item -type directory -force $SQL)

Copy-Item "D:\BUILDAGENT\_work\1\s\MediolanumReleaseNotes.txt" $(new-item -type directory -force $ReleaseNotes)
Copy-Item "D:\BUILDAGENT\_work\1\s\MediolanumInstallation.txt" $(new-item -type directory -force $ReleaseNotes)


Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_GUI\x64\Release\MEDIO_GUI.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.OMS.WF4Activities\bin\Release\x64\MEDIO.OMS.WF4Activities.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_COMPLIANCE\x64\Release\MEDIO_COMPLIANCE.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_DOB_INDICATOR\x64\Release\MEDIO_DOB_INDICATOR.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.OrderAutomation.net\bin\Release\x64\MEDIO.OrderAutomation.NET.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.ClauseBuilder.net\bin\Release\x64\MEDIO.ClauseBuilder.net.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.FXCompliance.net\MEDIO.FXCompliance.net\bin\Release\x64\MEDIO.FXCompliance.net.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.CORE\bin\Release\x64\MEDIO.CORE.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\CUST_UCITS\x64\Release\CUST_UCITS.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.BackOffice.net\bin\Release\x64\MEDIO.BackOffice.net.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_RBC_NAV\x64\Release\MEDIO_RBC_NAV.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.NestedRule.net\bin\Release\x64\MEDIO.NestedRule.net.dll" $(new-item -type directory -force $client)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.UserColumns.net\bin\Release\x64\MEDIO.UserColumns.net.dll" $(new-item -type directory -force $client)


Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_GUI\x64\Release\MEDIO_GUI.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.OMS.WF4Activities\bin\Release\x64\MEDIO.OMS.WF4Activities.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.TransactionAction\bin\Release\x64\MEDIO.TransactionAction.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_COMPLIANCE\x64\Release\MEDIO_COMPLIANCE.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_DOB_INDICATOR\x64\Release\MEDIO_DOB_INDICATOR.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.OrderAutomation.net\bin\Release\x64\MEDIO.OrderAutomation.NET.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.ClauseBuilder.net\bin\Release\x64\MEDIO.ClauseBuilder.net.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.FXCompliance.net\MEDIO.FXCompliance.net\bin\Release\x64\MEDIO.FXCompliance.net.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.CORE\bin\Release\x64\MEDIO.CORE.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\CUST_UCITS\x64\Release\CUST_UCITS.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.BackOffice.net\bin\Release\x64\MEDIO.BackOffice.net.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_RBC_NAV\x64\Release\MEDIO_RBC_NAV.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.NestedRule.net\bin\Release\x64\MEDIO.NestedRule.net.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_IntegrationServiceAction\x64\Release\MEDIO_IntegrationServiceAction.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\Medio.FIXPlugin.net\bin\Release\x64\Medio.FIXPlugin.net.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.RBCOrderAdapter.net\bin\Release\x64\MEDIO.RBCOrderAdapter.net.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\Mediolanum_RMA_FILTER\bin\Release\x64\Mediolanum_RMA_FILTER.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\Mediolanum_RMA_FILTER_CLI\x64\Release\Mediolanum_RMA_FILTER_CLI.dll" $(new-item -type directory -force $sharedApi)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.UserColumns.net\bin\Release\x64\MEDIO.UserColumns.net.dll" $(new-item -type directory -force $sharedApi)



Copy-Item "D:\BUILDAGENT\_work\1\s\Medio.FIXPlugin.net\bin\Release\x64\Medio.FIXPlugin.net.dll" $(new-item -type directory -force $marketFIX)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.CORE\bin\Release\x64\MEDIO.CORE.dll" $(new-item -type directory -force $marketFIX)
Copy-Item "D:\BUILDAGENT\_work\1\s\Mediolanum_RMA_FILTER\bin\Release\x64\Mediolanum_RMA_FILTER.dll" $(new-item -type directory -force $marketFIX)
Copy-Item "D:\BUILDAGENT\_work\1\s\Mediolanum_RMA_FILTER_CLI\x64\Release\Mediolanum_RMA_FILTER_CLI.dll" $(new-item -type directory -force $marketFIX)


Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO.RBCOrderAdapter.net\bin\Release\x64\MEDIO.RBCOrderAdapter.net.dll" $(new-item -type directory -force $marketOrder)

Copy-Item "D:\BUILDAGENT\_work\1\s\Mediolanum_RMA_FILTER\bin\Release\x64\Mediolanum_RMA_FILTER.dll" $(new-item -type directory -force $marketRMA)
Copy-Item "D:\BUILDAGENT\_work\1\s\Mediolanum_RMA_FILTER_CLI\x64\Release\Mediolanum_RMA_FILTER_CLI.dll" $(new-item -type directory -force $marketRMA)


Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL\bin\Release\x64\SophisETL.exe" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.Common\bin\Release\x64\SophisETL.Common.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.Extract\bin\Release\x64\SophisETL.Extract.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.ISEngine\bin\Release\x64\SophisETL.ISEngine.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.Load\bin\Release\x64\SophisETL.Load.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.Load.SOA\bin\Release\x64\SophisETL.Load.SOA.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.Queue\bin\Release\x64\SophisETL.Queue.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.Reporting\bin\Release\x64\SophisETL.Reporting.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.Transform\bin\Release\x64\SophisETL.Transform.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETL.Transform.Velocity\bin\Release\x64\SophisETL.Transform.Velocity.dll" $(new-item -type directory -force $ETL)
Copy-Item "D:\BUILDAGENT\_work\1\s\MEDIO_ETL\SophisETLGUI\bin\Release\x64\SophisETLGUI.exe" $(new-item -type directory -force $ETL)
