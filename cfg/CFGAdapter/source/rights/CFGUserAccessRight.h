#ifndef __CFGUserAccessRight_H__
#define __CFGUserAccessRight_H__

/**
* System includes
*/
#include "SphQSInc/SphAccessRight.h"

#pragma pack(8)

namespace sophis
{
	namespace quotesource
	{
		/** Right for an user to get market data.
		*/
		class CFGUserAccessRight : public adapter::IUserAccessRight
		{	
		//------------------------------------ PUBLIC ------------------------------------
		public:
			/** Dispose method

			@remarks 
			After this call, QuoteSource no longer operates on the instance, thus you can release it safely.

			@version 5.3.3.2
			*/
			virtual void destroy() /*= 0*/;

			/** Test if this user has right to receive market data with this ITopicAccessRight.

			@param right
			Topic access right to compare with.

			@return 
			true if this user has right to receive market data on topics with this {@link ITopicAccessRight}, false if not

			@version 5.3.3.2
			*/
			virtual bool hasRight(const adapter::ITopicAccessRight* right) /*= 0*/;
		
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

#endif // !__CFGUserAccessRight_H__
