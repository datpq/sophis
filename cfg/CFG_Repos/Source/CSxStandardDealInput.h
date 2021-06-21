#ifndef __CSxStandardDealInput_H__
#define __CSxStandardDealInput_H__

/*
** Includes
*/
#include "SphInc/Value/kernel/SphAMOverloadedDialogs.h"
#include "../Resource/resource.h"

/*
** Class CSAMTransactionDialog: for displaying and managing a trade deal.
This class will serve as a wrapper of the standard trade deal dialog.
A CSAMTransactionDialog-derived will customise functions and content of the standard dialog.
Note: 
Use the following procedures to modify the related objects to HISTOMVTS
- the procedure tk_add_histomvts_column to add a new column
- the procedure tk_modify_histomvts_column to modify the datatype of an existing column

The procedure 'tk_add_histomvts_column' does the following:

- Disable triggers on the tables involved (see below) 
- Add the column to these tables: HISTOMVTS, AUDIT_MVT, JO_HISTOMVTS, MVT_AUTO, PREVISION, FORECAST_FIXING, HISTOMVTS_VIRTUAL, 
EOY_TRADE, MVT_AUTO_VW_TMP, PREVISION_VW_TMP, FORECAST_NEWDEAL, FORECAST_NEWPOSITION, MVTBACK, MODIFIED_ARCHIVE  
- Add VARCHAR(255) column with the given name to MA_PENDING_TICKETS. MA_PROCESSED_TICKETS for the market adapter. 
- Modify the bodies of the following parallel forecasts-related objects: 
- views: MVT_AUTO_VW, PREVISION_VW 
- triggers: MVT_AUTO_VW_TRG, PREVISION_VW_TRG 
- procedures: PREPARE_MVT_AUTO_VW_TMP, APPLY_MVT_AUTO_VW_TMP, PREPARE_PREVISION_VW_TMP, APPLY_PREVISION_VW_TMP 
- Modify the following Value views if necessary: 
- INTEREST_REPORTING, SR_FEES_REPORTING, SR_REPORTING, UNEXEC_ORDER_REPORTING, PFR_SIMU_VIEW 
- Restore touched triggers' statuses. 

Human-readable log is output to the serveroutput, useful for debugging. 

The user executing tk_add_histomvts_column needs to have CREATE VIEW, CREATE TRIGGER, CREATE PROCEDURE privileges. 
This was usually not the case prior to this development. The procedure checks for these before starting. 
tk_add_histomvts_column can be executed multiple times with the same arguments without unwanted side effects � 
it expects that the column might have been added already and handles exceptions quite well. 
*/
class CSxStandardDealInput : public sophis::value::CSAMTransactionDialog
{
	//------------------------------------ PUBLIC ------------------------------------
public:	


	/**Validates an element in the context.
	When selection moves from one element of the dialog to another one, the method CSRElement::Validation()
	of the current element is called and (if the call ends successfully) so is CSRFitDialog::ElementValidation().
	The method is overridden in order to :
	- check the coherence of the current setting,
	- take an appropriate action with respect to the current context.
	@param EAId_Modified is the absolute ID of the modified element
	@version 4.5.2
	*/
	virtual	void	ElementValidation(int EAId_Modified);

	virtual void	OpenAfterInit(void);

	// Fields enumeration
	enum // already without ID_ITEM_SHIFT
	{
		eBrokerTva		= IDC_BROKER_TVA		- ID_ITEM_SHIFT,
		eGrossTva			= IDC_GROSS_TVA			- ID_ITEM_SHIFT,
		eCounterpartyTva	= IDC_COUNTERPARTY_TVA	- ID_ITEM_SHIFT,
		eMarketTva		= IDC_MARKET_TVA		- ID_ITEM_SHIFT,
		eSpreadHT			= IDC_SPREAD_HT - ID_ITEM_SHIFT,
		eSpreadHTLabel  = IDC_SPREAD_HT_LABEL - ID_ITEM_SHIFT,
		eRepoAmount		= IDC_REPO_AMOUNT - ID_ITEM_SHIFT,
		eRepoAmountLabel  = IDC_REPO_AMOUNT_LABEL - ID_ITEM_SHIFT,
		eInterestAmountLabel		= IDC_INTEREST_AMOUNT_LABEL		- ID_ITEM_SHIFT,
		eInterestAmount = IDC_INTEREST_AMOUNT - ID_ITEM_SHIFT,
		eFinalAmount = IDC_FINAL_AMOUNT - ID_ITEM_SHIFT,
		eInterestAmountHTLabel		= IDC_INTEREST_AMOUNT_HT_LABEL		- ID_ITEM_SHIFT,
		eInterestAmountHT =	IDC_INTEREST_AMOUNT_HT - ID_ITEM_SHIFT,
		eFinalAmountHTLabel = IDC_FINAL_AMOUNT_HT_LABEL - ID_ITEM_SHIFT,
		eFinalAmountHT = IDC_FINAL_AMOUNT_HT - ID_ITEM_SHIFT,
		eNbFields = 13
	};

	//------------------------------------ PROTECTED ------------------------------------
protected:	

	/** Set the text field according to the external key: TEXT_<externalkey>
	*/    

	void SetTvaAmount(const double fieldAmount, const int eTvaField, const double rate);	

	void ComputeTvaAmounts();
	//------------------------------------ PRIVATE ------------------------------------
private:

	DECLARATION_DIALOG(CSxStandardDealInput)
		static const char * __CLASS__;
};


#endif //!__CSxStandardDealInput_H__