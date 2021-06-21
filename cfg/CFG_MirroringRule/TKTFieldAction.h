#ifndef __TKTFieldAction_H__
	#define __TKTFieldAction_H__

/*
** Includes
*/
#include "SphInc/portfolio/SphTransactionAction.h"
#include "SphInc/portfolio/SphTransaction.h"

/*
** Class
*/
class TKTFieldAction : public sophis::portfolio::CSRTransactionAction
{
	DECLARATION_TRANSACTION_ACTION(TKTFieldAction);

//------------------------- PUBLIC -------------------------
public:

	virtual void NotifyCreated(const CSRTransaction &transaction, tools::CSREventVector & message, long event_id)
				throw (sophisTools::base::ExceptionBase);

	/** Ask for a modification of a transaction.
	When modifying, all triggers will be called via VoteForModification to check whether they accept the
	modification in the order eOrder + lexicographical order.
	@param original is the original transaction before any modification.
	@param transaction is the modified transaction. It is a non-const object so you can
	modify it.
	@throws VoteException if you reject that modification.
	*/
	virtual void VoteForModification(const CSRTransaction & original, CSRTransaction &transaction)
		throw (sophis::tools::VoteException);

	
//------------------------------------ PRIVATE --------------------------------
private:

	static const char* __CLASS__;


};

#endif //!__TKTFieldAction_H__