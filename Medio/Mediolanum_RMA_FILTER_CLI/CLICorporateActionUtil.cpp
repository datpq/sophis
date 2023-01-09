#pragma once
#include "CSxCorporateActionUtil.h"
#using <mscorlib.dll>
using namespace System;
using namespace System::Collections::Generic;
using namespace System::Text;
using namespace System::Runtime::InteropServices;

namespace CorporateActionUtil
{
	public ref class CLICorporateActionUtil
	{
		//static System::Collections::Generic::Dictionary<String^, long>^ GetAccountFundMap();
		//int sicovam, double coefficient, int businessEvent, int exDivDate, int paymentDate
	public:
		static System::Boolean^ CreateFreeAttributionWrapper(System::Int32 sicovam, System::Double coefficient, System::Int32 businessEvent, System::Int32 exDivDate, System::Int32 date, System::Int32 paymentDate, System::String^ comment);
		static System::Boolean^ CreateMergerWrapper(System::Int32 sicovam, System::Double take_over, System::Double coefficient, System::Double convRatioNum, System::Int32 convRatioDenom, System::Int32 businessEvent, System::Int32 exDivDate, System::Int32 date, System::Int32 paymentDate, System::Boolean withAveragePrice, System::String^ comment);
		static System::Boolean^ CreateDemergerWrapper(System::Int32 sicovam, System::Int32 diffused_code, System::Double coefficient, System::Double convRatioNum, System::Int32 convRatioDenom, System::Int32 businessEvent, System::Int32 exDivDate, System::Int32 date, System::Int32 paymentDate, System::String^ comment);
		CLICorporateActionUtil(void);
		~CLICorporateActionUtil(void);
	};

	String^ MarshalNativeToManaged(const char* str)
	{
		String^ strNew = gcnew String(str);
		return strNew;
	}

	char* MarshalManagedToNative(String^ managedStr)
	{
		char* retval = NULL;
		IntPtr ip = Marshal::StringToHGlobalAnsi(managedStr);  
		const char* str = static_cast<const char*>(ip.ToPointer());  

		retval = new char[strlen(str)+1];
		strcpy(retval,str);

		Marshal::FreeHGlobal( ip );  

		return retval;
	}

	System::Boolean^ CLICorporateActionUtil::CreateFreeAttributionWrapper(System::Int32 sicovam, System::Double coefficient, System::Int32 businessEvent, System::Int32 exDivDate, System::Int32 date, System::Int32 paymentDate, System::String^ comment)
	{
		char* nativeComment = MarshalManagedToNative(comment);
		System::Boolean^ retval = CSxCorporateActionUtil::CreateFreeAttribution(sicovam,coefficient,businessEvent,exDivDate,date,paymentDate,nativeComment);
		return retval;
	}

	System::Boolean^ CLICorporateActionUtil::CreateMergerWrapper(System::Int32 sicovam, System::Double take_over, System::Double coefficient, System::Double convRatioNum, System::Int32 convRatioDenom, System::Int32 businessEvent, System::Int32 exDivDate, System::Int32 date, System::Int32 paymentDate, System::Boolean withAveragePrice, System::String^ comment)
	{
		char* nativeComment = MarshalManagedToNative(comment);
		System::Boolean^ retval = CSxCorporateActionUtil::CreateMerger(sicovam,take_over,coefficient,convRatioNum,convRatioDenom,businessEvent,exDivDate,date,paymentDate,nativeComment);
		return retval;
	}

	System::Boolean^ CLICorporateActionUtil::CreateDemergerWrapper(System::Int32 sicovam, System::Int32 diffused_code, System::Double coefficient, System::Double convRatioNum, System::Int32 convRatioDenom, System::Int32 businessEvent, System::Int32 exDivDate, System::Int32 date, System::Int32 paymentDate, System::String^ comment)
	{
		char* nativeComment = MarshalManagedToNative(comment);
		System::Boolean^ retval = CSxCorporateActionUtil::CreateDemerger(sicovam,diffused_code,coefficient,convRatioNum,convRatioDenom,businessEvent,exDivDate,date,paymentDate,nativeComment);
		return retval;
	}

	CLICorporateActionUtil::CLICorporateActionUtil(void)
	{

	}


	CLICorporateActionUtil::~CLICorporateActionUtil(void)
	{

	}
}