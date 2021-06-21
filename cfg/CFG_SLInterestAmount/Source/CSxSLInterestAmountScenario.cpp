#pragma warning(disable:4251)
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/SphMacros.h"
#include  __STL_INCLUDE_PATH(fstream)
#include  __STL_INCLUDE_PATH(string)
#include "SphLLInc/Sphtools/io/FileList.h"
#include "SphLLInc/Sphtools/io/File.h"
#include "SphLLInc/Sphtools/io/OutputStream_fwd.h"
#include "SphLLInc/Sphtools/io/FileOutputStream.h"
#include "SphTools/SphExceptions.h"
#include "SphSDBCInc/SphSQLQuery.h"
//DPH
#if (TOOLKIT_VERSION < 720)
#include "SphLLInc\misc\ConfigurationFileWrapper.h";
#else
#include "SphInc\misc\ConfigurationFileWrapper.h";
#endif
#include "SphInc/backoffice_kernel/SphKernelStatusGroup.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/instrument/SphLoanAndRepo.h"
#include "SphInc/collateral/SphLoanAndRepoDialog.h"
#include "SphInc/market_data/SphMarketData.h"

#include "CSxSLInterestAmountScenario.h"
#include "CSxInterest.h"
#include "CSxTools.h"

using namespace _STL;
using namespace sophis::instrument;
using namespace sophis::collateral;
using namespace sophis::market_data;
using namespace sophis::misc;
using namespace sophisTools::io;

/*static*/ const char* CSxSLInterestAmountScenario::__CLASS__ = "CSxSLInterestAmountScenario";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CSxSLInterestAmountScenario)

//-------------------------------------------------------------------------------------------------------------
eProcessingType	CSxSLInterestAmountScenario::GetProcessingType() const
{
    BEGIN_LOG("GetProcessingType");
    END_LOG();
	return pManagerPreference;
}


//-------------------------------------------------------------------------------------------------------------
void CSxSLInterestAmountScenario::Run()
{
    BEGIN_LOG("Run");

	try
	{
		// Output directory
		_STL::string filePath;
		ConfigurationFileWrapper::getEntryValue("CFG_SLInterestAmount","OutputDirectory", filePath, ".");

		//File name
		_STL::string fileName;
		ConfigurationFileWrapper::getEntryValue("CFG_SLInterestAmount", "FileName", fileName, "SLInterestAmount.txt");

		_STL::string outputStr = "Refcon, Interest amount\r\n";

		//Get SL trades that have a wrong interest amount
		_STL::string sqlQuery = "select H.refcon, H.sicovam, date_to_num(H.dateneg), date_to_num(H.commission_date), H.quantite, H.SPREAD_HT, H.montant from histomvts H, Titres T, business_events B  where H.sicovam = T.sicovam " 
									"and H.type = B.ID and T.Type = 'P' and  H.CFG_INTEREST_AMOUNT = 0 and B.name = 'Repo / SL Initiation'";

		struct SSResult
		{		
			long fRefcon;
			long fSicovam;		
			long fTrxDate;	
			long fCommissionDate;
			double fQuantity;
			double fSpreadHT;
			double fAmount;
		};

		SSResult	*resultBuffer=NULL;
		int			nbResults = 0;

		CSRStructureDescriptor	desc(7, sizeof(SSResult));

		ADD(&desc, SSResult, fRefcon, rdfInteger);
		ADD(&desc, SSResult, fSicovam, rdfInteger);
		ADD(&desc, SSResult, fTrxDate, rdfInteger);
		ADD(&desc, SSResult, fCommissionDate, rdfInteger);
		ADD(&desc, SSResult, fQuantity, rdfFloat);
		ADD(&desc, SSResult, fSpreadHT, rdfFloat);
		ADD(&desc, SSResult, fAmount, rdfFloat);

		//DPH
		//CSRSqlQuery::QueryWithNResults(sqlQuery.c_str(), &desc, (void **) &resultBuffer, &nbResults);
		CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery.c_str(), &desc, (void **)&resultBuffer, &nbResults);

		for (int i = 0; i < nbResults; i++)
		{
			const CSRLoanAndRepo* loanAndRepo = dynamic_cast<const CSRLoanAndRepo*>(CSRInstrument::GetInstance(resultBuffer[i].fSicovam));

			if (loanAndRepo)
			{
				eDealDirection dealDirection;

				if (resultBuffer[i].fQuantity >= 0)
					dealDirection = eBorrow;
				else
					dealDirection = eLend;

				double interestAmountHT = 0.;
				double interestAmount =  CSxInterest::GetSLRoundedInterest(loanAndRepo,resultBuffer[i].fAmount,resultBuffer[i].fSpreadHT/100.,resultBuffer[i].fTrxDate, 
												resultBuffer[i].fCommissionDate, fabs(resultBuffer[i].fQuantity),dealDirection,interestAmountHT);

				char buffer[256];
				sprintf_s(buffer, 256, "%ld, %lf\r\n",resultBuffer[i].fRefcon, interestAmount);

				outputStr += FROM_STREAM(buffer);
			}
		}

		//Generate output file

		_STL::string path = CSxTools::DuplicateChar(filePath, '\\');
		path += "\\" + CSxTools::NumToDateYYYYMMDD(gApplicationContext->GetDate());

		//Create output directory
		FileHandle fh;
		fh = File::create(path);
		if (!fh->exists())
			fh->mkdir(true);

		_STL::ofstream filestr;
		_STL::string fullPath = path + "\\" + fileName;
		filestr.open (fullPath.c_str(), _STL::ofstream::binary | _STL::ofstream::trunc);

		filestr.write (outputStr.c_str(),outputStr.size()+1);		

		filestr.close();

		CSRFitDialog::Message(FROM_STREAM("File successfully generated in " << fullPath.c_str()));

	}
	catch (const ExceptionBase & ex) 
	{
		CSRFitDialog::Message(FROM_STREAM("Error : " << ex.getError().c_str()));
		MESS(Log::error, ex.getError().c_str());
		LOG(Log::error, (const char*)ex);
	}
	catch (...) 
	{
		CSRFitDialog::Message("Unknown error");
		MESS(Log::error, "Unknown error");
		LOG(Log::error, "Unknown error");
	}
	
    END_LOG();
}


