#include <process.h>
#include <iostream>
#include <fstream>
#include <list>

#include "SphSDBCInc/SphSQLQueryBase.h"
#include "SphSDBCInc/SphSQLQuery.h"
#include "SphTools/SphLoggerUtil.h"
//#include "../../Excel/IncludeExcel.h"
#include "SphLLInc/misc/ConfigurationFileWrapper.h"

//#include "TickerItem.h"
#include "CSxDBClient.h"

const char* CSxDBClient::__CLASS__="CSxDBClient";

using namespace sophis::misc;
using namespace sophis::sql;


CSxDBClient::CSxDBClient(void)/*:*/
	//m_bInitialise(false),
	//m_bIsQuitting(false),
	//m_bListeningThreadRunning(false)
{
	//fExcelOpen = false;
	//fColTicker = 7; // col G
	//fColPRice = 8; // col H
	//fDelay = 60;
	//fPath = "";
	//fFileName = "";
}

CSxDBClient::~CSxDBClient(void)
{
	/*CloseExcel();
	m_vecLPL.clear();*/
}

//bool CSxDBClient::Init(CLivePriceListener *pLPL)
//{
//	BEGIN_LOG("Init");
//	ConfigurationFileWrapper::getEntryValue("TKT", "delay", fDelay, 300); 
//	MESS(Log::info, FROM_STREAM("DB Client Init ... Value: "<< fDelay ));
//	if (fDelay < 300) 
//	{
//		fDelay = 300;
//	}
//
//	ConfigurationFileWrapper::getEntryValue("TKT", "table", fTableName);
//	MESS(Log::info, FROM_STREAM("DB Client Init ... Value: "<< fTableName));
//			
//	m_vecLPL.push_back(pLPL);
//	END_LOG();
//	return Init();
//}

bool CSxDBClient::Init()
{
	if (!m_bInitialise)
	{
		m_bInitialise = true;
	}
	return true;
}




bool CSxDBClient::Connect()
{
	BEGIN_LOG("Start");

	return true;
}

bool CSxDBClient::Disconnect()
{
	return true;
}

unsigned __stdcall ClientMessageThreadFunc( void* pArguments )
{
	// Turn the void* back into a pointer to the CBloomberg object
	//CSxDBClient	*pSC = (CSxDBClient*)pArguments;

	//// Pass control back into the main object
	//pSC->ListenHandler();
	return 0;
}

bool CSxDBClient::ListenHandler()
{	
	//BEGIN_LOG("ListenHandler");

	//CTickerItem	ti;

	//while(m_bIsQuitting == false)
	//{
	//	OpenFile();
	//	Sleep(1000 * fDelay);
	//	ReadAndSendPrices(fColTicker, fColPRice);
	//	ClosingWorkBook();
	//}

	//puts("Ending listener thread");
	//m_bListeningThreadRunning = false;
	//END_LOG();
	return true;
}

bool CSxDBClient::InitListenerThread()
{
	BEGIN_LOG("InitListenerThread");

	//if (!m_bListeningThreadRunning)
	//{
	//	// Start new thread
	//	unsigned threadID;
	//	_beginthreadex( NULL, 0, &ClientMessageThreadFunc, this, 0, &threadID );
	//	m_bListeningThreadRunning = true;
	//}
	END_LOG();
	return true;
}

bool CSxDBClient::Shutdown()
{
	/*m_vecLPL.clear();
	m_bIsQuitting = true;*/
	return true;
}

bool CSxDBClient::GetRealTimeMonitorItem(CTickerItem &ti)
{
	//char	Buffer[33];
	//char	Length;
	//double	Last;
	//double	Ask;
	//double	Bid;
	//double	High;
	//double	Low;
	//double	Open;
	//double	YesterdayLast;
	//int		Volume;
	//bool	Valid;
	//int		TickCount;
	//int		Change;

	////Read TICK/Price and set ti
	////xxx

	//ti.SetTickerName(Buffer);
	//ti.SetLastPrice(Last);
	//ti.SetAskPrice(Ask);
	//ti.SetBidPrice(Bid);
	//ti.SetHighPrice(High);
	//ti.SetLowPrice(Low);
	//ti.SetOpenPrice(Open);
	//ti.SetYesterdayLast(YesterdayLast);
	//ti.SetTotalVolume(Volume);
	//ti.SetValid(Valid);
	//ti.SetTickCount(TickCount);
	//ti.SetWhatChanged(Change);
	return true;
}

bool CSxDBClient::StartRealTimeMonitoring(string ticker, int filter)
{
//	BEGIN_LOG("StartRealTimeMonitoring");
//
//	// Make sure the listener thread is running
//	InitListenerThread();
//
//	if (!Connect())
//	{
//		return false;
//	}
//
//// Send request to server .. not usefull in our case
//
//	Disconnect();
//
//	m_mapMonitoredTickers[ticker] = true;
//
//	END_LOG();
	return true;
}

string CSxDBClient::GetValueFromSheet(long line, long column)
{
	//CRange lRange;
	//char CellBegin[10];
	//COleVariant	CellValue;
	//IndexToString(line, column, CellBegin);
	//lRange = fWorksheet.get_Range( COleVariant(CellBegin), COleVariant(CellBegin));
	//CellValue = lRange.get_Item( COleVariant((long)1), COleVariant((long)1));
	//CRange ran(CellValue.pdispVal);
	//VARIANT value = ran.get_Value2();
	//CString valueString;
	//double	valueDouble;
	//switch (value.vt) 
	// {
	// case 5: // double
	// case 7: // date
	// 	valueDouble = value.dblVal;
	//	return FROM_STREAM(valueDouble);
	// 	break;
	// case 8: // bstr
	// 	valueString = CString(value.bstrVal);
	//	return FROM_STREAM(valueString);
	// 	break;
	// }
	return "";
}

void CSxDBClient::ReadAndSendPrices(long tickerCol, long PriceCol)
{
	BEGIN_LOG("ReadAndSendPrices");
	MESS(Log::warning, FROM_STREAM("ReadAndSendPrices loop ... " ));

	//long line = 3;
	//bool bRead = true;
	//while (bRead)
	//{
	//	bRead = true;
	//	MESS(Log::warning, FROM_STREAM("In the ReadAndSendPrices loop ..."));
	//	string ticker = GetValueFromSheet(line, tickerCol);
	//	double price = GetValueFromSheetDouble(line, PriceCol);
	//	if (strcmp(ticker.c_str(), "") != 0)
	//	{
	//		CTickerItem	ti;
	//		ti.SetTickerName(ticker);
	//		ti.SetLastPrice(price);
	//		MESS(Log::debug, FROM_STREAM("Price to proceed " << ticker.c_str()));

	//		CLivePriceListener	*pLPL;
	//		for (unsigned int i=0;i<m_vecLPL.size();i++)
	//		{
	//			pLPL = (CLivePriceListener*)m_vecLPL[i];
	//			if (pLPL != NULL)
	//			{
	//				pLPL->OnChange(&ti);
	//			}
	//		}
	//	}
	//	else
	//	{
	//		bRead = false;
	//	}
	//	line++;
	//}

	try
	{
		// Get list of fids
		struct SSxDep
		{
			char fIsin[20];
			char fDatcrs[10];
			double fValcrs;
			double fIndcrs;
			double fCumQuanti;
			double fCumVol;
			double fPlh;
			double fPlb;
			double fOpen;
			double fHigh;
			double fClose;
			double fVwap;
		};
		int nbResult	= 0;
		SSxDep * deps	= new SSxDep();

		_STL::string query = FROM_STREAM("SELECT ISIN, DATCRS, VALCRS, INDCRS, CUMQUANTI, CUMVOL, PLH, PLB, OPEN, HIGH, CLOSE, VWAP FROM " << fTableName.c_str());

		CSRStructureDescriptor	desc(12, sizeof(SSxDep));
		ADD(&desc, SSxDep, fIsin, rdfString);
		ADD(&desc, SSxDep, fDatcrs,	rdfString);
		ADD(&desc, SSxDep, fValcrs,	rdfFloat);
		ADD(&desc, SSxDep, fIndcrs,	rdfFloat);
		ADD(&desc, SSxDep, fCumQuanti,	rdfFloat);
		ADD(&desc, SSxDep, fCumVol,	rdfFloat);
		ADD(&desc, SSxDep, fPlh,	rdfFloat);
		ADD(&desc, SSxDep, fPlb,	rdfFloat);
		ADD(&desc, SSxDep, fOpen,	rdfFloat);
		ADD(&desc, SSxDep, fHigh,	rdfFloat);
		ADD(&desc, SSxDep, fClose,	rdfFloat);
		ADD(&desc, SSxDep, fVwap,	rdfFloat);

		CSRSqlQuery::QueryWithNResults(query.c_str(), &desc, (void**)&deps, &nbResult);
		for(int i = 0; i < nbResult; ++i)
		{
			CTickerItem	ti;
			ti.SetTickerName(deps[i].fIsin);
			ti.SetLastPrice(deps[i].fClose);
			//MESS(Log::debug, FROM_STREAM("Price to proceed " << deps[i].fIsin.c_str()));

			////CLivePriceListener	*pLPL;
			//for (unsigned int i=0;i<m_vecLPL.size();i++)
			//{
			//	pLPL = (CLivePriceListener*)m_vecLPL[i];
			//	if (pLPL != NULL)
			//	{
			//		pLPL->OnChange(&ti);
			//	}
			//}
		}
	}
	catch(_STL::exception &eex)
	{
		MESS(Log::system_error,FROM_STREAM("Internal Error: " << eex.what()));
	}
	catch(...)
	{
		MESS(Log::system_error,"Internal Error");
	}

	END_LOG();

}


string CSxDBClient::GetValueFromSheet(string line, string column)
{
	//CRange lRange;
	//char CellBegin[10];
	//COleVariant	CellValue;
	////IndexToString(line, column, CellBegin);
	//lRange = fWorksheet.get_Range( COleVariant(FROM_STREAM(column.c_str() << line.c_str())), COleVariant(FROM_STREAM(column.c_str() << line.c_str())));
	//CellValue = lRange.get_Item( COleVariant((long)1), COleVariant((long)1));
	//CRange ran(CellValue.pdispVal);
	//VARIANT value = ran.get_Value2();
	//CString valueString;
	//double	valueDouble;
	//switch (value.vt) 
	//{
	//case 5: // double
	//case 7: // date
	//	valueDouble = value.dblVal;
	//	return FROM_STREAM(valueDouble);
	//	break;
	//case 8: // bstr
	//	valueString = CString(value.bstrVal);
	//	return FROM_STREAM(valueString);
	//	break;
	//}
	return "";
}

double CSxDBClient::GetValueFromSheetDouble(long line, long column)
{
	//CRange lRange;
	//char CellBegin[10];
	//COleVariant	CellValue;
	//IndexToString(line, column, CellBegin);
	//lRange = fWorksheet.get_Range( COleVariant(CellBegin), COleVariant(CellBegin));
	//CellValue = lRange.get_Item( COleVariant((long)1), COleVariant((long)1));
	//CRange ran(CellValue.pdispVal);
	//VARIANT value = ran.get_Value2();
	//CString valueString;
	//double	valueDouble;
	//switch (value.vt) 
	//{
	//case 5: // double
	//case 7: // date
	//	valueDouble = value.dblVal;
	//	return valueDouble;
	//	break;
	//case 8: // bstr		
	//	return 0.0;
	//	break;
	//}
	return 0.0;
}

char * CSxDBClient::IndexToString(int row, int col, char *strResult)
{
	//if(col > 26)
	//	sprintf(strResult, "%c%c%d",'A'+(col-1)/26-1, 'A'+(col-1)%26, row);
	//else
	//	sprintf(strResult, "%c%d", 'A' + (col-1)%26, row);

	return strResult;
}
