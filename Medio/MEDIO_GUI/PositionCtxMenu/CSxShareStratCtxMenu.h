#ifndef __CSxShareStratCtxMenu_H__
	#define __CSxShareStratCtxMenu_H__

/*
** Includes
*/
#include "SphInc/portfolio/SphPositionCtxMenu.h"
#include "SphInc/portfolio/SphPosition.h"

/**
Class CSxPositionCtxMenu:
Define a contextual menu on positions.
*/
class CSxShareStratCtxMenu : public sophis::portfolio::CSRPositionCtxMenu
{
//------------------------------------ PUBLIC ---------------------------------
public:

	/** 
	Return true if this menu must be displayed with this position selection.
	Else return false.
	@param positionList is the list of positions selected before right click.
	Do not delete or modify this list.
	@return true if the menu must be displayed with this list of positions.
	*/
	virtual bool IsAuthorized(const sophis::portfolio::CSRPositionVector& positionList) const override 
	{
		return false;
	}

	/**
	Perform the action associated to the menu on the list of positions.
	@param positionList is the list of positions selected. Do not delete or modify this list.
	If one wants to modify a position, clone it (with Clone_Api() ) and save it:
	- see SphPosition.h. 
	*/
	virtual void Action(sophis::portfolio::PSRExtraction extraction, const sophis::portfolio::CSRPositionVector& positionList, const char* ActionName = "") const override
	{
	}

	virtual bool IsFolioAuthorized(const CSRPortfolioVector &folioVector) const override;
	

	/**
	*  Perform the action associated to the menu on the list of portfolio.
	*  @param positionList is the list of positions selected. Do not delete or modify this list.
	*  If one wants to modify a position, clone it (with Clone_Api() ) and save it:
	*  - see SphPosition.h. 
	*/
	virtual void FolioAction(const CSRPortfolioVector &folioVector, const char* ActionName = "") const override;

//------------------------------------ PRIVATE --------------------------------
private:
	static const char* __CLASS__;
	DECLARATION_POSITION_CTX_MENU(CSxShareStratCtxMenu)
};


#endif //!__CSxPositionCtxMenu_H__