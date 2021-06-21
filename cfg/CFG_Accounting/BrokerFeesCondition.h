////////////////////////////////////////////////////////////////
// BrokerFeesCondition.h
////////////////////////////////////////////////////////////////

#pragma once

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphBrokerFees.h"

/*
** Class
*/
class CSxBrokerFeesCondition : public sophis::backoffice_kernel::CSRBrokerFeesConditionTransaction
{
//------------------------------------------- PUBLIC -------------------------------------------
public:
	DECLARATION_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesCondition);

	/*
	** Test if Broker Fees has to be applied.
	*/
	virtual bool get_condition(const sophis::portfolio::CSRTransaction & trade ) const;

//------------------------------------------- PROTECTED -------------------------------------------
protected:

//------------------------------------------- PRIVATE -------------------------------------------
private:
	/*
	** Logger data
	*/
	static const char * __CLASS__;

};