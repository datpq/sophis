#include "SphInc/gui/SphMenu.h"
#include "SphInc/backoffice_kernel/SphThirdPartyEnums.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphInc/backoffice_kernel/SphThirdParty.h"

#include "CSxCustomMenu.h"
#include "Constants.h"


using namespace sophis::backoffice_kernel;
using namespace _STL;
using namespace sophis::sql;
using namespace sophis::instrument;


///////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxCustomMenuStringQuery -------------------------*/

CSxCustomMenuStringQuery::CSxCustomMenuStringQuery(sophis::gui::CSREditList *list, int CNb_Menu, string sqlQuery)
:CSRCustomMenu(list, CNb_Menu, true, NSREnums::dNullTerminatedString, NAME_SIZE+1),
fData(NULL)
{
	struct SSResult
	{				
		char name[NAME_SIZE+1];
		long code;
	};

	SSResult *resultBuffer = NULL;
	int			   nbResults = 0;

	CSRStructureDescriptor	desc(2, sizeof(SSResult));

	ADD(&desc, SSResult, name, rdfString);
	ADD(&desc, SSResult, code, rdfInteger);

	//DPH
	//CSRSqlQuery::QueryWithNResults(sqlQuery.c_str(), &desc, (void **) &resultBuffer, &nbResults);
	CSRSqlQuery::QueryWithNResultsWithoutParam(sqlQuery.c_str(), &desc, (void **)&resultBuffer, &nbResults);

	for (int i = 0; i < nbResults ; i++)
	{				
		fData.push_back(string(resultBuffer[i].name));

		_STL::map<string, long>::const_iterator iter = fStr2CodeMap.find(string(resultBuffer[i].name));
		if (iter == fStr2CodeMap.end())
			fStr2CodeMap[string(resultBuffer[i].name)] = resultBuffer[i].code;

		_STL::map<long, string>::const_iterator iter2 = fCode2StrMap.find(resultBuffer[i].code);
		if (iter2 == fCode2StrMap.end())
			fCode2StrMap[resultBuffer[i].code] = resultBuffer[i].name;
	}	

	fDecalageInitial = 0 ;
	BuildMenu() ;
	SetValueFromList();

	delete [] resultBuffer;
}

CSxCustomMenuStringQuery::~CSxCustomMenuStringQuery()
{
}

void CSxCustomMenuStringQuery::BuildMenu()
{
	for (size_t i = 0; i < fData.size(); i++ )
		this->AddElement(fData[i].c_str()) ;
}

void CSxCustomMenuStringQuery::SetValueFromList()
{
	if(fData.size() != 0 && (short)fData.size() >= fListValue)
		strcpy_s((char*)fValue, 256, fData[fListValue - 1].c_str());
	else
		strcpy_s((char*)fValue, 256, "");
}

void CSxCustomMenuStringQuery::SetListFromValue()
{
	for (size_t i = 0; i < fData.size(); i++)
	{
		if (fData[i] == (char*)fValue)
		{
			fListValue = (short)i + 1;
			return;
		}
	}
	fListValue = 0;
}

long CSxCustomMenuStringQuery::GetCode(_STL::string str)
{
	_STL::map<string, long>::const_iterator iter = fStr2CodeMap.find(str);
	if (iter != fStr2CodeMap.end())
		return fStr2CodeMap[str];
	else
		return 0;
}

string CSxCustomMenuStringQuery::GetVal(long code)
{
	_STL::map<long, string>::const_iterator iter = fCode2StrMap.find(code);
	if (iter != fCode2StrMap.end())
		return fCode2StrMap[code];
	else
		return "";
}


///////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxThirdPartyMenu -------------------------*/

CSxThirdPartyMenu::CSxThirdPartyMenu(sophis::gui::CSREditList *list, int CNb_Menu):
CSxCustomMenuStringQuery(list,CNb_Menu,"select NAME, IDENT from TIERS order by NAME")
{
	Disable();
}

///////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxInternalFundMenu -------------------------*/

CSxInternalFundMenu::CSxInternalFundMenu(sophis::gui::CSREditList *list, int CNb_Menu):
CSxCustomMenuStringQuery(list,CNb_Menu,"select LIBELLE, SICOVAM from TITRES where type = 'Z' and TYPEPRO = 1 order by LIBELLE")
{
	Disable();
}


///////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxCustomMenuLong -------------------------*/

CSxCustomMenuLong::CSxCustomMenuLong(sophis::gui::CSREditList *list, int CNb_Menu, bool editable,
									 NSREnums::eDataType	valueType,
									 unsigned int valueSize, short listValue, const char *columnName):
CSRCustomMenu(list,CNb_Menu,editable, valueType, valueSize,listValue,columnName)
{					
}

CSxCustomMenuLong::~CSxCustomMenuLong()
{
}

void CSxCustomMenuLong::BuildMenu()
{	
}

void CSxCustomMenuLong::SetValueFromList()
{
	if(fData.size() != 0 && (short)fData.size() >= fListValue)
		*((long*)fValue) = fData[fListValue - 1];
	else
		*((long*)fValue) = 0;
}

void CSxCustomMenuLong::SetListFromValue()
{
	for (size_t i = 0; i < fData.size(); i++)
	{
		if (fData[i] == *((long*)fValue))
		{
			fListValue = (short)i + 1;
			return;
		}
	}
	fListValue = 0;
}

///////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxThirdTypeMenu -------------------------*/

CSxThirdTypeMenu::CSxThirdTypeMenu(sophis::gui::CSREditList *list, int CNb_Menu, const char *columnName)
:CSxCustomMenuLong(list,CNb_Menu,true, NSREnums::dLong, 0,1,columnName)
{

	fData.push_back(eFundPromoter);
	fData.push_back(eApporteurAffaire);	

	fDecalageInitial = 0;
	BuildMenu();
	SetValueFromList();	
}

void CSxThirdTypeMenu::BuildMenu()
{
	for (size_t i = 0; i < fData.size(); i++ )
	{		
		_STL::string typeName = GetThirdTypeName(fData[i]);		

		this->AddElement(typeName.c_str());				
	}
}

_STL::string CSxThirdTypeMenu::GetThirdTypeName(long thirdType)
{
	_STL::string ret;

	switch(thirdType)
	{
	case eFundPromoter:
		ret = "Promoteur de fonds";
		break;
	case eApporteurAffaire:
		ret = "Apporteur d'affaires";
		break;		
	default:
		ret = "error";
		break;
	}

	return ret;
}

////////////////////////////////////////////////////////////////////////////////
/*-------------------------- CSxComputationMethodMenu -------------------------*/

CSxComputationMethodMenu::CSxComputationMethodMenu(sophis::gui::CSREditList *list, int CNb_Menu, const char *columnName):
CSxCustomMenuLong(list,CNb_Menu,true, NSREnums::dLong, 0,1,columnName)
{				
	fData.push_back(eProrata);
	fData.push_back(eAverageAM);
	fData.push_back(eAverageCGR);
	fData.push_back(eNoComputationMethod);

	fDecalageInitial = 0;
	BuildMenu();
	SetValueFromList();
}

void CSxComputationMethodMenu::BuildMenu()
{
	for (size_t i = 0; i < fData.size(); i++ )
	{		
		char buffer[NAME_SIZE];
		switch(fData[i])
		{
		case eProrata:
			sprintf_s(buffer,NAME_SIZE,"Prorata");
			break;
		case eAverageAM:
			sprintf_s(buffer,NAME_SIZE,"Moyenne pondérée AM");
			break;
		case eAverageCGR:
			sprintf_s(buffer,NAME_SIZE,"Moyenne pondérée CGR");
			break;
		case eNoComputationMethod:
			sprintf_s(buffer,NAME_SIZE,"XXX");
			break;
		default:
			sprintf_s(buffer,NAME_SIZE,"error");
			break;
		}

		this->AddElement(buffer);				
	}
}
