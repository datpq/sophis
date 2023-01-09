#pragma once
#include "CSxForexUtils.h"
#using <mscorlib.dll>

namespace ForexUtils
{
	public ref class CLIForexUtils
	{
		//static System::Collections::Generic::Dictionary<String^, long>^ GetAccountFundMap();
		//int sicovam, double coefficient, int businessEvent, int exDivDate, int paymentDate
	public:
		static System::Boolean IsFXPairReversed(int leftCcy, int rightCcy)
		{
			System::Boolean retval = CSxForexUtils::IsFXPairReversed(leftCcy, rightCcy);
			return retval;
		}

		static System::Int32 GetFWDInstrCode(System::Int32 leftCcy, System::Int32 rightCcy, System::Int32 date)
		{
			System::Int32 retval = CSxForexUtils::GetFWDInstrCode(leftCcy, rightCcy, date);
			return retval;
		}

		static System::Int32 GetNDFInstrCode(System::Int32 leftCcy, System::Int32 rightCcy, System::Int32 date)
		{
			System::Int32 retval = CSxForexUtils::GetNDFInstrCode(leftCcy, rightCcy, date);
			return retval;
		}

		CLIForexUtils(void);
		~CLIForexUtils(void);
	};



	
		
	CLIForexUtils::CLIForexUtils(void)
	{

	}


	CLIForexUtils::~CLIForexUtils(void)
	{

	}
}

