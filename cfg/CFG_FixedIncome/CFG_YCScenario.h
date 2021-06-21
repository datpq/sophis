#pragma once

#ifndef __CFG_YCScenario__H__
	#define __CFG_YCScenario__H__

/*
** Includes
*/
// standard
#include "SphInc/scenario/SphScenario.h"

/*
** Class
* Basic class of all Sophis scenarios.
* This class may be overloaded by Toolkit programmers in order to integrate new scenarios in RISQUE.
* Scenarios take benefit of CalculationServer : they can be computed remotely on Calculators.
*/
class CFG_YCScenario : public sophis::scenario::CSRScenario
{
//------------------------------------ PUBLIC ------------------------------------
public:

	DECLARATION_SCENARIO(CFG_YCScenario);

	/**
	* Returns the type of the scenario, it has no effect on the way it works
	* but only on the context in which it can be launched.
	* By default, it is pUserPreference
	*/
	virtual sophis::scenario::eProcessingType	GetProcessingType() const override;

	/**
	* To run your scenario (see "How to Create a Scenario"), this method is mandatory otherwise RISQUE
	* will not do anything.
	*/
	virtual	void Run() override;

//------------------------------------ PRIVATE --------------------------------
private:
	static int yCurveIdent;
	// For log purpose
	static const char* __CLASS__;

};

#endif // !__CFG_YCScenario__H__
