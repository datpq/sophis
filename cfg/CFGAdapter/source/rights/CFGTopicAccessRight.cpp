/**
* System includes
*/
	/**
* Application includes
*/
#include "CFGTopicAccessRight.h"


/**
* Used Namespace
*/
using namespace sophis::quotesource;

/*
** Statics
*/
const char* CFGTopicAccessRight::__CLASS__="CFGTopicAccessRight";

/*
** Methods
*/
//-------------------------------------------------------------------------------------------------------------
/*virtual*/ void CFGTopicAccessRight::destroy() /*= 0*/
{

}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ bool CFGTopicAccessRight::equal(const adapter::ITopicAccessRight* right) /*= 0*/
{
	//all in one market
	return true;
}

//-------------------------------------------------------------------------------------------------------------
/*virtual*/ _STL::string CFGTopicAccessRight::toString() /*= 0 */
{
	_STL::string str("CFG");
	return str;
}