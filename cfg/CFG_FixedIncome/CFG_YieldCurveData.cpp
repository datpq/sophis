#pragma warning(disable:4251)
#include "CFG_YieldCurveData.h"
#include "SphInc\inflation\SphInflationCustomMenu.h"
#include "SphInc\value\kernel\SphAMGlobalFunctions.h"
#include "SphTools\SphArchive.h"
#include "SphSDBCInc\SphSQLQuery.h"
#pragma warning(push)
#pragma warning(disable:4103) //  '...' : alignment changed after including header, may be due to missing #pragma pack(pop)
#include __STL_INCLUDE_PATH(stdio.h)
#pragma warning(pop)

CFG_YieldCurveData::CFG_YieldCurveData()
: CSRInfoSup()
{
	fValueDate = 0L;
	fIsUsed	  = true;
	fRateCode = 0L;
};

void CFG_YieldCurveData::SaveToDB(long courbeId, long pointId)
{
	char cSQL[512] = "";
	long isNotUsed = (long) (!fIsUsed);
	sprintf_s(cSQL, "insert into %s (IDENT, COURBE, %s, %s, %s) "
		"values (%ld, %ld, %ld, %ld, %ld)", 
		GetDBTableName(), GetDBColumnName(0), GetDBColumnName(1), GetDBColumnName(2),
		pointId, courbeId, fValueDate, isNotUsed, fRateCode);
	//DPH
	//if(!CSRSqlQuery::QueryWithoutResult(cSQL))
	if (!CSRSqlQuery::QueryWithoutResultAndParam(cSQL))
		CSRSqlQuery::Commit();
	else CSRSqlQuery::RollBack();
};

void CFG_YieldCurveData::LoadFromDB(long courbeId, long pointId)
{
	struct Var { long fIdent; long fCourbe; };
	CSRStructureDescriptor varDesc(2, sizeof(Var));
	ADD(&varDesc, Var, fIdent, rdfInteger);
	ADD(&varDesc, Var, fCourbe, rdfInteger);
	Var var = { pointId, courbeId };

	struct Res { long fValueDate; long fIsNotUsed; long fRateCode; };
	CSRStructureDescriptor resDesc(3, sizeof(Res));
	ADD(&resDesc, Res, fValueDate, rdfInteger);
	ADD(&resDesc, Res, fIsNotUsed, rdfInteger);
	ADD(&resDesc, Res, fRateCode, rdfInteger);
	Res res = { 0L, 0L, 0L };

	char cSQL[512] = "";
	sprintf_s(cSQL, "select %s, %s, %s from %s where IDENT = :IDENT and COURBE = :COURBE "
		, GetDBColumnName(0), GetDBColumnName(1), GetDBColumnName(2), GetDBTableName());

	CSRSqlQuery sqlQuery(cSQL, &varDesc, &resDesc, false, false);
	sqlQuery.QueryWith1Result(&var, &res);
	fValueDate = res.fValueDate;
	fIsUsed = (res.fIsNotUsed == 0);
	fRateCode = res.fRateCode;
};

CSRInfoSup*	CFG_YieldCurveData::Clone() const
{
	CFG_YieldCurveData* pClone = dynamic_cast<CFG_YieldCurveData*>(sophis::misc::gGlobalFunctions->new_YCRateInfoSup());
	if(NULL == pClone) return NULL;
	pClone->fValueDate = fValueDate;
	pClone->fIsUsed	 = fIsUsed;
	pClone->fRateCode = fRateCode;
	return pClone;
};

int CFG_YieldCurveData::GetElementCount() const
{
	return 3;
};

CSRElement* CFG_YieldCurveData::new_CSRElement(int i, long currency, CSREditList *list, int CNb_Column, bool isYieldCurve)
{
	switch (i)
	{
	case 0:
		//return new CSREditDate(list, CNb_Column);
		return new CSREditDate(list, CNb_Column, 0L, "VALUE DATE", false, false, false, false, "VALUE_DATE");
	case 1:
		return new CSREnableRatePopUp(list,CNb_Column);
	case 2:
		{
			CSRElement* ptr = NULL;
			if(isYieldCurve)
				ptr = new CSRSelectInterestRate(list, CNb_Column, currency);
			else
				ptr = new inflation::CSRInflationRateMenu(list, CNb_Column);
			ptr->SetPossibleToReset(true);
			return ptr;
		}
	}
	return NULL;
};

const char* CFG_YieldCurveData::GetColumnName(int i) const
{
	switch (i)
	{
	case 0:
		return "Value Date";
	case 1:
		return "Enable/Disable";
	case 2:
		return "Rate code";
	}
	return "";
};

const char* CFG_YieldCurveData::GetDBColumnName(int i) const
{
	switch (i)
	{
	case 0:
		return "VALUE_DATE";
	case 1:
		return "USE_POINT";
	case 2:
		return "RATE_CODE";
	default:
		return "";
	}
};

const char* CFG_YieldCurveData::GetDBTableName() const
{
	return "INFO_SUP";
};

void CFG_YieldCurveData::SetValue(int i, CSRElement* element) const
{
	switch (i)
	{
	case 0:
		element->SetValue(&fValueDate);
		break;
	case 1:
		{
			short isUsed = 1;
			if(!fIsUsed)
				isUsed = 2;
			element->SetValue(&isUsed);
		}
		break;
	case 2:
		element->SetValue(&fRateCode);
		break;
	default:
		return;
	}
};

void CFG_YieldCurveData::GetValue(int i,  const CSRElement* element)
{
	switch (i)
	{
	case 0:
		element->GetValue(&fValueDate);
		break;
	case 1:
		{
			short isUsed;
			element->GetValue(&isUsed);
			fIsUsed = (isUsed == 1 || isUsed == 0) ;
		}
		break;
	case 2:
		element->GetValue(&fRateCode);
		break;
	default:
		return;
	}
};

//DPH
struct CFGInfoSupDBPoint : public sophis::DAL::YieldCurvePoints::DBPoint
{
	long    ValueDate{};
	long	IsNotUsed{};
	long	RateCode{};
};

//DPH
long CFG_YieldCurveData::GetDBStructSize() const
{
	return sizeof(CFGInfoSupDBPoint);
}

//DPH
void CFG_YieldCurveData::AddCSROut(int i, sophis::DAL::YieldCurvePoints::DBPoint* PointBuffer, sophis::sql::CSRStatement& Statement) const
{
	CFGInfoSupDBPoint* ChildPointBuffer = static_cast<CFGInfoSupDBPoint*> (PointBuffer);
	switch (i)
	{
	case 0:
		Statement << CSROut(GetDBColumnName(0), ChildPointBuffer->ValueDate);
		break;
	case 1:
		Statement << CSROut(GetDBColumnName(1), ChildPointBuffer->IsNotUsed);
		break;
	case 2:
		Statement << CSROut(GetDBColumnName(2), ChildPointBuffer->RateCode);
		break;
	}
}

//DPH
void CFG_YieldCurveData::GetValue(const sophis::DAL::YieldCurvePoints::DBPoint* DBPoint)
{
	const CFGInfoSupDBPoint* pInfoSupDBPoint = static_cast<const CFGInfoSupDBPoint*> (DBPoint);
	fIsUsed = (pInfoSupDBPoint->IsNotUsed == 0);
	fRateCode = pInfoSupDBPoint->RateCode;
	fValueDate = pInfoSupDBPoint->ValueDate;
}

CSRArchive& CFG_YieldCurveData::GetInfoSupData(CSRArchive & ar) const
{
	ar	<< fValueDate
		<< fIsUsed
		<< fRateCode;
	return ar;
};

const CSRArchive& CFG_YieldCurveData::SetInfoSupData(const CSRArchive & ar)
{
	ar	>> fValueDate
		>> fIsUsed
		>> fRateCode;
	return ar;
};

void CFG_YieldCurveData::BeforeRateCalibration(SSYieldPoint& pt)
{

};

void CFG_YieldCurveData::AfterRateCalibration(SSYieldPoint& pt)
{

};




CSREnableRatePopUp::CSREnableRatePopUp(CSREditList* listBase, int ident)
: CSRCustomMenu(listBase, ident,true,NSREnums::dShort,0,1)
{
	AddElement("Enabled");
	//AddElement("Disabled");
};

void CSREnableRatePopUp::Reset()
{
	if(fKind == NSREnums::dShort)
		*(short*)fValue = 1;
};

