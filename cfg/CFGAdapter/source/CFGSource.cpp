//#include "stdafx.h"
#include "SphTools/SphCommon.h"
#include __STL_INCLUDE_PATH(memory)

#include "CFGSource.h"

#include <memory>
#include <time.h>
#include <process.h>
#include <gcroot.h>
#include <msclr\marshal.h>
#include "SphInc\SphMacros.h"
//#include "SphQSInc/SphQuoteSourceCallback.h"
#include "SphQSInc/SphSourceCallback.h"
#include "SphTools/SphLoggerUtil.h"

namespace msclr{
	namespace interop{
		template <> inline System::String^ marshal_as(const String_t& _from_obj)
		{
			return details::InternalAnsiToStringHelper(_from_obj.c_str(), _from_obj.length());
		}

		template <> inline String_t marshal_as(System::String^ const & _from_obj)
		{
			if (_from_obj == nullptr)
			{
				throw gcnew System::ArgumentNullException(_EXCEPTION_NULLPTR);
			}
			String_t _to_obj;
			size_t _size = details::GetAnsiStringSize(_from_obj);

			// copy buffer for non emty strings, else leave default string.
			// ( size includes terminating char)
			if(_size > 1)
			{
				// -1 because resize will automatically +1 for the NULL
				_to_obj.resize(_size-1);
				char *_dest_buf = &(_to_obj[0]);
				details::WriteAnsiString(_dest_buf, _size, _from_obj);
			}
			return _to_obj;
		}
	}
}

using namespace msclr::interop;
using namespace sophis::quotesource;
using namespace sophis::quotesource::adapter;
using namespace sophis::quotesource::thread;
using namespace sophis::quotesource::configuration;
using namespace System;

const char* CFGSource::__CLASS__="CFGSource";

bool CFGSource::VerboseMode = false;

using namespace std;

CFGSource::CFGSource()
{
	BEGIN_LOG("CFGSourceSimple");
	END_LOG();
}



//void CFGSource::SetListener(sophis::quotesource::adapter::IMarketDataListener_H in_listener)
//{
//	m_ServiceStatus = false;
//	m_listener = in_listener;	
//}
CFGSource::CFGSource(sophis::quotesource::adapter::IQuoteSourceCallback* quoteSource, /*CFGSource*/ sophis::quotesource::adapter::IMarketDataListener_H in_listener)
{
	BEGIN_LOG("CFGSource");
	m_quoteSource = quoteSource;

	m_ServiceStatus = false;
	m_listener = in_listener;
	
	END_LOG();
}
 
CFGSource::~CFGSource()
{
	m_listener->_remove_ref();
}

void CFGSource::init(void) throw(sophisTools::base::ExceptionBase)
{
	BEGIN_LOG("init");

	MESS(Log::verbose, "[CFG Source]: init");

	 sophis::quotesource::configuration::CFGAdapterConfigurationGroup^ adapterConfiguration = nullptr;
	try { 
		//adapterConfiguration = safe_cast<CFGAdapterConfigurationGroup^>(QuoteSourceConfigurationGroup::Current->Adapter);
		
        adapterConfiguration = safe_cast<CFGAdapterConfigurationGroup^>(sophis::quotesource::configuration::CFGAdapterConfigurationGroup::Current);
	
	}
	catch (System::InvalidCastException^ ) {
		throw sophisTools::base::GeneralException("The Adapter section is not a valid configuration for Bloomberg Adapters");
	}
	


	m_Username = marshal_as<String_t>(adapterConfiguration->SQLSourceConfiguration->Username);
	m_Password = marshal_as<String_t>(adapterConfiguration->SQLSourceConfiguration->Password);
	m_RefreshInterval = adapterConfiguration->CFGAdapterConfiguration->RefreshInterval;
	m_OracleServer = marshal_as<String_t>(adapterConfiguration->SQLSourceConfiguration->DatabaseInstance);

	MESS(Log::info, "[CFG Source]: Database Username = " << m_Username);
	MESS(Log::info, "[CFG Source]: Database Instance = " << m_OracleServer);
	MESS(Log::info, "[CFG Source]: RefreshInterval = " << m_RefreshInterval << " second(s)");
	
	//try connect to database
	//DPH
	m_OracleConnection = gcnew Oracle::ManagedDataAccess::Client::OracleConnection();
	m_OracleConnection->ConnectionString = gcnew System::String(FROM_STREAM("Data Source="<<m_OracleServer<<";User Id="<<m_Username<<";Password="<<m_Password<<";"));
	//m_OracleConnection = gcnew System::Data::OracleClient::OracleConnection();
	//m_OracleConnection->ConnectionString = gcnew System::String(FROM_STREAM("Data Source="<<m_OracleServer<<";User Id="<<m_Username<<";Password="<<m_Password<<";"));

	try{
		m_OracleConnection->Open();
	}
	catch(System::Exception^ ex){
		throw sophisTools::base::GeneralException(marshal_as<String_t>(ex->Message).c_str());
	}

	//m_UpdateInterval in second
	try{
		m_thread = gcnew PollingThread(this,m_RefreshInterval*1000);
	}catch(System::Exception^ ex){
		throw sophisTools::base::GeneralException(marshal_as<String_t>(ex->Message).c_str());
	}

	END_LOG();
}
void CFGSource::start()
throw(sophisTools::base::ExceptionBase){
	BEGIN_LOG("start");
	try{
		if(!m_thread)
			//DPH
			//MESS(Log::system_error,"[CFG Source]: NULL pointer on m_thread");
			MESS(Log::error, "[CFG Source]: NULL pointer on m_thread");
		m_thread->start();
	}
	catch(System::Exception^ ex){
		throw sophisTools::base::GeneralException(marshal_as<String_t>(ex->Message).c_str());
	}
	m_ServiceStatus = true;
	if(!m_listener)
		//DPH
		//MESS(Log::system_error,"[CFG Source]: NULL pointer on m_listener");
		MESS(Log::error, "[CFG Source]: NULL pointer on m_listener");
	m_listener->OnServiceStatus(m_ServiceStatus);
	END_LOG();
}

void CFGSource::stop()
{
	BEGIN_LOG("Stop" );
	MESS(Log::verbose, "[CFG Source]: stop");
	try{
		m_thread->stop();
	}catch(System::Exception^ ex){
		MESS(Log::error, marshal_as<String_t>(ex->Message));
	}
	END_LOG();
}

void CFGSource::dispose(){
	BEGIN_LOG("dispose");
	MESS(Log::verbose, "[CFG Source]: dispose");
	m_thread = nullptr;

	try{
		m_OracleConnection->Close();
	}
	catch(System::Exception^ ex){
		MESS(Log::error, marshal_as<String_t>(ex->Message));
	}
	END_LOG();
}
void CFGSource::subscribe(const sophis::quotesource::String_t& in_topicKey)
{
	BEGIN_LOG("Subscribe" );
	MESS(Log::verbose,FROM_STREAM("[CFG Source]: subscribe to "<<in_topicKey.c_str()));
	SimpleSync sync(m_SubMutex);
	m_topics.insert(in_topicKey);
	END_LOG();
}

void CFGSource::unsubscribe(const sophis::quotesource::String_t& in_topicKey)
{
	BEGIN_LOG("UnSubscribe" );
	MESS(Log::verbose, "[CFG Source]: unsubscribe");
	SimpleSync sync(m_SubMutex);
	m_topics.erase(in_topicKey);
	END_LOG();
}




/* virtual */void  CFGSource::run()
{

					// TO DO: query the db for topics, delete all records retrieved, send updates to the quote source

BEGIN_LOG("run");
	try{
		MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Source]: run");
		//first, check connection
		if(m_OracleConnection->State == System::Data::ConnectionState::Broken || m_OracleConnection->State == System::Data::ConnectionState::Closed)
		{
			MESS(Log::error, "[CFG Source]: Connection to database broken, reconnecting");
			m_listener->OnServiceStatus(false);
			try{
				m_OracleConnection->Close();
			}
			catch(...){}
			m_OracleConnection->Open();
			m_listener->OnServiceStatus(true);
		}
		_STL::vector<CFGQuote_H> images;
		FillQuotation(images);
		if(!images.empty()) 
			m_listener->OnMarketDataEvent(images);
	}
	catch(System::Exception^ ex){
		MESS(Log::error,marshal_as<String_t>(ex->Message));
	}
	catch(...){
		MESS(Log::error, "[CFG Source]: unexpected");
	}

END_LOG();
}



//void CFGSource::OnChange(CTickerItem *pTI)
//{
//	BEGIN_LOG("OnChange" );
//
//	//IQuoteBatch* images = m_quoteSource->createQuoteBatch();
//	//IQuote* aQuote = images->getQuoteOnTopic(pTI->GetTickerName().c_str());
//
//	//aQuote->insert(eLast, pTI->GetLastPrice());
//	//aQuote->insert(eMid, pTI->GetMidPrice());
//	//aQuote->insert(eOpen,pTI->GetOpenPrice());
//	//aQuote->insert(eYesterday,pTI->GetYesterdayLast());
//	//aQuote->insert(eHigh,pTI->GetHighPrice());
//	//aQuote->insert(eLow,pTI->GetLowPrice());
//	//aQuote->insert(eBid,pTI->GetBidPrice());
//	//aQuote->insert(eAsk,pTI->GetAskPrice());
//
//	//std::map<int,double> quotes;
//	//quotes[eLast] = pTI->GetLastPrice();
//	//quotes[eMid] = pTI->GetMidPrice();
//	//quotes[eOpen] = pTI->GetOpenPrice();
//	//quotes[eYesterday] = pTI->GetYesterdayLast();
//	//quotes[eHigh] = pTI->GetHighPrice();
//	//quotes[eLow] = pTI->GetLowPrice();
//	//quotes[eBid] = pTI->GetBidPrice();
//	//quotes[eAsk] = pTI->GetAskPrice();
//
//	//fQuoteCache[pTI->GetTickerName().c_str()] = quotes;
//
//	//m_quoteSource->notifyTopicImages(images);
//
//
//	//delete images;
//	END_LOG();
//
//}

void CFGSource::createSnapshot(const char* topicName)
{
	BEGIN_LOG("createSnapshot");
	MESS(Log::verbose, "[CFG Source]: createSnapshot");
	//IQuoteBatch* batch = m_quoteSource->createQuoteBatch();
	//IQuote* aQuote = batch->getQuoteOnTopic(topicName);

	//aQuote->insert(eLast,fQuoteCache[topicName][eLast]);	
	//aQuote->insert(eMid,fQuoteCache[topicName][eMid]);	
	//aQuote->insert(eOpen,fQuoteCache[topicName][eOpen]);	
	//aQuote->insert(eYesterday,fQuoteCache[topicName][eYesterday]);	
	//aQuote->insert(eHigh,fQuoteCache[topicName][eHigh]);	
	//aQuote->insert(eLow,fQuoteCache[topicName][eLow]);	
	//aQuote->insert(eBid,fQuoteCache[topicName][eBid]);	
	//aQuote->insert(eAsk,fQuoteCache[topicName][eAsk]);	

	//m_quoteSource->notifySnapshots(batch);
	//delete batch;
	END_LOG();
}

void CFGSource::FillQuotation(_STL::vector<CFGQuote_H>& out_quotes){
	BEGIN_LOG("FillQuotation");
/*
	Long_t _date = sophisTools::CSRDay::GetSystemDate();
	Long_t _time = sophisTools::CSRDay::GetSystemTime();
	CFGDataField_H tf= new CFGDataField("TIME");
	tf->setType(CFGFieldDataType::TIME); Long_t* timebuf = new Long_t;
	*timebuf = _time; tf->setBuffer(timebuf);
	CFGDataField_H df = new CFGDataField("DATE");
	df->setType(CFGFieldDataType::DATE); Long_t* datebuf = new Long_t;
	*datebuf = _date; df->setBuffer(datebuf);
*/

	MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Source]: Entering in the FillQuotation method (source)");
	//DPH
	//System::Data::OracleClient::OracleDataReader^ reader = nullptr;
	Oracle::ManagedDataAccess::Client::OracleDataReader^ reader = nullptr;
	//read values
	try{
		//query database
		System::String^ query = "SELECT ISIN, DATCRS, VALCRS, INDCRS, CUMQUANTI, CUMVOL, PLH, PLB, OPEN, HIGH, CLOSE, VWAP, ID FROM CFG_PRICES";
		//DPH
		//System::Data::OracleClient::OracleCommand^ cmd = gcnew System::Data::OracleClient::OracleCommand(query,m_OracleConnection);
		Oracle::ManagedDataAccess::Client::OracleCommand^ cmd = gcnew Oracle::ManagedDataAccess::Client::OracleCommand(query, m_OracleConnection);
		reader = cmd->ExecuteReader();
		int max_id = 0;
		while(reader->Read()){
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, "[CFG Source]: Entering in the while loop");

			CFGQuote_H _quote = new CFGQuote;

			_STL::string isin = marshal_as<_STL::string>(reader->GetString(0));
			_quote->identifier = isin;
			if(m_topics.find(isin) == m_topics.end())
			{
				// Not in topics list, continue
				continue;
			}

			/** 
			* DateTime cannot be null
			*   NB: We need the date for the snap (because we can now save for past dates...)
			*/
			DateTime dt = reader->GetDateTime(1);
			TimeSpan span = dt.Subtract(DateTime(1904,1,1));
			CFGDataField_H df_day = new CFGDataField("DAY");
			df_day->setType(CFGFieldDataType::DATE); Long_t* datebuf = new Long_t;
			*datebuf = (Long_t)span.TotalDays; df_day->setBuffer(datebuf); // Quotation assumed to be for today
			_quote->fields.push_back(df_day);

			CFGDataField_H df_time = new CFGDataField("TIME");
			df_time->setType(CFGFieldDataType::TIME); Long_t* timebuf = new Long_t;
			*timebuf = (Long_t)(span.Hours*3600 + span.Minutes*60 +span.Seconds); df_time->setBuffer(timebuf); // Quotation assumed to be for today
			_quote->fields.push_back(df_time);
			
			// Last cannot be null
			double valcrs = System::Decimal::ToDouble(reader->GetDecimal(2));
			CFGDataField_H _valcrs = new CFGDataField("VALCRS");
			_valcrs->setType(CFGFieldDataType::DOUBLE);
			_valcrs->setBuffer(new Real_t(valcrs));
			_quote->fields.push_back(_valcrs);

			// All fields below can be null. So test for each whether value is null
			double indcrs = NOTDEFINED;
			if(! reader->IsDBNull(3))
			{	
				indcrs = System::Decimal::ToDouble(reader->GetDecimal(3));
				CFGDataField_H _indcrs = new CFGDataField("INDCRS");
				_indcrs->setType(CFGFieldDataType::DOUBLE);
				_indcrs->setBuffer(new Real_t(indcrs));
				_quote->fields.push_back(_indcrs);
			}

			double cumQuanti = NOTDEFINED;
			if(! reader->IsDBNull(4))
			{	
				cumQuanti = System::Decimal::ToDouble(reader->GetDecimal(4));
				CFGDataField_H _cumQuanti = new CFGDataField("CUMQUANTI");
				_cumQuanti->setType(CFGFieldDataType::DOUBLE);
				_cumQuanti->setBuffer(new Real_t(cumQuanti));
				_quote->fields.push_back(_cumQuanti);
			}

			double cumVol = NOTDEFINED;
			if(! reader->IsDBNull(5))
			{	
				cumVol= System::Decimal::ToDouble(reader->GetDecimal(5));
				CFGDataField_H _cumVol = new CFGDataField("CUMVOL");
				_cumVol->setType(CFGFieldDataType::DOUBLE);
				_cumVol->setBuffer(new Real_t(cumVol));
				_quote->fields.push_back(_cumVol);
			}

			double plh = NOTDEFINED;
			if(! reader->IsDBNull(6))
			{	
				plh= System::Decimal::ToDouble(reader->GetDecimal(6));
				CFGDataField_H _plh = new CFGDataField("PLH");
				_plh->setType(CFGFieldDataType::DOUBLE);
				_plh->setBuffer(new Real_t(plh));
				_quote->fields.push_back(_plh);
			}

			double plb = NOTDEFINED;
			if(! reader->IsDBNull(7))
			{	
				plb= System::Decimal::ToDouble(reader->GetDecimal(7));
				CFGDataField_H _plb = new CFGDataField("PLB");
				_plb->setType(CFGFieldDataType::DOUBLE);
				_plb->setBuffer(new Real_t(plb));
				_quote->fields.push_back(_plb);
			}

			double open = NOTDEFINED;
			if(! reader->IsDBNull(8))
			{	
				open= System::Decimal::ToDouble(reader->GetDecimal(8));
				CFGDataField_H _open = new CFGDataField("OPEN");
				_open->setType(CFGFieldDataType::DOUBLE);
				_open->setBuffer(new Real_t(open));
				_quote->fields.push_back(_open);
			}

			double high = NOTDEFINED;
			if(! reader->IsDBNull(9))
			{	
				high= System::Decimal::ToDouble(reader->GetDecimal(9));
				CFGDataField_H _high = new CFGDataField("HIGH");
				_high->setType(CFGFieldDataType::DOUBLE);
				_high->setBuffer(new Real_t(high));
				_quote->fields.push_back(_high);
			}

			double close = NOTDEFINED;
			if(! reader->IsDBNull(10))
			{	
				close= System::Decimal::ToDouble(reader->GetDecimal(10));
				CFGDataField_H _close = new CFGDataField("CLOSE");
				_close->setType(CFGFieldDataType::DOUBLE);
				_close->setBuffer(new Real_t(close));
				_quote->fields.push_back(_close);
			}

			double vwap = NOTDEFINED;
			if(! reader->IsDBNull(11))
			{	
				vwap= System::Decimal::ToDouble(reader->GetDecimal(11));
				CFGDataField_H _vwap = new CFGDataField("VWAP");
				_vwap->setType(CFGFieldDataType::DOUBLE);
				_vwap->setBuffer(new Real_t(vwap));
				_quote->fields.push_back(_vwap);
			}

			int row_id = System::Decimal::ToInt32(reader->GetInt32(12));
			if(row_id > max_id)
			{	max_id = row_id;}

			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("isin = " << isin));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("df = " << (*datebuf) ));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("dt = " << (*timebuf) ));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("valcrs = " << valcrs));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("indcrs = " << indcrs));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("cumQuanti = " << cumQuanti));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("cumVol = " << cumVol));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("plh = " << plh));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("plb = " << plb));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("open = " << open));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("high = " << high));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("close = " << close));
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM("vwap = " << vwap));

			out_quotes.push_back(_quote);
		}
		reader->Close();

		// Delete now...
		if(max_id)
		{
			System::String^ deleteQuery = "DELETE CFG_PRICES WHERE ID <= " + max_id;
			//DPH
			//System::Data::OracleClient::OracleCommand^ deleteCmd = gcnew System::Data::OracleClient::OracleCommand(deleteQuery,m_OracleConnection);
			Oracle::ManagedDataAccess::Client::OracleCommand^ deleteCmd = gcnew Oracle::ManagedDataAccess::Client::OracleCommand(deleteQuery, m_OracleConnection);
			int nbRowDeleted = deleteCmd->ExecuteNonQuery();
			MESS(CFGSource::VerboseMode ? Log::info : Log::debug, FROM_STREAM(nbRowDeleted<< " rows deleted with ID <= "<<max_id));
		}

		//// Dispose cursors...
		//cmd->Dispose();
		//deleteCmd->Dispose();

	}
	catch(System::Exception^ ex)
	{
		if(reader != nullptr) reader->Close();
		MESS(Log::error, marshal_as<String_t>(ex->Message));
	}
	reader = nullptr;
	END_LOG();
}






//////////////////////////////////////////////////////////////////////////
const sophis::quotesource::String_t& CFGDataField::getName() const{
	return field_name;
}
//////////////////////////////////////////////////////////////////////////
CFGFieldDataType::Type CFGDataField::getType() const{
	return type;
}
//////////////////////////////////////////////////////////////////////////
CFGDataField::CFGDataField(const sophis::quotesource::String_t& in_field_name)
:field_name(in_field_name),data(NULL){}

//////////////////////////////////////////////////////////////////////////
CFGDataField::~CFGDataField(){
	if(data){
		switch(type){
		case CFGFieldDataType::DATE:
		case CFGFieldDataType::TIME:
		case CFGFieldDataType::INTEGER:
			delete (Long_t*)data;
			break;
		case CFGFieldDataType::DOUBLE:
		case CFGFieldDataType::DATETIME:
			delete (Real_t*)data;
			break;
		case CFGFieldDataType::STRING:
			delete[] (char*)data;
			break;
		default:
			break;
		}
	}
	data = NULL;
}


//////////////////////////////////////////////////////////////////////////
sophis::quotesource::Long_t CFGDataField::getInteger() const{
	if(!data) return 0;
	return (*(Long_t*)data);
}

//////////////////////////////////////////////////////////////////////////
sophis::quotesource::Real_t CFGDataField::getDouble() const{
	if(!data) return 0.0;
	return (*(Real_t*)data);
}

//////////////////////////////////////////////////////////////////////////
sophis::quotesource::String_t CFGDataField::getString() const{
	if(!data) return "";
	const char* str = (char*)data;
	return String_t(str);
}


//////////////////////////////////////////////////////////////////////////
void CFGDataField::setType(CFGFieldDataType::Type in_type){
	type = in_type;
}
//////////////////////////////////////////////////////////////////////////
void CFGDataField::setBuffer(void* in_data){
	data = in_data;
}
