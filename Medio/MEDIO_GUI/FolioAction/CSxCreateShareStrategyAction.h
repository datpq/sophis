#ifndef __CreateShareStrategyAction_H__
	#define __CreateShareStrategyAction_H__

/*
** Includes
*/
#include "SphInc\value\strategy\SphStrategyTriggers.h"
#include "SphInc\value\strategy\SphFolioStrategy.h"


using namespace sophis::strategy;
/*
** Class
*/
class CreateShareStrategyAction : public sophis::strategy::CSAmFolioStrategyAction
{
	DECLARATION_FOLIO_STRATEGY_ACTION(CreateShareStrategyAction);

//------------------------------------ PUBLIC ---------------------------------
public:
	static void CreateShare(const CSAmFolioStrategy& strategy, long folioId);
	/**
	*	When one modifies a strategy and save it, we check if the underlying share NAV exists
	*   If it does not exist we create it, open it and throw a message that asks to update the manager code
	*   It is a workaround to create the NAV share afterward
	**/
	virtual void VoteForModification(const CSAmFolioStrategy & original, const CSAmFolioStrategy & newVersion) 
		throw (sophis::tools::VoteException, sophisTools::base::ExceptionBase) override;

private:
	static const char* __CLASS__;
};

/*
** Class
*/
class CreateShareStrategyAllocationAction : public sophis::strategy::CSAmStrategyAllocationAction
{
	DECLARATION_STRATEGY_ALLOCATION_ACTION(CreateShareStrategyAllocationAction);

//------------------------------------ PUBLIC ---------------------------------
public:
	/**
	*	The first time a folio is associated to a strategy, we create the share NAV
	*   If the share NAV already exists, we throw a warning
	*   If it does not exist, we create it, open it and throw a message that asks to update the manager code
	**/
	virtual void VoteForCreation(const SStrategyAllocation& strategyAllocation)
		throw (sophis::tools::VoteException, sophisTools::base::ExceptionBase) override;

	/**
	*	Update the NAV share with the new allocation properties
	**/
	virtual void VoteForModification(const SStrategyAllocation& original, const SStrategyAllocation& newVersion)
		throw (sophis::tools::VoteException, sophisTools::base::ExceptionBase) override;

private:
	static const char* __CLASS__;
};

#endif //!CreateShareStrategyAction_H__