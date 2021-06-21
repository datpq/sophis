#ifndef __CFGAdapter_H__
#define __CFGAdapter_H__

/**
* System includes
*/
#include "SphQSInc/SphAdapter.h"
#include "SphInc/SphMacros.h"
#include __STL_INCLUDE_PATH(map)

/**
* Application includes
*/
#include "CFGSource.h"

using namespace sophis::quotesource::adapter;
using namespace sophis::quotesource;

#ifdef WIN32
#	pragma warning(push)
#	pragma warning(disable:4275) // Can not export a class derivated from a non exported one
#	pragma warning(disable:4251) // Can not export a class agregating a non exported one
#endif



#pragma pack(8)

extern "C" __declspec(dllexport) sophis::quotesource::adapter::IQuoteSourceAdapter* createAdapter();

namespace sophis
{
	namespace quotesource
	{
		/**	Interface of toolkit adapter	
		*/
		//public sophis::quotesource::adapter::IAdapter, public IMarketDataListener
		class CFGAdapter : public sophis::quotesource::adapter::IQuoteSourceAdapter, public IMarketDataListener
		{	
		//------------------------------------ PUBLIC ------------------------------------
		public: // c_tor() and destructor
			CFGAdapter();
			~CFGAdapter();
		public:

			/**	Initialize adapter

			@param callback 
			Callback interface through which notifications are made.

			@throw ExceptionBase if an error occurs and should stop QuoteSource.

			@remarks
			QuoteSource calls this method immediately after having instantiated adapter. QuoteSource also 
			passes a callback interface through which answers to data/permission requests are notified. Any
			exception thrown in this function causes QuoteSource to stop.

			@see IQuoteSourceCallback

			@version 6.1.0.0
			*/
			virtual void init(sophis::quotesource::adapter::IQuoteSourceCallback_H callback) throw(sophisTools::base::ExceptionBase) /*= 0*/;

			/** Start receiving market data

			@throw ExceptionBase if an error occurs and should stop QuoteSource.

			@remarks
			By returning from this function, QuoteSource begins to address requests and	it listens to Market 
			data and permissions by the callback interface. Any exception thrown in this function causes QuoteSource to stop.

			@version 6.1.0.0
			*/

			virtual void start() throw(sophisTools::base::ExceptionBase)/*= 0*/;

			/**	Stop to receive market data.

			@remarks
			Interrupt market data flow through the callback interface. Reverse the effect of {@link IUserAdapter::start}.
			Do not throw in this function.

			@version 6.1.0.0
			*/
			virtual void stop() /*= 0*/;

			/**	Dispose adapter

			@remarks
			Dispose adapter when process is to about terminate. Do not throw in this function.

			@version 6.1.0.0
			*/
			virtual void dispose() /*= 0*/;

			/** Create a sophisTag from a given fid.

			@param fid 
			The fid which is missing from default dictionary file but is used elsewhere. 				

			@param type
			Expected data type for the external field

			@return	
			A long integer which will be converted into a 4 octet sophisTag.

			@throw ExceptionBase if the field id is not defined or expected type does not conform to external field's data type


			@remarks 
			Default sophisTag/fid mapping is defined in the adapter's dictionary file aside. 
			However, fid can also be found in the database or in QuoteSource's configuration file that 
			is missing from the default dictionary. A sophisTag should be created on the fly according to 
			the given fid.

			@version 6.1.0.0
			*/

			virtual sophis::quotesource::Tag_t createTag(const  String_t& fid, sophis::quotesource::data::FieldDataType::Value type) throw(sophisTools::base::ExceptionBase) /*= 0*/;

			/** Subscribe to a topic

			@param topicKey
			Key of topic to subscribe.

			@remarks
			Subscribe to a topic provoke a market image on the topic demanded followed by periodical
			updates till {@link IUserAdapter::unsubscribe} is called. If succeed in the subscription, notify image
			by {@link IQuoteSourceCallback::notifyTopicImages} and updates by {@link IQuoteSourceCallback::notifyTopicUpdates}.
			If an error occurs, notify the error by {@link IQuoteSourceCallback::notifyTopicError} instead.
			Notification could be made either synchronously or asynchronously.

			@version 6.1.0.0
			*/
			virtual void subscribe(const sophis::quotesource::data::TopicKey_t& topicKey) /*= 0*/;

			/** Unsubscribe a topic

			@param topicKey 
			Key of topic to unsubscribe.

			@remarks
			Terminate {@link IAdapter::subscribe} on a topic.

			@version 6.1.0.0
			*/
			virtual void unsubscribe(const sophis::quotesource::data::TopicKey_t& topicKey) /*= 0*/;

			/** Demand market data image on snapshot topics for saving
			@param closure
			Correlation id for the request

			@param requests 
			Snapshot request

			@remarks
			Asking for save data. Request contains a set of topic to snap as well as an interested time interval.
			Notify data by {@link IQuoteSourceCallback::notifySnapshots} if succeed, 
			or an error by {@link IQuoteSourceCallback::notifySnapshotError}.

			@version 6.1.0.0
			*/
			virtual void requestSnapshot(Long_t closure, const sophis::quotesource::data::SnapshotRequest& requests) /*= 0*/;


			///** Request authorization on this source for an front-end application
			//@param userID 
			//External ID of an user. The ID is the string got from table RT_USERS in database

			//@param clientID 
			//Identifier of a SOPHIS front-end application instance.

			//@param userInfo 
			//A series of name/value binomials defined in front-end application's configuration file plus IPs of client machine.

			//@remarks
			//Notify the result (true/false) by {@link IQuoteSourceCallback::notifyUserAuthorization}

			//@version 6.1.0.0
			//*/		
			//virtual void requestUserAuthorization(const String_t& userID, const String_t& clientID, const sophis::quotesource::data::PropertyList& userInfo) /*= 0*/;

			///** Request on access right for real-time data on a topic

			//@param topicKey 
			//Key of topic.

			//@remarks
			//Notify a {@link ITopicAccessRight} by {@link IQuoteSourceCallback::notifyTopicAccessRights} or an error 
			//by {@link IQuoteSourceCallback::notifyTopicAccessRightsError}.

			//@version 6.1.0.0
			//*/
			//virtual void requestTopicAccessRight(const sophis::quotesource::data::TopicKey_t& topicKey) /*= 0*/;

			///**	Request on User access right

			//@param userID 
			//External ID of an user. The ID is the string got from table RT_USERS in database.

			//@remarks
			//Notify an {@link IUserAccessRight} by {@link IQuoteSourceCallback::notifyUserAccessRights}, or notify 
			//error by {@link IQuoteSourceCallback::notifyUserAccessRightsError}.

			//@version 6.1.0.0
			//*/
			//virtual void requestUserAccessRights(const String_t& userID) /*= 0*/;	

			/** Client's leaving notification

			@param clientsIdsList 
			A list of client IDs

			@version 6.1.0.0
			*/
			virtual void removeClients(const sophis::quotesource::data::ClientIdList& clientsIdsList) /*= 0*/;

			/** Source Provider Name

			@return the source provider name
			A String

			@version 7.2.0.0
			*/
			virtual String_t getProviderName() { return "CFGProviderName"; }

			/** Whether this source needs a license

			@return whether this source needs a license
			A boolean

			@version 7.2.0.0
			*/
			virtual bool isLicensedModule() { return false; }

		//------------------------------------ PUBLIC ------------------------------------
		public:
			virtual void OnMarketDataEvent(const _STL::vector<CFGQuote_H>& in_quotes);
			virtual void OnServiceStatus(bool running);
			
		//------------------------------------ PRIVATE ------------------------------------
		private:
			
			//class SourceSimu : public Runnable
			//{
			//public:
			//	SourceSimu(simuAdapter&);
			//	virtual ~SourceSimu();
			//	void init();
			//	void dispose();
			//	void start();
			//	void stop();
			//	void subscribe(const sophis::quotesource::data::TopicKey_t&);
			//	void unsubscribe(const sophis::quotesource::data::TopicKey_t&);

			//	void createImage(adapter::IQuote*);

			//	virtual void run();

			//private:
			//	simuAdapter&	 m_adapter;
			//	bool				 m_ExitRequest;
			//	SimpleThread*		 m_Publisher;

			//	_STL::vector<_STL::string> m_RandomString;

			//	HANDLE					   m_SubMutex;
			//	_STL::set<sophis::quotesource::data::TopicKey_t> m_Subscriptions;
			//};
			//friend class SourceSimu;

			sophis::quotesource::adapter::IQuoteSourceCallback_H m_QuoteSource;
	
			
			CFGSource*	m_Source;

			sophis::quotesource::thread::MutexWrapper m_SubMutex;
			_STL::map<sophis::quotesource::data::TopicKey_t,IQuote_H>	m_QuoteCacheMap;

			/** For log purpose
			@version 1.0.0.0
			*/
			static const char * __CLASS__;

		};
	}
}
#pragma pack()


#endif // !__CFGAdapter_H__
