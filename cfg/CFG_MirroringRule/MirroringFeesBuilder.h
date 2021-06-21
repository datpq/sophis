#ifndef __MirroringFeesBuilder_H__
	#define __MirroringFeesBuilder_H__


/*
** Includes
*/
#include "SphInc/portfolio/SphMirrorTransactionBuilder.h"
#include "SphInc/portfolio/SphTransaction.h"

/**
Class MirroringFeesBuilder:
Interface to create a new building method in Mirror Rules Definitions
Only available with mirroring module.
*/
class MirroringFeesBuilder : public sophis::portfolio::CSRMirrorTransactionBuilder
{
//------------------------------------ PUBLIC ---------------------------------
public:

	// this function is to be used to overload how the specified mirrored transaction is to be created/dealt with.
	virtual void generate(const sophis::portfolio::CSRTransaction* mainMvt, sophis::portfolio::CSRTransaction* mirrorMvt) const
							throw (sophis::portfolio::CSRMirrorTransactionBuilderException) /*= 0*/;

//------------------------------------ PRIVATE --------------------------------
private:

	static const char* __CLASS__;
};

#endif