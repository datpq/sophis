#ifndef __CFG_Maths_H__
#define __CFG_Maths_H__

#include "SphInc/SphMacros.h"
#include "SphInc/static_data/SphCalendar.h"
#include "SphInc/static_data/SphDayCountBasis.h"
#include "SphInc/static_data/SphYieldCalculation.h"
#pragma warning(push)
#pragma warning(disable:4103) //  '...' : alignment changed after including header, may be due to missing #pragma pack(pop)
#include __STL_INCLUDE_PATH(math.h)
#pragma warning(pop)
#include __STL_INCLUDE_PATH(limits)



#ifndef __MAX
#define __MAX(a,b) (((a)>(b)) ? (a) : (b))
#endif
#ifndef	__MIN
#define __MIN(a,b) (((a)>(b)) ? (b) : (a))
#endif

class CalcFunction	
{
public:
	CalcFunction(){};
	virtual ~CalcFunction(){};
	virtual	void	Start(){};
	virtual void	End(){};
	virtual double	f(double x)=0;
};

class CFG_Maths
{
public:

	// Utils
	static double Truncate(double number, int nbDecimals);
	static double Round(double number, int nbDecimals);
	static double GetZCFromCompoundFactor(double cf, long timeToMaturity, eDayCountBasisType dcb, eYieldCalculationType yc, const CSRCalendar* calendar=NULL);
	static double GetCompoundFactorFromZC(double zc, long timeToMaturity, eDayCountBasisType dcb, eYieldCalculationType yc, const CSRCalendar* calendar=NULL);
	static double ConvertZCRate(double zc_from, long timeToMaturity, eDayCountBasisType dcb_from, eYieldCalculationType yc_from, eDayCountBasisType dcb_to, eYieldCalculationType yc_to, const CSRCalendar* calendar);
	static long GetTimeToMaturity1y(const CSRCalendar* calendar);

	//Brent algo
	static bool BrentRootFinder(CalcFunction& funct, double f0, double xMin, double xMax, double accuracy, double& output, int iterMax, bool onlyCheckRight, bool findBracketing);
	static bool FindBracket(CalcFunction& funct, double f0, double& xMin, double& xMax, double& fMin, double& fMax, int iterMax, bool onlyOnTheRight, bool computeMinMax);
	inline static double checkBracketing(double y0, double yMin, double yMax)
	{
		return ((yMin<y0 && y0<yMax) || (yMin>y0 && y0>yMax));
	};
	inline static double sign(double absoluteValue, double signToUse)
	{
		return ((signToUse) >= 0.0) ? fabs(absoluteValue) : -fabs(absoluteValue);
	};

};

#endif //!__CFG_Maths_H__