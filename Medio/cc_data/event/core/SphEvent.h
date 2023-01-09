#ifndef _SOPHIS_EVENT_EVENT_H
#define _SOPHIS_EVENT_EVENT_H

#include "SphTools/base/CommonOS.h"
#include "SphEventCoreExport.h"
#include "SphField.h"

#include <errno.h>
#include <stddef.h>

#pragma managed(push, off)
#include __STL_INCLUDE_PATH(string)
#pragma managed(pop)

EVENT_PROLOG

namespace sophisTools
{
	namespace comm
	{
		class SerializableRiskEvent;
	} // namespace comm
} // namespace sophisTools

namespace sophis
{
	namespace event
	{
		class ISEvent
		{
		public:
			// SenderHost accessors
			SOPHIS_EVENT_CORE virtual _STL::string & GetSenderHost(_STL::string & value) const = 0;
			SOPHIS_EVENT_CORE virtual _STL::string GetSenderHost() const = 0;
			SOPHIS_EVENT_CORE virtual errno_t GetSenderHost(char * value, size_t size) const = 0;
			SOPHIS_EVENT_CORE virtual void SetSenderHost(const _STL::string & value) = 0;

			// SenderPID accessors
			SOPHIS_EVENT_CORE virtual long GetSenderPID() const = 0;
			SOPHIS_EVENT_CORE virtual void SetSenderPID(long value) = 0;

			// SenderId accessors
			SOPHIS_EVENT_CORE virtual long GetSenderId() const = 0;
			SOPHIS_EVENT_CORE virtual void SetSenderId(long value) = 0;

			// SenderUser accessors
			SOPHIS_EVENT_CORE virtual long GetSenderUser() const = 0;
			SOPHIS_EVENT_CORE virtual void SetSenderUser(long value) = 0;

			// EventId accessors
			SOPHIS_EVENT_CORE virtual long GetEventId() const = 0;
			SOPHIS_EVENT_CORE virtual void SetEventId(long value) = 0;

			// CreationTime accessors
			SOPHIS_EVENT_CORE virtual long long GetCreationTime() const = 0;
			SOPHIS_EVENT_CORE virtual void SetCreationTime(long long value) = 0;

			// Other methods
			SOPHIS_EVENT_CORE virtual long GetWhat() const = 0;
			SOPHIS_EVENT_CORE virtual std::string GetEventCategory() const = 0;
			SOPHIS_EVENT_CORE virtual long GetPacketId() const = 0;
			SOPHIS_EVENT_CORE virtual long GetPacketClass() const = 0;
			SOPHIS_EVENT_CORE virtual bool HasHighPriority() const = 0;
			SOPHIS_EVENT_CORE virtual ISEvent * Clone() const = 0;
			SOPHIS_EVENT_CORE virtual void Read(const sophisTools::comm::SerializableRiskEvent & sre) = 0;
			SOPHIS_EVENT_CORE virtual void Write(sophisTools::comm::SerializableRiskEvent & sre) const = 0;
			SOPHIS_EVENT_CORE virtual void GetFields(CSFields & fields) const = 0;
		}; // class ISEvent
	} // namespace event
} // namespace sophis

EVENT_EPILOG
#endif // _SOPHIS_EVENT_EVENT_H
