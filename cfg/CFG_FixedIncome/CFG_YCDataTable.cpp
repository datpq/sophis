#include "CFG_YCDataTable.h"

#pragma warning (disable:4351)
#include "SphInc/GUI/SphCode.h"
#include "SphInc/GUI/sphDialog.h"
#include "SphInc/Portfolio/sphTransaction.h"
#include "SphSDBCInc/queries/SphQuery.h"
#include "SphInc/GUI/SphEditElement.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphSDBCInc/queries/SphQueries.h"
#include "SphSDBCInc/queries/SphInserter.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphSDBCInc\queries\SphQueryBuffered.h"
#include "SphSDBCInc\connection\SphConnection.h"
#include "SphSDBCInc/tools/SphWrapperForMap.h"
#include "sphinc/gui/SphEditElement.h"


using namespace _STL;
using namespace sophisTools;
using namespace sophis;
using namespace portfolio;
using namespace gui;
using namespace sophis::misc;


using namespace sophis::gui;
using namespace sophis::static_data;
using namespace sophis::portfolio;
using namespace sophisTools::logger;
using namespace sophisTools;
using namespace sophis::misc;


const char* CFG_YCDataTable::__CLASS__ = "CFG_YCDataTable";
int CFG_YCDataTable::fCurveId = 0;

std::map<long, ycPointsInfos> CFG_YCDataTable::ycPointsMap;


CFG_YCDataTable::CFG_YCDataTable(CSRFitDialog* dialog, int component)
	: CSREditList(dialog, component)
{

	fListeValeur = NULL;
	SetMaxSelection(-1);
	fColumnCount = eColumnCount;
	fColumns = new SSColumn[fColumnCount];

	fColumns[eDate].fColumnName = "Date";
	fColumns[eDate].fElement = new CSRStaticDate(this, eDate, 0);
	fColumns[eDate].fColumnWidth = 170;
	fColumns[eDate].fAlignmentType = aLeft;


	fColumns[eRate].fColumnName = "Rate";
	fColumns[eRate].fElement = new CSRStaticDouble(this, eRate, 4, 0.0, 999999999999, 0);
	fColumns[eRate].fColumnWidth = 170;
	fColumns[eRate].fAlignmentType = aLeft;

	fColumns[eValueDate].fColumnName = "Value Date";
	fColumns[eValueDate].fElement = new CSREditDate(this, eValueDate, 0);
	fColumns[eValueDate].fColumnWidth = 170;
	fColumns[eValueDate].fAlignmentType = aLeft;

	SetDynamicSize(true);
	SetLineCount(0);

}

void CFG_YCDataTable::LoadList(long curveId)
{
	BEGIN_LOG("LoadList");
	try
	{
		ycPointsMap.clear();
		ycPointsMap = LoadCFGYieldPoints(fCurveId);
		size_t nbLines = ycPointsMap.size();

		SetLineCount((int)nbLines);
		int i = 0;
		for each(auto item in ycPointsMap)
		{

			CSRElement* pElementMatDate = nullptr;
			CSRElement* pElementValDate = nullptr;
			CSRElement* pElementRate = nullptr;
			LoadLine(i);

			pElementMatDate = GetElementByIndex(eDate);
			pElementValDate = GetElementByIndex(eValueDate);
			pElementRate = GetElementByIndex(eRate);
			if (pElementMatDate != nullptr && pElementValDate != nullptr && pElementRate != nullptr)
			{
				pElementMatDate->SetValue(&(item.first));
				pElementValDate->SetValue(&(item.second.valueDate));
				pElementRate->SetValue(&(item.second.rate));

			}
			SaveLine(i);
			i++;
		}

		Update();

	}
	catch (const CSROracleException& ex)
	{
		MESS(Log::error, FROM_STREAM("Oracle exception occured while trying to load yc points: " << (const char *)ex));
	}
	catch (const CSRDatabaseException& ex)
	{
		MESS(Log::error, FROM_STREAM("Database exception occured while trying to load yc points: " << (const char *)ex));
	}
	catch (const ExceptionBase& ex)
	{
		MESS(Log::error, FROM_STREAM("Exception occured while trying to load yc points: " << (const char *)ex));
	}
	catch (...)
	{
		MESS(Log::error, "Unknown error occured while trying to load yc points");
	}

	END_LOG();
}

void CFG_YCDataTable::CommandSave()
{
	BEGIN_LOG("CommandSave");

	size_t nbLines = GetLineCount();
	vector<ycPointsInfos> ycInfosVect;
	ycPointsInfos data;

	for (size_t i = 0; i < nbLines; i++)
	{

		CSRElement* pElementMatDate = nullptr;
		CSRElement* pElementValDate = nullptr;
		CSRElement* pElementRate = nullptr;
		LoadLine(i);

		pElementMatDate = GetElementByIndex(eDate);
		pElementValDate = GetElementByIndex(eValueDate);
		pElementRate = GetElementByIndex(eRate);
		if (pElementMatDate != nullptr && pElementValDate != nullptr  && pElementRate != nullptr)
		{
			pElementMatDate->GetValue(&data.maturityDate);
			pElementValDate->GetValue(&data.valueDate);
			pElementRate->GetValue(&data.rate);

			data.ident = ycPointsMap[data.maturityDate].ident;
			data.courbe = ycPointsMap[data.maturityDate].courbe;

			ycInfosVect.push_back(data);
		}
	}

	try
	{
		CSRQuery queryYCPoints;

		queryYCPoints << "delete from CFG_YC_INFOS WHERE COURBE= (select ident from GR_INFOSCOURBE where graphe=(select GRAPHE from courbetaux where CODE=" << CSRIn(fCurveId) << "))";
		queryYCPoints.Execute();

		CSRInserter<ycPointsInfos> inserter;
		inserter << "INSERT INTO CFG_YC_INFOS (IDENT,COURBE,MATURITY_DATE,RATE,VALUE_DATE) "
			<< " VALUES ("
			<< InOffset(&ycPointsInfos::ident) << ", "
			<< InOffset(&ycPointsInfos::courbe) << ", "
			<< InOffset(&ycPointsInfos::maturityDate) << ", "
			<< InOffset(&ycPointsInfos::rate) << ", "
			<< InOffset(&ycPointsInfos::valueDate) << ") ";

		inserter.Insert(ycInfosVect);

		if (!inserter.GetConnection().Commit())
		{
			MESS(Log::warning, "Commit on CSRInserter Connection failed!");
		}
	}
	catch (sophisTools::base::ExceptionBase & ex)
	{
		MESS(Log::error, "Error while inserting yc points in custom table; ex=(" << ex << ")");
		throw;
	}

	END_LOG();
}


std::map<long, ycPointsInfos> CFG_YCDataTable::LoadCFGYieldPoints(long curveId)
{
	std::map<long, ycPointsInfos> ycPointsMap;

	using WrapperForMap = CSRWrapperForMap<std::map<long, ycPointsInfos>>;
	CSRQueryBuffered<WrapperForMap> query;
	WrapperForMap ycPointsWrapper;

	query << "SELECT "
		<< OutOffset("MATURITY_DATE", ycPointsWrapper, ycPointsWrapper.first)
		<< OutOffset("IDENT", ycPointsWrapper, ycPointsWrapper.second.ident)
		<< OutOffset("COURBE", ycPointsWrapper, ycPointsWrapper.second.courbe)
		<< OutOffset("RATE", ycPointsWrapper, ycPointsWrapper.second.rate)
		<< OutOffset("VALUE_DATE", ycPointsWrapper, ycPointsWrapper.second.valueDate)
		<< " from CFG_YC_INFOS WHERE COURBE= (select ident from GR_INFOSCOURBE where graphe=(select GRAPHE from courbetaux where CODE=" << CSRIn(curveId) << "))"
		<< "ORDER BY MATURITY_DATE ASC";

	ycPointsMap.clear();
	query.FetchAll(ycPointsMap);

	return ycPointsMap;
}
CFG_YCDataTable::~CFG_YCDataTable() {}



