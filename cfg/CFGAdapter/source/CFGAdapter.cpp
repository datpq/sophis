/**
* System includes
*/
#include "SphTools/SphLoggerUtil.h"

/**
* Application includes
*/
//#include "SphQSInc/SphDataDef.h"
#include "..\SphDataDef.h"
#include "SphQSInc/SphSourceCallback.h"
#include "CFGAdapter.h"

/**
* Used Namespace
*/
using namespace sophis::quotesource::adapter;
using namespace sophis::quotesource;
using namespace sophis::quotesource::data;
using namespace sophis::quotesource::thread;
using namespace sophis::quotesource::configuration;



/*
** Statics
*/
const char* CFGAdapter::__CLASS__="CFGAdapter";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------

//Factory: entry point
extern "C" __declspec(dllexport) sophis::quotesource::adapter::IQuoteSourceAdapter* createQuoteSourceAdapter() 
{
	return new CFGAdapter();
}

//-------------------------------------------------------------------------------------------------------------
CFGAdapter::CFGAdapter()
{
	m_Source = NULL;
}
//-------------------------------------------------------------------------------------------------------------
CFGAdapter::~CFGAdapter()
{
	if(m_Source)
	{
		m_Source->_remove_ref();
	}
}
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ sophis::quotesource::Tag_t CFGAdapter::createTag(const sophis::quotesource::String_t& fid, sophis::quotesource::data::FieldDataType::Value type) 
throw(sophisTools::base::ExceptionBase)/* =0*/
{
	BEGIN_LOG("createTag");
	
	// TO DO

	END_LOG();

	// don't forget to change the returned value. -1 will cause issues
	//return -index;
	return 0;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGAdapter::init(sophis::quotesource::adapter::IQuoteSourceCallback_H callback) 
throw(sophisTools::base::ExceptionBase) /*= 0*/
{
	
	BEGIN_LOG("init");

	MESS(Log::info, "[CFG Adapter]: *************CFG Adapter v1.0.0.0***********");

//#ifdef _DEBUG
//	_STL::cout << "[CFG Adapter]: #### Wait Enter for attach ###";
//	_STL::cin.get();
//#endif

	sophis::quotesource::configuration::CFGAdapterConfigurationGroup^ adapterConfiguration = nullptr;
	try { 
		//adapterConfiguration = safe_cast<CFGAdapterConfigurationGroup^>(CFGAdapterConfigurationGroup::Current->Adapter);
		 adapterConfiguration = safe_cast<CFGAdapterConfigurationGroup^>(sophis::quotesource::configuration::CFGAdapterConfigurationGroup::Current);
	
	}
	catch (System::InvalidCastException^ ) {
		throw sophisTools::base::GeneralException("[CFG Adapter]: The Adapter section is not a valid configuration for Bloomberg Adapters");
	}

   
	CFGSource::VerboseMode = adapterConfiguration->CFGAdapterConfiguration->VerboseMode;
	//sophis::quotesource::adapter::SourceFormatType::Type sourceType = (sophis::quotesource::adapter::SourceFormatType::Type)adapterConfiguration->CFGAdapterConfiguration->SourceFormat;

	MESS(Log::info, "[CFG Adapter]: Initializing adapter and creating source...");
	m_QuoteSource = callback;
	
	IMarketDataListener_H this_H = this;
	m_Source = new CFGSource(callback.in(), this_H);//CFGSourceFactory::CreateSource(IMarketDataListener_H::_duplicate(this),sourceType);
	m_Source->init();
	
	MESS(Log::info, "[CFG Adapter]: Adapter has been successfully initialized");

	END_LOG();
}


//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGAdapter::start() throw(sophisTools::base::ExceptionBase) /*= 0*/
{
	BEGIN_LOG("start");

	MESS(Log::info, "[CFG Adapter]: Starting adapter...");
	m_Source->start();
	MESS(Log::info, "[CFG Adapter]: Adapter has successfully started");
	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGAdapter::stop() /*= 0*/
{
	BEGIN_LOG("stop");

	MESS(Log::info, "[CFG Adapter]: Stopping adapter...");
	m_Source->stop();
	MESS(Log::info, "[CFG Adapter]: Adapter has successfully stopped");

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGAdapter::dispose() /*= 0*/
{
	BEGIN_LOG("dispose");
	MESS(Log::verbose, "[CFG Adapter]: dispose");
	
	m_Source->dispose();
	m_Source = NULL;
	m_QuoteSource = NULL;

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGAdapter::subscribe(const sophis::quotesource::data::TopicKey_t& in_key) /*= 0*/{
	BEGIN_LOG("subscribe");

	MESS(Log::verbose, "[CFG Adapter]: Subscribing topic["<<in_key.toString()<<"]");
	
	SimpleSync sync(m_SubMutex);
	_STL::map<TopicKey_t,IQuote_H>::iterator iter = m_QuoteCacheMap.find(in_key);
	if(iter == m_QuoteCacheMap.end()){
		// first subscription, add into index map
		MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Adapter]: Topic["<<in_key.toString()<<"] not yet subscribed, insert it into cache map");

		m_QuoteCacheMap[in_key] = NULL; //not yet in the cache
		m_Source->subscribe(in_key.toString());
	}
	else{
		MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Adapter]: Topic["<<in_key.toString()<<"] has been subscribed");
		IQuote_H& quote = m_QuoteCacheMap[in_key];
		if(NULL != quote){ //has image cached, return image immediately
			_STL::vector<IQuote_H> images; images.push_back(quote);
			m_QuoteSource->notifyTopicImages(images);
		}
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGAdapter::unsubscribe(const sophis::quotesource::data::TopicKey_t& in_key) /*= 0*/
{
	BEGIN_LOG("unsubscribe");

	MESS(Log::verbose, "[CFG Adapter]: Unsubscribing topic["<<in_key.toString()<<"]");

	SimpleSync sync(m_SubMutex);
	_STL::map<TopicKey_t,IQuote_H>::iterator iter = m_QuoteCacheMap.find(in_key);
	if(iter != m_QuoteCacheMap.end()){
		// first subscription, add into index map
		MESS(Log::verbose, "[CFG Adapter]: Topic["<<in_key.toString()<<"] found in cache map, remove it");
		m_QuoteCacheMap.erase(iter); //not yet in the cache
		m_Source->unsubscribe(in_key.toString());
	}
	else{
		MESS(Log::verbose, "[CFG Adapter]: Topic["<<in_key.toString()<<"] not found in cache map");
	}

	END_LOG();
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGAdapter::requestSnapshot(sophis::quotesource::Long_t closure, const sophis::quotesource::data::SnapshotRequest& requests) /*= 0*/
{
	BEGIN_LOG("requestSnapshot");
	
	MESS(Log::verbose, "[CFG Adapter]: requestSnapshot");
	
	_STL::map<Long_t,_STL::map<Date_t,QuoteBatch>> snapshots; 
	QuoteBatch& quotes = snapshots[closure][requests.m_StartDate];
	SnapshotErrorQ errors;

	MESS(Log::debug, "[CFG Adapter]: Received snapshot request for " << requests.m_Topics.size() << " topics on begin date<" << requests.m_StartDate << ">, end date<" << requests.m_EndDate << ">");
	MESS(Log::debug, "[CFG Adapter]: Today is " << m_QuoteSource->currentGMTDate());
	MESS(Log::debug, "Type is " << requests.m_Type);

	if(requests.m_Type ==  SnapshotRequestType::EOD_REQUEST)
	{ 
		SimpleSync sync(m_SubMutex);
		MESS(Log::debug, "[CFG Adapter]: Got m_SubMutex ");
		MESS(Log::debug, "[CFG Adapter]: About to iterate on snapshot request of "<< requests.m_Topics.size() <<" topics...");
		for(_STL::set<TopicKey_t>::const_iterator iterSet = requests.m_Topics.begin(); iterSet != requests.m_Topics.end(); iterSet++)
		{	
			MESS(Log::debug, "[CFG Adapter]: Searching image for topic "<< (*iterSet).toString() );	
			_STL::map<TopicKey_t,IQuote_H>::iterator iterMap = m_QuoteCacheMap.find(*iterSet);
			if(iterMap != m_QuoteCacheMap.end() && NULL != iterMap->second)
			{
				MESS(Log::debug, "[CFG Adapter]: found image for topic[" << iterSet->toString() << "]");
				quotes.push_back(iterMap->second);
			}
			else
			{
				SnapshotError err; err.closure = closure; err.date = m_QuoteSource->currentGMTDate();
				err.topicKey = *iterSet; err.reason = "[CFG Adapter]: No data received on topic"; err.errortype = SnapshotError::SUB_ERROR;
				err.errorcode = "CFGAdapter.002";
				errors.push_back(err);
			}
		}
	}
	else{
		SnapshotError err; err.closure = closure; err.date = m_QuoteSource->currentGMTDate();
		err.reason = "This source does not support historical data request"; err.errortype = SnapshotError::SOURCE_ERROR;
		err.errorcode = "CFG.001";
		errors.push_back(err);
	}
	if(!quotes.empty()){
		m_QuoteSource->notifySnapshots(snapshots);
	}
	if(!errors.empty()){
		m_QuoteSource->notifySnapshotError(errors);
	}

	END_LOG();
}

////-------------------------------------------------------------------------------------------------------------
///*virtual*/ void CFGAdapter::requestUserAccessRights(const sophis::quotesource::String_t& userID) /*= 0*/
//{
//	BEGIN_LOG("requestUserAccessRights");
//	MESS(Log::verbose, "[CFG Adapter]: requestUserAccessRights");
//	//m_QuoteSource->notifyUserAccessRights(userID,&m_UserAccessRightMap[userID]);
//
//	END_LOG();
//}
//
////-------------------------------------------------------------------------------------------------------------
///*virtual*/ void CFGAdapter::requestUserAuthorization(const String_t& userID, const String_t& clientID, const sophis::quotesource::data::PropertyList& userInfo) /*= 0*/
//{
//	BEGIN_LOG("requestUserAuthorization");
//
//	MESS(Log::verbose, "[CFG Adapter]: requestUserAuthorization");
//	m_QuoteSource->notifyUserAuthorization(clientID,userID,true,"CFG");
//
//	END_LOG();
//}
//
//
////-------------------------------------------------------------------------------------------------------------
///*virtual*/ void CFGAdapter::requestTopicAccessRight(const sophis::quotesource::data::TopicKey_t& topicKey) /*= 0*/
//{
//	BEGIN_LOG("requestTopicAccessRight");
//
//	MESS(Log::verbose, "[CFG Adapter]: requestUserAuthorization");
//	m_QuoteSource->notifyTopicAccessRights(topicKey,NULL);
//
//	END_LOG();
//}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGAdapter::removeClients(const sophis::quotesource::data::ClientIdList& clientsIdsList) /*= 0*/
{
	BEGIN_LOG("removeClients");

	MESS(Log::verbose, "[CFG Adapter]: removeClients");
	// TO DO

	END_LOG();
}


/*virtual*/ void CFGAdapter::OnMarketDataEvent(const _STL::vector<CFGQuote_H> &in_quotes){
	BEGIN_LOG("OnMarketDataEvent");

	MESS(Log::debug, "[CFG Adapter]: OnMarketDataEvent");
	MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Adapter]: Receive data which contains "<< in_quotes.size() << " quotes");
	
	QuoteBatch updates;
	QuoteBatch images;

	if(!in_quotes.empty()){
		
		MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Adapter]: Receive data which contains "<< in_quotes.size() << " quotes");

		SimpleSync sync(m_SubMutex);
		for(_STL::vector<CFGQuote_H>::const_iterator iter = in_quotes.begin();
			iter != in_quotes.end(); iter++){
			const CFGQuote_H& _quote = *iter;
			TopicKey_t _key(_quote->identifier);
			MESS(Log::debug, "[CFG Adapter]: About to process quote for topic "<< _quote->identifier);

			//DPH save historical prices
			//IQuote_H _data = m_QuoteSource->createQuote(_key,DictionaryType::STREAMING);
			IQuote_H _data = m_QuoteSource->createQuote(_key, DictionaryType::SNAPSHOT);
			for(_STL::vector<CFGDataField_H>::const_iterator iterV = _quote->fields.begin();
				iterV != _quote->fields.end(); iterV++){
				const CFGDataField_H& _field = *iterV;
				try{
					switch(_field->getType()){
					case CFGFieldDataType::STRING:
						_data->insert(_field->getName(),_field->getString());
						break;
					case CFGFieldDataType::INTEGER:
					case CFGFieldDataType::TIME:
					case CFGFieldDataType::DATE:
						_data->insert(_field->getName(),_field->getInteger());
						break;
					case CFGFieldDataType::DOUBLE:
					case CFGFieldDataType::DATETIME:
						_data->insert(_field->getName(),_field->getDouble());
						break;
					default:
						break;
					}
				}catch(const sophisTools::base::ExceptionBase& ex){
					LOG_EX(ex,Log::comm);
				}
			}
			_STL::map<TopicKey_t,IQuote_H>::iterator iterM = m_QuoteCacheMap.find(_key);
			if(iterM != m_QuoteCacheMap.end()){ //subscribed
				if(NULL == iterM->second){ 
					MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Adapter]: Received image on topic["<< _quote->identifier <<"]");
					images.push_back(_data); 
				}
				else{
					MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Adapter]: Received an update on topic["<< _quote->identifier <<"]");
					updates.push_back(_data); 
				};
				//should always be a image so we keep always the last quote
				iterM->second = _data;
			}
			else
			{
				MESS(Log::info, "[CFG Adapter]: Received quote for topic not in list: "<< _quote->identifier);
			}
/*			else{
				//register in map, do not notify
				m_QuoteCacheMap[_key] = _data;
			}*/
		}
	}
	if(!images.empty()) 
		m_QuoteSource->notifyTopicImages(images);
	if(!updates.empty()) 
		m_QuoteSource->notifyTopicUpdates(updates);
	END_LOG();
}

void CFGAdapter::OnServiceStatus(bool running){
	BEGIN_LOG("OnServiceStatus");
	if(running){ 
		//DPH
		//m_QuoteSource->notifyServiceUp(); 
		m_QuoteSource->notifyServiceUp(false);
		MESS(Log::info,FROM_STREAM("[CFG Adapter]: OnServiceStatus Up"));
	}
	else { 
		//DPH
		//m_QuoteSource->notifyServiceDown(); 
		m_QuoteSource->notifyServiceDown(false);
		MESS(Log::info,FROM_STREAM("[CFG Adapter]: OnServiceStatus Down"));
	}
	END_LOG();
}