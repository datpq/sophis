#include "CSxRBCTradeIDCriteria.h"
#include "../MEDIO_GUI/GUI/CSxTransactionDlg.h"

const char* CSxRBCTradeIDCriteria::__CLASS__ = "CSxRBCTradeIDCriteria";

_STL::map<long,_STL::string> CSxRBCTradeIDCriteria::RBCTradeIDs;

//-------------------------------------------------------------------------
void CSxRBCTradeIDCriteria::GetName(long code, char* name, size_t size) const
{
	_STL::string rbcTradeId;
	try{
		rbcTradeId = RBCTradeIDs[code];
	}
	catch(...)
	{

	}
	if(!rbcTradeId.empty())
	{
		name = new char[rbcTradeId.size()+1];
		strcpy_s(name,size,rbcTradeId.c_str());
	}
}

//-------------------------------------------------------------------------
void CSxRBCTradeIDCriteria::GetCode(SSReportingTrade* mvt, TCodeList &list) const
{
	try{
		long tradeId = (long) mvt->refcon;
		const CSRTransaction * trade = new CSRTransaction(tradeId);
		if(trade!=NULL)
		{
			char buffer[256];
			trade->LoadUserElement();
			trade->LoadGeneralElement(CSxTransactionDlg::eRBCTradeId,buffer);
			_STL::string str = buffer;
			RBCTradeIDs[tradeId] = str;
			SSOneValue value;
			value.fCode = tradeId;
			list.insert(list.end(),value);
		}
	}
	catch (...)
	{

	}
}

