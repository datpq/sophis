#ifndef __CSxISInterfacesInstrumentAction_H__
#define __CSxISInterfacesInstrumentAction_H__

/*
** Includes
*/
#include "SphInc/instrument/SphInstrumentAction.h"

/*
** Class
*/
class CSxISInterfacesInstrumentAction : public sophis::instrument::CSRInstrumentAction
{
	DECLARATION_INSTRUMENT_ACTION(CSxISInterfacesInstrumentAction);

//------------------------------------ PUBLIC ---------------------------------
public:

	/** Ask for a creation of an instrument.
	When creating, all of the triggers will be called via VoteForCreation, to check whether they accept the creation in the order eOrder + lexicographical order.
	@param instrument is the instrument to create. It is a non-const object so it can
	be modified. The instrument ID is not created yet.
	@throws VoteException if you reject that creation.
	*/
	virtual void VoteForCreation(CSRInstrument &instrument)
		throw (tools::VoteException , sophisTools::base::ExceptionBase);

	/** Ask for a modification of an instrument.
	When modifying, all of the triggers will be called via VoteForModification to check if they accept the
	modification in the order eOrder + lexicographical order.
	@param instrument is the instrument to modify. It is a non-const object so it can
	be modified. To get the original instrument, do gApplicationContext->GetCSRInstrument(instrument.GetCode()).
	@throws VoteException if you reject that modification.
	*/
	virtual void VoteForModification(CSRInstrument &instrument, NSREnums::eParameterModificationType type)
		throw (tools::VoteException, sophisTools::base::ExceptionBase);

	void UpdateISInterfacesExternalReferences(CSRInstrument &instrument, const char* actionTypeValue, const char* integrStatusValue);

	void GetCFGExternalReference(CSRInstrument &instrument,char* val);

};

#endif //!CSxISInterfacesInstrumentAction_H__