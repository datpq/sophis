/*
** Includes
*/
#include "SphTools/SphLoggerUtil.h"
#include "CSxTVARatesDlg.h"
#include "CSxTVARatesScenario.h"

/*static*/ const char* CSxTVARatesScenario::__CLASS__ = "CSxTVARatesScenario";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_SCENARIO(CSxTVARatesScenario)

//-------------------------------------------------------------------------------------------------------------
eProcessingType	CSxTVARatesScenario::GetProcessingType() const
{
    BEGIN_LOG("GetProcessingType");
    END_LOG();
	return pUserPreference;
}


//-------------------------------------------------------------------------------------------------------------
void CSxTVARatesScenario::Run()
{
    BEGIN_LOG("Run");

	try
	{						
		CSxTVARatesDlg::Display();			
	}
	catch(sophisTools::base::ExceptionBase &ex)
	{
		CSRFitDialog::Message(FROM_STREAM("ERROR ("<<ex<<") while running \"TVA Rates\" dialog"));		
	}
	catch(...)
	{		
		CSRFitDialog::Message(FROM_STREAM("Unhandled exception occured while running \"TVA Rates\" dialog"));
	}

    END_LOG();
}


