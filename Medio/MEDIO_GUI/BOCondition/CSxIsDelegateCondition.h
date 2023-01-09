#ifndef __CSxIsDelegateCondition_H__
	#define __CSxIsDelegateCondition_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphKernelCondition.h"



class CSxIsDelegateCondition : public sophis::backoffice_kernel::CSRKernelCondition
{
	DECLARATION_WORKFLOW_DEF_CONDITION(CSxIsDelegateCondition);

//------------------------------------ PUBLIC ---------------------------------
public:

	/*
	*	Pure virtual method.
	*	Used by the framework while selecting the rule from Workflow Definition rules set.
	*	Method is called for Condition1, Condition2, Condition3 columns. Logical 'AND' is used
	*	to make decision if to select the matching rule - found by framework.
	*	The result has to be TRUE to make the rule selected.
	*	@tr is the reference to the transaction associated with the processed deal;
	*	it is the final (resp. initial) state for a deal created or modified (resp. canceled).
	*	@sel is a structure giving some information about the instrument created; As the instrument may be
	*	not yet created, the structure gives some data coming from the future instrument created; if the instrument is created,
	*	the structure gives the data from the instrument.
	*	@return is the boolean and is calculated by the client code.
	*/
	virtual bool GetCondition(	const sophis::portfolio::CSRTransaction& tr, 
								const sophis::backoffice_kernel::SSKernelInstrumentSelector &sel) const /*= 0*/;


private:


	static const char* __CLASS__;
};

#endif //!__CSxIsDelegateCondition_H__