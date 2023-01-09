#ifndef __CSxPortfolioColumnRBCNAV_H__
	#define __CSxPortfolioColumnRBCNAV_H__

/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphPortfolioColumnCache.h"
#include "SphInc/portfolio/SphExtraction.h"

class CSxPortfolioColumnRBCNAV : public sophis::portfolio::CSRCachedPortfolioColumn
{
//------------------------------------ PUBLIC ---------------------------------
public:
	virtual	void ComputePortfolioCell(const SSCellKey& key,
		SSCellValue			*value,
		SSCellStyle			*style
	) const override;

	virtual	void ComputeUnderlyingCell(const SSCellKey& key,
		SSCellValue			*value,
		SSCellStyle			*style
	) const override;

	virtual	void ComputePositionCell(const SSCellKey& key,
		SSCellValue			*value,
		SSCellStyle			*style
	) const override;

	/**
	 * Default 60
	 */
	virtual short GetDefaultWidth() const;

//------------------------------------ PRIVATE --------------------------------
private:

	DECLARATION_PORTFOLIO_COLUMN(CSxPortfolioColumnRBCNAV)
	static const char* __CLASS__;
	static const long GetStrategyOfPosition(const CSRPosition& position, PSRExtraction extraction);

};

#endif //!__CSxPortfolioColumnRBCNAV_H__