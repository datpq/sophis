#include "CSxThirdPartyActionCountryCode.h"
#include "SphInc/backoffice_kernel/SphThirdParty.h"
#include "SphTools/SphLoggerUtil.h"
#include "SphInc/gui/SphEditList.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphSDBCInc/queries/SphQuery.h"

using namespace sophis::backoffice_kernel;
using namespace sophis::gui;

const char* CSxThirdPartyActionCountryCode::__CLASS__ = "CSxThirdPartyActionCountryCode";

CSxThirdPartyActionCountryCode::CSxThirdPartyActionCountryCode(void)
{
	BEGIN_LOG("CSxThirdPartyActionCountryCode");
	MESS(Log::info, "Begin");
	if (CSxThirdPartyActionCountryCode::ALL_COUNTRY_CODES.empty()) {
		//21.3
		CSRQuery query;
		char isocode[4] = { '\0' };
		query.SetName("GetIsoCodeDTCC_REFERENCES_ALERT_CODES");
		query << "SELECT DISTINCT " << CSROut::FromStr("ISO_CODE", isocode) << " FROM DTCC_REFERENCES_ALERT_CODES";
		while (query.Fetch())
		{
			std::string str(isocode);
			ALL_COUNTRY_CODES.insert(str);
		}

		//7.1.3
		//struct SCountryCode
		//{
		//	char isocode[3];
		//};
		//CSRStructureDescriptor * gabarit = new CSRStructureDescriptor(1, sizeof(SCountryCode));
		//ADD(gabarit, SCountryCode, isocode, rdfString);
		//SCountryCode * arrItems;
		//int count = 0;
		//errorCode err = CSRSqlQuery::QueryWithNResults("SELECT DISTINCT ISO_CODE FROM DTCC_REFERENCES_ALERT_CODES",
		//	gabarit, (void **)&arrItems, &count);
		//for(int i=0; i<count; i++) {
		//	std::string str(arrItems[i].isocode);
		//	ALL_COUNTRY_CODES.insert(str);
		//}
		//delete gabarit;
		MESS(Log::info, "ALL_COUNTRY_CODES initialized. size = " << ALL_COUNTRY_CODES.size());
	}
	MESS(Log::info, "End");
	END_LOG();
}


CSxThirdPartyActionCountryCode::~CSxThirdPartyActionCountryCode(void)
{
}

void CSxThirdPartyActionCountryCode::VoteForCreation(sophis::backoffice_kernel::CSRThirdPartyDlg &tparty)
	throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForCreation");
	MESS(Log::debug, "Begin");
	checkCountryCode(tparty);
	MESS(Log::debug, "End");
	END_LOG();
}

void CSxThirdPartyActionCountryCode::VoteForModification(sophis::backoffice_kernel::CSRThirdPartyDlg &tparty)
	throw (sophis::tools::VoteException)
{
	BEGIN_LOG("VoteForModification");
	MESS(Log::debug, "Begin");
	checkCountryCode(tparty);
	MESS(Log::debug, "End");
	END_LOG();
}

void CSxThirdPartyActionCountryCode::checkCountryCode(sophis::backoffice_kernel::CSRThirdPartyDlg &tparty)
	throw (sophis::tools::VoteException)
{
	BEGIN_LOG("checkCountryCode");
	MESS(Log::debug, "Begin(GetIdent=" << tparty.GetIdent() << ", GetName=" << tparty.GetName() << ")");
	const CSRThirdParty* ctpy = CSRThirdParty::GetCSRThirdParty(tparty.GetIdent());
	CSREditList * SSIs = tparty.GetSettlementInstructionList();
	if (SSIs->GetLineCount() > 0) {
		int countryCodeColIdx = SSIs->GetColumnPosition("Country Code");
		MESS(Log::debug, "SSI GetLineCount=" << SSIs->GetLineCount() << ", countryCodeColIdx=" << countryCodeColIdx);
		for(int i=0; i<SSIs->GetLineCount(); i++) {
			char countryCode[10] = {'\0'};
			SSIs->LoadElement(i, countryCodeColIdx, countryCode);
			MESS(Log::debug, "SSI countryCode=" << countryCode);
			if (strlen(countryCode) != 0 && ALL_COUNTRY_CODES.count(std::string(countryCode)) == 0)
			{
				std::stringstream ss; ss << "Country Code " << countryCode << " is not valid. Valid values are defined in DTCC_REFERENCES_ALERT_CODES.";
				throw VoteException(ss.str().c_str());
			}
		}
	}
	//SSThirdPartySettlementIter ssiIt, ssiEnd;
	//ctpy->GetSettlementInstructionList(ssiIt, ssiEnd);
	//while(ssiIt != ssiEnd) {
	//	MESS(Log::debug, "SSI code=" << ssiIt->second->code << ", fCountryCode=" << ssiIt->second->fCountryCode);
	//	//if (std::find(ALL_COUNTRY_CODES.begin(), ALL_COUNTRY_CODES.end(), ssiIt->second->fCountryCode) != ALL_COUNTRY_CODES.end())
	//	if (ALL_COUNTRY_CODES.count(ssiIt->second->fCountryCode) == 0)
	//	{
	//		std::stringstream ss; ss << "Country Code " << ssiIt->second->fCountryCode << " is not defined";
	//		throw VoteException(ss.str().c_str());
	//	}
	//	ssiIt++;
	//}
	MESS(Log::debug, "End");
	END_LOG();
}
