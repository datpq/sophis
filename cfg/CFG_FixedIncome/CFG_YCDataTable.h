#pragma once
#include "SphInc/gui/SphEditList.h"


struct ycPointsInfos
{
	unsigned long long ident;
	unsigned long long courbe;
	long maturityDate;
	double rate;
	long valueDate;
};

class CFG_YCDataTable : public CSREditList
{
public:
	CFG_YCDataTable(sophis::gui::CSRFitDialog* dialog, int component);
	void LoadList(long curveId);
	void CommandSave();	
	virtual ~CFG_YCDataTable();
	static std::map<long, ycPointsInfos> LoadCFGYieldPoints(long curveId);
	static int fCurveId;
	static std::map<long,ycPointsInfos> ycPointsMap;
	enum
	{	
		eDate,
		eRate,
		eValueDate,
		eColumnCount
	};
private:
	static const char* __CLASS__;

	//public:
};
