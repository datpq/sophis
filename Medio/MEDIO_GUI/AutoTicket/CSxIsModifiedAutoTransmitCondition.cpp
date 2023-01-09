#pragma warning(push)
#pragma warning(disable:4251)

/*
** Includes
*/
#include "CSxIsModifiedAutoTransmitCondition.h"
#pragma warning(pop)


/*
** Namespace
*/
using namespace sophis::portfolio;

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ const char* CSxIsModifiedAutoTransmitCondition::GetName() const /*= 0*/
{
	return "MEDIO : IS MODIFIED AUTO";
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CSxIsModifiedAutoTransmitCondition::AppliedTo(const CSRTransaction &transaction) const
{
	if (transaction.GetAdjustment() == -1 && transaction.GetBackOfficeType() != static_cast<eBackOfficeType>(-1))
		return true;
	return false;
}
