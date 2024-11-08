#pragma once
#ifndef __CSxPositionVisibilityHook_H__
#define __CSxPositionVisibilityHook_H__

/*
** Includes
*/
#include "SphLLInc\portfolio\SphPositionVisibility.h"


using namespace sophis;
using namespace portfolio;
using namespace std;


class CSxPositionVisibilityHook : public CSRPositionVisibilityHook
{
public:
	static bool _seeExpiredFXUserRight;
	static bool _seeHedgeUserRight;
	static set<string> _HedgeSet;

	
	CSxPositionVisibilityHook() {};
	~CSxPositionVisibilityHook() {};

protected:
	virtual bool GetPositionVisibilityHook( const TViewMvts * position,
													const TViewFolio * folio,
													const sophis::portfolio::PSRExtraction& extraction,
													long activePortfolioCode,
													ListeEtat viewType,
													etat_ligne filter,
													bool & visible) const;

	// For log purpose
	static const char* __CLASS__;
	
};

static CSxPositionVisibilityHook gTKTPosVisibilityHook;


#endif //__CSxPositionVisibilityHook_H__
