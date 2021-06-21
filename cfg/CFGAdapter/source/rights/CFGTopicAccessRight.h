#ifndef __CFGTopicAccessRight_H__
#define __CFGTopicAccessRight_H__

/**
* System includes
*/
#include "SphInc/SphMacros.h"
#include __STL_INCLUDE_PATH(string)
#include "SphQSInc/SphAccessRight.h"

/**
* Application includes
*/


#ifdef WIN32
#	pragma warning(push)
#	pragma warning(disable:4275) // Can not export a class derivated from a non exported one
#	pragma warning(disable:4251) // Can not export a class agregating a non exported one
#endif


#pragma pack(8)

namespace sophis
{
	namespace quotesource
	{
		/** topic access right limits access to market data from non granted users.
		*/
		class CFGTopicAccessRight : public adapter::ITopicAccessRight
		{	
		//------------------------------------ PUBLIC ------------------------------------
		public:
			/** Dispose method.

			@remarks 
			After this call, QuoteSource no longer operates on the instance, thus you can release it safely.

			@version 5.3.3.2
			*/
			virtual void destroy() /*= 0*/;

			/** Compare two topic if they have equivalent access right.

			@param right 
			topic access right to compare with.

			@return 
			true if two rights are equivalent, false if not.

			@version 5.3.3.2
			*/
			virtual bool equal(const adapter::ITopicAccessRight* right) /*= 0*/;

			/**String representation of a topic access right.

			@return 
			A string representation.

			@remark	
			It is displayed in SOPHIS CONSOLE as external market id.

			@version 5.3.3.2
			*/
			virtual _STL::string toString() /*= 0 */;

		//------------------------------------ PRIVATE ------------------------------------
		private:

			/** For log purpose
			@version 1.0.0.0
			*/
			static const char * __CLASS__;
		};
	}
}// end of namespace

#pragma pack()

#ifdef WIN32
#	pragma warning(pop)
#endif

#endif // !__CFGTopicAccessRight_H__
