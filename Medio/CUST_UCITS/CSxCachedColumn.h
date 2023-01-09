#pragma once

#include "SphInc/SphMacros.h"
#include "SphTools/SphCommon.h"

#include __STL_INCLUDE_PATH(set)
#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(tuple)


#include "SphInc/portfolio/SphPortfolioColumn.h"
#include "SphInc/portfolio/SphExtraction.h"
#include "Sphinc\value\benchmark\SphAMBenchmarkRetriever.h"
#include "CSxColumnHelper.h"

typedef /*sophis::portfolio::CSRExtraction**/long* TExctractionID;

struct SSxFolioLevelBuffer
{
	bool operator < (const SSxFolioLevelBuffer & b) const;

	long portfolioCode;
	TExctractionID extraction;

	double	value;
	long	decimal;
};

struct SSxPositionValue
{
	SSxPositionValue(){ value = 0; decimal = 0; };
	SSxPositionValue(double	_value, long _decimal){ value = _value; decimal = _decimal; };
	double	value;
	long	decimal;
};

/**
*struct and typefed for position caching
*/
typedef _STL::map<long, SSxPositionValue> TPositionValueMap;
typedef _STL::map<long, TPositionValueMap> TFolioValueMap;
typedef _STL::map<TExctractionID, TFolioValueMap> TExtractionValueMap;
// iterator
typedef _STL::map<long, SSxPositionValue>::const_iterator TPositionIt;
typedef _STL::map<long, TPositionValueMap>::const_iterator TFolioIt;
typedef _STL::map<TExctractionID, TFolioValueMap>::const_iterator TExtractionIt;

/**
* struct and typedef to identify lines on a given sicovam. NB: needed to get smooth recompute in a Dynamic Order Builder session ...
*/
//typedef _STL::tuple<TExctractionID, long, long> TPositionFullPath;//extraction ID, folio ID, position ID
//typedef _STL::set<TPositionFullPath> TPositionSet;
//typedef _STL::map<long, TPositionSet> TPositionSetPerSicovam;


class CSxCachedColumn : public sophis::portfolio::CSRCachedPortfolioColumn
{
	//------------------------------------ PUBLIC (overload)--------------------------------
public:

	CSxCachedColumn(void);

	/// Get a portfolio cell (value and format if wanted) from a portfolio. Look in cache first, else call ComputePortfolioCell
	virtual	void			GetPortfolioCell(long				activePortfolioCode,
		long				portfolioCode,
		PSRExtraction		extraction,
		SSCellValue			*cellValue,
		SSCellStyle			*cellStyle,
		bool				onlyTheValue) const;

	virtual void GetPositionCell(const CSRPosition&	position,
		long				activePortfolioCode,
		long				portfolioCode,
		PSRExtraction		extraction,
		long				underlyingCode,
		long				instrumentCode,
		SSCellValue			*cellValue,
		SSCellStyle			*cellStyle,
		bool				onlyTheValue) const;


	//------------------------------------ PROTECTED (asbtract)------------------------------
protected:
	virtual	void			ComputePortfolioCell(long				  activePortfolioCode,
		long				  portfolioCode,
		PSRExtraction		  extraction,
		SSCellValue			  *cellValue,
		SSCellStyle			  *cellStyle,
		bool				  onlyTheValue) const;

	virtual	void			ComputePositionCell(const CSRPosition&	position,
		long				activePortfolioCode,
		long				portfolioCode,
		PSRExtraction		extraction,
		long				underlyingCode,
		long				instrumentCode,
		SSCellValue			*cellValue,
		SSCellStyle			*cellStyle,
		bool				onlyTheValue) const;

	// ------------------------------------PROTECTED(Consolidate Under)-------------------- -
		void			ConsolidateUnder(bool indicatorInCurrency,//if true, applies FX to the portfolioCode currency. Else basic sum
		long				  activePortfolioCode,
		long				  portfolioCode,
		PSRExtraction		  extraction,
		SSCellValue			  *cellValue,
		SSCellStyle			  *cellStyle,
		bool				  onlyTheValue) const;

		virtual void SetPositionStyle(SSCellStyle			*cellStyle, long currencyCode)const{ return CSxColumnHelper::SetStyleForDouble(*cellStyle, currencyCode, true, 0, false); };
		virtual void SetPortfolioStyle(SSCellStyle			*cellStyle, long currencyCode)const{ return CSxColumnHelper::SetStyleForDouble(*cellStyle, currencyCode, true, 0, true); };

		void FlushAll(void)const;
		//------------------------------------ PROTECTED (code factorized - folio level caching) ---------------
protected:

	mutable int fBufferConsolidationVersion;
	mutable int fBufferCalculationVersion;
	mutable _STL::set<SSxFolioLevelBuffer> fFolioBuffer;
	//------------------------------------ PROTECTED (code factorized - position level caching) ---------------
	virtual bool IsPositionCacheEnabled()const{ return true; };
	virtual bool FlushOnlyAfterComputation()const{ return true; };// if not, PortfolioColumnRefreshVersion only

	mutable TExtractionValueMap fExtractionValueMap;
	//mutable TPositionSetPerSicovam fPositionSetPerSicovam;
private:
	static const char* __CLASS__;

};



