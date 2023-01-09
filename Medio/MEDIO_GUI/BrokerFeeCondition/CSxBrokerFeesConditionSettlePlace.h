#pragma once
#ifndef __CSxBrokerFeesConditionSettlePlace_H__
#define __CSxBrokerFeesConditionSettlePlace_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphBrokerFees.h"

#define	My_INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION_SETTLEMENT_PLACE(classkey, name) \
	INITIALISE_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesConditionSettlePlace, name) \
	CSxMapping::MyDico[classkey] = name;

class CSxMapping
{
public:
	CSxMapping(){};
	static _STL::map<int, _STL::string> MyDico;
};

struct SSxSEDOL
{
	char fLibelle[100];
	SSxSEDOL()
	{
		fLibelle[0] = '\0';
	};
};


class CSxBrokerFeesConditionSettlePlace : public sophis::backoffice_kernel::CSRBrokerFeesConditionTransaction
{
	//------------------------------------------- PUBLIC -------------------------------------------
public:
	DECLARATION_BROKERFEES_RULES_CONDITION_TRANSACTION(CSxBrokerFeesConditionSettlePlace);

	CSxBrokerFeesConditionSettlePlace::CSxBrokerFeesConditionSettlePlace()
	{
		classKey = CSxMapping::MyDico.size();
	}

	/*
	** Test if Broker Fees has to be applied.
	*/
	virtual bool get_condition(const sophis::portfolio::CSRTransaction & trade) const;

	//------------------------------------------- PROTECTED -------------------------------------------
protected:

	//------------------------------------------- PRIVATE -------------------------------------------
private:
	/*
	** Logger data
	*/
	int classKey;
	static const char * __CLASS__;

};




#endif //!__CSxBrokerFeesConditionSettlePlace_H__