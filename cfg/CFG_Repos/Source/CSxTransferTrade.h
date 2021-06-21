#ifndef __CSxTransferTrade_H__
#define __CSxTransferTrade_H__

#include "SphInc/portfolio/SphTransferTrade.h"
#include "SphInc/scenario/SphScenario.h"


class CSxTransferTrade : public sophis::portfolio::CSRTransferTrade
{
public:		
	virtual long TransferForCreationTrade(CSRTransaction &transaction);

	virtual eActionToTransfer TransferCorporateAction(const CorporateAction &corporateAction);
	
	static CSRTransferTrade * CreateInstance() 
	{
		return new CSxTransferTrade;
	}

private:
	static const char * __CLASS__;
};

class CSxExecute : public CSRScenario	
{
public:
	DECLARATION_SCENARIO(CSxExecute)
	
	virtual eProcessingType	GetProcessingType() const;

	virtual	void	Run();

private:
	static const char* __CLASS__;

};

#endif //!__CSxTransferTrade_H__