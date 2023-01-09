#ifndef __CSxPortfolioColumnRBCWeight_H__
	#define __CSxPortfolioColumnRBCWeight_H__

/*
** Includes
*/
// standard
#include "SphInc/portfolio/SphPortfolioColumnCache.h"
#include "SphInc/portfolio/SphExtraction.h"

class CSxPortfolioColumnRBCWeight : public sophis::portfolio::CSRCachedPortfolioColumn
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

	DECLARATION_PORTFOLIO_COLUMN(CSxPortfolioColumnRBCWeight)
	static const char* __CLASS__;
};

#endif //!__CSxPortfolioColumnRBCWeight_H__