#ifndef __BOModifiableFields_H__
	#define __BOModifiableFields_H__

/*
** Includes
*/
// standard
#include "SphInc/backoffice_kernel/SphKernelRights.h"


/*
** Class
	Interface to describe fields in the purchase dialog to be able to modify.
	This is used in a workflow rule to disable events applicable to the deal.
*/

class BOModifiableFields : public sophis::backoffice_kernel::CSRKernelEditModel
{
//------------------------------------ PUBLIC ---------------------------------
public:

	DECLARATION_KERNEL_EDIT_MODEL(BOModifiableFields);

	/** method to decide if user has access right to control
	@param right is a string containing user right
	@param item_number is ID of	control to which access is required
	@returns true if has access the control item, otherwise - false
	*/
	virtual bool has_access(const char* right, long item_number) const;

//------------------------------------ PRIVATE --------------------------------
private:

	static const char* __CLASS__;

};

#endif //!__BOModifiableFields_H__