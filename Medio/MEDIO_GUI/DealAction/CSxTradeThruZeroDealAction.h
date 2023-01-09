#ifndef __CSxTradeThruZeroDealAction_H__
	#define __CSxTradeThruZeroDealAction_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphKernelEngine.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include __STL_INCLUDE_PATH(vector)

enum eTradeReportingType
{
	BuyLong, SellLong, BuyCover, SellShort, Long, Short, UnDefined
};

class CSxTradeThruZeroDealAction : public sophis::backoffice_kernel::CSRKernelEngine
{
	DECLARATION_KERNEL_ENGINE(CSxTradeThruZeroDealAction);

//------------------------------------ PUBLIC ---------------------------------
public:

	CSxTradeThruZeroDealAction() : CSRKernelEngine()
	{
		_KernStatusGroupID = GetStatusGroupFromConfig();
		_EXTREF_RBC_REPORTING_BE = GetExtRefNameBEFromConfig();
		_EXTREF_RBC_REPORTING_BE_EXCEPTION = GetExtRefNameBEExceptionFromConfig();
	}

	/** Method called when an event is executed on a deal and the workflow tells to execute this action.
	This is called during an internal notify transaction action.
	By default, calls the other Run method (for compatibility reason).
	@param original is the transaction before modification/deletion. 0 if new transaction.
	@param final is the transaction after creation/modification. 0 if deletion.
	@param generationType is defined at the action level.
	@param mess is a list of events to add your own messages.
	@param event_id is the kernel event exectuted to move from one transition to another one.
	@version 5.3 add a parameter event_id.
	@version 5.2 add a parameter final.
	*/
	virtual void Run( const sophis::portfolio::CSRTransaction* original,
						const sophis::portfolio::CSRTransaction* final,
						const _STL::vector<long>& recipientType,
						sophis::backoffice_kernel::eGenerationType generationType,
						sophis::tools::CSREventVector& mess,
						long event_id) const;

private:

	CSRTransactionVector* GetProvisionTrades(const CSRTransaction* transaction, bool IncludeCurrentTrade = true) const;
	static bool IsShortSelling(double totalQty);
	static void SetExternalReference(TransactionIdent code, _STL::string name, _STL::string value);
	static double GetTotalQuantity(CSRTransactionVector* transactionVec);
	eTradeReportingType GetTradeReportingType(const CSRTransaction* transaction, double provQty) const;

	static inline _STL::string ToString(eTradeReportingType v)
	{
		switch (v)
		{
		case BuyLong:   return "BuyLong";
		case SellLong:   return "SellLong";
		case BuyCover: return "BuyCover";
		case SellShort: return "SellShort";
		case Long: return "Long";
		case Short: return "Short";
		default:      return "UnDefined";
		}
	}

	static bool IsInStatusGroup(long statusID, long groupID);
	static long GetStatusGroupFromConfig();
	static _STL::string GetExtRefNameBEFromConfig();
	static _STL::string GetExtRefNameBEExceptionFromConfig();
	static long _KernStatusGroupID;
	static _STL::string _EXTREF_RBC_REPORTING_BE;
	static _STL::string _EXTREF_RBC_REPORTING_BE_EXCEPTION;
	static const char* __CLASS__;
};

#endif //!__CSxTradeThruZeroDealAction_H__