#ifndef __CSxTransactionAction_H__
#define __CSxTransactionAction_H__

/*
** Includes
*/
#include "SphInc/portfolio/SphTransactionAction.h"
#include "SphInc/portfolio/SphTransaction.h"

/*
** Class
*/
class CSxTransactionAction : public sophis::portfolio::CSRTransactionAction
{
	DECLARATION_TRANSACTION_ACTION(CSxTransactionAction);
private:
	static const char * __CLASS__;
	//------------------------- PUBLIC -------------------------
public:

	/** Ask for a creation of a transaction.
	When creating, all the triggers will be called via VoteForCreation to check if they accept the
	creation in the order eOrder + lexicographical order.
	At this stage, the position ID and the ID of the transaction are not
	necessarily created.
	@param transaction is the transaction to create. It is a non-const object so you can
	modify it.
	@throws VoteException if you reject that creation.
	*/
	virtual void VoteForCreation(CSRTransaction &transaction)
		throw (sophis::tools::VoteException);	

	virtual void VoteForModification(const CSRTransaction & original, CSRTransaction &transaction, long event_id)
		throw (sophis::tools::VoteException);

	void ComputeTvaAmounts(CSRTransaction &transaction);	
	double ComputeTvaAmount(const double fieldAmount, const double rate);

	void ComputeSLInterestAmount(CSRTransaction &transaction);

};

#endif //!__CSxTransactionAction_H__