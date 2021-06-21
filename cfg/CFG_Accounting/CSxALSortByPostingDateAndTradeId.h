#ifndef __CSxALSortByPostingDateAndTradeId_H__
#define __CSxALSortByPostingDateAndTradeId_H__
/*
** Includes
*/
#include "SphInc/accounting/SphAuxiliaryLedger.h"
#include "SphInc/accounting/SphAccountingPosting.h"

/*
** Class 
*/
class CSxALSortByPostingDateAndTradeId : public sophis::accounting::CSRAuxiliaryLedgerSort
{
	//------------------------------------ PUBLIC ---------------------------------
public:
	DECLARATION_AUX_LEDGER_RULES_SORT(CSxALSortByPostingDateAndTradeId)

	virtual bool IsLower(const SSFinalPostingAuxID &f1 , const SSFinalPostingAuxID &f2) const; // not used
	int FirstToSort(const SSFinalPostingAuxID &a, const SSFinalPostingAuxID &b, int splitPostingType) const;
	static bool OldSortAL();

	//------------------------------------ PROTECTED --------------------------------
protected:
	static const bool isOldSortAL;
	
	//------------------------------------ PRIVATE --------------------------------
private:

	// For log purpose
	static const char* __CLASS__;	
};


#endif // ! __CSxALSortByPostingDateAndTradeId_H__
