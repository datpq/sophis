/*
** Includes
*/

// specific
#include "BOModifiableFields.h"
#include "SphInc/gui/SphTransactionDialog.h"


/*
** Namespace
*/
using namespace sophis::backoffice_kernel;
using namespace sophis::gui;

/*
** Statics
*/
/*static*/  const char* BOModifiableFields::__CLASS__ = "ModifiableFields";


/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool BOModifiableFields::has_access(const char* right,	long item_number) const
{
	if( has_right(right)&& item_matched(	item_number,
											// BO remarks
											eDIBORemarks,
											// add the relative id of the control
											-1					// end-of-list
											))
	{
		return true;
	} else {
		return false;
	}
}

