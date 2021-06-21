/**
* System includes
*/

/**
* Application includes
*/
#include "CFGUserAccessRight.h"


/**
* Used Namespace
*/
using namespace sophis::quotesource;


/*
** Statics
*/
const char* CFGUserAccessRight::__CLASS__="CFGUserAccessRight";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGUserAccessRight::destroy() /*= 0*/
{

}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CFGUserAccessRight::hasRight(const adapter::ITopicAccessRight* right) /*= 0*/
{
	return true;
}