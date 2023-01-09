////TODO CRITICAL issues on the SphCustomReportingCallback

#ifndef __CsxBusinessEventReportingColumnComputer_H__
#define __CsxBusinessEventReportingColumnComputer_H__

/*
** Includes
*/
// standard
#include "SphLLInc/portfolio/SphFolioStructures.h" //Recommended by Zhu Lei, to be confirmed with R&D
#include "SphInc/portfolio/SphPortfolioEnums.h"
#include "SphInc\portfolio\SphCustomReportingCallback.h"

struct SxDataDouble
{
	double data{};
};

class CsxBusinessEventReportingColumnComputer : public sophis::portfolio::CSRCustomReportingColumnComputer<SxDataDouble>
{
public:
	CsxBusinessEventReportingColumnComputer();
	SxDataDouble GetDefault() { return { 0.0 }; };


	/**
	Compute value for a given transaction
	@return true if value was computed, false if does not apply for this trade

	@param value Output for computed value
	@param extraction Trade's extraction
	@param positionId Trade's position identifier
	@param trade CSRTransaction object
	*/
	virtual bool GetValue(SxDataDouble& value, 
		sophis::portfolio::PSRExtraction extraction, 
		long positionId, 
		const sophis::portfolio::CSRTransaction& trade) override;
	

	/**
	Aggregate value for a hierarchical position
	@return true if value was computed, false if does not apply for this position

	@param value Output for computed value
	@param buffer Collection of all values for position's trades
	@param extraction Position's extraction
	@param positionId Position identifier
	@param folioId Portfolio identifier
	@param instrumentCode Instrument identifier of the flat position
	*/
	//virtual bool AggregateValue(T& value, const std::vector<T> &buffer, PSRExtraction extraction, long positionId, long folioId, long instrumentCode, long positionCcy)
	virtual bool AggregateValue(SxDataDouble& value, 
		const _STL::vector<SxDataDouble> &buffer, 
		sophis::portfolio::PSRExtraction extraction, 
		long positionId, 
		long folioId, 
		long instrumentCode, 
		long positionCcy) override;
	
	/**
		Aggregate value on the fly for a hierarchical position
		@return true if value was computed, false if does not apply for this position

		@param value Output for computed value
		@param extraction Trade's extraction
		@param positionid Trade's position identifier
		@param trade CSRTransaction object
	*/
	//virtual bool AggregateValueOnTheFly(T& value, PSRExtraction extraction, long positionId, const CSRTransaction& trade, CSRReportingCallback::Direction factor)
	virtual bool AggregateValueOnTheFly(SxDataDouble& value, 
		sophis::portfolio::PSRExtraction extraction, 
		long positionId, 
		const sophis::portfolio::CSRTransaction& trade, 
		sophis::portfolio::CSRReportingCallback::Direction factor) override;
	

	/**
		Aggregate value on the fly for a hierarchical position
		@return true if value was computed, false if does not apply for this position

		@param value Output for computed value
		@param extraction Trade's extraction
		@param positionid Trade's position identifier
		@param trade CSRTransaction object
	*/
	// virtual bool AggregateValueForFlat(T& value,	const T& hierValue,	PSRExtraction extraction,long positionId,long folioId,long instrumentCode,sophis::instrument::ePositionType lineType)
	virtual bool AggregateValueForFlat(SxDataDouble& value,
		const SxDataDouble& hierValue, 
		sophis::portfolio::PSRExtraction extraction, 
		long positionId,
		long folioId,
		long instrumentCode, 
		sophis::instrument::ePositionType lineType/*, long metaModelId*/) override;
	

	/**
	Do we compute flat ?
	*/
	virtual bool ComputeFlat() override
	{
		//flat view is bugged...
		return false;
	}

	/**
	Do we compute agrgegated value for portfolio ?
	*/
	virtual bool ComputePortfolio() override
	{
		return true;
	}

private:
	static const char* __CLASS__;
	static long	fBusinessEventGroup;

};

typedef sophis::portfolio::CSRCustomReportingCallback<SxDataDouble, CsxBusinessEventReportingColumnComputer> CsxBusinessEventReportingColumnCallback;




#endif //!__ReportingCallBack_H__