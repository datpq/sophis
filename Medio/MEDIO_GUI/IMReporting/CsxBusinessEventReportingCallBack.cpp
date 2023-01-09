
/*
** Includes
*/
// specific
#include "SphInc/misc/ConfigurationFileWrapper.h"
#include "SphInc/backoffice_kernel/SphBackOfficeKernel.h"
#include "SphTools/SphLoggerUtil.h"
#include "CsxBusinessEventReportingCallBack.h"

using namespace sophis::backoffice_kernel;
using namespace sophis::portfolio;

const char* CsxBusinessEventReportingColumnComputer::__CLASS__ = "CsxBusinessEventReportingColumnComputer";

long CsxBusinessEventReportingColumnComputer::fBusinessEventGroup = -1;

CsxBusinessEventReportingColumnComputer::CsxBusinessEventReportingColumnComputer()
{
	BEGIN_LOG("CsxBusinessEventReportingColumnComputer");
	_STL::string businessEventGroup = "";
	try
	{
		ConfigurationFileWrapper::getEntryValue("IMReporting", 
												"BusinessEventGroupName", 
												businessEventGroup);

		ISRBOKernelEnvironment::KBEGroups grps = gBOKernelEnvironment->GetAllKernBusinessEventsGroups();
		bool hasFound = false;
		for(ISRBOKernelEnvironment::KBEGroups::const_iterator itr = grps.begin();itr!=grps.end();++itr)
		{
			if(businessEventGroup.compare(itr->second.name) == 0)
			{
				hasFound = true;
				fBusinessEventGroup = itr->first;
				break;
			}
		}
		if (hasFound == false)
			throw sophisTools::base::GeneralException(FROM_STREAM("Unknown business event group: " << businessEventGroup));

	}
	catch(...)
	{
		LOG(Log::warning, "IMReporting/BusinessEventGroupName is not defined in .config file. IM columns will not be computed.");
		fBusinessEventGroup = -1;
	}
	
	END_LOG();
	
}

bool CsxBusinessEventReportingColumnComputer::GetValue(SxDataDouble& value, PSRExtraction extraction, long positionId, const CSRTransaction& trade)
{
	if( gBOKernelEnvironment->IsBusinessEventInGroup(trade.GetTransactionType(), fBusinessEventGroup)  )
	{
		value.data = trade.GetNetAmount();
		return true;
	}
	return false;
}


bool CsxBusinessEventReportingColumnComputer::AggregateValue(SxDataDouble& value, const _STL::vector<SxDataDouble> &buffer, PSRExtraction extraction, long positionId, long folioId, long instrumentCode, long positionCCY)
{
	_STL::vector<SxDataDouble>::const_iterator ite = buffer.begin();
	while(ite != buffer.end())
	{
		value.data += ite->data;
		ite++;
	}
	return true;
}

bool CsxBusinessEventReportingColumnComputer::AggregateValueOnTheFly(SxDataDouble& value, PSRExtraction extraction, long positionId, const CSRTransaction& trade, CSRReportingCallback::Direction factor)
{
	if( gBOKernelEnvironment->IsBusinessEventInGroup(trade.GetTransactionType(), fBusinessEventGroup)  )
	{
		value.data += trade.GetNetAmount();
		return true;
	}
	return false;
}
															  
bool CsxBusinessEventReportingColumnComputer::AggregateValueForFlat(SxDataDouble& value, const SxDataDouble& hierValue, PSRExtraction extraction, long positionId, long folioId, long instrumentCode, sophis::instrument::ePositionType lineType) // , long metaModelId)
{
	value.data += hierValue.data;
	return true;
}
