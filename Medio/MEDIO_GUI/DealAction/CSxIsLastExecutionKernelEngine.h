#ifndef __CsxIsLastExecutionKernelEngine_H__
#define __CsxIsLastExecutionKernelEngine_H__

/*
** Includes
*/
#include "SphInc/backoffice_kernel/SphKernelEngine.h"
//#include "SphInc/portfolio/SphPortfolioIdentifiers.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/tools/SphValidation.h"
#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"
#include "SPhinc\misc/ConfigurationFileWrapper.h"
#include __STL_INCLUDE_PATH(vector)



class CSxApplyEvent : public sophis::tools::CSRAbstractEvent
{
public:
	CSxApplyEvent(TransactionIdent refCon)
		: fRefCon(refCon)
	{}

	//*************************************************************************************************
	// Event added in an EventBridge when the soap call failed -> change the back office status
	// of the current transaction.
	//*************************************************************************************************
	virtual void Send()	throw (sophisTools::base::ExceptionBase);

	static bool IsLast(TransactionIdent id);

	static bool IsFullyExecuted(long orderId);

	static double GetOrderQuantity(long orderId);

	static double GetExecutedQuantity(long orderId);

	static std::vector<TransactionIdent> GetTradesFromOrder(long orderid);

	TransactionIdent	fRefCon;
	static _STL::set<TransactionIdent> fTradesAlreadyPlayed;
private:
	static const char* __CLASS__;
	};

//We selected a KernelEngine because:
//  - we need a 'CSREventVector' object (the check is done after the commit)
//  - we limit the check (if it is in a 'voteForModification' it may impact more trades)
class CsxIsLastExecutionKernelEngine : public sophis::backoffice_kernel::CSRKernelEngine
{
	DECLARATION_KERNEL_ENGINE(CsxIsLastExecutionKernelEngine);

	//------------------------------------ PUBLIC ---------------------------------
public:
	CsxIsLastExecutionKernelEngine();

	static int			fKernelEvent;

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
	virtual void Run(const sophis::portfolio::CSRTransaction* original,
		const sophis::portfolio::CSRTransaction* final,
		const _STL::vector<long>& recipientType,
		sophis::backoffice_kernel::eGenerationType generationType,
		sophis::tools::CSREventVector& mess,
		long event_id) const;

private:
	static const char* __CLASS__;

};

#endif //!__CsxIsLastExecutionKernelEngine_H__