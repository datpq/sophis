using System;
using System.Data;
using System.Collections;
using System.Threading;
using System.IO;
using System.Text;

using sophis;
using sophis.utils;
using sophis.portfolio;
using sophis.misc;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;

//@DPH
using Eff.UpgradeUtilities;

namespace dnPortfolioColumn
{


    public class Calc_Duration
    {
        // Holds the instance of the singleton class
        private static Calc_Duration Instance = null;
        private ORCLFactory.ORCLFactory CS_ORCL;

        public CSMMarketData Context;
       

        //Constructeur
        private Calc_Duration()
        {
            Context = CSMMarketData.GetCurrentMarketData();
            //CS_ORCL = new ORCLFactory.ORCLFactory();
            ORCLFactory.ORCLFactory.Initialize();
        }
        //public void Close()
        //{
        //    CS_ORCL.CloseAll();
        //}

        /// <summary>
        /// Returns an instance of CFCompute
        /// </summary>
        public static Calc_Duration GetInstance()
        {
            Instance = new Calc_Duration();
            return Instance;
        }

        /// <summary>
        /// Génère la table des flux restants de la date "reportingdate" à la maturité de l'obligation
        /// </summary>
        /// <param name="InstrumentCode"></param>
        /// <param name="reportingdate"></param>
        /// <returns>System.Collections.ArrayList</returns>
        public System.Collections.ArrayList GetBondExplanationArray(int InstrumentCode, int reportingdate)
        {
            System.Collections.ArrayList explicationArray;
            try
            {
                CSMBond Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam

                //Informations sur l'obligation
                int IssueDate = Bond.GetIssueDate();
                int MaturityDate = Bond.GetMaturity();
                int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                int PariPassudate = Bond.GetPariPassuDate(reportingdate);

                //Table des CF restants de l'obligation (table "Explanation" de Sophis)
                explicationArray = new System.Collections.ArrayList();
                SSMExplication explication = new SSMExplication();
                explication.transactionDate = reportingdate;
                explication.settlementDate = reportingdate + SettlementShift;
                explication.pariPassuDate = PariPassudate;
                explication.endDate = MaturityDate;
                explication.withCredit = false;
                explication.discounted = false;
                Bond.GetRedemptionExplication(Context, explicationArray, explication);

                return explicationArray;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot load explanation array for Instrument " + InstrumentCode);
                return null;
            }
        }
        //Focntion de calcul de la duration Crédit et Taux

        public double ComputeDurationValue(CSMPosition Position, CSMInstrument Instrument)
        {

           CarryYtmCompute ICarry = CarryYtmCompute.GetInstance();

            //@SB

           if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesCTAllotmentID() ||
           Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesAllotmentID() ||
           Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsCTAllotmentID() ||
           Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetListedOptionsAllotmentID() ||
           Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID() ||
           Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCIRDerivativesAllotmentID()
           )
           {
               return 0;
           }
           else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetCDSAllotmentID() ||
               Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetTRSAllotmentID() ||
               Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCStockDerivativesAllotmentID()
               )
           {
               //Création de la colonne pour récupérer les données
               CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn("Credit Risk Sensitivity");
               if (col != null)
               {
                   int folioCode = 0;
                   CSMPortfolio folio = Position.GetPortfolio();
                   if (folio != null)
                       folioCode = folio.GetCode();

                   double Svalue = 0.0;
                   int activePortfolioCode = 0;
                   int underlyingCode = 0;
                   int instrumentCode = Instrument.GetCode();
                   bool onlyTheValue = false;

                   //paramètre
                   SSMCellValue cvalue = new SSMCellValue();
                   SSMCellStyle cstyle = new SSMCellStyle();
                   CSMExtraction extraction = new CSMExtraction();

                   cstyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;

                   col.GetPositionCell(Position, activePortfolioCode, folioCode, null, underlyingCode, instrumentCode, ref cvalue, cstyle, onlyTheValue);
                   Svalue = cvalue.doubleValue;
                   return Svalue * 10000;
               }
               ////Delta * duration underlying
               //int undCode = 0;
               //double delta = Instrument.GetDelta(ref undCode);
               //CSMOption opt = CSMOption.GetInstance(Instrument.GetCode());
               //if(opt != null) // Should always be true
               //{
               //    CSMInstrument instr = CSMInstrument.GetInstance(opt.GetUnderlying(0));
               //    return instr.GetDuration() * delta;
               //}

           }

            //@SB

            //SI futures
            if (Instrument.GetInstrumentType() == 'F')
            {
                return Instrument.GetDuration();
            }
            else
            {

                int Tytm = ICarry.ComputeTreeYTM(Position, Instrument);
                //calcul du cas 
                if (Tytm == 1 || Tytm == 4) return Instrument.GetDuration();//FonctionAdd.GetValuefromSophisDouble(Position, Instrument, "Duration");//IG (YTM) ou CMS ou First Call< Today

                else if (Tytm == 2)
                    return FonctionAdd.GetValuefromSophisDouble(Position, Instrument, "Duration to Call MtM"); // Sub not CMS et FirstCall>Today(YTC)

                else if (Tytm == 3)
                    return FonctionAdd.GetValuefromSophisDouble(Position, Instrument, "Duration to Worst MtM");//HY (YTW)

                else return 0;

            }
        }

        //_Duration Crédit
        //
        //Type de l'instrument
        public int GetCreditInstrumentType(CSMPosition Position,int InstrumentCode, int reportingdate)
        {
            int InstrumentType;
            try
            {
                InstrumentType = 0;
                int IsFixed = 0;
                int isFloat = 0;
                int isRedemption = 0;
                CSMInstrument Instrument = CSMInstrument.GetInstance(InstrumentCode);

                switch (Instrument.GetInstrumentType())
                {
                    case 'O'://Obligation
                        //Table des CF de l'obligation de son émission à sa maturité
                        int IssueDate = Instrument.GetIssueDate();
                        int RedemptionDate = Instrument.GetExpiry();//Date de remboursement
                        System.Collections.ArrayList RedemptionArray = new System.Collections.ArrayList();
                        Instrument.GetRedemption(RedemptionArray, IssueDate, RedemptionDate);
                        int nbCF = RedemptionArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                        SSMRedemption IthRedemption;// ième flux

                        for (int j = 0; j < nbCF; j++)
                        {
                            IthRedemption = (sophis.instrument.SSMRedemption)RedemptionArray[j];
                            eMFlowType FlowType = IthRedemption.flowType;
                            switch (FlowType)
                            {
                                case sophis.instrument.eMFlowType.M_ftFixed://fixed
                                    IsFixed = 1;
                                    break;
                                case sophis.instrument.eMFlowType.M_ftFloating://floating
                                    isFloat = 1;
                                    break;

                                //Que fait-on ds ces cas????????
                                case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                                    if (j == 0) isRedemption = 1;
                                    break;
                                case sophis.instrument.eMFlowType.M_ftFloatingSwap://FloatingSwap
                                    break;
                                case sophis.instrument.eMFlowType.M_ftCapitalFlow://CapitalFlow
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (IsFixed == 1 & isFloat == 0) { InstrumentType = 1; }//Fixed
                        if (IsFixed == 0 & isFloat == 1) { InstrumentType = 2; }//Float pur
                        if (IsFixed == 1 & isFloat == 1) { InstrumentType = 3; }//FixedToFloat
                        break;
                        //Convertibles
                    case 'D' :
                        if (FonctionAdd.GetValuefromSophisString(Position, Instrument, "Allotment") == "Convertibles")
                        {
                            int IssueDate_d = Instrument.GetIssueDate();
                            int RedemptionDate_d = Instrument.GetExpiry();//Date de remboursement
                            System.Collections.ArrayList RedemptionArray_d = new System.Collections.ArrayList();
                            Instrument.GetRedemption(RedemptionArray_d, IssueDate_d, RedemptionDate_d);
                            int nbCF_d = RedemptionArray_d.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                            SSMRedemption IthRedemption_d;// ième flux

                            for (int j = 0; j < nbCF_d; j++)
                            {
                                IthRedemption_d = (sophis.instrument.SSMRedemption)RedemptionArray_d[j];
                                eMFlowType FlowType_d = IthRedemption_d.flowType;
                                switch (FlowType_d)
                                {
                                    case sophis.instrument.eMFlowType.M_ftFixed://fixed
                                        IsFixed = 1;
                                        break;
                                    case sophis.instrument.eMFlowType.M_ftFloating://floating
                                        isFloat = 1;
                                        break;

                                    //Que fait-on ds ces cas????????
                                    case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                                        break;
                                    case sophis.instrument.eMFlowType.M_ftFloatingSwap://FloatingSwap
                                        break;
                                    case sophis.instrument.eMFlowType.M_ftCapitalFlow://CapitalFlow
                                        break;
                                    default:
                                        break;
                                }
                            }
                            if (IsFixed == 1 & isFloat == 0) { InstrumentType = 1; }//Fixed
                            if (IsFixed == 0 & isFloat == 1) { InstrumentType = 2; }//Float pur
                            if (IsFixed == 1 & isFloat == 1) { InstrumentType = 3; }//FixedToFloat
                        }
                        else InstrumentType = 0;   
                        break;
                    case 'S'://CDS
                        InstrumentType = 4;
                        break;
                    default://Ni CDS, ni bond fixed/float/fixedToFloat
                        InstrumentType = 0;
                        break;
                }
                return InstrumentType;
            }
            catch (Exception)
            {
                Console.WriteLine("InstrumentType cannot be computed for Instrument " + InstrumentCode);
                return 0;
            }
        }

        //Duration IR
        public double OLD_ComputeDurationIR(CSMInstrument Instrument, int reportingdate, bool ComputeUntilNextFixingOnly)
        {
            sophis.static_data.eMDayCountBasisType DayCountBasisType;
            double Duration;
            int InstrumentCode = Instrument.GetCode();

            System.Collections.ArrayList explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);

            try
            {

                if (Instrument.GetInstrumentType() == 'O')
                {
                    CSMBond Bond = CSMBond.GetInstance(Instrument.GetCode());
                    DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                }
                else if (Instrument.GetInstrumentType() == 'S')
                {
                    CSMSwap Swap = CSMSwap.GetInstance(Instrument.GetCode());
                    DayCountBasisType = Swap.GetMarketYTMDayCountBasisType();
                }
                else DayCountBasisType = Instrument.GetMarketAIDayCountBasisType();
                //Informations à calculer
                double PresentValue = 0;//Somme des pV de tous les flux
                Duration = 0;//Duration Macaulay
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)

                //Variables relatives au ième flux
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthDiscountFactor;//exp(-ytm*temps du cf): actualisation au Ytm pr le calcul de la duration. Exp car duration Macaulay

                //Calcul de la duration
                for (int j = 0; j < nbCF; j++)
                {
                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                    eMFlowType FlowType = IthRedemption.flowType;
                    switch (FlowType)
                    {
                        case sophis.instrument.eMFlowType.M_ftFixed://fixed
                            IthCoupon = IthRedemption.coupon;
                            break;
                        case sophis.instrument.eMFlowType.M_ftFloating://floating
                            IthCoupon = IthRedemption.coupon;
                            if (ComputeUntilNextFixingOnly == true)
                            {
                                IthCoupon = 0;
                                j = nbCF;
                            }//Le 1er coupon flottant aura son coupon ajusté >> on n'est sensible que jusqu'à la fin de période du dernier coupon "fixe"
                            //Attention, un coupon est dit "fixe" dans la table explication quand il ne changera pas dans le futur par opposition aux coupons "float" qui sont seulement estimés et donc changeront au moment du fixing
                            //Si on s'arrete au prochain fixing, on néglige tous les flux après le premier flux flottant 
                            break;
                        case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                            IthCoupon = IthRedemption.redemption;
                            break;
                        //Je ne sais pas quoi faire dans les cas suivants
                        case sophis.instrument.eMFlowType.M_ftFloatingSwap://FloatingSwap
                            IthCoupon = 0;
                            break;
                        case sophis.instrument.eMFlowType.M_ftCapitalFlow://CapitalFlow
                            IthCoupon = 0;
                            break;
                        default:
                            IthCoupon = 0;
                            break;
                    }
                    IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                    IthDiscountFactor = Math.Exp(-Instrument.GetYTMMtoM() * IthDate);
                    PresentValue += IthCoupon * IthDiscountFactor;
                    Duration += IthCoupon * IthDiscountFactor * IthDate;
                }
                if (PresentValue == 0) { Duration = 0; }//Si il n'y a aucun flux, la duration est nulle
                else { Duration = Duration / PresentValue; }
                return Duration;
            }
            catch (Exception)
            {
                Console.WriteLine("Macaulay Duration cannot be computed for Instrument " + InstrumentCode);
                return 0;
            }
        }

        //Duration IR Fixed
        public double ComputeDurationIRFix(CSMInstrument Instrument, int reportingdate, int workoutdate,double yield)
        {
            double Duration = 0;
            int InstrumentCode = Instrument.GetCode();
            sophis.static_data.eMDayCountBasisType DayCountBasisType;
            //Définition de la liste des Flux
            System.Collections.ArrayList explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);
            try
            {
                //Définition de la base de l'instrument
                if (Instrument.GetInstrumentType() == 'O')
                {
                    CSMBond Bond = CSMBond.GetInstance(Instrument.GetCode());
                    DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                }
                else if (Instrument.GetInstrumentType() == 'S')
                {
                    CSMSwap Swap = CSMSwap.GetInstance(Instrument.GetCode());
                    DayCountBasisType = Swap.GetMarketYTMDayCountBasisType();
                }
                else DayCountBasisType = Instrument.GetMarketAIDayCountBasisType();


                //Variable de calcul de la Duration Cas Fixed
                //Informations à calculer
                double PresentValue = 0;//Somme des pV de tous les flux
                Duration = 0;//Duration Macaulay
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                double ActCpn=1;
                //Variables relatives au ième flux
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthDiscountFactor;//exp(-ytm*temps du cf): actualisation au Ytm pr le calcul de la duration. Exp car duration Macaulay
               
                //Calcul de la duration
                for (int j = 0; j < nbCF; j++)
                {

                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                    eMFlowType FlowType = IthRedemption.flowType;
                    switch (FlowType)
                    {
                        case sophis.instrument.eMFlowType.M_ftFixed://fixed
                            if (workoutdate <= IthRedemption.endDate)
                            {
                                ActCpn = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(workoutdate, IthRedemption.endDate)/CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentDayCount(IthRedemption.startDate, IthRedemption.endDate);
                            }
                            IthCoupon = IthRedemption.coupon *ActCpn;
                            break;
                        case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                            IthCoupon = IthRedemption.redemption;
                            break;
                        default:
                            IthCoupon = 0;
                            break;
                    }
                    if (workoutdate <= IthRedemption.endDate)
                    {
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, workoutdate);
                    }
                    else
                    {
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                    }
                        
                    IthDiscountFactor = Math.Exp(-yield * IthDate);
                    //Calcul de la duration
                    PresentValue += IthCoupon * IthDiscountFactor;
                    Duration += IthCoupon * IthDiscountFactor * IthDate;
                    //Si on atteint la date de work out on ajoute la redemtion finale, et on sort de la boucle
                    if (workoutdate <= IthRedemption.endDate)
                    {
                        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[nbCF -1];

                        IthCoupon = IthRedemption.redemption;//pondéré par le nombre de jours le coupon 
                        PresentValue += IthCoupon * IthDiscountFactor;
                        Duration += IthCoupon * IthDiscountFactor * IthDate;
                        j = nbCF;
                    }


                }
                if (PresentValue == 0) { Duration = 0; }//Si il n'y a aucun flux, la duration est nulle
                else { Duration = Duration / PresentValue; }
                
                return Duration;


            }
            catch (Exception)
            {
                Console.WriteLine("Macaulay Duration cannot be computed for Instrument " + InstrumentCode);
                return 0;
            }
        }
        //Duration IR Float
        public double ComputeDurationIRFloat(CSMInstrument Instrument, int reportingdate, int workoutdate)
        {
            double Duration = 0;
            int InstrumentCode = Instrument.GetCode();
            sophis.static_data.eMDayCountBasisType DayCountBasisType;
            //Définition de la liste des Flux
            System.Collections.ArrayList explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);
            try
            {
                Duration = 0;
                //Définition de la base de l'instrument
                if (Instrument.GetInstrumentType() == 'O')
                {
                    CSMBond Bond = CSMBond.GetInstance(Instrument.GetCode());
                    DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                }
                else if (Instrument.GetInstrumentType() == 'S')
                {
                    CSMSwap Swap = CSMSwap.GetInstance(Instrument.GetCode());
                    DayCountBasisType = Swap.GetMarketYTMDayCountBasisType();
                }
                else DayCountBasisType = Instrument.GetMarketAIDayCountBasisType();

                //Variable de calcul de la Duration Cas Float
                SSMRedemption IthRedemption;

                //Calcul de la duration taux. Temps jusqu'au prochain fixing
                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[0];

                Duration = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);

                return Duration;
            }
            catch (Exception)
            {
                Console.WriteLine("Macaulay Duration cannot be computed for Instrument " + InstrumentCode);
                return 0;
            }
        }
        //Duration IR FixToFloat
        public double ComputeDurationIRFixToFloat(CSMInstrument Instrument, int reportingdate, int workoutdate, double yield)
        {
            double Duration = 0;
            int InstrumentCode = Instrument.GetCode();
            sophis.static_data.eMDayCountBasisType DayCountBasisType;

            //Définition de la liste des Flux
            System.Collections.ArrayList explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);

            try
            {
                //Définition de la base de l'instrument
                if (Instrument.GetInstrumentType() == 'O')
                {
                    CSMBond Bond = CSMBond.GetInstance(Instrument.GetCode());
                    DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                }
                else if (Instrument.GetInstrumentType() == 'S')
                {
                    CSMSwap Swap = CSMSwap.GetInstance(Instrument.GetCode());
                    DayCountBasisType = Swap.GetMarketYTMDayCountBasisType();
                }
                else DayCountBasisType = Instrument.GetMarketAIDayCountBasisType();

                //Variable de calcul de la Duration Cas Fixed
                //Informations à calculer
                double PresentValue = 0;//Somme des pV de tous les flux
                Duration = 0;//Duration Macaulay
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)

                //Variables relatives au ième flux
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthDiscountFactor;//exp(-ytm*temps du cf): actualisation au Ytm pr le calcul de la duration. Exp car duration Macaulay

                //Calcul de la duration
                for (int j = 0; j < nbCF; j++)
                {
                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                    eMFlowType FlowType = IthRedemption.flowType;
                    switch (FlowType)
                    {
                        case sophis.instrument.eMFlowType.M_ftFixed://fixed
                            IthCoupon = IthRedemption.coupon;
                            
                            IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                            IthDiscountFactor = Math.Exp(-Instrument.GetYTMMtoM() * IthDate);
                            PresentValue += IthCoupon * IthDiscountFactor;
                            Duration += IthCoupon * IthDiscountFactor * IthDate;

                           break;
                        case sophis.instrument.eMFlowType.M_ftFloating: //float (on sort de la boucle et on ajoute la redemption)
                           if (j != 0)
                           {
                               j = nbCF;
                               IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.startDate);
                               IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[nbCF - 1];
                               IthCoupon = IthRedemption.redemption;
                               IthDiscountFactor = Math.Exp(-Instrument.GetYTMMtoM() * IthDate);
                               PresentValue += IthCoupon * IthDiscountFactor;
                               Duration += IthCoupon * IthDiscountFactor * IthDate;
                           }
                           break;
                        case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                            IthCoupon = IthRedemption.redemption;
                            IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                            IthDiscountFactor = Math.Exp(-Instrument.GetYTMMtoM() * IthDate);
                            PresentValue += IthCoupon * IthDiscountFactor;
                            Duration += IthCoupon * IthDiscountFactor * IthDate;
                            break;
                        default:
                            IthCoupon = 0;
                            break;
                    }
                    //Si on atteint la date de work out on ajoute la redemtion finale, et on sort de la boucle
                    if (workoutdate == IthRedemption.endDate && j!=nbCF)
                    {
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[nbCF - 1];
                        IthDiscountFactor = Math.Exp(-yield * IthDate);
                        IthCoupon = IthRedemption.redemption;
                        PresentValue += IthCoupon * IthDiscountFactor;
                        Duration += IthCoupon * IthDiscountFactor * IthDate;
                        j = nbCF;
                    }
                    
                }
                if (PresentValue == 0) { Duration = 0; }//Si il n'y a aucun flux, la duration est nulle
                else { Duration = Duration / PresentValue; }
                
                return Duration;
            }
            catch (Exception)
            {
                Console.WriteLine("Macaulay Duration cannot be computed for Instrument " + InstrumentCode);
                return 0;
            }
        }

    }

    /// <summary>
    /// Classe contenant les méthodes de calcul des durations
    /// </summary>
    public class PC_DurationCompute
    {
        // Holds the instance of the singleton class
        private static PC_DurationCompute Instance = null;
        public double Ytm;
        private CSMMarketData Context;
        private System.Collections.ArrayList explicationArray;
        private sophis.static_data.eMDayCountBasisType DayCountBasisType;

        //Constructeur
        private PC_DurationCompute(int InstrumentCode, int reportingdate)
        {
            Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            switch (CSMInstrument.GetInstance(InstrumentCode).GetInstrumentType())
            {
                case 'O'://Obligation
                    CSMBond Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam
                    int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                    int PariPassudate = Bond.GetPariPassuDate(reportingdate);
                    double DirtyPrice = Bond.GetDirtyPriceByZeroCoupon(Context, reportingdate, reportingdate + SettlementShift, PariPassudate);
                    explicationArray = GetBondExplanationArray(InstrumentCode, reportingdate);
                    Ytm = GetBondYTMByDirtyPrice(InstrumentCode, DirtyPrice, reportingdate);
                    DayCountBasisType = Bond.GetMarketYTMDayCountBasisType();
                    break;

                case 'S'://CDS
                    CSMSwap Swap = CSMSwap.GetInstance(InstrumentCode);
                    DayCountBasisType = Swap.GetMarketYTMDayCountBasisType();
                    Ytm = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode) / 100;
                    explicationArray = null;
                    break;

                default:
                    CSMInstrument Instrument = CSMInstrument.GetInstance(InstrumentCode);
                    DayCountBasisType = Instrument.GetMarketAIDayCountBasisType();
                    Ytm = Instrument.GetTheoreticalValue();
                    explicationArray = null;
                    break;
            }
        }

        /// <summary>
        /// Returns an instance of PC_DurationCompute
        /// </summary>
        public static PC_DurationCompute GetInstance(int InstrumentCode, int reportingdate)
        {
            Instance = new PC_DurationCompute(InstrumentCode, reportingdate);
            return Instance;
        }

        /// <summary>
        ///Calcul de la duration Macaulay à partir de la table "Explanation" de Sophis
        ///Cette provédure donne la duration ds sa déf classique ie moyenne pondérée des CF par leur tps de tombée
        ///Fonctionne quel que soit le type d'oblig fixed/float/fixedTofloat
        /// </summary>
        /// <param name="InstrumentCode"> sicovam de l'obligation </param>
        /// <param name="reportinddate">
        /// false si on calcule la duration avec l'ensemble des flux jusqu'à la maturité
        /// true si on calcule la duration jusqu'au prochain fixing seulement
        /// </param>
        /// <param name="ComputeUntilNextFixingOnly"></param>
        /// <returns>renvoie la duration Macaulay comme moyenne pondérée des flux par leur instant de tombée.</returns>
        public double ComputeBondMacDurationByExplanationArray(int InstrumentCode, int reportingdate, bool ComputeUntilNextFixingOnly)
        {
            double Duration;
            try
            {
                //Informations à calculer
                double PresentValue = 0;//Somme des pV de tous les flux
                Duration = 0;//Duration Macaulay
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)

                //Variables relatives au ième flux
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthDiscountFactor;//exp(-ytm*temps du cf): actualisation au Ytm pr le calcul de la duration. Exp car duration Macaulay

                //Calcul de la duration
                for (int j = 0; j < nbCF; j++)
                {
                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                    eMFlowType FlowType = IthRedemption.flowType;
                    switch (FlowType)
                    {
                        case sophis.instrument.eMFlowType.M_ftFixed://fixed
                            IthCoupon = IthRedemption.coupon;
                            break;
                        case sophis.instrument.eMFlowType.M_ftFloating://floating
                            IthCoupon = IthRedemption.coupon;
                            if (ComputeUntilNextFixingOnly == true)
                            {
                                IthCoupon = 0;
                                j = nbCF;
                            }//Le 1er coupon flottant aura son coupon ajusté >> on n'est sensible que jusqu'à la fin de période du dernier coupon "fixe"
                            //Attention, un coupon est dit "fixe" dans la table explication quand il ne changera pas dans le futur par opposition aux coupons "float" qui sont seulement estimés et donc changeront au moment du fixing
                            //Si on s'arrete au prochain fixing, on néglige tous les flux après le premier flux flottant 
                            break;
                        case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                            IthCoupon = IthRedemption.redemption;
                            break;

                        //Je ne sais pas quoi faire dans les cas suivants
                        case sophis.instrument.eMFlowType.M_ftFloatingSwap://FloatingSwap
                            IthCoupon = 0;
                            break;
                        case sophis.instrument.eMFlowType.M_ftCapitalFlow://CapitalFlow
                            IthCoupon = 0;
                            break;
                        default:
                            IthCoupon = 0;
                            break;
                    }
                    IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                    IthDiscountFactor = Math.Exp(-Ytm * IthDate);
                    PresentValue += IthCoupon * IthDiscountFactor;
                    Duration += IthCoupon * IthDiscountFactor * IthDate;
                }
                if (PresentValue == 0) { Duration = 0; }//Si il n'y a aucun flux, la duration est nulle
                else { Duration = Duration / PresentValue; }
                return Duration;
            }
            catch (Exception)
            {
                Console.WriteLine("Macaulay Duration cannot be computed for Instrument " + InstrumentCode);
                return 0;
            }
        }

        /// <summary>
        ///Calcul de la Risky duration Macaulay à partir de la table "Explanation" de Sophis et de la proba de défaut
        ///Cette procédure donne la duration dans sa définition classique ie moyenne pondérée des CF par leur tps de tombée
        ///Valable quel que soit le type d'obligation fixed/float/fixedTofloat
        /// </summary>
        /// <param name="InstrumentCode"> sicovam de l'obligation </param>
        /// <param name="reportinddate">
        /// false si on calcule la duration avec l'ensemble des flux jusqu'à la maturité
        /// true si on calcule la duration jusqu'au prochain fixing seulement
        /// </param>
        /// <param name="ComputeUntilNextFixingOnly"></param>
        /// <returns>renvoie la Risky duration Macaulay comme moyenne pondérée des flux espérés par leur instant de tombée.</returns>
        public double ComputeBondRiskyMacDurationByExplanationArray(int InstrumentCode, int reportingdate, bool ComputeUntilNextFixingOnly)
        {
            double Duration;
            try
            {
                CSMBond Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam

                //Probabilité de défault et de survie           
                int Issuercode = Bond.GetIssuerCode();
                int Currency = Bond.GetCurrency();
                int seniority = Bond.GetSeniority();//CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(Issuercode, Currency).GetDefaultSeniority();
                int defevent = Bond.GetDefaultEvent();
                double Recoveryrate = CSMMarketData.GetCurrentMarketData().GetCSRCreditRisk(Issuercode, Currency).GetRecoveryRate(seniority, defevent);
                if (Recoveryrate == 0)//on force la recovery
                {
                    if (seniority == 101) { Recoveryrate = 0.4; }//Senior
                    if (seniority == 102) { Recoveryrate = 0.05; }//Sub
                }

                double zcrate = GetZCRate(InstrumentCode, reportingdate);
                double implcds = (Ytm - zcrate);
                double cleanSpread = implcds / (1 - Recoveryrate);
                double DefaultProbability;//Proba qu'il y ait défaut au temps i
                double SurvivalProbability;//Proba que l'emetteur n'ai pas fait défaut au temps i
                double TotalDefaultProbability = 0;//Proba que l'emetteur ait fait défaut au temps i
                //>> TotalDefaultProbability i = DefaultProbability_i + DefaultProbability_(i-1) +...+ DefaultProbability_1

                //Informations à calculer
                double PresentValue = 0;//Somme des pV de ts les flux
                double RDuration = 0;//Risky Duration Macaulay

                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)

                //Variables relatives à au ième flux
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthExpectedCoupon;//espérance du ième flux
                double IthDiscountFactor;// exp(-ytm*temps du cf): actualisation au Ytm pr le calcul de la duration. Exp car duration Macaulay
                double Notional = Bond.GetNotional();

                //Initialisation des probas de défaut et de survie pour la 1ère période de flux (nécessaire si le 1er flux est la rédemption)
                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[0];
                IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                SurvivalProbability = Math.Pow(1 / (1 + cleanSpread), IthDate);
                DefaultProbability = 1 - SurvivalProbability - TotalDefaultProbability;

                //Calcul de la duration risky
                for (int j = 0; j < nbCF; j++)
                {
                    IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                    eMFlowType FlowType = IthRedemption.flowType;

                    //Calcul des probas de défaut et de survie au temps i
                    IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                    SurvivalProbability = Math.Pow(1 / (1 + cleanSpread), IthDate);

                    if (FlowType.Equals(sophis.instrument.eMFlowType.M_ftRedemption) == false)
                    {
                        DefaultProbability = 1 - SurvivalProbability - TotalDefaultProbability;
                        TotalDefaultProbability += DefaultProbability;
                    }//Attention: la proba de defaut de la redemption doit être la meme que la proba de défaut du flux précédent 

                    switch (FlowType)
                    {
                        case sophis.instrument.eMFlowType.M_ftFixed://fixed
                            IthCoupon = IthRedemption.coupon;
                            IthExpectedCoupon = SurvivalProbability * IthCoupon + DefaultProbability * Notional * Recoveryrate;
                            //on considère qu'un bond qui fait défaut ne verse pas d'accrued -vrai en pratique: fonctionnement différent d'un cds.
                            //on considère qu'un défaut intervient en milieu de période mais la recovery étant un évenement très aléatoire, on considère
                            //que le flux est touché en fin de période
                            break;
                        case sophis.instrument.eMFlowType.M_ftFloating://floating
                            IthCoupon = IthRedemption.coupon;
                            IthExpectedCoupon = SurvivalProbability * IthCoupon + DefaultProbability * Notional * Recoveryrate;
                            if (ComputeUntilNextFixingOnly == true)
                            {
                                IthExpectedCoupon = 0;//Le coupon n'est sensible que jusqu'à la fin de période du dernier coupon "fixe"
                                //On considère que le notionel remboursé en cas de défaut n'est pas sensible à un changement de taux au delà 
                                //de la date de fixing
                                j = nbCF;
                            }
                            //Attention, un coupon est dit "fixe" dans la table explication quand il ne changera pas dans le futur par opposition aux coupons "float" qui sont seulement estimés et donc changeront au moment du fixing
                            //Si on s'arrete au prochain fixing, on néglige ts les CF après le premier flux flottant 
                            break;
                        case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                            IthCoupon = IthRedemption.redemption;
                            //capital remboursé: entièrement si survie, Recovery si défaut
                            IthExpectedCoupon = SurvivalProbability * IthCoupon + DefaultProbability * Notional * Recoveryrate;
                            break;

                        case sophis.instrument.eMFlowType.M_ftFloatingSwap://
                            IthExpectedCoupon = 0;
                            break;
                        case sophis.instrument.eMFlowType.M_ftCapitalFlow://CapitalFlow
                            IthExpectedCoupon = 0;
                            break;
                        default:
                            IthExpectedCoupon = 0;
                            break;
                    }
                    IthDiscountFactor = Math.Exp(-Ytm * IthDate);
                    PresentValue += IthExpectedCoupon * IthDiscountFactor;
                    RDuration += IthExpectedCoupon * IthDiscountFactor * IthDate;
                }
                if (PresentValue == 0) { RDuration = 0; }//Si il n'y a aucun flux, la duration est nulle
                else { RDuration = RDuration / PresentValue; }
                return RDuration;
            }
            catch (Exception)
            {
                Console.WriteLine("Macaulay Risky Duration cannot be computed for Instrument " + InstrumentCode);
                return 0;
            }
        }

        /// <summary>
        /// Permet de savoir si l'instrument est un CDS/obligation fixed/float/fixedToFloat
        /// Renvoie: 
        ///         1 si fixed
        ///         2 si float
        ///         3 si FixedToFloat
        ///         4 si CDS
        ///         0 si aucun de ces instruments
        /// </summary>
        /// <param name="InstrumentCode"></param>
        /// <param name="reportingdate"></param>
        /// <returns></returns>
        public int GetCreditInstrumentType(int InstrumentCode, int reportingdate)
        {
            int InstrumentType;
            try
            {
                InstrumentType = 0;
                int IsFixed = 0;
                int isFloat = 0;
                CSMInstrument Instrument = CSMInstrument.GetInstance(InstrumentCode);

                switch (Instrument.GetInstrumentType())
                {
                    case 'O'://Obligation
                        //Table des CF de l'obligation de son émission à sa maturité
                        int IssueDate = Instrument.GetIssueDate();
                        int RedemptionDate = Instrument.GetExpiry();//Date de remboursement
                        System.Collections.ArrayList RedemptionArray = new System.Collections.ArrayList();
                        Instrument.GetRedemption(RedemptionArray, IssueDate, RedemptionDate);
                        int nbCF = RedemptionArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                        SSMRedemption IthRedemption;// ième flux

                        for (int j = 0; j < nbCF; j++)
                        {
                            IthRedemption = (sophis.instrument.SSMRedemption)RedemptionArray[j];
                            eMFlowType FlowType = IthRedemption.flowType;
                            switch (FlowType)
                            {
                                case sophis.instrument.eMFlowType.M_ftFixed://fixed
                                    IsFixed = 1;
                                    break;
                                case sophis.instrument.eMFlowType.M_ftFloating://floating
                                    isFloat = 1;
                                    break;

                                //Que fait-on ds ces cas????????
                                case sophis.instrument.eMFlowType.M_ftRedemption://redemption
                                    break;
                                case sophis.instrument.eMFlowType.M_ftFloatingSwap://FloatingSwap
                                    break;
                                case sophis.instrument.eMFlowType.M_ftCapitalFlow://CapitalFlow
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (IsFixed == 1 & isFloat == 0) { InstrumentType = 1; }//Fixed
                        if (IsFixed == 0 & isFloat == 1) { InstrumentType = 2; }//Float pur
                        if (IsFixed == 1 & isFloat == 1) { InstrumentType = 3; }//FixedToFloat
                        break;

                    case 'S'://CDS
                        InstrumentType = 4;
                        break;
                    default://Ni CDS, ni bond fixed/float/fixedToFloat
                        InstrumentType = 0;
                        break;
                }
                return InstrumentType;
            }
            catch (Exception)
            {
                Console.WriteLine("InstrumentType cannot be computed for Instrument " + InstrumentCode);
                return 0;
            }
        }

        /// <summary>
        /// Calcul de la duration risky crédit d'un CDS à partir de sa table de flux:
        /// On utilise les flux risky, i.e. les flux pondérés par leur probabilité d'occurence et par la recovery
        /// La duration est caclulée avec les flux que l'on espère RECEVOIR.
        /// </summary>
        /// <param name="InstrumentCode"></param>
        /// <param name="reportingdate"></param>
        /// <returns></returns>
        public double GetCDSMacDurationByExplanationArray(int InstrumentCode, int reportingdate)
        {
            double Duration;
            try
            {
                CSMSwap Swap = CSMSwap.GetInstance(InstrumentCode);
                System.Collections.ArrayList PLeg = new System.Collections.ArrayList();//Paid Leg
                System.Collections.ArrayList RLeg = new System.Collections.ArrayList();//Received Leg
                Swap.GetSwapInformation(Context, RLeg, PLeg);

                CSMCashFlowInformation IthLeg = new CSMCashFlowInformation();
                double IthEndPeriod;//Date de fin de période du ième flux
                double IthExpectedCF;//ième CF espéré
                double IthDate;
                double IthDiscountFactor;
                int nbRCF = RLeg.Count;
                int nbPCF = PLeg.Count;

                //On considère l'espérance des flux: on tient donc compte des flux des deux jambes

                /////////////////////Receiving Leg/////////////////////////////////////////////////
                //Rechercher la première période de coupon à prendre en compte: 
                //c'est la 1ère dont la date de fin est postérieure à la date de reporting
                int FirstPeriodToConsider = 0;
                IthLeg = (sophis.instrument.CSMCashFlowInformation)RLeg[0];
                IthEndPeriod = IthLeg.end_date;
                while (IthEndPeriod < reportingdate)
                {
                    FirstPeriodToConsider++;
                    IthLeg = (sophis.instrument.CSMCashFlowInformation)RLeg[FirstPeriodToConsider];
                    IthEndPeriod = IthLeg.end_date;
                }

                //Calcul de la duration
                Duration = 0;
                double PresentValue = 0;
                for (int j = FirstPeriodToConsider; j < nbRCF; j++)
                {
                    IthLeg = (sophis.instrument.CSMCashFlowInformation)RLeg[j];
                    IthExpectedCF = IthLeg.amount * IthLeg.probability;
                    IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthLeg.end_date);
                    IthDiscountFactor = Math.Exp(-Ytm * IthDate);

                    PresentValue += IthExpectedCF * IthDiscountFactor;
                    Duration += IthExpectedCF * IthDiscountFactor * IthDate;
                }

                /////////////////////Paying Leg/////////////////////////////////////////////////
                //Rechercher la première période de coupon à prendre en compte: 
                //c'est la 1ère dont la date de fin est postérieure à la date de reporting
                FirstPeriodToConsider = 0;
                IthLeg = (sophis.instrument.CSMCashFlowInformation)PLeg[0];
                IthEndPeriod = IthLeg.end_date;
                while (IthEndPeriod < reportingdate)
                {
                    FirstPeriodToConsider++;
                    IthLeg = (sophis.instrument.CSMCashFlowInformation)PLeg[FirstPeriodToConsider];
                    IthEndPeriod = IthLeg.end_date;
                }
                //Calcul de la duration
                for (int j = FirstPeriodToConsider; j < nbPCF; j++)
                {
                    IthLeg = (sophis.instrument.CSMCashFlowInformation)PLeg[j];
                    IthExpectedCF = - IthLeg.amount * IthLeg.probability;
                    IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthLeg.end_date);
                    IthDiscountFactor = Math.Exp(-Ytm * IthDate);

                    PresentValue += IthExpectedCF * IthDiscountFactor;
                    Duration += IthExpectedCF * IthDiscountFactor * IthDate;
                }

                if (PresentValue == 0) { Duration = 0; }//Si il n'y a aucun flux, la duration est nulle
                else { Duration = Duration / PresentValue; }
                return Duration;
            }
            catch (Exception)
            {
                Console.WriteLine("Macaulay Duration Credit cannot be computed for CDS Instrument " + InstrumentCode);
                return 0;
            }
        }

        /// <summary>
        /// Calcul le rendement actuariel continu de l'obligation à partir de son dirty price.
        /// On utilise la méthode de la bi-section 
        /// </summary>
        /// <param name="InstrumentCode"></param>
        /// <param name="DirtyPrice"></param>
        /// <returns></returns>
        public double GetBondYTMByDirtyPrice(int InstrumentCode, double DirtyPrice, int reportingdate)
        {
            double tkoYtm;
            try
            {
                //Méthode de calcul dichotomique
                double a, b, c;
                a = -0.5; b = 1.3;
                CSMBond Bond = CSMBond.GetInstance(InstrumentCode);
                int Maturity = Bond.GetExpiry();
                double Nominal = Bond.GetNotional();
                double DirtyPriceTest = 0;

                //Travail sur les flux restants de l'obligation (table "Explanation" de Sophis)
                int nbCF = explicationArray.Count;//Nombre de CF (le remboursement est compté comme un CF supplémentaire)
                SSMRedemption IthRedemption;
                double IthDate;//Date avec laquelle on actualise le ième CF
                double IthCoupon;//ième flux
                double IthDiscountFactor;
                int nbiter = 0;

                while ((b - a) > Math.Pow(10, -5) && nbiter < 1000)
                {
                    c = (a + b) * 0.5;
                    DirtyPriceTest = 0;
                    for (int j = 0; j < nbCF; j++)
                    {
                        IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[j];
                        IthCoupon = IthRedemption.coupon + IthRedemption.redemption;
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                        IthDiscountFactor = Math.Exp(-c * IthDate);
                        DirtyPriceTest += IthCoupon * IthDiscountFactor;
                    }
                    DirtyPriceTest = DirtyPriceTest / Nominal;
                    if (DirtyPriceTest > DirtyPrice * 0.01)
                    {
                        a = c;
                    }
                    else
                    {
                        b = c;
                    }
                    nbiter++;
                }
                tkoYtm = ((a + b) * 0.5);
                return tkoYtm;


                //Méthode Sophis
                //CSMBond Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam
                //CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
                //int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                //int PariPassudate = Bond.GetPariPassuDate(reportingdate);
                //int OwnershipShift = Bond.GetSettlementShift();
                //double Dirty = Bond.GetDirtyPriceByZeroCoupon(Context, reportingdate, reportingdate + SettlementShift, PariPassudate);
                //tkoYtm = Bond.GetYTMByDirtyPrice(reportingdate, reportingdate + SettlementShift, reportingdate + OwnershipShift, Dirty);
                //return tkoYtm;

            }
            catch (Exception)
            {
                Console.WriteLine("Tko Ytm cannot be computed for bond Instrument " + InstrumentCode);
                return 0;
            }
        }

        /// <summary>
        /// Calcul de la duration Modified à partir de la duration Macaulay
        /// </summary>
        /// <param name="MacDuration">Duration Macaulay</param>
        /// <param name="Ytm">Rendement actuariel</param>
        /// <param name="nbCFByYear">Nombre de flux par an -2 si coupon semia annuel</param>
        /// <returns></returns>
        public double GetModDurationByMacDuration(double MacDuration, double Ytm, double nbCFByYear)
        {
            double ModDuration;
            try
            {
                ModDuration = MacDuration / (1 + Ytm / nbCFByYear);
                return ModDuration;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot convert Macaulay duration to Modified duration for Instrument ");
                return 0;
            }
        }

        /// <summary>
        /// Génère la table des flux restants de la date "reportingdate" à la maturité de l'obligation
        /// </summary>
        /// <param name="InstrumentCode"></param>
        /// <param name="reportingdate"></param>
        /// <returns>System.Collections.ArrayList</returns>
        public System.Collections.ArrayList GetBondExplanationArray(int InstrumentCode, int reportingdate)
        {
            System.Collections.ArrayList explicationArray;
            try
            {
                CSMBond Bond = CSMBond.GetInstance(InstrumentCode);//Création de l'obligation à partir de son sicovam

                //Informations sur l'obligation
                int IssueDate = Bond.GetIssueDate();
                int MaturityDate = Bond.GetMaturity();
                int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                int PariPassudate = Bond.GetPariPassuDate(reportingdate);

                //Table des CF restants de l'obligation (table "Explanation" de Sophis)
                explicationArray = new System.Collections.ArrayList();
                SSMExplication explication = new SSMExplication();
                explication.transactionDate = reportingdate;
                explication.settlementDate = reportingdate + SettlementShift;
                explication.pariPassuDate = PariPassudate;
                explication.endDate = MaturityDate;
                explication.withCredit = false;
                explication.discounted = false;
                Bond.GetRedemptionExplication(Context, explicationArray, explication);

                return explicationArray;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot load explanation array for Instrument " + InstrumentCode);
                return null;
            }
        }

        public double GetCDSNbCFByYear(int instrumentCode)
        {
            double couponsByYear;
            try
            {
                CSMSwap Swap = CSMSwap.GetInstance(instrumentCode);
                CSMLeg Leg;
                if (Swap.GetLegFlowType(0).ToString() == "M_lfCredit")
                {
                    Leg = Swap.GetLeg(1);
                }
                else
                {
                    Leg = Swap.GetLeg(0);
                }
                eMSwapPeriodicityType PeriodicityType = Leg.GetPeriodicityType();
                switch (PeriodicityType.ToString())
                {
                    case "M_spUndefined":
                        couponsByYear = 1;
                        break;
                    case "M_spMonthly":
                        couponsByYear = 12;
                        break;
                    case "M_spQuarterly":
                        couponsByYear = 4;
                        break;
                    case "M_spSemiAnnually":
                        couponsByYear = 2;
                        break;
                    case "M_spAnnual":
                        couponsByYear = 1;
                        break;
                    case "M_spFinal":
                        couponsByYear = 1;
                        break;
                    case "M_spWeekly":
                        couponsByYear = 52;
                        break;
                    case "M_spDaily":
                        couponsByYear = 365.25;
                        break;
                    case "M_sp2Weeks":
                        couponsByYear = 52 / 2;
                        break;
                    case "M_sp3Weeks":
                        couponsByYear = 52 / 3;
                        break;
                    case "M_sp4Weeks":
                        couponsByYear = 52 / 4;
                        break;
                    case "M_spIMM":
                        couponsByYear = 4;
                        break;
                    case "M_spHourly":
                        couponsByYear = 365.25 * 24;
                        break;
                    case "M_sp2Months":
                        couponsByYear = 6;
                        break;
                    case "M_sp5Months":
                        couponsByYear = 12 / 5;
                        break;
                    case "M_sp7Months":
                        couponsByYear = 12 / 7;
                        break;
                    default:
                        couponsByYear = 1;
                        break;
                }
                return couponsByYear;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot load explanation array for CDS Instrument " + instrumentCode);
                return 1;
            }
        }
        public double GetBondNbCFByYear(int instrumentCode)
        {
            double couponsByYear;
            try
            {
                CSMBond Bond = CSMBond.GetInstance(instrumentCode);
                switch (Bond.GetMarketBondPeriodicityType().ToString())
                {
                    case "M_spUndefined":
                        couponsByYear = 1;
                        break;
                    case "M_spMonthly":
                        couponsByYear = 12;
                        break;
                    case "M_spQuarterly":
                        couponsByYear = 4;
                        break;
                    case "M_spSemiAnnually":
                        couponsByYear = 2;
                        break;
                    case "M_spAnnual":
                        couponsByYear = 1;
                        break;
                    case "M_spFinal":
                        couponsByYear = 1;
                        break;
                    case "M_spWeekly":
                        couponsByYear = 52;
                        break;
                    case "M_spDaily":
                        couponsByYear = 365.25;
                        break;
                    case "M_sp2Weeks":
                        couponsByYear = 52 / 2;
                        break;
                    case "M_sp3Weeks":
                        couponsByYear = 52 / 3;
                        break;
                    case "M_sp4Weeks":
                        couponsByYear = 52 / 4;
                        break;
                    case "M_spIMM":
                        couponsByYear = 4;
                        break;
                    case "M_spHourly":
                        couponsByYear = 365.25 * 24;
                        break;
                    case "M_sp2Months":
                        couponsByYear = 6;
                        break;
                    case "M_sp5Months":
                        couponsByYear = 12 / 5;
                        break;
                    case "M_sp7Months":
                        couponsByYear = 12 / 7;
                        break;
                    default:
                        couponsByYear = 1;
                        break;
                }
                return couponsByYear;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot load explanation array for Bond Instrument " + instrumentCode);
                return 1;
            }

        }
        public double GetZCRate(int instrumentCode, int reportingdate)
        {
            double zcrate = 0;

            try
            {
                CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(instrumentCode);
                int currency = InstrumentPtr.GetCurrency();
                double maturity = InstrumentPtr.GetExpiry();
                zcrate = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(currency, reportingdate, maturity);
                eMDayCountBasisType dcbt = CSMPreference.GetDayCountBasisType();
                double eqyc = CSMDayCountBasis.GetCSRDayCountBasis(dcbt).GetEquivalentYearCount(Convert.ToInt32(reportingdate), Convert.ToInt32(maturity));
                zcrate = Math.Pow((zcrate), 1 / (eqyc)) - 1;
                return zcrate;
            }

            catch (Exception)
            {
                Console.WriteLine("Cannot compute ZC Rate for Instrument " + instrumentCode);
                return 0;
            }
        }
        public double GetBondDirtyPrice(int instrumentCode, int reportingdate)
        {
            double dirtyPrice;
            try
            {
                CSMBond Bond = CSMBond.GetInstance(instrumentCode);
                CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
                int OwnershipShift = Bond.GetSettlementShift();//Get the delivery shift to calculate ownership
                int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
                int PariPassudate = Bond.GetPariPassuDate(reportingdate);

                dirtyPrice = Bond.GetDirtyPriceByZeroCoupon(Context, reportingdate, reportingdate + SettlementShift, PariPassudate);
                return dirtyPrice;
            }
            catch (Exception)
            {
                Console.WriteLine("Cannot compute dirty price for Instrument " + instrumentCode);
                return 0;
            }
        }
    }

    /// <summary>
    /// Durations selon le type d'instrument et gestion des caches sophis
    /// </summary>
    
    public class DataSourceDurationTaux
    {
        // Holds the instance of the singleton class
        private static DataSourceDurationTaux Instance = null;

        // Used to cache the results
        //Bond: la duration taux est (re)calculée quand l'instrument ou le spot change
        public static Hashtable DataCacheDurationTauxBOND;
        public static Hashtable DataCacheInstrVersionBOND;
        public static Hashtable DataCacheSpotBOND;

        //CDS: la duration taux est (re)calculée quand l'utilisateur fait F8/F9
        //Le cache renvoie 0 quang l'instrument a changé 
        public static Hashtable DataCacheDurationTauxCDS;
        public static Hashtable DataCacheInstrVersionCDS;

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceDurationTaux()
        {
            DataCacheDurationTauxBOND = new Hashtable();
            DataCacheInstrVersionBOND = new Hashtable();
            DataCacheSpotBOND = new Hashtable();

            DataCacheDurationTauxCDS = new Hashtable();
            DataCacheInstrVersionCDS = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceDurationTaux GetInstance()
        {
            int SophisDate = CSMMarketData.GetPositionDate();
            if (null == Instance)
            {
                Instance = new DataSourceDurationTaux();
            }
            return Instance;
        }

        public double Get_tko_modDuration_taux(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int Instrumentcode = Instrument.GetCode();
            CSMMarketData context = CSMMarketData.GetCurrentMarketData();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0)
            {
                switch (Instrument.GetInstrumentType().ToString())
                {
                    case "O"://Obligation
                        //La valeur est (re)calculée:
                        //si le cache est vide 
                        //si la version de l'instrument change
                        //si le spot change
                        if (!DataCacheDurationTauxBOND.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersionBOND[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpotBOND[Instrumentcode] != context.GetSpot(Instrumentcode))
                        {
                            FillCacheBOND(Position, Instrument);
                        }

                        // At this point, the value that this method should return must be available in the cache
                        if (DataCacheDurationTauxBOND.ContainsKey(Instrumentcode))
                        {
                            return (double)DataCacheDurationTauxBOND[Instrumentcode];
                        }
                        else { return 0; }//on ne devrait jamais passer par là ms au cas où


                    case "S"://CDS
                        //RERMARQUE: la duration dépend de valeur MtM mais on utilise quand même une table par instrument
                        //dont les clefs sont les instruments (et non pas les positions) car un CDS ne peut être utilisé dans différentes positions (un contrat par position)

                        //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
                        if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
                        {
                            VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                            VersionClass.DeleteCache();
                        }

                        //Si la version de l'instr change, on met 0 ds le cache
                        if (DataCacheInstrVersionCDS.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersionCDS[Instrumentcode] != Instrument.GetVersion())
                        {
                            if (DataCacheDurationTauxCDS.ContainsKey(Instrumentcode))
                                DataCacheDurationTauxCDS[Instrumentcode] = 0.0;
                            else
                                DataCacheDurationTauxCDS.Add(Instrumentcode, 0.0);

                            //mise à jour de la version de l'instrument
                            DataCacheInstrVersionCDS[Instrumentcode] = Instrument.GetVersion();
                        }

                        //La valeur est (re)calculée:
                        //si le cache est vide
                        if (!DataCacheDurationTauxCDS.ContainsKey(Instrumentcode))
                        {
                            FillCacheCDS(Position, Instrument);
                        }

                        // At this point, the value that this method should return must be available in the cache
                        if (DataCacheDurationTauxCDS.ContainsKey(Instrumentcode))
                        {
                            return (double)DataCacheDurationTauxCDS[Instrumentcode];
                        }
                        else { return 0; }//on ne devrait jamais passer par là ms au cas où

                    default:
                        return 0;
                }
            }
            else { return 0; }
        }

        private void FillCacheBOND(CSMPosition Position, CSMInstrument Instrument)
        {
            int instrumentCode = Instrument.GetCode();

            //Mise à jour de la valeur de la duration
            double tko_modDuration_taux = Compute_tko_modDuration_taux(instrumentCode,VersionClass.Get_ReportingDate(), Position);
            if (DataCacheDurationTauxBOND.ContainsKey(instrumentCode))
                DataCacheDurationTauxBOND[instrumentCode] = tko_modDuration_taux;
            else
                DataCacheDurationTauxBOND.Add(instrumentCode, tko_modDuration_taux);

            //Mise à jour de la version de l'instrument
            int instVersion = Instrument.GetVersion();
            if (DataCacheInstrVersionBOND.ContainsKey(instrumentCode))
                DataCacheInstrVersionBOND[instrumentCode] = instVersion;
            else
                DataCacheInstrVersionBOND.Add(instrumentCode, instVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
            if (DataCacheSpotBOND.ContainsKey(instrumentCode))
                DataCacheSpotBOND[instrumentCode] = Spot;
            else
                DataCacheSpotBOND.Add(instrumentCode, Spot);
        }

        private void FillCacheCDS(CSMPosition Position, CSMInstrument Instrument)
        {
            int instrumentCode = Instrument.GetCode();

            //Mise à jour de la valeur de la duration
            double tko_modDuration_taux = Compute_tko_modDuration_taux(instrumentCode, VersionClass.Get_ReportingDate(), Position);
            if (DataCacheDurationTauxCDS.ContainsKey(instrumentCode))
                DataCacheDurationTauxCDS[instrumentCode] = tko_modDuration_taux;
            else
                DataCacheDurationTauxCDS.Add(instrumentCode, tko_modDuration_taux);

            //Mise à jour de la version de l'instrument
            int instVersion = Instrument.GetVersion();
            if (DataCacheInstrVersionCDS.ContainsKey(instrumentCode))
                DataCacheInstrVersionCDS[instrumentCode] = instVersion;
            else
                DataCacheInstrVersionCDS.Add(instrumentCode, instVersion);
        }


        /// <summary>
        /// Renvoie la duration Modified non risky quel que soit le type d'instrument (fixed/ float/ FixedToFloat/ CDS)
        /// </summary>
        /// <param name="instrumentCode"></param>
        /// <param name="reportingdate"></param>
        /// <returns></returns>
        public double Compute_tko_modDuration_taux(int instrumentCode, int reportingdate, CSMPosition positionPtr)//int positionCode)
        {
            double TKO_duration_taux = 0;//duration
            double ytm;//rendement actuariel
            double dirtyPrice;
            double n;//nombre de flux par an

            PC_DurationCompute IDuration = PC_DurationCompute.GetInstance(instrumentCode, reportingdate);//Instance de la classe Duration

            int CreditInstrumentType = IDuration.GetCreditInstrumentType(instrumentCode, reportingdate);
            switch (CreditInstrumentType)
            {
                case 1://Fixed
                    TKO_duration_taux = IDuration.ComputeBondMacDurationByExplanationArray(instrumentCode, reportingdate, false);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 2://Float pur
                    TKO_duration_taux = IDuration.ComputeBondMacDurationByExplanationArray(instrumentCode, reportingdate, true);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 3://FixedToFloat
                    TKO_duration_taux = IDuration.ComputeBondMacDurationByExplanationArray(instrumentCode, reportingdate, true);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 4://CDS
                    CSMSwap CDS = CSMSwap.GetInstance(instrumentCode);
                    double spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
                    n = IDuration.GetCDSNbCFByYear(instrumentCode);
                    ytm = spot * 0.0001;
                    double nominal = positionPtr.GetInstrumentCount() * CSMInstrument.GetInstance(instrumentCode).GetNotional();
                    TKO_duration_taux = -CDS.GetRho() / (positionPtr.GetAssetValue()*1000/nominal);
                    break;
                default://Autre
                    n = 1;
                    ytm = 0;
                    break;
            }
            //Passage de la duration Macaulay à la duration Modified
            TKO_duration_taux = IDuration.GetModDurationByMacDuration(TKO_duration_taux, IDuration.Ytm, n);
            return TKO_duration_taux;
        }

     }

    public class DataSourceDurationCredit
    {
        // Holds the instance of the singleton class
        private static DataSourceDurationCredit Instance = null;

        // Used to cache the results
        public static Hashtable DataCacheDurationCred;
        public static Hashtable DataCacheInstrVersion;
        public static Hashtable DataCacheSpot;

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceDurationCredit()
        {
            DataCacheDurationCred = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceDurationCredit GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceDurationCredit();

            return Instance;
        }

        public double Get_tko_modDuration_credit(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int Instrumentcode = Instrument.GetCode();
            CSMMarketData context = CSMMarketData.GetCurrentMarketData();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0)
            {
                switch (Instrument.GetInstrumentType().ToString())
                {
                    case "O"://Obligation:
                        //La valeur est (re)calculée:
                        //si le cache est vide 
                        //si la version de l'instrument change
                        //si le spot change
                        if (!DataCacheDurationCred.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                        {
                            FillCache(Instrument);
                        }

                        // At this point, the value that this method should return must be available in the cache
                        if (DataCacheDurationCred.ContainsKey(Instrumentcode))
                        {
                            return (double)DataCacheDurationCred[Instrumentcode];
                        }
                        else { return 0; }//on ne devrait jamais passer par là ms au cas où

                    case "S"://CDS
                        //RERMARQUE: la duration dépend de valeur MtM mais on utilise quand même la table 
                        //dont les clefs sont les instruments (et non pas les positions) car un CDS ne peut être utilisé dans différentes positions (un contrat par position)

                        //La valeur est (re)calculée:
                        //si le cache est vide 
                        //si la version de l'instrument change
                        //si le spot change
                        if (!DataCacheDurationCred.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                        {
                            FillCache(Instrument);
                        }

                        // At this point, the value that this method should return must be available in the cache
                        if (DataCacheDurationCred.ContainsKey(Instrumentcode))
                        {
                            return (double)DataCacheDurationCred[Instrumentcode];
                        }
                        else { return 0; }//on ne devrait jamais passer par là ms au cas où

                    default:
                        return 0;
                }
            }
            else { return 0; }
        }

        private void FillCache(CSMInstrument Instrument)
        {
            int instrumentCode = Instrument.GetCode(); ;

            //Mise à jour de la valeur de la duration
            double tko_modDuration_credit = Compute_tko_modDuration_credit(instrumentCode, VersionClass.Get_ReportingDate());
            if (DataCacheDurationCred.ContainsKey(instrumentCode))
                DataCacheDurationCred[instrumentCode] = tko_modDuration_credit;
            else
                DataCacheDurationCred.Add(instrumentCode, tko_modDuration_credit);

            //Mise à jour de la version de l'instrument
            int instVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(instrumentCode))
                DataCacheInstrVersion[instrumentCode] = instVersion;
            else
                DataCacheInstrVersion.Add(instrumentCode, instVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
            if (DataCacheSpot.ContainsKey(instrumentCode))
                DataCacheSpot[instrumentCode] = Spot;
            else
                DataCacheSpot.Add(instrumentCode, Spot);
        }
        /// <summary>
        /// Renvoie la duration Modified non risky quel que soit le type d'instrument (fixed/ float/ FixedToFloat/ CDS)
        /// </summary>
        /// <param name="instrumentCode"></param>
        /// <param name="reportingdate"></param>
        /// <returns></returns>
        public double Compute_tko_modDuration_credit(int instrumentCode, int reportingdate)
        {
            double TKO_duration_credit = 0;//duration
            double ytm;//rendement actuariel
            double dirtyPrice;
            double n;//nombre de flux par an

            PC_DurationCompute IDuration = PC_DurationCompute.GetInstance(instrumentCode, reportingdate);//Instance de la classe Duration

            int CreditInstrumentType = IDuration.GetCreditInstrumentType(instrumentCode, reportingdate);
            switch (CreditInstrumentType)
            {
                case 1://Fixed
                    TKO_duration_credit = IDuration.ComputeBondMacDurationByExplanationArray(instrumentCode, reportingdate, false);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 2://Float pur
                    TKO_duration_credit = IDuration.ComputeBondMacDurationByExplanationArray(instrumentCode, reportingdate, false);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 3://FixedToFloat
                    TKO_duration_credit = IDuration.ComputeBondMacDurationByExplanationArray(instrumentCode, reportingdate, false);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 4://CDS
                    CSMSwap CDS = CSMSwap.GetInstance(instrumentCode);
                    double spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
                    n = IDuration.GetCDSNbCFByYear(instrumentCode);
                    ytm = spot * 0.0001;

                    TKO_duration_credit = IDuration.GetCDSMacDurationByExplanationArray(instrumentCode, reportingdate);
                    break;
                default://Autre
                    n = 1;
                    ytm = 0;
                    break;
            }
            //Passage de la duration Macaulay à la duration Modified
            TKO_duration_credit = IDuration.GetModDurationByMacDuration(TKO_duration_credit, IDuration.Ytm, n);
            return TKO_duration_credit;
        }


    }
    
    public class DataSourceRiskDurationTaux
    {
        // Holds the instance of the singleton class
        private static DataSourceRiskDurationTaux Instance = null;

        // Used to chache the results
        //Bond: la duration taux est (re)calculée quand l'instrument ou le spot change
        public static Hashtable DataCacheRDurationTauxBOND;
        public static Hashtable DataCacheInstrVersionBOND;
        public static Hashtable DataCacheSpotBOND;

        //CDS: la duration taux est (re)calculée quand l'utilisateur fait F8/F9
        //Le cache renvoie 0 quang l'instrument a changé 
        public static Hashtable DataCacheRDurationTauxCDS;
        public static Hashtable DataCacheInstrVersionCDS;

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceRiskDurationTaux()
        {
            DataCacheRDurationTauxBOND = new Hashtable();
            DataCacheInstrVersionBOND = new Hashtable();
            DataCacheSpotBOND = new Hashtable();

            DataCacheRDurationTauxCDS = new Hashtable();
            DataCacheInstrVersionCDS = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceRiskDurationTaux GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceRiskDurationTaux();

            return Instance;
        }

        public double Get_tko_modRiskDuration_taux(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int Instrumentcode = Instrument.GetCode();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            CSMMarketData context = CSMMarketData.GetCurrentMarketData();
            if (Nominal != 0){
                switch (Instrument.GetInstrumentType().ToString())
                {
                    case "O"://Obligation:
                        //La valeur est (re)calculée:
                        //si le cache est vide 
                        //si la version de l'instrument change
                        //si le spot change
                        if (!DataCacheRDurationTauxBOND.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersionBOND[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpotBOND[Instrumentcode] != context.GetSpot(Instrumentcode))
                        {
                            FillCacheBOND(Position,Instrument);
                        }

                        // At this point, the value that this method should return must be available in the cache
                        if (DataCacheRDurationTauxBOND.ContainsKey(Instrumentcode))
                        {
                            return (double)DataCacheRDurationTauxBOND[Instrumentcode];
                        }
                        else { return 0; }//on ne devrait jamais passer par là ms au cas où

                    case "S"://CDS
                        //RERMARQUE: la duration dépend de valeur MtM mais on utilise quand même une table par instrument
                        //dont les clefs sont les instruments (et non pas les positions) car un CDS ne peut être utilisé dans différentes positions (un contrat par position)

                        //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
                        if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
                        {
                            VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                            VersionClass.DeleteCache();
                        }

                        //Si la version de l'instr change, on met 0 ds le cache
                        if (DataCacheInstrVersionCDS.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersionCDS[Instrumentcode] != Instrument.GetVersion())
                        {
                            if (DataCacheRDurationTauxCDS.ContainsKey(Instrumentcode))
                                DataCacheRDurationTauxCDS[Instrumentcode] = 0.0;
                            else
                                DataCacheRDurationTauxCDS.Add(Instrumentcode, 0.0);

                            //mise à jour de la version de l'instrument
                            DataCacheInstrVersionCDS[Instrumentcode] = Instrument.GetVersion();
                        }

                        //La valeur est (re)calculée:
                        //si le cache est vide
                        if (!DataCacheRDurationTauxCDS.ContainsKey(Instrumentcode))
                        {
                            FillCacheCDS(Position, Instrument);
                        }

                        // At this point, the value that this method should return must be available in the cache
                        if (DataCacheRDurationTauxCDS.ContainsKey(Instrumentcode))
                        {
                            return (double)DataCacheRDurationTauxCDS[Instrumentcode];
                        }
                        else { return 0; }//on ne devrait jamais passer par là ms au cas où
                        
                    default:
                        return 0;
                }
            }
            else { return 0; }
        }

        private void FillCacheBOND(CSMPosition Position, CSMInstrument Instrument)
        {
            int instrumentCode = Instrument.GetCode();
            
            //Mise à jour de la valeur de la duration
            double tko_modRiskDuration_taux = Compute_tko_modRiskDuration_taux(instrumentCode, VersionClass.Get_ReportingDate(), Position);
            if (DataCacheRDurationTauxBOND.ContainsKey(instrumentCode))
                DataCacheRDurationTauxBOND[instrumentCode] = tko_modRiskDuration_taux;
            else
                DataCacheRDurationTauxBOND.Add(instrumentCode, tko_modRiskDuration_taux);

            //Mise à jour de la version de l'instrument
            int instVersion = Instrument.GetVersion();
            if (DataCacheInstrVersionBOND.ContainsKey(instrumentCode))
                DataCacheInstrVersionBOND[instrumentCode] = instVersion;
            else
                DataCacheInstrVersionBOND.Add(instrumentCode, instVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
            if (DataCacheSpotBOND.ContainsKey(instrumentCode))
                DataCacheSpotBOND[instrumentCode] = Spot;
            else
                DataCacheSpotBOND.Add(instrumentCode, Spot);
        }

        private void FillCacheCDS(CSMPosition Position, CSMInstrument Instrument)
        {
            int instrumentCode = Instrument.GetCode();

            //Mise à jour de la valeur de la duration
            double tko_modRiskDuration_taux = Compute_tko_modRiskDuration_taux(instrumentCode, VersionClass.Get_ReportingDate(), Position);
            if (DataCacheRDurationTauxCDS.ContainsKey(instrumentCode))
                DataCacheRDurationTauxCDS[instrumentCode] = tko_modRiskDuration_taux;
            else
                DataCacheRDurationTauxCDS.Add(instrumentCode, tko_modRiskDuration_taux);

            //Mise à jour de la version de l'instrument
            int instVersion = Instrument.GetVersion();
            if (DataCacheInstrVersionCDS.ContainsKey(instrumentCode))
                DataCacheInstrVersionCDS[instrumentCode] = instVersion;
            else
                DataCacheInstrVersionCDS.Add(instrumentCode, instVersion);
        }

        /// <summary>
        /// Renvoie la duration Modified non risky quel que soit le type d'instrument (fixed/ float/ FixedToFloat/ CDS)
        /// </summary>
        /// <param name="instrumentCode"></param>
        /// <param name="reportingdate"></param>
        /// <returns></returns>
        public double Compute_tko_modRiskDuration_taux(int instrumentCode, int reportingdate, CSMPosition positionPtr)
        {
            double tko_modRiskDuration_taux = 0;//duration
            double ytm;//rendement actuariel
            double dirtyPrice;
            double n;//nombre de flux par an

            PC_DurationCompute IDuration = PC_DurationCompute.GetInstance(instrumentCode, reportingdate);//Instance de la classe Duration

            int CreditInstrumentType = IDuration.GetCreditInstrumentType(instrumentCode, reportingdate);
            switch (CreditInstrumentType)
            {
                case 1://Fixed
                    tko_modRiskDuration_taux = IDuration.ComputeBondRiskyMacDurationByExplanationArray(instrumentCode, reportingdate, false);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 2://Float pur
                    tko_modRiskDuration_taux = IDuration.ComputeBondRiskyMacDurationByExplanationArray(instrumentCode, reportingdate, true);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 3://FixedToFloat
                    tko_modRiskDuration_taux = IDuration.ComputeBondRiskyMacDurationByExplanationArray(instrumentCode, reportingdate, true);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 4://CDS
                    CSMSwap CDS = CSMSwap.GetInstance(instrumentCode);
                    double spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
                    n = IDuration.GetCDSNbCFByYear(instrumentCode);
                    ytm = spot * 0.0001;
                    double nominal = positionPtr.GetInstrumentCount() * CSMInstrument.GetInstance(instrumentCode).GetNotional();
                    tko_modRiskDuration_taux = -CDS.GetRho() / (positionPtr.GetAssetValue() * 1000 / nominal);
                    break;
                default://Autre
                    n = 1;
                    ytm = 0;
                    break;
            }
            //Passage de la duration Macaulay à la duration Modified
            tko_modRiskDuration_taux = IDuration.GetModDurationByMacDuration(tko_modRiskDuration_taux, IDuration.Ytm, n);
            return tko_modRiskDuration_taux;
        }

    }
    
    public class DataSourceRiskDurationCredit
    {
        // Holds the instance of the singleton class
        private static DataSourceRiskDurationCredit Instance = null;

        // Used to chache the results
        public static Hashtable DataCacheRDurationCred;
        public static Hashtable DataCacheInstrVersion;
        public static Hashtable DataCacheSpot;

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceRiskDurationCredit()
        {
            DataCacheRDurationCred = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceRiskDurationCredit GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceRiskDurationCredit();

            return Instance;
        }

        public double Get_tko_modRiskDuration_credit(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int Instrumentcode = Instrument.GetCode();
            CSMMarketData context = CSMMarketData.GetCurrentMarketData();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0)
            {
                switch (Instrument.GetInstrumentType().ToString())
                {
                    case "O"://Obligation:
                        //La valeur est (re)calculée:
                        //si le cache est vide 
                        //si la version de l'instrument change
                        //si le spot change
                        if (!DataCacheRDurationCred.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                        {
                            FillCache(Instrument);
                        }

                        // At this point, the value that this method should return must be available in the cache
                        if (DataCacheRDurationCred.ContainsKey(Instrumentcode))
                        {
                            return (double)DataCacheRDurationCred[Instrumentcode];
                        }
                        else { return 0; }//on ne devrait jamais passer par là ms au cas où

                    case "S"://CDS
                        //RERMARQUE: la duration dépend de valeur MtM mais on utilise quand même la table 
                        //dont les clefs sont les instruments (et non pas les positions) car un CDS ne peut être utilisé dans différentes positions (un contrat par position)
                        
                        //La valeur est (re)calculée:
                        //si le cache est vide 
                        //si la version de l'instrument change
                        //si le spot change
                        if (!DataCacheRDurationCred.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                        {
                            FillCache(Instrument);
                        }

                        // At this point, the value that this method should return must be available in the cache
                        if (DataCacheRDurationCred.ContainsKey(Instrumentcode))
                        {
                            return (double)DataCacheRDurationCred[Instrumentcode];
                        }
                        else { return 0; }//on ne devrait jamais passer par là ms au cas où

                    default:
                        return 0;
                }
            }
            else { return 0; }
        }

        private void FillCache(CSMInstrument Instrument)
        {
            int instrumentCode = Instrument.GetCode();;

            //Mise à jour de la valeur de la duration
            double tko_modRiskDuration_credit = Compute_tko_modRiskDuration_credit(instrumentCode, VersionClass.Get_ReportingDate());
            if (DataCacheRDurationCred.ContainsKey(instrumentCode))
                DataCacheRDurationCred[instrumentCode] = tko_modRiskDuration_credit;
            else
                DataCacheRDurationCred.Add(instrumentCode, tko_modRiskDuration_credit);

            //Mise à jour de la version de l'instrument
            int instVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(instrumentCode))
                DataCacheInstrVersion[instrumentCode] = instVersion;
            else
                DataCacheInstrVersion.Add(instrumentCode, instVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
            if (DataCacheSpot.ContainsKey(instrumentCode))
                DataCacheSpot[instrumentCode] = Spot;
            else
                DataCacheSpot.Add(instrumentCode, Spot);

        }

        /// <summary>
        /// Renvoie la duration Modified non risky quel que soit le type d'instrument (fixed/ float/ FixedToFloat/ CDS)
        /// </summary>
        /// <param name="instrumentCode"></param>
        /// <param name="reportingdate"></param>
        /// <returns></returns>
        public double Compute_tko_modRiskDuration_credit(int instrumentCode, int reportingdate)
        {
            double TKO_Riskduration_credit = 0;//duration
            double ytm;//rendement actuariel
            double dirtyPrice;
            double n;//nombre de flux par an

            PC_DurationCompute IDuration = PC_DurationCompute.GetInstance(instrumentCode, reportingdate);//Instance de la classe Duration

            int CreditInstrumentType = IDuration.GetCreditInstrumentType(instrumentCode, reportingdate);
            switch (CreditInstrumentType)
            {
                case 1://Fixed
                    TKO_Riskduration_credit = IDuration.ComputeBondRiskyMacDurationByExplanationArray(instrumentCode, reportingdate, false);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 2://Float pur
                    TKO_Riskduration_credit = IDuration.ComputeBondRiskyMacDurationByExplanationArray(instrumentCode, reportingdate, false);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 3://FixedToFloat
                    TKO_Riskduration_credit = IDuration.ComputeBondRiskyMacDurationByExplanationArray(instrumentCode, reportingdate, false);
                    dirtyPrice = IDuration.GetBondDirtyPrice(instrumentCode, reportingdate);
                    n = IDuration.GetBondNbCFByYear(instrumentCode);
                    break;
                case 4://CDS
                    CSMSwap CDS = CSMSwap.GetInstance(instrumentCode);
                    double spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
                    n = IDuration.GetCDSNbCFByYear(instrumentCode);
                    ytm = spot * 0.0001;
                    TKO_Riskduration_credit = IDuration.GetCDSMacDurationByExplanationArray(instrumentCode, reportingdate);
                    break;
                default://Autre
                    n = 1;
                    ytm = 0;
                    break;
            }
            //Passage de la duration Macaulay à la duration Modified
            TKO_Riskduration_credit = IDuration.GetModDurationByMacDuration(TKO_Riskduration_credit, IDuration.Ytm, n);
            return TKO_Riskduration_credit;
        }


    }


    //Fonction de Calcul de duration crédit et Taux
    public class DataSourceDurationValue
    {
        // Holds the instance of the singleton class
        private static DataSourceDurationValue Instance = null;

        public static Hashtable DataCacheDurationValue;//Cahce pour le duration Value par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public Calc_Duration calcd;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceDurationValue()
        {
            DataCacheDurationValue = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            calcd = Calc_Duration.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceDurationValue GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceDurationValue();
            return Instance;
        }

        public double GetDurationValue(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
            if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
            {
                VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheDurationValue.ContainsKey(PositionIdentifier))
                        DataCacheDurationValue[PositionIdentifier] = 0.0;
                    else
                        DataCacheDurationValue.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheDurationValue.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheDurationValue.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheDurationValue[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }
        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {

            int PositionId = Position.GetIdentifier();
            double dur = calcd.ComputeDurationValue(Position, Instrument);
            if (DataCacheDurationValue.ContainsKey(PositionId))
                DataCacheDurationValue[PositionId] = dur;
            else
                DataCacheDurationValue.Add(PositionId, dur);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

        }

    }

    public class DataSourceDurationValueir
    {

        // Holds the instance of the singleton class
        private static DataSourceDurationValueir Instance = null;

        public static Hashtable DataCacheDurationValueir;//Cahce pour leTreeYTMValue par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public static Calc_Duration calc;
        public static CarryYtmCompute carry;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceDurationValueir()
        {
            DataCacheDurationValueir = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            calc = Calc_Duration.GetInstance();
            carry = CarryYtmCompute.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceDurationValueir GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceDurationValueir();
            return Instance;
        }


        public double GetDurationValueir(CSMPosition Position, CSMInstrument Instrument)
        {
            try
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug,
                        "BEGIN(Position={0}, Instrument={1})", Position.GetIdentifier(), Instrument.GetCode());
                }

                //Si la date de Sophis change, vide tous les caches
                if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
                {
                    VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                    VersionClass.DeleteCache();
                }

                //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
                if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
                {
                    VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion()); //Mise à jour de la refresh version
                    VersionClass.DeleteCache();
                }

                int PositionIdentifier =
                    Position.GetIdentifier(); //Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
                int Instrumentcode = Position.GetInstrumentCode();
                CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
                double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
                if (Nominal != 0)
                {
                    //Si la version de l'instr change, on met 0 ds le cache
                    if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                    {
                        if (DataCacheDurationValueir.ContainsKey(PositionIdentifier))
                            DataCacheDurationValueir[PositionIdentifier] = 0.0;
                        else
                            DataCacheDurationValueir.Add(PositionIdentifier, 0.0);

                        //mise à jour de la version de l'instrument
                        DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                    }

                    //La valeur est (re)calculée:
                    //si le cache est vide 
                    if (!DataCacheDurationValueir.ContainsKey(PositionIdentifier))
                    {
                        FillCache(Position, Instrument);
                    }

                    // At this point, the value that this method should return must be available in the cache
                    if (DataCacheDurationValueir.ContainsKey(PositionIdentifier))
                    {
                        return (double)DataCacheDurationValueir[PositionIdentifier];
                    }
                    else { return 0; }//on ne devrait jamais passer par là ms au cas où
                }
                else { return 0; }
            }
            catch (Exception e)
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                throw;
            }
            finally
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                }
            }
        }

        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {
            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(DataCacheDurationValueir.Count={0}, DataCacheInstrVersion.Count={1}, Position={2}, Instrument={3})",
                    DataCacheDurationValueir.Count, DataCacheInstrVersion.Count, Position.GetIdentifier(), Instrument.GetCode());
            }
            
            int PositionId = Position.GetIdentifier();
            double dur = ComputeTKODurationIr(Instrument,VersionClass.Get_ReportingDate(),Position);
            if (DataCacheDurationValueir.ContainsKey(PositionId))
                DataCacheDurationValueir[PositionId] = dur;
            else
                DataCacheDurationValueir.Add(PositionId, dur);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

            if (UpgradeExtensions.IsDebugEnabled())
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END(DataCacheDurationValueir.Count={0}, DataCacheInstrVersion.Count={1})",
                    DataCacheDurationValueir.Count, DataCacheInstrVersion.Count);
            }
        }

        public double ComputeTKODurationIr(CSMInstrument Instrument, int reportingdate, CSMPosition positionPtr)
        {
            //Ressources 
            //Classe de calcul
            Calc_Duration calc = Calc_Duration.GetInstance();
            CarryYtmCompute carry = CarryYtmCompute.GetInstance();

            //@SB
            CMString instrName = "";
            Instrument.GetName(instrName);
            CSMLog.Write("TKOIRDuration", "ComputeTKODurationIr", CSMLog.eMVerbosity.M_debug, "Getting TKO IR Duration Value for instrument : " + instrName + " Instrument code  : " + Instrument.GetCode().ToString() + " Allotment : " + Instrument.GetAllotment());
            //@SB


            int instrumentCode = Instrument.GetCode();
            double durIr = 0;//duration
            try
            {

                //@SB

                // Tikehau Specific
                if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesCTAllotmentID())
                {
                    // Tenor / 12
                    CSMFuture fut = CSMFuture.GetInstance(instrumentCode);
                    if (fut != null) //should always be true
                    {
                        SSMMaturity matur = fut.GetFormatedTenor();
                        return matur.fMaturity / 12.0;

                    }
                }
                else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRFuturesAllotmentID())
                {
                    // mod dur cheapest / conv. factor
                    sophis.finance.CSMNotionalFuture fut = sophis.finance.CSMNotionalFuture.GetInstance(instrumentCode);
                    if (fut != null) //should always be true
                    {
                        SSMCalcul data = new SSMCalcul();
                        int bondCode = fut.GetCheapest();
                        CSMBond bond = CSMBond.GetInstance(bondCode);
                        fut.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), bondCode, data);
                        return bond.GetModDuration(CSMMarketData.GetCurrentMarketData()) * 100.0 / data.fConcordanceFactor;
                    }
                }
                else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetListedOptionsAllotmentID())
                {
                    CSMLog.Write("TKOIRDuration", "ComputeTKODurationIr", CSMLog.eMVerbosity.M_debug, "Instrument : " + instrName + " is a listed options allotment ");

                    CSMOption opt = CSMOption.GetInstance(instrumentCode);
                    if (opt != null) //should always be true
                    {
                        CSMLog.Write("TKOIRDuration", "ComputeTKODurationIr", CSMLog.eMVerbosity.M_debug, "Instrument : " + instrName + " has been correctly casted to an option. ");

                        int underlyingCode = opt.GetUnderlying_API(0);
                        // mod dur cheapest / conv. factor
                        sophis.finance.CSMNotionalFuture notFut = sophis.finance.CSMNotionalFuture.GetInstance(underlyingCode);
                        CSMLog.Write("TKOIRDuration", "ComputeTKODurationIr", CSMLog.eMVerbosity.M_debug, "Instrument : " + instrName + " has underlying : " + underlyingCode.ToString());

                        if (notFut != null) //should always be true
                        {
                            CSMLog.Write("TKOIRDuration", "ComputeTKODurationIr", CSMLog.eMVerbosity.M_debug, "Instrument : " + instrName + " has underlying : " + underlyingCode.ToString() + " which is a notional future.");

                            SSMCalcul data = new SSMCalcul();
                            int bondCode = notFut.GetCheapest();
                            CSMBond bond = CSMBond.GetInstance(bondCode);
                            notFut.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), bondCode, data);
                            CSMLog.Write("TKOIRDuration", "ComputeTKODurationIr", CSMLog.eMVerbosity.M_debug, "Instrument : " + instrName + " has underlying : " + underlyingCode.ToString() + " which is a notional future. Its cheapest bond is : " + bondCode.ToString());

                            double delta = opt.GetDelta(ref underlyingCode);
                            CSMLog.Write("TKOIRDuration", "ComputeTKODurationIr", CSMLog.eMVerbosity.M_debug, "Instrument : " + instrName + " applying formula : delta = " + delta.ToString() + " factor = " + data.fConcordanceFactor.ToString() + " duration = " + bond.GetModDuration(CSMMarketData.GetCurrentMarketData()).ToString());

                            return bond.GetModDuration(CSMMarketData.GetCurrentMarketData()) * 100.0 / data.fConcordanceFactor;
                        }
                    }

                }
                else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsCTAllotmentID())
                {
                    // delta * tenor / 12
                    CSMOption opt = CSMOption.GetInstance(instrumentCode);
                    if (opt != null) //should always be true
                    {
                        int underlyingCode = 0;
                        double delta = opt.GetDelta(ref underlyingCode);
                        CSMFuture fut = CSMFuture.GetInstance(opt.GetUnderlyingCode());
                        if (fut != null) // should always be true
                        {
                            SSMMaturity matur = fut.GetFormatedTenor();
                            return matur.fMaturity / 12.0;
                        }
                    }
                }
                else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetCDSAllotmentID() ||
                    Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetTRSAllotmentID() ||
                    Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCStockDerivativesAllotmentID() ||
                    Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID() ||
                    Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOTCIRDerivativesAllotmentID()
                    )
                {
                    return -1 * Instrument.GetRho();
                }

                //else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIROptionsFuturesAllotmentID())
                //{
                //    // delta * mod dur cheapest / conv. factor
                //    CSMOption opt = CSMOption.GetInstance(instrumentCode); 
                //    if (opt != null) //should always be true
                //    {
                //        int underlyingCode = 0;
                //        double delta = opt.GetDelta(ref underlyingCode);
                //        sophis.finance.CSMNotionalFuture fut = sophis.finance.CSMNotionalFuture.GetInstance(opt.GetUnderlyingCode());
                //        if (fut != null) //should always be true
                //        {
                //            SSMCalcul data = new SSMCalcul();
                //            int bondCode = fut.GetCheapest();
                //            CSMBond bond = CSMBond.GetInstance(bondCode);
                //            fut.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), bondCode, data);
                //            return delta * bond.GetModDuration() / data.fConcordanceFactor;
                //        }
                //    }
                //}
                //else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetOptionsCDSAllotmentID())
                //{
                //    return 0;
                //}
                //else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetIRSwapsAllotmentID())
                //{
                //    // - Rho
                //    return -1 * Instrument.GetRho();
                //}
                //else if (Instrument.GetAllotment() == TKOAllotment.AllotmentData.GetSwaptionsAllotmentID())
                //{
                //    // Delta * IR Duration underlying swap
                //    CSMOption opt = CSMOption.GetInstance(instrumentCode);
                //    if (opt != null) // should always be true
                //    {
                //        int underlyingCode = 0;
                //        double delta = opt.GetDelta(ref underlyingCode);
                //        CSMSwap swap = CSMSwap.GetInstance(opt.GetUnderlyingCode());
                //        if (swap != null) // should always be true
                //        {
                //            return swap.GetDuration() * delta;
                //        }
                //    }
                //}
                // Default

                //@SB


                //Cas futures
                if (Instrument.GetInstrumentType() == 'F')
                {
                    return Instrument.GetDuration();
                }
                //Cas Obligation
                else if (Instrument.GetInstrumentType() == 'O' || Instrument.GetInstrumentType() == 'D')
                {
                        //Call/Worst/Maturity
                        int Tytm = 0;
                        int maturitydate = 0;
                        double yield = 0;
                        //calcul du cas 
                        Tytm = carry.ComputeTreeYTM(positionPtr, Instrument);

                        if (Tytm == 1 || Tytm == 4)
                        {
                            maturitydate = Instrument.GetExpiry();
                            yield = Instrument.GetYTMMtoM();
                        }
                        else if (Tytm == 2)
                        {
                            maturitydate = FonctionAdd.GetValuefromSophisDate(positionPtr, Instrument, "Date to Call MtM");// Sub not CMS et FirstCall>Today(YTC)
                            yield = FonctionAdd.GetValuefromSophisDouble(positionPtr, Instrument, "Yield to Call MtM")/100;
                        }
                        else if (Tytm == 3)
                        {
                            maturitydate = FonctionAdd.GetValuefromSophisDate(positionPtr, Instrument, "Date to Worst MtM");//Hy (YTW)
                            yield = FonctionAdd.GetValuefromSophisDouble(positionPtr, Instrument, "Yield to Worst MtM")/100; 
                        }
                        else maturitydate = 0;

                        //Type d'instrument (Fixed / Float / FixedOrFloat / CDS)
                        int Instrumenttype = calc.GetCreditInstrumentType(positionPtr, instrumentCode, reportingdate);

                        switch (Instrumenttype)
                        {
                            case 1://Fixed
                                //durIr = calc.ComputeDurationValue(positionPtr, Instrument);
                                durIr = calc.ComputeDurationIRFix(Instrument, reportingdate, maturitydate,yield);
                                break;
                            case 2://Float pur
                                durIr = calc.ComputeDurationIRFloat(Instrument, reportingdate, maturitydate);
                                break;
                            case 3://FixedToFloat
                                durIr = calc.ComputeDurationIRFixToFloat(Instrument, reportingdate, maturitydate,yield);
                                break;
                            case 4://CDS
                                CSMSwap CDS = CSMSwap.GetInstance(instrumentCode);
                                double spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
                                double nominal = positionPtr.GetInstrumentCount() * CSMInstrument.GetInstance(instrumentCode).GetNotional();
                                durIr = -CDS.GetRho() / (positionPtr.GetAssetValue() * 1000 / nominal);
                                break;
                            default://Autre
                                break;
                        }


                    return durIr;
                }
                else return 0;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }

 
    public class DataSourceDurationValueContrib
    {
        // Holds the instance of the singleton class
        private static DataSourceDurationValueContrib Instance = null;

        public static Hashtable DataCacheDurationValue;//Cahce pour le duration Value par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        public Calc_Duration calcd;
        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceDurationValueContrib()
        {
            DataCacheDurationValue = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            calcd = Calc_Duration.GetInstance();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceDurationValueContrib GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceDurationValueContrib();
            return Instance;
        }

        public double GetDurationValue(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
            if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
            {
                VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheDurationValue.ContainsKey(PositionIdentifier))
                        DataCacheDurationValue[PositionIdentifier] = 0.0;
                    else
                        DataCacheDurationValue.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheDurationValue.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheDurationValue.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheDurationValue[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }
        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {

            int PositionId = Position.GetIdentifier();
            double dur = calcd.ComputeDurationValue(Position, Instrument);
            if (DataCacheDurationValue.ContainsKey(PositionId))
                DataCacheDurationValue[PositionId] = dur;
            else
                DataCacheDurationValue.Add(PositionId, dur);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

        }

    }

    public class DataSourceDurationValueirContrib
    {
        // Holds the instance of the singleton class
        private static DataSourceDurationValueirContrib Instance = null;

        public static Hashtable DataCacheDurationirValue;//Cahce pour le duration Value par position
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceDurationValueirContrib()
        {
            DataCacheDurationirValue = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }

        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceDurationValueirContrib GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceDurationValueirContrib();
            return Instance;
        }

        public double GetDurationValueirContrib(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            //Si la version de Sophis change, vide les caches des colonnes dont la valeur dépend de la position
            if (VersionClass.Get_RefreshVersion() != CSMColumnConsolidate.GetRefreshVersion())
            {
                VersionClass.Set_RefreshVersion(CSMColumnConsolidate.GetRefreshVersion());//Mise à jour de la refresh version
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying dc ne fonctionne pas ds ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            CSMInstrument InstrumentPtr = CSMInstrument.GetInstance(Instrumentcode);
            double Nominal = Position.GetInstrumentCount() * InstrumentPtr.GetNotional();
            if (Nominal != 0)
            {
                //Si la version de l'instr change, on met 0 ds le cache
                if (DataCacheInstrVersion.ContainsKey(Instrumentcode) && (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    if (DataCacheDurationirValue.ContainsKey(PositionIdentifier))
                        DataCacheDurationirValue[PositionIdentifier] = 0.0;
                    else
                        DataCacheDurationirValue.Add(PositionIdentifier, 0.0);

                    //mise à jour de la version de l'instrument
                    DataCacheInstrVersion[Instrumentcode] = Instrument.GetVersion();
                }

                //La valeur est (re)calculée:
                //si le cache est vide 
                if (!DataCacheDurationirValue.ContainsKey(PositionIdentifier))
                {
                    FillCache(Position, Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheDurationirValue.ContainsKey(PositionIdentifier))
                {
                    return (double)DataCacheDurationirValue[PositionIdentifier];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }

        }
        private void FillCache(CSMPosition Position, CSMInstrument Instrument)
        {

            int PositionId = Position.GetIdentifier();
            double dur = ComputeTKODurationIr(Instrument, VersionClass.Get_ReportingDate(), Position);
            if (DataCacheDurationirValue.ContainsKey(PositionId))
                DataCacheDurationirValue[PositionId] = dur;
            else
                DataCacheDurationirValue.Add(PositionId, dur);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(Instrument.GetCode()))
                DataCacheInstrVersion[Instrument.GetCode()] = InstrVersion;
            else
                DataCacheInstrVersion.Add(Instrument.GetCode(), InstrVersion);

        }

        public double ComputeTKODurationIr(CSMInstrument Instrument, int reportingdate, CSMPosition positionPtr)
        {
            //Ressources 
            //Classe de calcul
            Calc_Duration calc = Calc_Duration.GetInstance();
            CarryYtmCompute carry = CarryYtmCompute.GetInstance();


            int instrumentCode = Instrument.GetCode();
            double durIr = 0;//duration
            try
            {

                //Cas futures
                if (Instrument.GetInstrumentType() == 'F')
                {
                    return Instrument.GetDuration();
                }
                //Cas Obligationml
                else if (Instrument.GetInstrumentType() == 'O' || Instrument.GetInstrumentType() == 'D')
                {
                    //Call/Worst/Maturity
                    int Tytm = 0;
                    int maturitydate = 0;
                    double yield = 0;
                    //calcul du cas 
                    Tytm = carry.ComputeTreeYTM(positionPtr, Instrument);

                    if (Tytm == 1 || Tytm == 4)
                    {
                        maturitydate = Instrument.GetExpiry();
                        yield = Instrument.GetYTMMtoM();
                    }
                    else if (Tytm == 2)
                    {
                        maturitydate = FonctionAdd.GetValuefromSophisDate(positionPtr, Instrument, "Date to Call MtM");// Sub not CMS et FirstCall>Today(YTC)
                        yield = FonctionAdd.GetValuefromSophisDouble(positionPtr, Instrument, "Yield to Call MtM")/100; 
                    }
                    else if (Tytm == 3)
                    {
                        maturitydate = FonctionAdd.GetValuefromSophisDate(positionPtr, Instrument, "Date to Worst MtM");//Hy (YTW)
                        yield = FonctionAdd.GetValuefromSophisDouble(positionPtr, Instrument, "Yield to Worst MtM")/100; 
                    }
                    else maturitydate = 0;

                    //Type d'instrum

                    int Instrumenttype = calc.GetCreditInstrumentType(positionPtr, instrumentCode, reportingdate);

                    switch (Instrumenttype)
                    {
                        case 1://Fixed
                            //durIr = calc.ComputeDurationValue(positionPtr, Instrument);
                            durIr = calc.ComputeDurationIRFix(Instrument, reportingdate, maturitydate, yield);
                            break;
                        case 2://Float pur
                            durIr = calc.ComputeDurationIRFloat(Instrument, reportingdate, maturitydate);
                            break;
                        case 3://FixedToFloat
                            durIr = calc.ComputeDurationIRFixToFloat(Instrument, reportingdate, maturitydate, yield);
                            break;
                        case 4://CDS
                            CSMSwap CDS = CSMSwap.GetInstance(instrumentCode);
                            double spot = CSMMarketData.GetCurrentMarketData().GetSpot(instrumentCode);
                            double nominal = positionPtr.GetInstrumentCount() * CSMInstrument.GetInstance(instrumentCode).GetNotional();
                            durIr = -CDS.GetRho() / (positionPtr.GetAssetValue() * 1000 / nominal);
                            break;
                        default://Autre
                            break;
                    }


                    return durIr;
                }
                else return 0;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

      
    }


}