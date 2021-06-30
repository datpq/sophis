#include "SphInc/gui/SphDialog.h"
#include "SphSDBCInc/exceptions/SphOracleException.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc\misc\ConfigurationFileWrapper.h"
#include "SphSDBCInc/queries/SphQueryBuffered.h"

#include "stdio.h"
#include <iostream>
#include <string>
#include <fstream>

#include "CSxGLscenario.h"
#include "CSxUtils.h"

using namespace std;
using namespace sophis::sql;

#define GLDefaultDirectory "GLInterface"

/*static*/ const char* CSxGLscenario::__CLASS__ = "CSxGLscenario";

struct StGLInterface {
	char ledger[50];
	char categorie[50];
	char	source[50];
	char currency[10];
	char accounting_date[10];
	char company[50];
	char account[80];
	char subaccount[80];
	char region[20];
	char relation[20];
	char future[20];
	double debit;
	double credit;
	char line_desc[100];
	char journal_name[50];
};

//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CSxGLscenario)

//-------------------------------------------------------------------------------------------------------------
eProcessingType	CSxGLscenario::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return  pSendToGL;
}

//-------------------------------------------------------------------------------------------------------------
void CSxGLscenario::Run()
{
	BEGIN_LOG("Run");

	try {
		MESS(Log::debug, FROM_STREAM("Lanching stored procedures :"));

		CSRQuery proceduresQuery;
		proceduresQuery << "\n\
						   					BEGIN \n\
											KAMCO_GL.AUDIT_Procedure; \n\
											KAMCO_GL.updateJournalName; \n\
											KAMCO_GL.UPDATE_POSTING_STATUS; \n\
											END;";
		proceduresQuery.Execute();

		_STL::vector<StGLInterface> rows;
		CSRQueryBuffered<StGLInterface> queryBuffered;
		queryBuffered << "SELECT"
			<< OutOffset("LEDGER", &StGLInterface::ledger)
			<< OutOffset("CATEGORY", &StGLInterface::categorie)
			<< OutOffset("SOURCE", &StGLInterface::source)
			<< OutOffset("CURRENCY", &StGLInterface::currency)
			<< OutOffset("ACCOUNTING_DATE", &StGLInterface::accounting_date)
			<< OutOffset("COMPANY", &StGLInterface::company)
			<< OutOffset("ACCOUNT", &StGLInterface::account)
			<< OutOffset("SUBACCOUNT", &StGLInterface::subaccount)
			<< OutOffset("REGION", &StGLInterface::region)
			<< OutOffset("RELATION", &StGLInterface::relation)
			<< OutOffset("FUTURE", &StGLInterface::future)
			<< OutOffset("DEBIT", &StGLInterface::debit)
			<< OutOffset("CREDIT", &StGLInterface::credit)
			<< OutOffset("LINE_DESCRIPTION", &StGLInterface::line_desc)
			<< OutOffset("JOURNAL_NAME", &StGLInterface::journal_name)
			<< " FROM GL_INTERFACE_AUDIT WHERE STATUS_AUDIT = 1 ORDER BY ACCOUNTING_DATE";

		queryBuffered.FetchAll(rows);

		MESS(Log::debug, FROM_STREAM("Generation of CSV file"));

		string fileStr = "";
		string csvDir = "";
		ConfigurationFileWrapper::getEntryValue("TOOLKIT", "GlInterfaceDirectory", csvDir, GLDefaultDirectory);

		fileStr = csvDir + "\\GLInterface_" + CSxUtils::GetCurDateTimeYYMMDDHHMMSS() + ".csv";
		ofstream csv_file;

		csv_file.open(fileStr.c_str(), std::ofstream::app);

		csv_file << "Ledger,Category,Source,Currency,Accounting Date,COMPANY,CURRENTCY,ACCOUNT,"
			<< "SUBACCOUNT,REGION,RELATION,FUTURE,Debit,Credit,"
			<< "Line Description,Journal Name,Journal Description \n";

		for (auto item : rows)
		{
			csv_file << item.ledger << ','; //Ledger
			csv_file << item.categorie << ','; // Catego
			csv_file << item.source << ','; //source			
			csv_file << item.currency << ','; //currency
			csv_file << item.accounting_date << ','; //accounting date
			csv_file << item.company << ','; //company
			csv_file << item.currency << ','; //currentcy
			csv_file << item.account << ','; //account
			csv_file << item.subaccount << ','; //subaccount
			csv_file << item.region << ','; //region
			csv_file << item.relation << ','; //relation
			csv_file << item.future << ','; //future
			csv_file << item.debit << ','; //debit
			csv_file << item.credit << ',';//credit
			csv_file << item.line_desc << ','; //line description = journale desc
			csv_file << item.journal_name << ','; //journal name
			csv_file << item.line_desc; //journal desc
			csv_file << '\n';
		}
		csv_file.close();

		CSRQuery queryAudit;
		queryAudit << "BEGIN KAMCO_GL.UPDATE_AUDIT_STATUS; END;";
		queryAudit.Execute();

		sophis::gui::CSRFitDialog::Message((string("CSV File generated at: ") + fileStr).c_str());
	}
	catch (const ifstream::failure& e) {
		MESS(Log::error, FROM_STREAM("IO Exception occured: " << e.what()));
	}
	catch (const CSROracleException& e) {
		MESS(Log::error, FROM_STREAM("Oracle error code: " << e.GetErrorCode() << ", reason: " << e.GetReason().c_str()));
	}
	catch (const ExceptionBase& e) {
		MESS(Log::error, FROM_STREAM("Exception occured: " << (const char *)e));
	}
	catch (...) {
		MESS(Log::error, "Unknown error occured");
	}
	END_LOG();
}

