#ifdef _WIN32
	#pragma once // Speeds up VC++ compilation
#endif

#ifndef __SOPHIS_VALUE_UCITS_DATA_H__
#define __SOPHIS_VALUE_UCITS_DATA_H__


// Toolkit includes
#include "SphInc/value/kernel/SphAssetManagementExports.h"
#include "SphInc/finance/SphProductType.h"
#include "SphInc/finance/SphProductFeature.h"
#include "SphLLInc\cdc.h"


#include __STL_INCLUDE_PATH(map)
#include __STL_INCLUDE_PATH(vector)

namespace sophis
{
	namespace instrument	
	{
		class CSRSwap;
		class CSRBond;
		class CSROption;
		class CSRCapFloor;
		class CSRFuture;
		class CSRLoanAndRepo;
		class CSRForexSpot;
		class CSREquity;
		class CSRForexFuture;
		class CSRPackage;
		class CSRDebtInstrument;
		class CSRCommission;
		class CSRContractForDifference;
	}

	namespace portfolio
	{
		template < class X >
		class  InstrumentTypesAndFeatures
		{
			public:
				
				static _STL::string getProductType(const X& instrument)
				{
						CSRProductType::prototype &product_list = CSRProductType::GetPrototype();
						CSRProductType::prototype::iterator scan;

						_STL::string productNameList = "";
						for(scan = product_list.begin(); scan != product_list.end(); ++scan)
						{
							const char* productName = scan->first;
							CSRProductType* productType = scan->second;
							try
							{
								if(productType->BelongsTo(instrument) && strncmp(productName, "Any", 3) != 0)
								{
									productNameList += '|';
									productNameList += productName;
								}
							}
							catch(...)
							{
							}

			
						}
					return productNameList;
				}

				static _STL::string getFeature(const X& instrument)
				{
						CSRProductFeature::prototype &feature_list = CSRProductFeature::GetPrototype();
						CSRProductFeature::prototype::iterator scan;

						_STL::string productFeatureList = "";
						for(scan = feature_list.begin(); scan != feature_list.end(); ++scan)
						{
							const char* productFeatureName = scan->first;
							CSRProductFeature* productFeature = scan->second;
							try
							{
								if(productFeature->BelongsTo(instrument) && strncmp(productFeatureName, "Any", 3) != 0)
								{
									productFeatureList += '|';
									productFeatureList += productFeatureName;
								}
							}
							catch(...)
							{
							}
			
						}
					return productFeatureList;
				}

	
				static bool matchProduct(const X& instrument, _STL::string& product)
				{
					CSRProductType::prototype &product_list = CSRProductType::GetPrototype();
					return product_list.GetData(product.c_str())->BelongsTo(instrument);
				}

				static bool matchFeature(const X& instrument, _STL::string& feature)
				{
					CSRProductFeature::prototype &feature_list = CSRProductFeature::GetPrototype();
					return feature_list.GetData(feature.c_str())->BelongsTo(instrument);
				}

				static bool matchProducts(const X& instrument, _STL::list<_STL::string>& products)
				{

					if (products.size() <= 0)
						return true;
	
					CSRProductType::prototype &product_list = CSRProductType::GetPrototype();

					_STL::list<_STL::string>::const_iterator it = products.begin();
					for(it = products.begin(); it !=  products.end(); it++)
					{
						if (product_list.GetData((*it).c_str())->BelongsTo(instrument) == false)
							return false;
					}
		
					return true;
				}

				static bool matchFeatures(const X& instrument, _STL::list<_STL::string>& features)
				{

					if (features.size() <= 0)
						return true;
	
					CSRProductFeature::prototype &feature_list = CSRProductFeature::GetPrototype();

					_STL::list<_STL::string>::const_iterator it = features.begin();
					for(it = features.begin(); it !=  features.end(); it++)
					{
						if (feature_list.GetData((*it).c_str())->BelongsTo(instrument) == false)
							return false;
					}
		
					return true;
				}

		};
	}

	
}


#endif // __SOPHIS_VALUE_UCITS_DATA_H__