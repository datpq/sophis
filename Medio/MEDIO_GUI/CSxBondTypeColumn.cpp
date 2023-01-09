

#include "CSxBondTypeColumn.h"
#include "SphTools/SphLoggerUtil.h"


#include "Sphinc\static_data\SphCurrency.h"

#include "Sphinc\instrument\SphInstrument.h"
#include "Sphinc\instrument\SphEquity.h"
#include "SphInc\instrument\SphBond.h"
#include "Sphinc\portfolio\SphPortfolio.h"
#include "Sphinc\market_data\SphMarketData.h"
#include "SphTools\SphExceptions.h"
#include "SphSDBCInc\queries\SphQuery.h"
#include "Sphinc\misc\ConfigurationFileWrapper.h"

const char* CSxBondTypeColumn::__CLASS__="CSxBondTypeColumn";

_STL::map<long,_STL::string> instrument_Type;

long refreshVersion =-1;

_STL::string parentSector="";

// CONSTRUCTOR

	CSxBondTypeColumn::CSxBondTypeColumn()
	{
		ConfigurationFileWrapper::getEntryValue("MEDIO_BOND_TYPE", "PARENT_SECTOR_NAME",parentSector,"BOND CLASSIFICATION");
		fGroup ="Toolkit";
	}

	void CSxBondTypeColumn::GetPortfolioCell(long				activePortfolioCode,
		long				portfolioCode,
		PSRExtraction		extraction,
		SSCellValue			*cellValue,
		SSCellStyle			*cellStyle,
		bool				onlyTheValue) const
	{
		BEGIN_LOG(__FUNCTION__);
		//nothing to show at folio level
		END_LOG();
	}

	void CSxBondTypeColumn::GetPositionCell(const CSRPosition&	position,
		long				activePortfolioCode,
		long				portfolioCode,
		PSRExtraction		extraction,
		long				underlyingCode,
		long				instrumentCode,
		SSCellValue			*cellValue,
		SSCellStyle			*cellStyle,
		bool				onlyTheValue) const
	{

		BEGIN_LOG(__FUNCTION__);


		cellStyle->kind =NSREnums::dNullTerminatedString;

		try 
		{

			if(!onlyTheValue)
			{
				cellStyle->alignment = aRight;
				cellStyle->null		 = nvZeroAndUndefined;
				long ccyCode = position.GetCurrency();
				if(ccyCode)
				{
				  const CSRCurrency * curr = CSRCurrency::GetCSRCurrency(ccyCode);
				  if (curr)
					   curr->GetRGBColor(&cellStyle->color);
				}
			}

			if(refreshVersion != CSRPortfolioColumn::GetRefreshVersion())
			{
				refreshVersion = CSRPortfolioColumn::GetRefreshVersion();
				refreshCache();
			}

			_STL::string bondType="";

			_STL::map<long,_STL::string>::iterator iter = instrument_Type.find(instrumentCode);
			if (iter==instrument_Type.end())
			{
				// check if the instrument in position is a bond:
				const CSRInstrument* inst = CSRInstrument::GetInstance(instrumentCode);

				if(inst)
				{
					char instrumentType=inst->GetType();

					if(instrumentType == 'O')
					{
						// Check if the bond has external reference CALC_TYP_DES populated

						SSComplexReference complexRef;
						//sprintf_s(complexRef.type, "CALC_TYP_DES");
						//sprintf_s(complexRef.value, "");
						complexRef.type = "CALC_TYP_DES";
						complexRef.value = "";
						if (inst->GetClientReference(instrumentCode, &complexRef))
						{
							//if So, sector is missing...
							bondType="MISSING";
						}
					}
				}
			}
			else
			{
				bondType=instrument_Type[instrumentCode];
			}

			strcpy_s(cellValue->nullTerminatedString, 256,bondType.c_str());
		}
		catch(...)
		{
		}
		END_LOG();
	}


		void			CSxBondTypeColumn::GetUnderlyingCell(	long				activePortfolioCode,
							long				portfolioCode,
							PSRExtraction		extraction,
							long				underlyingCode,
							SSCellValue			*cellValue,
							SSCellStyle			*cellStyle,
							bool				onlyTheValue) const
		{
			BEGIN_LOG(__FUNCTION__);

			END_LOG();
		}

		void CSxBondTypeColumn::refreshCache(void)
		{
			long sicovam;
			char type[41] = { '\0' };
			const char* parentSectorCstr = parentSector.c_str();
			
		
			try
			{
				instrument_Type.clear();

				CSRQuery getSicoAndType;
				getSicoAndType.SetName("GetSicoAndType");
				getSicoAndType << "SELECT "<< CSROut("E.SOPHIS_IDENT",sicovam) << "," << CSROut::FromStr("P.NAME",type) << 
					" FROM EXTRNL_REFERENCES_INSTRUMENTS E INNER JOIN EXTRNL_REFERENCES_DEFINITION D ON D.REF_NAME = 'CALC_TYP_DES' "
				   <<" AND E.REF_IDENT = D.REF_IDENT INNER JOIN SECTORS S  ON S.NAME   = E.VALUE INNER JOIN SECTORS P  ON P.ID     = S.PARENT AND "
				   << "P.PARENT = (select ID From sectors where name="<<CSRInRef::FromStr(parentSectorCstr)<<")";


				//CSRInRef::FromStr(parentSector.c_str())

				while (getSicoAndType.Fetch())
				{
					instrument_Type[sicovam] = type;
				}
			}
			catch(...)
			{
			}
		}
