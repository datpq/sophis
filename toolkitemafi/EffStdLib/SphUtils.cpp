#include "Inc/SphUtils.h"
#include "SphInc/SphUserRights.h"

using namespace eff::utils;

long SphUtils::GetRiskUserId()
{
	CSRUserRights userRights;
	return userRights.GetIdent();
}

const char * SphUtils::GetRiskUserName()
{
	CSRUserRights userRights;
	return userRights.GetName();
}

