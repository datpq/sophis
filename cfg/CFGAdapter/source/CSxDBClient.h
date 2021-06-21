#pragma once
//#include <afxsock.h>
#include __STL_INCLUDE_PATH(string)
#include <vector>
#include <map>
//#include "../../Excel/IncludeExcel.h"
#include "TickerItem.h"
#include "LivePriceListener.h"
//#include "afxmt.h"

using namespace std;

#define	SERVER_PORT	6060

class CSxDBClient
{
private:
	//vector<CLivePriceListener*>		m_vecLPL;
	bool							m_bInitialise;
	//CCriticalSection				m_CS;
	//bool							m_bListeningThreadRunning;
	//bool							m_bIsQuitting;
	//map<string, bool>				m_mapMonitoredTickers;

	//CApplication	fApplication;

	bool	InitListenerThread();
	bool	Connect();
	bool	Disconnect();


	void ReadAndSendPrices(long tickerCol, long PriceCol);


	double GetValueFromSheetDouble(long line, long column);
	string GetValueFromSheet(long line, long column);
	string GetValueFromSheet(string line, string column);

	char * IndexToString(int row, int col, char *strResult);



	long fColTicker;
	long fColPRice;

	_STL::string fTableName;
	long fDelay;

public:
			CSxDBClient(void);
			~CSxDBClient(void);
	//bool	Init(CLivePriceListener *pLPL);
	bool	Init();
	bool	ListenHandler();

	bool	Shutdown();
	bool	GetRealTimeMonitorItem(CTickerItem &ti);

	bool	StartRealTimeMonitoring(string ticker, int filter);
	bool	StartRealTimeMonitoring(vector<string> &vecTickers, int filter);

	static const char * __CLASS__;

};
