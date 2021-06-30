
/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "GenerateBilan.h"
#include "GenerateBilanDlg.h"
#include "CmdGenerate.h"
#include "Log.h"
#include "Resource\resource.h"

using namespace std;
using namespace eff::emafi::gui;
/*static*/ const char* GenerateBilan::__CLASS__ = "GenerateBilan";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(GenerateBilan)


//-------------------------------------------------------------------------------------------------------------
eProcessingType	GenerateBilan::GetProcessingType() const
{
	BEGIN_LOG("GetProcessingType");
	END_LOG();
	return pUserPreference;
}

//-------------------------------------------------------------------------------------------------------------
void GenerateBilan::Run()
{
	BEGIN_LOG("Run");

	// Create the dialog instance
	GenerateBilanDlg *dialog = new GenerateBilanDlg();
	if (fParam == NULL)
	{
		// Display a modal dialog
		dialog->DoDialog();
	}
	else
	{
		string s = fParam;
		string delimiter = ";";
		size_t pos = 0;
		string token;
		vector<string> arg;
		int i=0;
		while((pos = s.find(delimiter)) != string::npos && i < 7 )
		{
			token = s.substr(0, pos);
			s.erase(0, pos + delimiter.length());
			arg.push_back(token);
			i++;
		}
		arg.push_back(s);

		stringstream ss;
		ss << "REPORT_TYPE IN (";
		while((pos = s.find(delimiter)) != string::npos)
		{
			token = s.substr(0, pos);
			s.erase(0, pos + delimiter.length());
			ss << "'" + token + "'";
			ss << ",";
		}
		ss <<  "'" + s + "'";	
		ss << ")";	
		string whereClause = ss.str();
		
		bool is_simulation = (arg.at(6) == "true") ? 1 : 0;
		
		if(arg.size() == 8)
		{
			CmdGenerate * obj = new CmdGenerate(dialog, IDC_CMD_CmdGenerate - ID_ITEM_SHIFT);
			obj->generateReport(arg.at(0).c_str(), atol(arg.at(1).c_str()), atol(arg.at(2).c_str()), atol(arg.at(3).c_str()), arg.at(4), arg.at(5),is_simulation, whereClause);
		}
	}
	
	
	END_LOG();
}
