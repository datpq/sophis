#include "PollingThread.h"

using namespace System::Threading;
using namespace sophis::quotesource::thread;


//-----------------------------------------------------------------
MutexWrapper::MutexWrapper(){
	_mutex = gcnew Mutex();
}

//-----------------------------------------------------------------
MutexWrapper::~MutexWrapper(){
	_mutex = nullptr;
}

//-----------------------------------------------------------------
void MutexWrapper::lock(){
	_mutex->WaitOne();
}

//-----------------------------------------------------------------
void MutexWrapper::unlock(){
	_mutex->ReleaseMutex();
}

//-----------------------------------------------------------------
PollingThread::PollingThread(Runnable* in_callback, Long_t in_interval)
:_target(in_callback),_interval(in_interval),_exitflag(false){
	_thread = gcnew Thread(gcnew ThreadStart(this,&PollingThread::run));
	_waithandle = gcnew AutoResetEvent(false);
}

//-----------------------------------------------------------------
PollingThread::~PollingThread(){
	stop();
	_thread = nullptr;
	_waithandle = nullptr;
}

//-----------------------------------------------------------------
void PollingThread::start(){
	if(!_thread->IsAlive){
		_thread->Start();
	}
}

//-----------------------------------------------------------------
void PollingThread::stop(){
	if(_thread->IsAlive){
		_exitflag = true;
		_waithandle->Set();
		_thread->Join();
	}
}

//-----------------------------------------------------------------
void PollingThread::run(){
	while(!_exitflag){
		try{
			_target->run();
		}catch(const sophisTools::base::ExceptionBase&){}
		catch(...){}
		_waithandle->WaitOne((int)_interval);
	}
}
