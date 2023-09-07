#pragma once
#include "SphInc\portfolio\SphReportingCustomFilter.h"


using namespace sophis;
using namespace portfolio;

 class CSxReportingHedge:public CSRReportingCustomFilter
{
public :

	 CSxReportingHedge() {};
	 ~CSxReportingHedge() {};
	

protected:

	/** Callback when processing trade during the reporting
	*/
	virtual bool FilterDealHook(const SSReportingTrade* trade, const CSRAccountingPrinciple* principle, portfolio::PSRExtraction extraction) const override;


};

 static CSxReportingHedge gTKTReportingHook;
