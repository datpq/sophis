#pragma once
//#include "SphQSInc/SphDataDef.h"
#include "..\SphDataDef.h"
#include "SphQSInc/SphAdapter.h"
#include "PollingThread.h"

#include <gcroot.h>

namespace sophis
{
	namespace quotesource
	{
		namespace adapter
		{
			struct CFGFieldDataType{
				enum Type{
					STRING,
					DOUBLE,
					INTEGER,
					TIME,
					DATE,
					DATETIME
				};
			};

			struct SourceFormatType{
				enum Type{
					DATABASE
				};
			};

			struct CFGDataField : public sophisTools::base::UnsafeRefCount{
				CFGDataField(const String_t& in_field_name);
				virtual ~CFGDataField();

				void setBuffer(void* in_data);
				const String_t& getName() const;
				void setType(CFGFieldDataType::Type in_type);
				CFGFieldDataType::Type getType() const;
				Long_t getInteger() const;
				Real_t getDouble() const;
				String_t getString() const;

			private:
				String_t field_name;
				CFGFieldDataType::Type type;
				void* data;
			};
			typedef sophisTools::base::RefCountHandle<CFGDataField> CFGDataField_H;
			struct CFGQuote : public sophisTools::base::UnsafeRefCount{
				String_t identifier;
				_STL::vector<CFGDataField_H> fields;
			};
			typedef sophisTools::base::RefCountHandle<CFGQuote> CFGQuote_H;

			// Listener which accept quotations
			class IMarketDataListener : public sophisTools::base::UnsafeRefCount{
			public:
				virtual void OnMarketDataEvent(const _STL::vector<CFGQuote_H>& in_quotes) = 0;
				virtual void OnServiceStatus(bool running) = 0;
			};
			typedef sophisTools::base::RefCountHandle<IMarketDataListener> IMarketDataListener_H;

			class CFGSource : public sophis::quotesource::thread::Runnable, public sophisTools::base::UnsafeRefCount
			{
			protected:
				CFGSource();
			public:
				CFGSource(sophis::quotesource::adapter::IQuoteSourceCallback* quoteSource, sophis::quotesource::adapter::IMarketDataListener_H in_listener);
				~CFGSource();

				//void SetListener(IMarketDataListener_H in_listener);

				virtual void init() throw (sophisTools::base::ExceptionBase);
				void dispose();

				void start();
				void stop();
						
				void subscribe(const String_t& in_topicKey);
				void unsubscribe(const String_t& in_topicKey);
				//virtual	void	OnChange(CTickerItem *pTI);

				void FillQuotation(_STL::vector<CFGQuote_H>& out_quotes);

				sophis::quotesource::adapter::IQuoteSourceCallback* m_quoteSource;
				void createSnapshot(const char* topicName);

				static bool	VerboseMode;

				// IRunnable implem
				virtual void run();



			protected:
				

				//DPH
				//gcroot<System::Data::OracleClient::OracleConnection^> m_OracleConnection;
				gcroot<Oracle::ManagedDataAccess::Client::OracleConnection^> m_OracleConnection;

				gcroot<sophis::quotesource::thread::PollingThread^>  m_thread;
				IMarketDataListener_H	m_listener;
				sophis::quotesource::thread::MutexWrapper m_SubMutex;
				_STL::set<String_t> m_topics;
				
			private:
				static const char*		__CLASS__;
				String_t				m_Username;
				String_t				m_Password;
				String_t				m_OracleServer;
				Long_t					m_RefreshInterval;
				bool					m_ServiceStatus;		
				_STL::string			fTableName;

			};

			//typedef sophisTools::base::RefCountHandle<CFGSource> CFGSource_H;
		}
	}
}