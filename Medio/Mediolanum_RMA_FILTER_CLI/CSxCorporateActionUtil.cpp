#include "CSxCorporateActionUtil.h"
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/market_data/SphCorporateActionHandler.h"
#include "SphInc/market_data/SphCorporateActionMgr.h"
#include "SphInc/static_data/SphCorporateActionType.h"
#include "SphTools/SphLoggerUtil.h"

const char * CSxCorporateActionUtil::__CLASS__ = "CSxCorporateActionUtil";

bool CSxCorporateActionUtil::CreateFreeAttribution(int sicovam, double coefficient, int businessEvent, int exDivDate, int date, int paymentDate, char* comment)
{
	BEGIN_LOG("CreateFreeAttribution");
	_STL::vector<SSGlobalCorporateAction> caDataList;
	CSRCorporateActionHandler corporateActionHandler;
	SSGlobalCorporateAction globalCorporateAction;
	globalCorporateAction.sicovam = sicovam;
	//globalCorporateAction.adjust1 = new sophis::market_data::SSCorporateActionRead();
	SSCorporateActionRead adjust1; // = new sophis::market_data::SSCorporateActionRead();
	globalCorporateAction.adjustments.push_back(adjust1);
	//if(globalCorporateAction.adjustments[0]!=NULL) -- NO MORE A POINTER
	{
		globalCorporateAction.adjustments[0].sicovam =  sicovam;
		globalCorporateAction.adjustments[0].type = eAdjustementType::caFreeAttribution;
		globalCorporateAction.adjustments[0].businessEvent1 = (sophis::portfolio::eTransactionType) businessEvent;
		globalCorporateAction.adjustments[0].exDivDate = exDivDate;
		globalCorporateAction.adjustments[0].paymentDate = paymentDate;
		globalCorporateAction.adjustments[0].date = date;
		globalCorporateAction.adjustments[0].coefficient = coefficient;
		if(comment != NULL)
		{

			strcpy_s(globalCorporateAction.adjustments[0].comment,comment);
			delete comment;
			comment = NULL;
		}
		caDataList.push_back(globalCorporateAction);
	}
	//else
	//{
	//	MESS(Log::error,FROM_STREAM("Failed to create a new SSCorporateActionRead"));
	//}
	END_LOG();
	return corporateActionHandler.Write(caDataList);
}


bool CSxCorporateActionUtil::CreateMerger(int sicovam, int take_over, double coefficient, double convRatioNum, int convRatioDenom, int businessEvent, int exDivDate, int date, int paymentDate, bool withAveragePrice, char* comment)
{
	BEGIN_LOG("CreateMerger");
	_STL::vector<SSGlobalCorporateAction> caDataList;
	CSRCorporateActionHandler corporateActionHandler;
	SSGlobalCorporateAction globalCorporateAction;
	globalCorporateAction.sicovam = sicovam;
	//globalCorporateAction.adjust1 = new sophis::market_data::SSCorporateActionRead();
	SSCorporateActionRead adjust1; // = new sophis::market_data::SSCorporateActionRead();
	globalCorporateAction.adjustments.push_back(adjust1);
	//if(globalCorporateAction.adjust1!=NULL)
	{
		globalCorporateAction.adjustments[0].sicovam = sicovam;
		globalCorporateAction.adjustments[0].diffusedCode = take_over;
		globalCorporateAction.adjustments[0].type = (withAveragePrice)?(eAdjustementType::caMerge_average_price):(eAdjustementType::caMerger);
		globalCorporateAction.adjustments[0].businessEvent1 = (sophis::portfolio::eTransactionType) businessEvent;
		globalCorporateAction.adjustments[0].exDivDate = exDivDate;
		globalCorporateAction.adjustments[0].paymentDate = paymentDate;
		globalCorporateAction.adjustments[0].date = date;
		globalCorporateAction.adjustments[0].coefficient = coefficient;
		globalCorporateAction.adjustments[0].convRatioNum = convRatioNum;
		globalCorporateAction.adjustments[0].convRatioDenom = convRatioDenom;
		if(comment != NULL)
		{
			strcpy_s(globalCorporateAction.adjustments[0].comment,comment);
			delete comment;
			comment = NULL;
		}
		caDataList.push_back(globalCorporateAction);
	}
	//else
	//{
	//	MESS(Log::error,FROM_STREAM("Failed to create a new SSCorporateActionRead"));
	//}
	END_LOG();
	return corporateActionHandler.Write(caDataList);
}

bool CSxCorporateActionUtil::CreateDemerger(int sicovam, int diffused_code, double coefficient, double convRatioNum, int convRatioDenom, int businessEvent, int exDivDate, int date, int paymentDate, char* comment)
{
	BEGIN_LOG("CreateDemerger");
	_STL::vector<SSGlobalCorporateAction> caDataList;
	CSRCorporateActionHandler corporateActionHandler;
	SSGlobalCorporateAction globalCorporateAction;
	globalCorporateAction.sicovam = sicovam;
//	globalCorporateAction.adjust1 = new sophis::market_data::SSCorporateActionRead();
	SSCorporateActionRead adjust1; // = new sophis::market_data::SSCorporateActionRead();
	globalCorporateAction.adjustments.push_back(adjust1);
	//if(globalCorporateAction.adjust1!=NULL)
	{
		globalCorporateAction.adjustments[0].sicovam = sicovam;
		globalCorporateAction.adjustments[0].diffusedCode = diffused_code;
		globalCorporateAction.adjustments[0].type = eAdjustementType::caDemerger;
		globalCorporateAction.adjustments[0].businessEvent1 = (sophis::portfolio::eTransactionType) businessEvent;
		globalCorporateAction.adjustments[0].exDivDate = exDivDate;
		globalCorporateAction.adjustments[0].paymentDate = paymentDate;
		globalCorporateAction.adjustments[0].date = date;
		globalCorporateAction.adjustments[0].coefficient = coefficient;
		globalCorporateAction.adjustments[0].convRatioNum = convRatioNum;
		globalCorporateAction.adjustments[0].convRatioDenom = convRatioDenom;
		if(comment != NULL)
		{
			strcpy_s(globalCorporateAction.adjustments[0].comment,comment);
			delete comment;
			comment = NULL;
		}
		caDataList.push_back(globalCorporateAction);
	}
	//else
	//{
	//	MESS(Log::error,FROM_STREAM("Failed to create a new SSCorporateActionRead"));
	//}
	END_LOG();
	return corporateActionHandler.Write(caDataList);
}

CSxCorporateActionUtil::CSxCorporateActionUtil(void)
{

}


CSxCorporateActionUtil::~CSxCorporateActionUtil(void)
{
}
