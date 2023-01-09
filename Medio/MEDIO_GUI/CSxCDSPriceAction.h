#pragma once


/*
** Includes
*/
#include "SphInc/portfolio/SphTransactionAction.h"
#include "SphInc/portfolio/SphTransaction.h"

/*
** Class
*/
class CSxCDSPriceAction : public sophis::portfolio::CSRTransactionAction
{
    //------------------------- PUBLIC -------------------------
public:

    DECLARATION_TRANSACTION_ACTION(CSxCDSPriceAction);
    /** Ask for a creation of a transaction.
    When creating, all the triggers will be called via VoteForCreation to check if they accept the
    creation in the order eOrder + lexicographical order.
    At this stage, the position ID and the ID of the transaction are not
    necessarily created.
    @param transaction is the transaction to create. It is a non-const object so you can
    modify it.
    @throws VoteException if you reject that creation.
    */
    virtual void VoteForCreation(sophis::portfolio::CSRTransaction &transaction) override;

	virtual void VoteForModification(const CSRTransaction & original, CSRTransaction &transaction, long event_id)
				throw (sophis::tools::VoteException);

};
