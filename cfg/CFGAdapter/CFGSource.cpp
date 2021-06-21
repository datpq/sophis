#include "stdafx.h"
#include "CFGSource.h"
#include "SocketClient.h"
#include "TickerItem.h"
#include <time.h>
#include <process.h>
#include "SphQSInc/SphQuoteSourceCallback.h"
#include "SphTools/SphLoggerUtil.h"

using namespace sophis::quotesource;

const char* CFGSource::__CLASS__="CFGSource";


CFGSource::CFGSource()
{
	BEGIN_LOG("CFGSourceSimple");
	END_LOG();
}


CFGSource::CFGSource(sophis::quotesource::IQuoteSourceCallback* quoteSource )
{
	AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0);
	BEGIN_LOG("CFGSource");
	m_quoteSource = quoteSource;
	END_LOG();

}

CFGSource::~CFGSource()
{
}


void CFGSource::Start()
{
	BEGIN_LOG("Start");

	int IncomingSocket = 6543;
	
	client.Init(this, IncomingSocket);
	LOG(Log::info , "Init done");

	m_quoteSource->notifyServiceUp();
	LOG(Log::info , "Notify ServiceUp done");
	END_LOG();
}

void CFGSource::Stop()
{
	BEGIN_LOG("Stop" );
	m_quoteSource->notifyServiceDown();
	client.Shutdown();
	END_LOG();
}

void CFGSource::subscribe(const char* topicName)
{
	BEGIN_LOG("Subscribe" );
	LOG(Log::info, "before Monitoring");
	LOG(Log::info, topicName);
	if (!client.StartRealTimeMonitoring(topicName, CHANGE_ALL))
	{
		LOG(Log::error, "Unable to start monitoring, subscription failed");
	}
	END_LOG();
}


void CFGSource::unsubscribe(const char* topicName)
{
	BEGIN_LOG("UnSubscribe" );
	LOG(Log::debug, topicName);
	END_LOG();
}


void CFGSource::OnChange(CTickerItem *pTI)
{
	BEGIN_LOG("OnChange" );

	IQuoteBatch* images = m_quoteSource->createQuoteBatch();
	IQuote* aQuote = images->getQuoteOnTopic(pTI->GetTickerName().c_str());

	aQuote->insert(eLast, pTI->GetLastPrice());
	aQuote->insert(eMid, pTI->GetMidPrice());
	aQuote->insert(eOpen,pTI->GetOpenPrice());
	aQuote->insert(eYesterday,pTI->GetYesterdayLast());
	aQuote->insert(eHigh,pTI->GetHighPrice());
	aQuote->insert(eLow,pTI->GetLowPrice());
	aQuote->insert(eBid,pTI->GetBidPrice());
	aQuote->insert(eAsk,pTI->GetAskPrice());

	std::map<int,double> quotes;
	quotes[eLast] = pTI->GetLastPrice();
	quotes[eMid] = pTI->GetMidPrice();
	quotes[eOpen] = pTI->GetOpenPrice();
	quotes[eYesterday] = pTI->GetYesterdayLast();
	quotes[eHigh] = pTI->GetHighPrice();
	quotes[eLow] = pTI->GetLowPrice();
	quotes[eBid] = pTI->GetBidPrice();
	quotes[eAsk] = pTI->GetAskPrice();

	fQuoteCache[pTI->GetTickerName().c_str()] = quotes;

	m_quoteSource->notifyTopicImages(images);


	delete images;
	END_LOG();

}

void CFGSource::createSnapshot(const char* topicName)
{
	IQuoteBatch* batch = m_quoteSource->createQuoteBatch();
	IQuote* aQuote = batch->getQuoteOnTopic(topicName);

	aQuote->insert(eLast,fQuoteCache[topicName][eLast]);	
	aQuote->insert(eMid,fQuoteCache[topicName][eMid]);	
	aQuote->insert(eOpen,fQuoteCache[topicName][eOpen]);	
	aQuote->insert(eYesterday,fQuoteCache[topicName][eYesterday]);	
	aQuote->insert(eHigh,fQuoteCache[topicName][eHigh]);	
	aQuote->insert(eLow,fQuoteCache[topicName][eLow]);	
	aQuote->insert(eBid,fQuoteCache[topicName][eBid]);	
	aQuote->insert(eAsk,fQuoteCache[topicName][eAsk]);	

	m_quoteSource->notifySnapshots(batch);
	delete batch;
}