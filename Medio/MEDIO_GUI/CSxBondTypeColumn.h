#ifndef __CSxBondTypeColumn_H__
	#define __CSxBondTypeColumn_H__

#include "Sphinc\portfolio\SphPortfolioColumn.h"


class CSxBondTypeColumn : public CSRPortfolioColumn
{

public:

	static sophis::portfolio::CSRPortfolioColumn* NewDeriveSophis() { return new CSxBondTypeColumn; }

	CSxBondTypeColumn();

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
	
	virtual	void			GetUnderlyingCell(	long				activePortfolioCode,
							long				portfolioCode,
							PSRExtraction		extraction,
							long				underlyingCode,
							SSCellValue			*cellValue,
							SSCellStyle			*cellStyle,
							bool				onlyTheValue) const;

	static void refreshCache(void);

// static _STL::map<long,_STL::string> instrument_Type;

//------------------------------------ PRIVATE --------------------------------
private:

	static const char* __CLASS__;
};

#endif //!__CSxBondTypeColumn_H__