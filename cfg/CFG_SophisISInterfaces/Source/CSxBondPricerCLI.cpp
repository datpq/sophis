#pragma warning(disable:4251)

#include <vcclr.h>

#include "SphInc/SphMacros.h"
#include "SphTools/SphLoggerUtil.h"
#include __STL_INCLUDE_PATH(string)
#include __STL_INCLUDE_PATH(vector)
#include "SphInc/instrument/SphInstrument.h"
#include "SphInc/market_data/SphMarketData.h"

#include "CSxBondPricer.h"

#using <mscorlib.dll>
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

void MarshalNetToStlString(System::String^ s, _STL::string& os)
{
	using System::IntPtr;
	using System::Runtime::InteropServices::Marshal;

	const char* chars = (const char*)(Marshal::StringToHGlobalAnsi(s)).ToPointer( );
	os = chars;
	Marshal::FreeHGlobal(IntPtr((void*)chars));
}

/*static*/ const char* __CLASS__ = "CSxBondPricerCLI";

//C++/CLI wrapper
public ref class CSxBondPricerCLI
{	
public:			

	System::Double GetSensitivity(System::Int32 instrumentCode, System::String^ sensitivityUserColumnName)
	{
		BEGIN_LOG("GetSensitivity");

		System::Double ret = 0;

		try
		{
			//instrument code
			int instrumentCodeCpp = safe_cast<int>(instrumentCode);
			
			// sensitivity user column name
			_STL::string sensitivityUserColumnNameCpp = "";
			MarshalNetToStlString(sensitivityUserColumnName, sensitivityUserColumnNameCpp);			

			double retCpp = CSxBondPricer::GetInstance()->GetSensitivity(instrumentCodeCpp,sensitivityUserColumnNameCpp.c_str());
			ret = safe_cast<double>(retCpp);
		}
		catch(sophisTools::base::ExceptionBase &ex)
		{		
			_STL::string mess = FROM_STREAM("Error (" << ex << ") in GetSensitivity method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess); 
		}
		catch(...)
		{	
			_STL::string mess = FROM_STREAM("Unhandled exception occured in GetSensitivity method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess);
		}

		END_LOG();

		return ret;
	}

	System::Double GetSensitivity(sophis::instrument::CSMInstrument^ instrument, System::Double dirtyPrice)
	{
		BEGIN_LOG("GetSensitivity");

		System::Double ret = 0;

		try
		{	
			// dirty price
			double dirtyPriceCpp = safe_cast<double>(dirtyPrice);			
			
			double retCpp = CSxBondPricer::GetInstance()->GetSensitivity(instrument->m_pInternalPtr, dirtyPriceCpp, *gApplicationContext);
			
			ret = safe_cast<double>(retCpp);
		}
		catch(sophisTools::base::ExceptionBase &ex)
		{		
			_STL::string mess = FROM_STREAM("Error (" << ex << ") in GetSensitivity method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess); 
		}
		catch(...)
		{	
			_STL::string mess = FROM_STREAM("Unhandled exception occured in GetSensitivity method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess);
		}

		END_LOG();

		return ret;
	}
	
	void ComputeTheoretical(System::String^ instrRef, System::Int32 date, System::String^ curveFamilyName, System::Int32 pricingModel, 
							System::Int32 quotationType, System::Double spread, List<System::String^>^ maturitiesList, List<System::Double>^ ratesList, 
							System::Double% price, System::Double% ytm, System::Double% accrued, System::Double% duration, System::Double% sensitivity)
	{
		BEGIN_LOG("ComputeTheoretical");
		
		try
		{
			//instrument reference
			_STL::string instrRefCpp = "";
			MarshalNetToStlString(instrRef, instrRefCpp);

			//date
			int dateCpp = safe_cast<int>(date);			

			// curve family name
			_STL::string curveFamilyNameCpp = "";
			MarshalNetToStlString(curveFamilyName, curveFamilyNameCpp);

			//pricing model
			int pricingModelCpp = safe_cast<int>(pricingModel);

			//quotation type
			int quotationTypeCpp = safe_cast<int>(quotationType);

			//spread
			double spreadCpp = safe_cast<double>(spread);

			//list of maturities
			_STL::vector<_STL::string> listOfmaturitiesCpp;

			_STL::string maturity = "";
			for each(System::String^ str in maturitiesList)
			{
				MarshalNetToStlString(str, maturity);			
				listOfmaturitiesCpp.push_back(maturity);
			}

			//list of rates
			_STL::vector<double> listOfRatesCpp;
			double rate = 0;

			for each(System::Double oneRate in ratesList)
			{						
				rate = safe_cast<double>(oneRate);
				listOfRatesCpp.push_back(rate);
			}

			double priceCpp = 0;
			double ytmCpp = 0;
			double accruedCpp = 0;
			double durationCpp = 0;
			double sensitivityCpp = 0;			

			CSxBondPricer::GetInstance()->ComputeTheoretical(instrRefCpp.c_str(),dateCpp,curveFamilyNameCpp.c_str(),pricingModelCpp,quotationTypeCpp,
														spreadCpp,listOfmaturitiesCpp,listOfRatesCpp,priceCpp,ytmCpp,accruedCpp,durationCpp,sensitivityCpp);		

			price = safe_cast<double>(priceCpp);
			ytm = safe_cast<double>(ytmCpp);
			accrued = safe_cast<double>(accruedCpp);
			duration = safe_cast<double>(durationCpp);
			sensitivity = safe_cast<double>(sensitivityCpp);			
		}		
		catch(sophisTools::base::ExceptionBase &ex)
		{		
			_STL::string mess = FROM_STREAM("Error (" << ex << ") in ComputeTheoretical method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess); 
		}
		catch(...)
		{	
			_STL::string mess = FROM_STREAM("Unhandled exception occured in ComputeTheoretical method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess);
		}

		END_LOG();
	}

	System::Double GetPriceFromYTM(System::String^ instrRef, System::Int32 date, System::Int32 pricingModel, System::Int32 quotationType, System::Double ytm)
	{
		BEGIN_LOG("GetPriceFromYTM");

		System::Double ret = 0;

		try
		{
			//instrument reference
			_STL::string instrRefCpp = "";
			MarshalNetToStlString(instrRef, instrRefCpp);

			//date
			int dateCpp = safe_cast<int>(date);			
			
			//pricing model
			int pricingModelCpp = safe_cast<int>(pricingModel);

			//quotation type
			int quotationTypeCpp = safe_cast<int>(quotationType);

			//ytm
			double ytmCpp = safe_cast<double>(ytm);			

			double priceCpp = 0;						

			priceCpp = CSxBondPricer::GetInstance()->GetPriceFromYTM(instrRefCpp.c_str(),dateCpp,pricingModelCpp,quotationTypeCpp,ytmCpp);		

			ret = safe_cast<double>(priceCpp);						
		}		
		catch(sophisTools::base::ExceptionBase &ex)
		{		
			_STL::string mess = FROM_STREAM("Error (" << ex << ") in GetPriceFromYTM method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess); 
		}
		catch(...)
		{	
			_STL::string mess = FROM_STREAM("Unhandled exception occured in GetPriceFromYTM method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess);
		}

		END_LOG();

		return ret;
	}

	System::Double GetYTMFromPrice(System::String^ instrRef, System::Int32 date, System::Int32 pricingModel, System::Int32 quotationType, System::Double price)
	{
		BEGIN_LOG("GetYTMFromPrice");

		System::Double ret = 0;

		try
		{
			//instrument reference
			_STL::string instrRefCpp = "";
			MarshalNetToStlString(instrRef, instrRefCpp);

			//date
			int dateCpp = safe_cast<int>(date);			

			//pricing model
			int pricingModelCpp = safe_cast<int>(pricingModel);

			//quotation type
			int quotationTypeCpp = safe_cast<int>(quotationType);

			//ytm
			double priceCpp = safe_cast<double>(price);			

			double ytmCpp = 0;						

			ytmCpp = CSxBondPricer::GetInstance()->GetYTMFromPrice(instrRefCpp.c_str(),dateCpp,pricingModelCpp,quotationTypeCpp,priceCpp);		

			ret = safe_cast<double>(ytmCpp);						
		}		
		catch(sophisTools::base::ExceptionBase &ex)
		{		
			_STL::string mess = FROM_STREAM("Error (" << ex << ") in GetYTMFromPrice method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess); 
		}
		catch(...)
		{	
			_STL::string mess = FROM_STREAM("Unhandled exception occured in GetYTMFromPrice method");			
			MESS(Log::error, mess.c_str());
			const char* buffer = mess.data();
			System::String^ refMess = System::Runtime::InteropServices::Marshal::PtrToStringAnsi((System::IntPtr)(char*)buffer);

			END_LOG();
			throw gcnew System::Exception(refMess);
		}

		END_LOG();

		return ret;
	}
};
