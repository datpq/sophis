#ifndef __MediolanumTransactionAction_H__
	#define __MediolanumTransactionAction_H__

/*
** Includes
*/
#include "SphInc/portfolio/SphTransactionAction.h"
#include "SphInc/portfolio/SphTransaction.h"

/*
** Class
*/
class MediolanumTransactionAction : public sophis::portfolio::CSRTransactionAction
{
	DECLARATION_TRANSACTION_ACTION(MediolanumTransactionAction);

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

	/** Ask what to send when creating.
	When saving, when all the triggers have accepted the creation, they are called again
	in the order eOrder + lexicographical order via
	NotifyCreated to check if they have something to save in the database or send to other servers.
	At this stage, the position ID and the ID of the transaction is created.
	@param transaction is the transaction to create. It is a const object so you cannot
	modify it.
	@param message is an event vector to put your messages to send after the data has been committed.
	If you need to send data, you cannot do it immediately because you must be sure that the
	message is in concordance with the database, which will be the case after committing. Moreover, if there is an exception in another trigger, the message must not be sent.
	@throws ExceptionBase if you reject that creation. Do not send a VoteException. This can be problematic, as in case of multi-saving, it will assume that no data or message is done. Do not commit, nor Rollback either,
	this is performed elsewhere.
	*/
	virtual void NotifyCreated(const CSRTransaction &transaction, tools::CSREventVector & message)
		throw (sophisTools::base::ExceptionBase);

	/** Ask what to send when modifying.
	When saving, when all triggers have accepted the modification, they are called again
	in the order eOrder + lexicographical order via
	NotifyModified to check whether they have data to save in the database or send to other servers.
	@param original is the original transaction before any modification.
	@param transaction is the modified transaction. It is a const object so you cannot
	modify it.
	@param message is an event vector to put your messages to send after the data are committed.
	If you need to send data, you cannot do it immediately because you must be sure that the
	message is in concordance with the database. which will be true after committing. Moreover, if there is an exception in another trigger, the message must not be sent.
	@throws ExceptionBase if you reject the modification. Do not send a VoteException. This can cause problems, as in case of multi-saving it will assume that no data or message is done. Do not commit, nor Rollback either,
	this is performed elsewhere.
	*/
	virtual void NotifyModified(const CSRTransaction &original, const CSRTransaction &transaction, tools::CSREventVector & message)
		throw (sophisTools::base::ExceptionBase);

	/** Ask what to notify when deleting.
	When deleting, when all the triggers have accepted the deletion, they are called again
	in the eOrder + lexicographical order via
	NotifyDeleted to check if there is something to save in the database or send to other servers.
	@param transaction is the original transaction before any modification.
	@param message is an event vector to put your messages to send after the data are committed.
	If you need to send data, you cannot do it immediately because you must be sure that the
	message is in concordance with the database which will be the case after committing. Moreover, if there is an exception in another trigger, the message must not be sent.
	@throws ExceptionBase if you reject the deletion. Do not send a VoteException. This can be problematic, as in case of multi-saving it will assume that no data or message is done. Do not commit, nor Rollback either,
	this is performed elsewhere.
	*/
	virtual void NotifyDeleted(const CSRTransaction &transaction, tools::CSREventVector & message)
		throw (sophisTools::base::ExceptionBase);

	bool InsertOrUpdateExtRefToDB(const TransactionIdent ident, const _STL::string& refName, const _STL::string& refValue);

private:
	static const char * __CLASS__;
	static _STL::string CashUserNames;
};

#endif //!__MediolanumTransactionAction_H__