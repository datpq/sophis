#ifndef __CSxOptionModel_H__
	#define __CSxOptionModel_H__

/*
** Includes
*/
#include "SphInc/instrument/SphOption.h"
#include "SphInc/instrument/SphInstrument.h"

/*
** Class definition for a clause.
*/
class CSxOptionModel : public sophis::instrument::CSROption
{
//------------------------------------ PRIVATE --------------------------------
private:
	
	DECLARATION_OPTION(CSxOptionModel)

//------------------------------------ PUBLIC ------------------------------------
public:
	
	virtual bool			IsWithMarginCall() const override;

};


#endif // __CSxOptionModel_H__
