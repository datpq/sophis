#include "CSxFOAssetClassCriterium.h"
#include "SphInc\portfolio\SphExtraction.h"
#include "SphInc\portfolio\SphPortfolio.h"
#include "SphLLInc\interface\CustomMenu.h"
#include "SphTools\SphLoggerUtil.h"
#include <SphInc/instrument/SphForexFuture.h>
#include <SphInc/instrument/SphFuture.h>
#include <SphSDBCInc/params/outs/SphOut.h>
#include <SphSDBCInc/queries/SphQuery.h>

#include <SphInc\finance\SphPricer.h>

//-------------------------------------------------------------------------
/*static*/ const char* CSxFOAssetClassCriterium::__CLASS__="CSxThirdPartyRatingCriterium";
/*static*/ const char* CSxFOAssetClassCriterium::_ThirdPartyProperty = MEDIO_THIRDPARTY_PROP_RATING;


//-------------------------------------------------------------------------
/*virtual*/ void CSxFOAssetClassCriterium::GetCode( SSReportingTrade* mvt, TCodeList &list)  const
{
	BEGIN_LOG("GetCode");
	MESS(Log::debug, "Begin - Reporting Trade");
	list.clear();
	SSOneValue val;
	val.fType = 0;
	val.fCode = NOTDEFINED;
	list.push_back(val);

	const CSRInstrument* instrument = CSRInstrument::GetInstance(mvt->sicovam);
	if(instrument)
	{
		SSPricerHandler pricerHandler = CSRPricer::GetPricerByPriority(*instrument);
		//SSMetaModelHandler metaModelHandler = CSRMetaModel::GetMetaModelByPriority(*instrument);
		const CSRPricer* pricer =  &pricerHandler.model;
		if(pricer)
		{
			const CSRComputationResults* results = instrument->GetComputationResults(instrument->GetCode());
			GetCode(*instrument, results, list);
		}
	}

	MESS(Log::debug, "Code : " << list[0].fCode);
	MESS(Log::debug, "End");
	END_LOG();
}


//-------------------------------------------------------------------------
/*virtual*/ void CSxFOAssetClassCriterium::GetCode( const sophis::instrument::ISRInstrument& instr, 
			                     const sophis::CSRComputationResults* results, 
			                     TCodeList& list)  const
{
	BEGIN_LOG("GetCode");
	MESS(Log::debug, "Begin - Instrument");
	list.clear();
	SSOneValue val;
	val.fType = 0;
	val.fCode = NOTDEFINED;
	list.push_back(val);

	const CSRCriterium* ranking = GetRankingCriterium();
	if(ranking != NULL)
	{
		ranking->GetCode(instr, results, list);
	}
	/*if(IsFXInstrument(instr))
	{
		const CSRInstrument* inst = GetOneEquityinstrument();
		if(inst)
		{
			ranking->GetCode(*inst, results, list);			
		}
	}*/

	MESS(Log::debug, "Code : " << list[0].fCode);

	MESS(Log::debug, "End");
	END_LOG();
}

//-------------------------------------------------------------------------
/*virtual*/ void CSxFOAssetClassCriterium::GetName(long code, char* name,size_t size) const
{
	BEGIN_LOG("GetName");
	MESS(Log::debug, "Begin");
	if (code == -1)
	{
		strcpy_s(name, 10, "N/A");
		return;
	}
	const CSRCriterium* ranking = GetRankingCriterium();
	if(ranking != NULL)
	{
		ranking->GetName(code, name,size);
		MESS(Log::debug, "Code = " << code);
		MESS(Log::debug, "Name = '" << name << "'");
	}
	END_LOG();
}


//-------------------------------------------------------------------------
const CSRCriterium* CSxFOAssetClassCriterium::GetRankingCriterium() const
{
	BEGIN_LOG("GetRankingCriterium");
	if(fRankingCriterium == NULL)
	{
		CSRCriterium::prototype::iterator itFind = CSRCriterium::GetPrototype().find("Ranking FO Asset Class");
		if (itFind != CSRCriterium::GetPrototype().end())
		{
			MESS(Log::debug, "Found");
			fRankingCriterium = itFind->second;
		}
	}
	END_LOG();
	return fRankingCriterium;
}


//-------------------------------------------------------------------------
const bool CSxFOAssetClassCriterium::IsFXInstrument(const sophis::instrument::CSRInstrument& inst) const
{
	BEGIN_LOG("IsFXInstrument");

	bool isNDF = false, isFX = false, isFXForward = false, res = false;
	if(&inst)
	{
		const CSRForexSpot* forex = dynamic_cast<const CSRForexSpot*>(&inst);
		const CSRNonDeliverableForexForward* ndf = dynamic_cast<const CSRNonDeliverableForexForward*>(&inst);
		const CSRForexFuture* future = dynamic_cast<const CSRForexFuture*>(&inst);
		if (future != NULL) isFXForward = future->GetCurrencyCode() != future->GetExpiryCurrency();
		res = (forex != NULL || ndf != NULL || isFXForward);
	}
	END_LOG();
	return res;
}


//-------------------------------------------------------------------------
const CSRInstrument* CSxFOAssetClassCriterium::GetOneEquityinstrument() const
{
		BEGIN_LOG("GetOneEquityinstrument");
		if(!fOneEquityInstrument)
		{
			sophis::sql::CSRQuery oneIntQuery;
			oneIntQuery.SetName("GetOneEquityinstrument");
			long sico = 0;

			oneIntQuery	<< "SELECT "
				<< sophis::sql::CSROut("sicovam", sico)
				<< "from titres where type = 'A' and rownum = 1";

			if (oneIntQuery.Fetch())
			{
				LOG(Log::debug, FROM_STREAM("Sicovam fetched: "<<sico));
				END_LOG();
			}
			fOneEquityInstrument = CSRInstrument::GetInstance(sico);
		}

		END_LOG();
		return fOneEquityInstrument;
}





