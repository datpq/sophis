/*
** Include
*/
#include "CFG_Popup.h"

#include "SphInc\gui\SphButton.h"
#include "SphInc\gui\SphEditElement.h"
#include "SphTools\SphLoggerUtil.h"
#include "Resource\resource.h"
#include "SphInc/SphPreference.h"

/*
** Namespace
*/
using namespace sophis::gui;
using namespace sophis::portfolio;
using namespace sophis::tools;
using namespace sophis::instrument;
using namespace sophis;
using namespace sophis::static_data;

/*
** Static
*/
const char * CSxScenarioPopup::__CLASS__ = "CSxScenarioPopup";


//----------------------------------------------------------------------------------
/*
** Methods
*/
CSxScenarioPopup::CSxScenarioPopup() : CSRFitDialog()
{
	BEGIN_LOG("CSxScenarioPopup");
	
	fResourceId = IDD_DIALOG_SCENARIO   - ID_DIALOG_SHIFT;
	fElementCount = 2;
	fElementList = new CSRElement*[fElementCount];

	int nb = 0;
	_STL::string message="This scenario executes an SQL Script in batch mode.\n"
		"To lunch it, create a batch file that contains the following command prompt:\n"
        "SphValue.exe -SSQLScenario:'ScriptFileName.sql'";
	

	if (fElementList)
	{			
		fElementList[nb++] = new CSROKButton(this,	editOk);		
	    fElementList[nb++] = new CSRStaticText (this,	editText, 200,message.c_str());
	}

	END_LOG();
}

//------------------------------------------------------------------------------------------------------------------------------------------
CSxScenarioPopup::~CSxScenarioPopup()
{
	BEGIN_LOG("~CSxScenarioPopup");
	END_LOG();
}

//----------------------------------------------------------------------------------
void CSxScenarioPopup::ElementValidation(int EAId_Modified)
{
	BEGIN_LOG("ElementValidation");

	
	END_LOG();

}

//-------------------------------------------------------------------------------------------------------------------------------------------
void CSxScenarioPopup::OnOK()
{
	BEGIN_LOG("OnOK");

	CSRFitDialog::OnOK();
	
	END_LOG();
}



