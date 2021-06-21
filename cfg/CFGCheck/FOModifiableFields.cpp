/*
** Includes
*/

// specific
#include "FOModifiableFields.h"
#include "SphInc/gui/SphTransactionDialog.h"


/*
** Namespace
*/
using namespace sophis::backoffice_kernel;
using namespace sophis::gui;

/*
** Statics
*/
/*static*/  const char* FOModifiableFields::__CLASS__ = "FOModifiableFields";


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool FOModifiableFields::has_access(const char* right,	long item_number) const
{
	if ( has_right(right)&& item_matched(	item_number,
											// BO remarks
											eDIBORemarks,
											// Entity
											eDIEntity,
											// add the relative id of the control
											-1					// end-of-list
											))
	{
		return false;
	} else 
	{
		return true;
	}
}

