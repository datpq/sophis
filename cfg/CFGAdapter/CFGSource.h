#pragma once
#include "LivePriceListener.h"
#include "TickerItem.h"
#include "SphQSInc/SphUserAdapter.h"
#include "SocketClient.h"


class CFGSource: public CLivePriceListener
{
public:
	CFGSource(sophis::quotesource::IQuoteSourceCallback* quaoteSource);
	CFGSource();
	~CFGSource();

	void init();
	void dispose();

	void Start();
	void Stop();
			
	void subscribe(const char* topicName);
	void unsubscribe(const char* topicName);
	virtual	void	OnChange(CTickerItem *pTI);

	sophis::quotesource::IQuoteSourceCallback* m_quoteSource;
	void createSnapshot(const char* topicName);

private:
	CSocketClient		client;


	std::map<std::string,std::map<int,double>> fQuoteCache;

	enum eTags
	{
		eLast = 6,
		eMid = 134,
		eOpen = 19,
		eYesterday = 21,
		eHigh = 12,
		eLow = 13,
		eBid = 22,
		eAsk =25
	};
	static const char * __CLASS__;


};
