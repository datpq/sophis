/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "GLscenario.h"
#include "Config.h"
#include "SqlUtils.h"

#include "SqlWrapper.h"
#include "StringUtils.h"
#include "systemutils.h"
#include "stdio.h"
#include <iostream>
#include <string>
//#include <vector>
#include <cstdlib>
#include <fstream>
#include <sstream>
#include <iostream>

using namespace std;
using namespace eff::utils;


/*static*/ const char* GLscenario::__CLASS__ = "GLscenario";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(GLscenario)

//-------------------------------------------------------------------------------------------------------------
eProcessingType	GLscenario::GetProcessingType() const
{
    BEGIN_LOG("GetProcessingType");
    END_LOG();
	return  pSendToGL;
}


//-------------------------------------------------------------------------------------------------------------
void GLscenario::Run()
{
    BEGIN_LOG("Run");
	 

	// from ACCOUNT_POSTING_KB_GL : to avoid updating the original Account_posting table
	string query = StrFormat("select TO_CHAR(posting_date, 'DD/MM/YYYY')  Accounting_Date, ACCOUNT_NUMBER, amount*decode(credit_debit, 'D', 1, 0) Debit, \n\
		amount*decode(credit_debit, 'C', 1, 0) credit, \n\
		devise_to_str(currency) currency, \n\
		to_char(posting_date,'YYMM') Journal_Name \n\
		from ACCOUNT_POSTING_KB_GL where \n\
		status in (1,2,7,9,10,4) and \n\
		posting_date <= sysdate and \n\
		posting_type in(select ID from account_posting_types where name != 'Asset' and name != 'Technical' and summary is not null) and \n\
		rownum< 10");  // rownum<10 : to avoid updating all lines

	SqlWrapper* reportWrapper = new SqlWrapper("c10,c80,d,d,c4,c10", query.c_str());


	/******/
	std::string fileStr;


	string csvDir = Config::getSettingStr(TOOLKIT_SECTION, "GlInterfaceDirectory");
	

	fileStr = csvDir+ "\\GL Interface_" + GetWindowsUserName() + "_" + GetCurDateTimeYYMMDDHHMMSS() + ".csv";


	/******/
		ofstream myfile;
	
	
		myfile.open(fileStr.c_str(), std::ofstream::app);
	
		myfile << "Upl,Ledger,Category,Source,Currency,Accounting Date,COMPANY,CURRENTCY,ACCOUNT,";
		myfile << "SUBACCOUNT,REGION,RELATION,FUTURE,Debit,Credit," ;
		myfile << "Line Description,Journal Name,Journal Description \n";
		for (int i = 0; i < reportWrapper->GetRowCount(); i++)
		{
			myfile << ','; //upl
			myfile << ','; //Ledger
			myfile << ','; // Catego
			myfile << "Manual" << ','; //source			
			myfile << (*reportWrapper)[i][4].value<string>() << ','; //currency
			myfile << (*reportWrapper)[i][0].value<string>() << ','; //accounting date
			myfile << ','; //company
			myfile << (*reportWrapper)[i][4].value<string>() << ','; //currentcy
			myfile << (*reportWrapper)[i][1].value<string>() << ','; //account
			myfile << ','; //subaccount
			myfile << ','; //region
			myfile << ','; //relation
			myfile <<"00" <<','; //future
			myfile << (*reportWrapper)[i][2].value<double>()<<','; //debit
			myfile << (*reportWrapper)[i][3].value<double>() << ',';//credit
			myfile << ','; //line description
			myfile << (*reportWrapper)[i][5].value<string>() << ','; //journal name
			myfile << ','; //journal desc
			myfile << '\n';

		}
		
	
		myfile.close();
	
		try {
			SqlUtils::QueryWithoutResultException("BEGIN KAMCO_GL.UPDATESTATUS_; END;");
		}
		catch (const CSROracleException &e) {
			ERROR("Database error code = %d, reason = %s", e.GetErrorCode(), e.GetReason().c_str());
		}

    END_LOG();
}


