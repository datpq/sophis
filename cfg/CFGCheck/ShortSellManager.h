///////////////////////////////////////////////////////////////////////
// ShortSellManager.h 
///////////////////////////////////////////////////////////////////////

#pragma once

/*
** Class
*/
class CSxShortSellManager
{
//------------------------------------ PUBLIC ------------------------------------
public:
	/*
	** Destructor
	*/
	virtual ~CSxShortSellManager();

	/*
	** Singleton
	*/
	static CSxShortSellManager & getInstance();

	/*
	**
	**
	**
	*/
	double GetQuantityAvailableOrLended(long sicovam,long paymentDate,long fundId, bool lended);

//------------------------------------ PROTECTED ------------------------------------
protected:

//------------------------------------ PRIVATE ------------------------------------
private:
	/*
	** Constructor
	*/
	CSxShortSellManager();

	/*
	** Logger data
	*/
	static const char * __CLASS__;
};
