#include "CFG_Maths.h"
#include "SphInc/market_data/SphMarketData.h"
#include "SphInc/static_data/SphInterestRate.h"


double CFG_Maths::Truncate(double number, int nbDecimals)
{
	if(nbDecimals < 0)
		return number;

	number += 10e-8;
	double mult = pow(10.0, nbDecimals);
	double temp = floor(number * mult);
	double val = temp / mult;
	return val;
};

double CFG_Maths::Round(double number, int nbDecimals)
{
	if(nbDecimals < 0)
		return number;

	//number += 10e-8;
	number += 1e-9;
	double mult = pow(10.0, nbDecimals);
	double temp = floor(number * mult + 0.5);
	double val = temp / mult;
	return val;
};
double CFG_Maths::GetZCFromCompoundFactor(double cf, long timeToMaturity, eDayCountBasisType dcb, eYieldCalculationType yc, const CSRCalendar* calendar)
{
	long today = gApplicationContext->GetDate();
	double dt = CSRDayCountBasis::GetCSRDayCountBasis(dcb)->GetEquivalentYearCount(today, today + timeToMaturity, SSDayCountCalculation(calendar));
	double zc = CSRYieldCalculation::GetCSRYieldCalculation(yc)->GetRate(cf - 1.0, dt);
	return zc;
};

double CFG_Maths::GetCompoundFactorFromZC(double zc, long timeToMaturity, eDayCountBasisType dcb, eYieldCalculationType yc, const CSRCalendar* calendar)
{
	long today = gApplicationContext->GetDate();
	double dt = CSRDayCountBasis::GetCSRDayCountBasis(dcb)->GetEquivalentYearCount(today, today + timeToMaturity, SSDayCountCalculation(calendar));
	double cf = 1.0 + CSRYieldCalculation::GetCSRYieldCalculation(yc)->GetCouponRate(zc, dt);
	return cf;
};

double CFG_Maths::ConvertZCRate(double zc_from, long timeToMaturity, eDayCountBasisType dcb_from, eYieldCalculationType yc_from, eDayCountBasisType dcb_to, eYieldCalculationType yc_to, const CSRCalendar* calendar)
{
	long today = gApplicationContext->GetDate();
	long maturity = today + timeToMaturity;
	double t_from = CSRDayCountBasis::GetCSRDayCountBasis(dcb_from)->GetEquivalentYearCount(today, maturity, SSDayCountCalculation(calendar));
	double t_to = CSRDayCountBasis::GetCSRDayCountBasis(dcb_to)->GetEquivalentYearCount(today, maturity, SSDayCountCalculation(calendar));
	double cr = CSRYieldCalculation::GetCSRYieldCalculation(yc_from)->GetCouponRate(zc_from, t_from);
	double zc_to = CSRYieldCalculation::GetCSRYieldCalculation(yc_to)->GetRate(cr, t_to);
	return zc_to;
};

long CFG_Maths::GetTimeToMaturity1y(const CSRCalendar* calendar)
{
	long today = gApplicationContext->GetDate();
	SSMaturity mat_1y;
	mat_1y.fMaturity = 1;
	mat_1y.fType = 'y';
	long timeToMaturity_1y = SSMaturity::GetDayCount(mat_1y, today, calendar);
	return timeToMaturity_1y;
};

bool CFG_Maths::BrentRootFinder(CalcFunction &funct, 
					 double 		f0, 
					 double 		xMin, 
					 double 		xMax, 
					 double 		accuracy, 
					 double 		&output, 
					 int			iterMax,
					 bool			onlyCheckRight,
					 bool			findBracketing)
{
	if(fabs(xMin-xMax)<accuracy)
	{
		output	= xMin;
		double mean = .5*(xMin+xMax);
		double fa1	= funct.f(0.9*mean)-f0;
		double fb2	= funct.f(1.1*mean)-f0;
		return (fabs(funct.f(.5*(xMin+xMax))-f0) > fabs(fb2-fa1)/(.2*mean)*accuracy)? false:true;
	}

	static double doubleNumericLimit = _STL::numeric_limits<double>::epsilon(); //Machine double precision.

	int iter;
	funct.Start();
	double	a	= xMin,
			b	= xMax,
			c	= xMax,
			d = 0,e = 0,min1,min2;
	double	fa	= funct.f(a)-f0,
			fb	= funct.f(b)-f0,
			fc,p,q,r,s,accuracy1,xMiddle;
	if ((fa > 0.0 && fb > 0.0) || (fa < 0.0 && fb < 0.0))
	{
		double	faBis	= fa+f0,
				fbBis	= fb+f0;
		//Root must be bracketed in Brent algorithm, we try to find a bracketing interval
		if(!findBracketing || (findBracketing &&
			!FindBracket(	funct, 
							f0, 
							a, 
							b,
							faBis, 
							fbBis,  
							20,
							onlyCheckRight,
							false))) // when left
		{
			output = 0.;
			funct.End();
			return false;
		}

		fa = faBis-f0;
		fb = fbBis-f0;
	}

	fc=fb;
	for (iter=0; iter<iterMax; iter++) 
	{
		if ((fb > 0.0 && fc > 0.0) || (fb < 0.0 && fc < 0.0)) 
		{
			c	= a; //Rename a, b, c and adjust bounding interval
			//d. 
			fc	= fa;
			e	= d = b-a;
		}

		if (fabs(fc) < fabs(fb)) 
		{
			a = b; 
			b = c;
			c = a;
			fa= fb;
			fb= fc;
			fc= fa;
		}
		accuracy1	= 2.0*doubleNumericLimit*fabs(b)+0.5*accuracy; //Convergence check.
		xMiddle		= 0.5*(c-b);
		if (fabs(xMiddle) <= accuracy1 || fb == 0.0) 
		{
			output = b;
			funct.End();
			return true;
		}

		if (fabs(e) >= accuracy1 && fabs(fa) > fabs(fb)) 
		{
			s = fb/fa; //Attempt inverse quadratic interpolation.
			if (a == c) 
			{
				p = 2.0*xMiddle*s;
				q = 1.0-s;
			} 
			else 
			{
				q = fa/fc;
				r = fb/fc;
				p = s*(2.0*xMiddle*q*(q-r)-(b-a)*(r-1.0));
				q = (q-1.0)*(r-1.0)*(s-1.0);
			}

			if (p > 0.0) 
				q = -q; //Check whether in bounds.

			p = fabs(p);
			min1	= 3.0*xMiddle*q-fabs(accuracy1*q);
			min2	= fabs(e*q);
			if (2.0*p < (min1 < min2 ? min1 : min2)) 
			{
				e = d; //Accept interpolation.
				d = p/q;
			} 
			else 
			{
				d = xMiddle; //Interpolation failed, use bisection.
				e = d;
			}
		} 
		else 
		{ //Bounds decreasing too slowly, use bisection.
			d = xMiddle;
			e = d;
		}

		a = b; //Move last best guess to a.
		fa= fb;
		if (fabs(d) > accuracy1) //Evaluate new trial root.
			b += d;
		else
			b += sign(accuracy1,xMiddle);
		fb=funct.f(b)-f0;
	}

	//Maximum number of iterations exceeded in Brent algorithm
	funct.End();
	return false; //Never get here.
};

bool CFG_Maths::FindBracket(	CalcFunction	&funct, 
								double			f0, 
								double			&xMin, 
								double			&xMax,
								double			&fMin, 
								double			&fMax,  
								int				iterMax,
								bool			onlyOnTheRight,
								bool			computeMinMax)
{
	//-----------------------------------------------------------------------------
	// just some check so that xMax>xMin
	if(xMin == xMax)
	{
		xMax = (xMin!=0.) ? 2.*xMin : xMin+1.;
		if(!computeMinMax)
			fMax = funct.f(xMax);
	}

	if(xMin>xMax)
	{
		double interm	= xMin;
		xMin			= xMax;
		xMin			= interm;

		if(!computeMinMax)
		{
			interm	= fMin;
			fMin	= fMax;
			fMin	= interm;
		}
	}
	//-----------------------------------------------------------------------------

	if(computeMinMax)
	{
		fMin = funct.f(xMin);
		fMax = funct.f(xMax);
	}
	
	if(checkBracketing(f0,fMin,fMax))
		return true;
	
	
	double	xNew, factor, slope, ecart,
			xOldRight	= xMin,
			fOldRight	= fMin,
			xOldLeft	= xMax,
			fOldLeft	= fMax,
			origin		= (xMin+xMax)/2. ; // central point for the bracketing schearch

	bool checkRight = true; // begin on the right

	

	for(int iter = 0; iter<iterMax; iter++)
	{
		if(checkRight)
		{
			slope = (fMax - fOldRight) / (xMax - xOldRight);

			ecart = (fMax-f0)/(xMax - origin);

			if(slope*ecart>0)
				xNew = 2.*xMax - origin;// no solution
			else
			{	
				factor = (slope != 0.) ? __MAX( __MIN( 2.*fabs(ecart/slope) , 20.) , 2.) : 20.;

				xNew = xMax  + factor*(xMax - origin);
			}

			xOldRight	= xMax;
			xMax		= xNew;
			
			fOldRight	= fMax;
			fMax		= funct.f(xMax);

			if(checkBracketing(f0,fOldRight,fMax))
			{
				xMin	= xOldRight;
				fMin	= fOldRight;
				return true;
			}
		}
		else
		{
			slope = (fMin - fOldLeft) / (xMin - xOldLeft);
			ecart = (fMin-f0)/(origin - xMin);

			if(slope*ecart>0)
				xNew = 2.*xMin - origin;// no solution
			else
			{	
				factor = (slope != 0.) ? __MAX( __MIN( 2.*fabs(ecart/slope) , 20.) , 2.) : 20.;

				xNew = xMin  + factor*(xMin - origin);
			}

			xOldLeft	= xMin;
			xMin		= xNew;

			fOldLeft	= fMin;
			fMin		= funct.f(xMin);

			if(checkBracketing(f0,fOldLeft,fMin))
			{
				xMax	= xOldLeft;
				fMax	= fOldLeft;
				return true;
			}
		}

		if(!onlyOnTheRight)
			checkRight = !checkRight;
	}

	return false;
};
