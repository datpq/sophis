#ifndef __CFGObligationsMirroringRule_H__
#define __CFGObligationsMirroringRule_H__


/*
** Includes
*/
#include "SphInc/portfolio/SphMirrorTransactionBuilder.h"
#include "SphInc/portfolio/SphTransaction.h"

/**
Class CFGObligationsMirroringRule.cpp:
Interface to create a new building method in Mirror Rules Definitions
Only available with mirroring module.
*/
class CFGObligationsMirroringRule : public sophis::portfolio::CSRMirrorTransactionBuilder
{
//------------------------------------ PUBLIC ---------------------------------
public:

	// this function is to be used to overload how the specified mirrored transaction is to be created/dealt with.
	virtual void generate(const sophis::portfolio::CSRTransaction* mainMvt, sophis::portfolio::CSRTransaction* mirrorMvt) const
							throw (sophis::portfolio::CSRMirrorTransactionBuilderException);

	void SetTvaAmount(sophis::portfolio::CSRTransaction * transaction, const double fieldAmount, const int eTvaField, const double rate) const;

//------------------------------------ PRIVATE --------------------------------
private:
	// Logger data
	static const char * __CLASS__;
};

#endif