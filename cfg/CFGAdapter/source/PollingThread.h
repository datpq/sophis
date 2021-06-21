#ifndef __POLLING_THREAD_H__
#define __POLLING_THREAD_H__

//#include "SphQSInc/SphDataDef.h"
#include "../SphDataDef.h"
#include "gcroot.h"

namespace sophis{
	namespace quotesource{
		namespace thread{
			class MutexWrapper{
			public:
				MutexWrapper();
				~MutexWrapper();

				void lock();
				void unlock();

			private:
				gcroot<System::Threading::Mutex^> _mutex;
			};

			struct SimpleSync{
				SimpleSync(MutexWrapper& in_mutex):_mutex(in_mutex){ _mutex.lock(); }
				~SimpleSync(){ _mutex.unlock(); }

			private:
				MutexWrapper& _mutex;
			};

			//runnable interface
			struct Runnable{
				virtual void run() = 0;
				virtual ~Runnable() {}
			};

			ref class PollingThread{
			public:
				PollingThread(Runnable* callback, Long_t in_interval);
				~PollingThread();

				void start();
				void stop();
				void run();

			private:
				Long_t _interval;
				Runnable* _target;
				volatile bool _exitflag;
				System::Threading::Thread^ _thread;
				System::Threading::AutoResetEvent^ _waithandle; 
			};
		}
	}
}
#endif