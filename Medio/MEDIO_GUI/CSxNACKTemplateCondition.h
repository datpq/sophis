
#ifndef __CSxNACKTemplateCondition_H__
#define __CSxNACKTemplateCondition_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphTpDocGenRulesCondition.h"



class CSxNACKTemplateCondition : public sophis::backoffice_kernel::ISRTpDocGenRulesCondition
{
	DECLARATION_TP_DOC_GEN_RULES_CONDITION(CSxNACKTemplateCondition);

	//------------------------------------ PUBLIC ---------------------------------
public:

	/** Method called when an event is executed on a deal and the workflow tells to execute this action.
	This is called during an internal notify transaction action when new transaction is created.
	Action is excluded from execution if this method returns false.
	@param transaction - new transaction.
	*/
	//virtual bool VoteForCreation(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/;

	virtual bool get_condition(const portfolio::CSRTransaction& trade
		, const backoffice_kernel::SSDocGenerationCriteria& criteria) const /*= 0*/;


	/** Method called when an event is executed on a deal and the workflow tells to execute this action.
	This is called during an internal notify transaction action when transaction is modified.
	Action is excluded from execution if this method returns false.
	@param original is the transaction before modification.
	@param transaction - transaction after modification.
	*/
	//virtual bool VoteForModification(const sophis::portfolio::CSRTransaction & original, const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/;

	/** Method called when an event is executed on a deal and the workflow tells to execute this action.
	This is called during an internal notify transaction action when transaction is deleted.
	Action is excluded from execution if this method returns false.
	@param transaction - transaction for deletion.
	*/
	//virtual bool VoteForDeletion(const sophis::portfolio::CSRTransaction &transaction) const /*= 0*/;

	//------------------------------------ PRIVATE --------------------------------
private:
	//bool checkCondition(const sophis::portfolio::CSRTransaction &transaction) const;

	static const char* __CLASS__;
};

#endif //!__CSxIsBrokerDIMCondition_H__
