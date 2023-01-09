#include "CSxDynamicOrderBuilderPosCtxMenu.h"
#include "SphTools/SphLoggerUtil.h"
#include <SphInc/GUI/SphInstrumentUI.h>

const char* CSxDynamicOrderBuilderPosCtxMenu::__CLASS__ = "CSxDynamicOrderBuilderPosCtxMenu";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_POSITION_CTX_MENU(CSxDynamicOrderBuilderPosCtxMenu)



//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxDynamicOrderBuilderPosCtxMenu::IsFolioAuthorized(const CSRPortfolioVector &folioVector) const const
{
	BEGIN_LOG("IsFolioAuthorized");
	
	return true;
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CSxDynamicOrderBuilderPosCtxMenu::FolioAction(const CSRPortfolioVector &folioVector, const char* ActionName) const
{
	BEGIN_LOG("FolioAction")
	

	END_LOG();
}