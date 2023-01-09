#ifndef __CSxMarketValueAggregateColumn_H__
	#define __CSxMarketValueAggregateColumn_H__

/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphPortfolioColumn.h"
#include "SphInc/portfolio/SphExtraction.h"

/*
** Interface for handling columns in portfolio dialogs.
To add a column, derive this class, using the macro DECLARATION_PORTFOLIO_COLUMN in your header
and INITIALISE_PORTFOLIO_COLUMN in UNIVERSAL_MAIN.
It is better that it compiles in the API part of your code as it is used by SphLimits
to define generic indicators for the limit servers.
Moreover, the main methods giving the values to display must have no MFC code;
For instance, you cannot show a dialog using CSRFitDialog::Message.
In this case, unwanted MFC messages such as "Une op�ration non support� a �t� tent�" can appear
or the window display can be altered.
You must also take care of using the class {@link CSRSqlQuery} method to write some
queries. In the verbose mode, in case of errors, a dialog appears. It is better to disable
the verbose mode using {@link CSRSqlQuery::SetVerbose} and to deal with the
error code yourself.
All virtual portfolio IDs referred to in the documentation of the class refer to
the ID given by the extraction. If the extraction is the main one, it is the portfolio
(in the table FOLIO), but generally it has to be interpreted by the key of the
extraction.
@see CSRExtractionKey
@see CSRExtraction
*/
class CSxMarketValueAggregateColumn : public sophis::portfolio::CSRPortfolioColumn
{
//------------------------------------ PUBLIC ---------------------------------
public:

	/// Get a portfolio cell (value and format if wanted) from a portfolio
	virtual	void			GetPortfolioCell(	long				activePortfolioCode,
												long				portfolioCode,
												PSRExtraction		extraction,
												SSCellValue			*cellValue,
												SSCellStyle			*cellStyle,
												bool				onlyTheValue) const ;
	/// Get a portfolio cell (value and format if wanted) from an underlying
	virtual	void			GetUnderlyingCell(	long				activePortfolioCode,
												long				portfolioCode,
												PSRExtraction		extraction,
												long				underlyingCode,
												SSCellValue			*cellValue,
												SSCellStyle			*cellStyle,
												bool				onlyTheValue) const ;
	/// Get a portfolio cell (value and format if wanted) from a position
	virtual	void			GetPositionCell(	const CSRPosition&	position,
												long				activePortfolioCode,
												long				portfolioCode,
												PSRExtraction		extraction,
												long				underlyingCode,
												long				instrumentCode,
												SSCellValue			*cellValue,
												SSCellStyle			*cellStyle,
												bool				onlyTheValue) const ;

//------------------------------------ PRIVATE --------------------------------
private:

	DECLARATION_PORTFOLIO_COLUMN(CSxMarketValueAggregateColumn)

};

#endif //!__CSxMarketValueAggregateColumn_H__