
/*
** Includes
*/
// specific
#include "CSxOptionModel.h"
#include "SphTools/SphLoggerUtil.h"
/*
** Namespace
*/
using namespace sophis::instrument;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
CONSTRUCTOR_OPTION(CSxOptionModel, CSROption)

/*virtual*/ bool CSxOptionModel::IsWithMarginCall() const
{
	bool IsWithMarginCall = false;	
	
	eInstrumentStatus status = this->GetStatus();
	if(status == sAvailable || status == sNotSet)
	{
		IsWithMarginCall = true;
	}
	return IsWithMarginCall;
}