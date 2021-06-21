
#pragma warning(disable:4251)
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "CFG_Scenario.h"
#include <stdio.h>

//DPH
//#include "stlport/stl/_fstream.h"
#include <fstream>
#include "SphSDBCInc\exceptions\SphOracleException.h"

#include <shlwapi.h>

using namespace std;
using namespace sophis::sql;


/*static*/ const char* CSxScenario::__CLASS__ = "CSxScenario";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CSxScenario)

//-------------------------------------------------------------------------------------------------------------
eProcessingType	CSxScenario::GetProcessingType() const
{
    BEGIN_LOG("GetProcessingType");   
	
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void CSxScenario::Run()
{
    BEGIN_LOG("Run");	
    MESS(Log::info, "Start Scenario");
	MESS(Log::info, "Batch Mode = " << fBatchMode);
	_STL::string commande="";
	MESS(Log::info, "After commande init");
	char fileName[256] = "";
	MESS(Log::info, "After fileName ini");
	
	if(fBatchMode)
	{
		MESS(Log::info,"Start Scenario in Batch Mode");
		try
		{	
			MESS(Log::info, "Before reading the sql File " << fParam);
			 strcpy_s(fileName,256,fParam);
			 MESS(Log::info, "After reading the sql File " << fParam);
			  MESS(Log::info, "The sql File is " << fileName);
			 if(!PathFileExistsA(fileName))
			 {
					MESS(Log::error," The  script file '" << fileName <<" ' does not exist ");
					return;
			 }
			 else
				 MESS(Log::info," The  script file '" << fileName <<" is found ");
			 FILE * pFile;
	         errno_t errorID = fopen_s (&pFile, fileName ,"r");
		     if (pFile == NULL || errorID != 0)
			 {  
				char errorMessage[512] = "";
				strerror_s(errorMessage, 512, errorID);
				MESS(Log::error,"Could not read the script file ' " << fileName << " ' (code " << errorID << ", ' " << errorMessage << " ')");
				return;
			 }
			 else
			 {
            MESS(Log::info," The  script file '" << fileName <<" is parsed ");
			 }
		     fclose(pFile);
		     pFile = NULL;
			 _STL::ifstream myfile(fileName);
	         if (myfile.is_open())
			 {
				   MESS(Log::info, "-- Parsing SQL file:"<<fileName);
				   while (!myfile.eof())
				   {
						_STL::string line;
						getline(myfile, line);
						 MESS(Log::info, "-- Parsing the line :"<<line);
						if (!line.empty())
							{	
								commande.clear();
//								_STL::string::size_type sep = line.find(";");
//								commande=line.substr(0,sep);
								commande = line;
								try
								{
									//DPH
									//errorCode err= CSRSqlQuery::QueryWithoutResult(commande.c_str());
									errorCode err = CSRSqlQuery::QueryWithoutResultAndParam(commande.c_str());
									if(err==0)
									{
						 			  MESS(Log::info,"The request '"<<commande<<"' in SQL file ' "<<fileName<<" ' is executed successfuly" );
									}
									if (err!=0)
                         			{
									  MESS(Log::warning, "Oracle Error " << err << " while executing request  ' " << commande<<" '  in SQL file '"<<fileName<<"'");
								    }	

								 }
								//DPH
								//catch (OracleException& ex)
								catch (CSROracleException& ex)
								{
									MESS(Log::error,"Oracle exception:  "<<ex<<"  , while executing the request  ' "<<commande<<" '");
								}


							}
						else
						{
                            MESS(Log::info, "-- The line :"<<line<<"is empty");
						}
					}
			  }            

			 CSRSqlQuery::Commit();

			
			
		}
		
		catch(...)
		{
			MESS(Log::error,"Exception in SQLScenario ");
		}
	}
	else
	{
		MESS(Log::info, "GUI mode:the scenario do nothing in GUI mode,launch it in batch mode");
	}
	
	MESS(Log::info,"End Scenario");

	
    END_LOG();
}
//-------------------------------------------------------------------------------------------------------------


