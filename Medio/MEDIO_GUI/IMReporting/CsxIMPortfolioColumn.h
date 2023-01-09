#ifndef __CsxIMPortfolioColumn_H__
	#define __CsxIMPortfolioColumn_H__

/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphPortfolioColumnCache.h"
#include "SphInc/portfolio/SphExtraction.h"

//We should not have to use a 'CSRCachedPortfolioColumn', the reporting callback is the cache object.
//But it is bugged
class CsxIMPortfolioColumn : public sophis::portfolio::CSRCachedPortfolioColumn
{
//------------------------------------ PUBLIC ---------------------------------
public:

	// this method should be overriden
	virtual	void ComputePortfolioCell(const SSCellKey& key, 
		SSCellValue			*value, 
		SSCellStyle			*style
	) const override;
		
	// this method should be overriden
	virtual	void ComputeUnderlyingCell(const SSCellKey& key, 
		SSCellValue			*value, 
		SSCellStyle			*style
	) const override;
			
	// this method should be overriden
	virtual	void ComputePositionCell(const SSCellKey& key,
		SSCellValue			*value, 
		SSCellStyle			*style
	) const override;

	/**
	 * Default 60
	 */
	virtual short GetDefaultWidth() const override;

//------------------------------------ PRIVATE --------------------------------
private:
	void AggregateFolio(double& aggregateValue, const CSRPortfolio* folio, PSRExtraction extraction) const;
	DECLARATION_PORTFOLIO_COLUMN(CsxIMPortfolioColumn)

};

#endif //!__CsxIMPortfolioColumn_H__