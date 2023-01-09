#pragma once // speed up VC++ compilation

/**
* 
* @author : OP
* @date : 2002/09/26
*
* This class was previously defined in riclist_cdc.h and implemented in CSDynamicFID.c
*
*/

#ifndef __CSRIC_H__
#define __CSRIC_H__

/*
** System includes
*/

/*
** Application includes
*/
#define kTailleMaxiRic  80
#include "SphLLInc/SphBasicDataExports.h"	
#include __STL_INCLUDE_PATH(string)

/*
** defines
*/

/*
** typedef and classes
*/

#if (defined(WIN32)||defined(_WIN64))
#	pragma warning(push)
#	pragma warning(disable:4275) // Can not export a class derivated from a non exported one
#	pragma warning(disable:4251) // Can not export a class agregating a non exported one
#endif


SPH_PROLOG

#define RESDLOGRIC 381
#define NbChampsRIC  9

typedef struct {
	long	sicovam;
	short	fid;
	char    prefixe[7];
	char	reuter[kTailleMaxiRic];
	char	servisen[kTailleMaxiRic];
	char	GL[kTailleMaxiRic];	
	char	triarch[kTailleMaxiRic];
	char	fist[kTailleMaxiRic];
	char	tibco[kTailleMaxiRic];
} TRicList;

// The windowPtr parameter in CSRic is only useful
// in the dialog window to specify the message to send
// it can be null
namespace sophis
{
	namespace event
	{
		class CSRIC;
	}

	namespace tools
	{
		namespace dataModel
		{
			class DataSet;
		}
	}
}

class SOPHIS_FIT CSRic 
{
public:
	static CSRic* GetCSRic(long sicovam=0);
	virtual ~CSRic();
	
	CSRic ();
	CSRic(long sicovam, bool search_default_FID = false);
	CSRic(TRicList* h);
	CSRic(const CSRic& Ric);
	//CSRic(long sicovam,const char * fullric,short fid);
	CSRic& operator=(const CSRic&);

			void	UpdatePrefix();
	
	static  Boolean		charger(long sicovam,CSRic& ric, bool search_default_FID = false);
	static  Boolean		ExisteDeja(long sicovam);
	        int			Save(WindowPtr wind);
	        int			Save(WindowPtr wind,bool updatePrefix,bool sendMessage);
			void		supprimer_only_query();
 	virtual	int			supprimer(WindowPtr wind);
 			int			InsertDB(WindowPtr wind);
 			int			InsertDB(WindowPtr wind,bool sendMessage);
 			int			UpdateDB(WindowPtr wind);
 			int			UpdateDB(WindowPtr wind,bool sendMessage);
			bool		Update(char* type, char* value);
			void		Clean(int which);
			void		PosteMessage(long, WindowPtr wind);	
			void		LoadEvent(sophis::event::CSRIC &);	
	
	double    GetSpot() const;
	void      SetCours(double cours) {fDernierCours=cours;}
	TRicList* GetEnreg() const {return fEnregRic;}
	long      GetSicovam() const {return fEnregRic->sicovam;}
	
	short     GetFID()			const	{return fEnregRic->fid;}
	short     GetItemIntegr()	const	{return fItemIntegr;}
	short     GetItemFlux()		const	{return fItemFlux;}
	short     GetItemSource()	const	{return fItemSource;}
	bool	  GetGlobal(void)	const	{return fGlobal;}
	
	void	  DonneRic(char *str) const ;
	
	void	SetFID(short fid)		{ fEnregRic->fid = fid;	fModif = true;}
	void	SetGlobal(bool global)	{ fGlobal = global;		fModif = true;}
	void	SetIntegr(short inter)	{ fItemIntegr = inter;	fModif = true;}
	void	SetFlux(short flux)		{ fItemFlux = flux;		fModif = true;}
	void	SetSource(short source) { fItemSource = source; fModif = true;}
	void	SetSicovam(long sicovam){ fEnregRic->sicovam = sicovam; }
	void	SetPrefix(const char* prefix);

	void	UpdateFromDescription(const sophis::tools::dataModel::DataSet& dataSet);
	void	GetDescription(sophis::tools::dataModel::DataSet& dataSet) const;

	Boolean fNouveau;
	Boolean fModif;

	const TRicList * getRicList() const { return fEnregRic; }

	static void				setGlobalPrefix(const char *);
	static const char *		getGlobalPrefix(void);
	static void				getRicAndFid(long sicovam,_STL::string & ric,short & fid);

protected:
	TRicList*  fEnregRic;

	bool	fGlobal;
	short	fItemIntegr;
	short	fItemFlux, fItemSource;
	short	fItemFid;
	double	fDernierCours;

private:
	static _STL::string		fGlobalPrefix;
};

enum typeCotation
{
	kClotureNoUpdate,
	kLast,
	kBidAsk,
	kClotureUpdate,
	kCompens,
	kLastCompens,
	kBidAskCompens,
	kBidAskLast,
	kHistoClose
};



/*
** Globals
*/

/*
** Inline Methods
*/

SPH_EPILOG
#endif // __CSRIC_H__
