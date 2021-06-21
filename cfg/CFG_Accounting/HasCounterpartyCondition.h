#ifndef __HasCounterpartyCondition_H__
	#define __HasCounterpartyCondition_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphKernelActionCondition.h"



class CSxHasCounterpartyCondition : public sophis::backoffice_kernel::CSRKernelActionCondition
{
	DECLARATION_KERNEL_ACTION_CONDITION(CSxHasCounterpartyCondition);

//------------------------------------ PUBLIC ---------------------------------
public:

	/** Method called when an event is executed on a deal and the workflow tells to execute this action.
	This is called during an internal notify transaction action when new transaction is created.
	Action is excluded from execution if this method returns false.
	@param transaction - new transaction.
	*/
	virtual bool VoteForCreation(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/;

	/** Method called when an event is executed on a deal and the workflow tells to execute this action.
	This is called during an internal notify transaction action when transaction is modified.
	Action is excluded from execution if this method returns false.
	@param original is the transaction before modification.
	@param transaction - transaction after modification.
	*/
	virtual bool VoteForModification(const sophis::portfolio::CSRTransaction & original, const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/;

	/** Method called when an event is executed on a deal and the workflow tells to execute this action.
	This is called during an internal notify transaction action when transaction is deleted.
	Action is excluded from execution if this method returns false.
	@param transaction - transaction for deletion.
	*/
	virtual bool VoteForDeletion(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/;

//------------------------------------ PRIVATE --------------------------------
private:

	static const char* __CLASS__;
};

#endif //!__HasCounterpartyCondition_H__