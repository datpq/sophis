#ifndef __CFG_YieldCurveData_H__
#define __CFG_YieldCurveData_H__

#include "SphInc/market_data/SphYieldCurve.h"
#include "SphInc/gui/SphCustomMenu.h"

class CFG_YieldCurveData : public sophis::market_data::CSRInfoSup
{
//------------------------------------ PUBLIC ---------------------------------
public:
	CFG_YieldCurveData();
	virtual ~CFG_YieldCurveData() {};

	virtual CSRInfoSup*			Clone() const;

	virtual int					GetElementCount() const;
	virtual gui::CSRElement*	new_CSRElement(int i, long currency, sophis::gui::CSREditList *list, int CNb_Column, bool isYieldCurve = true);

	virtual const char*			GetColumnName(int i) const;
	virtual const char*			GetDBTableName() const;
	virtual const char*			GetDBColumnName(int i) const;

	virtual void				SetValue(int i, sophis::gui::CSRElement* element) const;
	virtual void				GetValue(int i, const sophis::gui::CSRElement* element);

	virtual void				SaveToDB(long curveId, long pointId); 
	virtual void				LoadFromDB(long curveId, long pointId);

	virtual sophis::tools::CSRArchive&			GetInfoSupData(sophis::tools::CSRArchive & ar) const;
	virtual const sophis::tools::CSRArchive&	SetInfoSupData(const sophis::tools::CSRArchive & ar);

	virtual void BeforeRateCalibration(SSYieldPoint& pt);
	virtual void AfterRateCalibration(SSYieldPoint& pt);

	long fValueDate;

	//DPH
	virtual void AddCSROut(int i, sophis::DAL::YieldCurvePoints::DBPoint* PointBuffer, sophis::sql::CSRStatement& Statement) const OVERRIDE;
	virtual void GetValue(const sophis::DAL::YieldCurvePoints::DBPoint* DBPoint) OVERRIDE;
	virtual long GetDBStructSize() const OVERRIDE;
};

class CSREnableRatePopUp : public sophis::gui::CSRCustomMenu
{
public:
	CSREnableRatePopUp(sophis::gui::CSREditList* listBase, int ident);
	void Reset();
};


#endif //!__CFG_YieldCurveData_H__