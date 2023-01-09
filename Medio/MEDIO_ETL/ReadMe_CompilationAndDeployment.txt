- Compilation
	Use 'StartSolution.bat' to open the solution. It includes the path of 'xsd.exe'.
	xsd.exe automatically translate setup schemas in .Net classes.
	
	Dependencies:
	- NVelocity: this component allows to replaces '$Tags' in the XML template by functional values.
	It is on GIT: https://scm-git-eur.misys.global.ad/projects/FT/repos/sharedfiles/browse/DotNet/ThirdParty/NVelocity_1.1.0.0
	- Sophis: SophisETL.ISEngine only has Sophis dependencies (because we use UniversalAdapter framework to setup IS).

- Deployment (Fast)
	Compile in release.
	Copy all the binaries from MEDIO_ETL\SophisETL\bin\Release into MEDIO_ETL\MedioSetup\NAV
	Note that Sophis.Services.30.XmlSerializers.dll is automatically copied by the build.
	But it is big (30Mb) and not needed in the deployment. You can remove it from the deployment.
	Add the dll SophisTools.dll (native dll used for IS setup)
	Adjust SophisETL.exe.config (using SophisConfigurationManager.exe) to point on a valid IntegrationSerivce.
	Run SophisETL.exe

- Deployment (Prod)
	One .bat file
	creates on the fly the file config\\sophis_etl.xml (with specific input file name, output file name, log file name...)
	Run SophisETL.exe