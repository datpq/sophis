/////////////////////////////////////////////////////////////////////////////////////////
// CSxAccountingQuantity.h
/////////////////////////////////////////////////////////////////////////////////////////

#pragma once

/*
** Includes
*/
#include "SphInc/accounting/SphAccountingQuantity.h"


/*
** Class
*/
class CSxAccountingQuantity : public sophis::accounting::CSRAccountingQuantity
{
//------------------------------------- PUBLIC -------------------------------------
public:
	DECLARATION_ACCOUNTING_QUANTITY(CSxAccountingQuantity);

//------------------------------------- PROTECTED -------------------------------------
protected:
	/** Get the quantity for the trade engine.
	@param trade is the transaction.
	@param flow is the amount sign.
	@param quantity is the output parameter quantity.
	@return what to populate.
	By default call the old interface for maintenance reason which return NotDefined.
	@version 4.5.0 add e new parameter for the sens.
	*/
	virtual ID_to_Populate get_quantity( const sophis::portfolio::CSRTransaction& trade, eFlow flow, double* quantity ) const ;

	/** Get the quantity for the P&L engine.
	@param position is the position.
	@param flow is the amount sign.
	@param quantity is the output parameter quantity.
	@return what to populate.
	By default call the old interface for maintenance reason which return NotDefined.
	@version 4.5.0 add e new parameter for the sens.
	*/
	virtual ID_to_Populate get_quantity( const sophis::portfolio::CSRPosition& position, eFlow flow, double* quantity ) const ;

	/** Get the quantity for the balance engine.
	@param instrument_id is the instrument id obtained with the group query in positing amount.
	@return what to populate.
	The quantity to populate comes from the group query.
	@since 4.5.0
	*/
	virtual ID_to_Populate populate_in_balance_engine( long instrument_id ) const ;

//------------------------------------- PRIVATE -------------------------------------
private:

	/*
	** Logger data
	*/
	static const char * __CLASS__;
};