#ifndef __CheckPrice_H__
	#define __CheckPrice_H__

/*
** Includes
*/
#include "SphInc/portfolio/SphTransactionAction.h"
#include "SphInc/portfolio/SphTransaction.h"
#include "SphInc/instrument/SphBond.h"


/*
** Class
*/
class CheckPrice : public sophis::portfolio::CSRTransactionAction
{
	DECLARATION_TRANSACTION_ACTION(CheckPrice);

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

	/** Ask for a deletion of a transaction.
	When deleting, all the triggers will be called via VoteForDeletion to check if they accept the
	modification in the order eOrder + lexicographical order.
	@param transaction is the original transaction before any modification.
	@throws VoteException if you reject that deletion.
	*/
	virtual void VoteForDeletion(const CSRTransaction &transaction)
		throw (sophis::tools::VoteException);



	double GetCollateralizedPrice(const CSRBond * currentBond, const sophis::portfolio::CSRTransaction * transaction, const CSRLoanAndRepo * currentLoanAndRepo);
	void BondCheckPrice(const sophis::portfolio::CSRTransaction * original, const sophis::portfolio::CSRTransaction * transaction, bool isDeleted = false);
//------------------------------------------------ PRIVATE ------------------------------------------------
private:
	/*
	** Logger data
	*/
	static const char * __CLASS__;

	static bool isForceMode;
	
	static bool isShare;
};

#endif //!__CheckPrice_H__