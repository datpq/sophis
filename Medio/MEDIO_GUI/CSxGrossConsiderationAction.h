#pragma once

#ifndef __DelegatedPortfolioTransactionAction_H__
#define __DelegatedPortfolioTransactionAction_H__

/*
** Includes
*/
#include "SphInc/portfolio/SphTransactionAction.h"
#include "SphInc/portfolio/SphTransaction.h"

/*
** Class
*/
class CSxGrossConsiderationAction : public sophis::portfolio::CSRTransactionAction
{
    //------------------------- PUBLIC -------------------------
public:

    DECLARATION_TRANSACTION_ACTION(CSxGrossConsiderationAction);
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
};

#endif //!__DelegatedPortfolioTransactionAction_H__
