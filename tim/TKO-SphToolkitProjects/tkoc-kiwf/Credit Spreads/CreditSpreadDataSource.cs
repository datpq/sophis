using System;
using System.Data;
using System.Collections;
using System.Threading;
using System.IO;
using System.Text;

using sophis;
using sophis.portfolio;
using sophis.misc;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;

namespace dnPortfolioColumn
{
    public class BinaryTreeNode : Node
    {
        public BinaryTreeNode() : base() { }
        public BinaryTreeNode(double FWrate, int Date, BinaryTreeNode NextL, BinaryTreeNode NextH, BinaryTreeNode PrevL, BinaryTreeNode PrevH)
        {
            base.Acces_FWRate = FWrate;
            base.Acces_Date = Date;

            NodeList2 Next = new NodeList2();
            Next[0] = NextL;
            Next[1] = NextH;
            NodeList2 Prev = new NodeList2();
            Prev[0] = PrevL;
            Prev[1] = PrevH;

            base.Acces_Next = Next;
            base.Acces_Prev = Prev;
        }
    }
    public class NodeList2 : CollectionBase
    {
        public NodeList2()
        {
            Node NodeL = new Node();
            Node NodeH = new Node();
            List.Add((Node)NodeL);
            List.Add((Node)NodeH);
        }
        public Node this[int index]
        {
            get
            {
                return ((Node)List[index]);
            }
            set
            {
                List[index] = value;
            }
        }
    }
    public class Node
    {
        private double FWRate;
        private int Date;
        private NodeList2 Next = null;
        private NodeList2 Prev = null;

        public double Acces_FWRate
        {
            get
            {
                return FWRate;
            }
            set
            {
                FWRate = value;
            }
        }
        public int Acces_Date
        {
            get
            {
                return Date;
            }
            set
            {
                Date = value;
            }
        }
        public NodeList2 Acces_Next
        {
            get
            {
                return Next;
            }
            set
            {
                Next = value;
            }
        }
        public NodeList2 Acces_Prev
        {
            get
            {
                return Prev;
            }
            set
            {
                Prev = value;
            }
        }
    }
    public class BinaryTree
    {
        //Tous les taux sur l'arbre binomial sont annualisés

        //Attributes
        private static BinaryTree Instance = null;
        private static BinaryTreeNode FWBinTree;
        private static int Currency;
        private static double Volatility;//IL FAUDRA CHANGER CELA POUR AVOIR UNE VRAIE VALEUR

        private static int Date;//Date du noeud courant dans l'arbre. Le 1er noeud a la date 0.
        private static int FirstNewRoad;//nombre de chemins valides dans l'arbre binomial de taux entre la racine et tous les noeuds de date N

        //Constructeur
        private BinaryTree(int Curr, double Vol, int TreeSize)
        {
            Currency = Curr;
            Volatility = Vol;
            ConstructFWBinTree(TreeSize);
        }
        public static BinaryTree GetInstance(int Currency, double Volatility, int TreeSize)
        {
            Instance = new BinaryTree(Currency, Volatility, TreeSize);
            return Instance;
        }

        //Accesseur
        public BinaryTreeNode GetFWBinTree()
        {
            return FWBinTree;
        }

        //Methods
        private static void ConstructFWBinTree(int TreeSize)
        {
            BinaryTreeNode LowestNodeAtPrevDate;//Noeud le plus bas dans l'arbre binomial à une date donnée

            int Reportingdate;
            double[] CashFlows;//Flux versés par l'obligation théorique
            double DiscountFactor;
            double ThBondMtM;//Valeur de l'obligation théorique en utilisant les taux FW marché
            double FWMtMrate;//Taux FW du marché
            double eqyc;//equivalent year count entre reporting date et autre date

            //Base de calcul dans pref.
            eMDayCountBasisType dcbt = CSMPreference.GetDayCountBasisType();

            //Initialisation: le 1er noeud est le taux F3M an date reportingdate
            Reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            FWMtMrate = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(Currency, Reportingdate, Reportingdate + 90);
            eqyc = CSMDayCountBasis.GetCSRDayCountBasis(dcbt).GetEquivalentYearCount(Convert.ToInt32(Reportingdate), Convert.ToInt32(Reportingdate + 90));
            FWMtMrate = Math.Pow((FWMtMrate), 1 / (eqyc)) - 1;//F3M à la date reportingdate
            
            Date = 0;
            FWBinTree = new BinaryTreeNode(FWMtMrate, Date, new BinaryTreeNode(), new BinaryTreeNode(), new BinaryTreeNode(), new BinaryTreeNode());

            LowestNodeAtPrevDate = FWBinTree;
            for (int i = 1; i <= TreeSize; i++)//Itération sur la date
            {

                //Initialisation du champ Date
                Date = i;

                //Construit les noeuds de la date N
                LowestNodeAtPrevDate = BuildNodesAtDateN(i, LowestNodeAtPrevDate);

                //Calcul de la valeur de l'obligation théorique en utilisant les taux forward du marché
                CashFlows = new double[i + 1];
                CashFlows = ComputeTheoreticalBond(i);
                ThBondMtM = 0;
                DiscountFactor = 1;
                for (int j = 0; j <= i; j++)//Itération sur les flux de l'obligation théorique
                {
                    //Taux FW du marché
                    if (j < 4)//3M, 6M, 9M, 12M >> taux F3m à maturité N
                    {
                        FWMtMrate = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(Currency, Reportingdate + j * 90, Reportingdate + (j + 1) * 90);
                        eqyc = CSMDayCountBasis.GetCSRDayCountBasis(dcbt).GetEquivalentYearCount(Convert.ToInt32(Reportingdate + j * 90), Convert.ToInt32(Reportingdate + (j + 1) * 90));
                        FWMtMrate = Math.Pow((FWMtMrate), 1 / (eqyc)) - 1;//F3M à la date reportingdate
                        DiscountFactor = DiscountFactor * Math.Exp(-FWMtMrate / 4);
                    }
                    else//1Y, 2Y, ... >> taux F1Y à maturité N
                    {
                        FWMtMrate = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(Currency, Reportingdate + (j - 3) * 365, Reportingdate + (j - 2) * 365);
                        eqyc = CSMDayCountBasis.GetCSRDayCountBasis(dcbt).GetEquivalentYearCount(Convert.ToInt32(Reportingdate + (j - 3) * 365), Convert.ToInt32(Reportingdate + (j - 2) * 365));
                        FWMtMrate = Math.Pow((FWMtMrate), 1 / (eqyc)) - 1;//F3M à la date reportingdate
                        DiscountFactor = DiscountFactor * Math.Exp(-FWMtMrate);
                    }
                    ThBondMtM += CashFlows[j] * DiscountFactor;
                }

                //Calcul des taux d'intéret sur l'arbre en date N:
                //Il faut que la valeur de l'obligation théorique calculée avec les taux FW du marché soit
                //égale à la valeur de l'obligation théorique calculée avec l'arbre binomial de taux.
                TargetFunction TargetComputeValuesAtDateN = new TargetFunction(ComputeRatesAtDateN);
                TargetValue(ThBondMtM, 0, 1, TargetComputeValuesAtDateN, 0.0001);
            }
        }
        /// <summary>
        /// Construit les N+1 noeuds de la date N en partant du noeud le + bas de la date N-1
        /// </summary>
        /// <param name="N"></param>
        /// <param name="LowestNodeAtPrevDate">Plus bas noued à la date N-1</param>
        /// <returns>Pointeur sur le noeud le + bas à la date N</returns>
        private static BinaryTreeNode BuildNodesAtDateN(int N, BinaryTreeNode LowestNodeAtPrevDate)
        {
            BinaryTreeNode Current;//Noeud courant (date N-1)
            BinaryTreeNode LastIterCurrent;//Noeud courant (date N-1) de l'itéraiton précédente
            NodeList2 NextList;//Liste des 2 noeuds suivants Current (date N) 
            NodeList2 PrevList;//Liste des 2 noeuds précédants Current (date N - 2)
            BinaryTreeNode PrevH;//Noeud haut précédent Current (date N-2)
            BinaryTreeNode PrevHNextH;//Noeud haut suivant PrevH (date N-1)
            BinaryTreeNode LowestNodeAtNDate;//Plus bas noeud de la date N -- valeur de retour

            //INITIALISATION: On part du noeud le + bas en date N-1
            Current = LowestNodeAtPrevDate;
            LastIterCurrent = Current;
            NextList = Current.Acces_Next;

            //création du noeud L suivant Prev en date N
            NextList[0] = new BinaryTreeNode(0, N, new BinaryTreeNode(), new BinaryTreeNode(), new BinaryTreeNode(), Current);
            LowestNodeAtNDate = (BinaryTreeNode)NextList[0];

            //création du noeud H suivant Prev en date N
            PrevList = Current.Acces_Prev;
            PrevH = (BinaryTreeNode)PrevList[1];
            if (N == 1) { PrevHNextH = new BinaryTreeNode(); }
            else { PrevHNextH = (BinaryTreeNode)PrevH.Acces_Next[1]; }
            NextList[1] = new BinaryTreeNode(0, N, new BinaryTreeNode(), new BinaryTreeNode(), Current, PrevHNextH);

            //Création des N-1 autres noeuds à la date N
            for (int i = 0; i < N - 1; i++)
            {
                Current = (BinaryTreeNode)PrevH.Acces_Next[1];
                NextList = Current.Acces_Next;
                PrevList = Current.Acces_Prev;
                PrevH = (BinaryTreeNode)PrevList[1];

                //Le noeud L suivant Prev en date N est déjà créé: c'est le noeud H créé à l'itération précédente
                NextList[0] = LastIterCurrent.Acces_Next[1];

                //création du noeud H suivant Prev en date N
                if (i == N - 2) { PrevHNextH = new BinaryTreeNode(); }
                else { PrevHNextH = (BinaryTreeNode)PrevH.Acces_Next[1]; }
                NextList[1] = new BinaryTreeNode(0, N, new BinaryTreeNode(), new BinaryTreeNode(), Current, PrevHNextH);

                LastIterCurrent = Current;
            }
            return LowestNodeAtNDate;
        }
        ///// <summary>
        ///// Calcule les taux d'intérets en date N de l'arbre binomial de taux.
        ///// Les étapes sont:
        ///// - Calcul des taux possibles en supposant connu le taux le + bas et la volatilité des taux
        ///// - Mise à jour des valeurs en date N sur l'arbre binomial de taux
        ///// - Resencement de tous les "chemins" de taux possibles jusqu'à la date N
        ///// - Calcul de la valeur de l'obligation théorique en utilisant l'arbre binomial
        ///// </summary>
        ///// <param name="N">Date N: le premier de l'arbre noeud correspond à la date 0</param>
        ///// <param name="Volatility">Volatilité annualisée des taux d'intéret</param>
        ///// <param name="FWBinTree">Arbre binomial des taux d'intérets (dans son itégralité)</param>
        ///// <returns>La valeur de l'obligation théorique calculée en utilisant l'arbre binomial de taux</returns>
        private static double ComputeRatesAtDateN(double Parameter)
        {
            int N = Date;

            //VARIABLES
            BinaryTreeNode Current;//Noeud courant
            BinaryTreeNode Prev;//Noeud precédent (haut ou bas)
            BinaryTreeNode LowestNode;//Noeud le plus bas dans l'arbre à la date N (plus petit taux de la date N)

            double[] BinTreeRates;//N+1 taux possibles à la date N sur l'arbre binomial
            double[][] AllRoads;//Tous les chemins de taux d'intérets du 1er noeud aux noeuds de la date N
            double[] Road;//Un chemin de taux d'intéret parmis ceux dans AllRoads
            double[] PVperRoad;//Valeur présente de l'obligation théorique pour les 2^N chemins sur l'arbre de taux 
            double[] CashFlows;//Flux versés par l'obligation théorique
            double DiscountFactor;//Facteur d'actualisation
            double ThValueByBinTree;//Valeur de l'obligation théorique calculée avec l'arbre binomial de taux

            //CALCUL DES TAUX POSSIBLES A LA DATE N A PARTIR DU TAUX LE + BAS ET DE LA VOLATILITE
            BinTreeRates = new double[N + 1];
            BinTreeRates[0] = Parameter;//taux le plus bas variable à changer quand on fait la valeur cible

            if (N < 4)//Volatilité sur 3 mois = Volatilité annuelle / Sqrt(4)
            {
                for (int i = 1; i < N + 1; i++)
                {
                    BinTreeRates[i] = Math.Exp(2 * Volatility / Math.Sqrt(4) + Math.Log(BinTreeRates[i - 1]));
                }
            }
            else//Volatilité sur 1 an
            {
                for (int i = 1; i < N + 1; i++)
                {
                    BinTreeRates[i] = Math.Exp(2 * Volatility + Math.Log(BinTreeRates[i - 1]));
                }
            }

            //REMPLI L'ARBRE BINOMIAL DES TAUX A LA DATE N AVEC LES TAUX CALCULES
            Current = FWBinTree;//Initialisation: on commence le parcours par la racine
            //Atteindre le noeud de plus basse valeur de taux en date N dans l'arbre
            for (int i = 0; i < N; i++)
            {
                Current = (BinaryTreeNode)Current.Acces_Next[0];
            }
            LowestNode = Current;
            LowestNode.Acces_FWRate = BinTreeRates[0];
            //Atteindre les autres noeuds de la date N
            for (int i = 0; i < N; i++)
            {
                Prev = (BinaryTreeNode)Current.Acces_Prev[1];
                Current = (BinaryTreeNode)Prev.Acces_Next[1];
                Current.Acces_FWRate = BinTreeRates[i + 1];
            }
            //A ce stade, l'arbre de taux est rempli jusqu'à la date N

            //ENSEMBLE DES CHEMINS DE TAUX POSSIBLES DU 1er NOEUD AUX NOEUDS DE LA DATE N
            AllRoads = new double[(int)Math.Pow(2, N)][];
            Road = new double[1];
            Road[0] = FWBinTree.Acces_FWRate;
            Current = LowestNode;
            //Parcours tous les noeuds de la date N 
            //cherche tous les chemins entre le premier noeud et les noeuds de date N: stockés dans AllRoads
            FirstNewRoad = 0;
            for (int i = 0; i < N + 1; i++)
            {
                Roadmap(AllRoads, N, Road, FWBinTree, Current);
                if (i != N)
                {
                    Prev = (BinaryTreeNode)Current.Acces_Prev[1];
                    Current = (BinaryTreeNode)Prev.Acces_Next[1];
                }
            }

            //FLUX VERSES PAR L'OBLIGATION THEORIQUE DE LA DATE INITIALE A LA DATE N
            CashFlows = new double[N + 1];
            CashFlows = ComputeTheoreticalBond(N);

            //VALEUR DE L'OBLIGATION THEORIQUE EN UTILISANT L'ARBRE BINOMIAL DE TAUX
            PVperRoad = new double[(int)Math.Pow(2, N)];
            ThValueByBinTree = 0;
            for (int i = 0; i < (int)Math.Pow(2, N); i++)
            {
                PVperRoad[i] = 0;
                DiscountFactor = 1;
                for (int k = 0; k < N + 1; k++)
                {
                    if (k < 4) { DiscountFactor = DiscountFactor * Math.Exp(-AllRoads[i][k] / 4); }//0M, 3M, 6M, 9M
                    else { DiscountFactor = DiscountFactor * Math.Exp(-AllRoads[i][k]); }//1Y, 2Y, ...
                    PVperRoad[i] += CashFlows[k] * DiscountFactor;
                }
                ThValueByBinTree += PVperRoad[i];
            }
            ThValueByBinTree = ThValueByBinTree / (int)Math.Pow(2, N);
            return ThValueByBinTree;
        }
        /// <summary>
        /// Trouve tous les chemins entre les noeuds Src et Dest
        /// </summary>
        /// <param name="AllRoads">Stocke les chemins valides</param>
        /// <param name="N"> Dernière date des chemins cherchés</param>
        /// <param name="Road"> Stocke le chemin parcouru jusqu'ici</param>
        /// <param name="Src"> Pointeur sur le noeud source</param>
        /// <param name="Dest"> Pointeur sur le noeud destination</param>
        private static void Roadmap(double[][] AllRoads, int N, double[] Road, BinaryTreeNode Src, BinaryTreeNode Dest)
        {
            double[] newRoad = new double[Road.GetLength(0) + 1];

            if (Src == Dest)
            {
                AllRoads[FirstNewRoad] = new double[N + 1];
                for (int i = 0; i < N + 1; i++)//Recopie du chemin valide dans AllRoads
                {
                    AllRoads[FirstNewRoad][i] = Road[i];
                }
                FirstNewRoad++;
            }
            else
            {
                if (Src.Acces_Date < Dest.Acces_Date)//Sinon, le chemin que l'on est en train de parcourir n'attend pas la destination souhaitée
                {
                    //recopie les valeurs de Road dans newRoad
                    for (int j = 0; j < Road.GetLength(0); j++)
                    {
                        newRoad[j] = Road[j];
                    }

                    //Chemin haut
                    newRoad[Road.GetLength(0)] = Src.Acces_Next[1].Acces_FWRate;
                    Roadmap(AllRoads, N, newRoad, (BinaryTreeNode)Src.Acces_Next[1], Dest);

                    //Chemin bas
                    newRoad[Road.GetLength(0)] = Src.Acces_Next[0].Acces_FWRate;
                    Roadmap(AllRoads, N, newRoad, (BinaryTreeNode)Src.Acces_Next[0], Dest);
                }
            }
        }
        /// <summary>
        /// Fonction "Valeur Cible" pour une fonction monotone décroissante
        /// </summary>
        /// <param name="Target">Résultat à atteindre en sortie de Function</param>
        /// <param name="LBorder">Borne inférieure limitant la valeur cible Target</param>
        /// <param name="HBorder">Borne supérieure limitant la valeur cible Target</param>
        /// <param name="Function">Fonction dont on veut que le résultat soit égal à Target. Elle doit être monotone décroissante</param>
        /// <param name="Precision">Précision à atteindre entre </param>
        /// <returns>La  valeur qui permet d'approcher Target à "precision" près</returns>
        private static double TargetValue(double Target, double LBorder, double HBorder, TargetFunction Function, double Precision)
        {
            double TheParam;//Parametre de Function permettant d'approcher au mieux Target
            double ResTest = 0;//Valeur retournée par Function pour le paramètre testé
            double ParamTest;//
            int nbiter = 0;
            while ((HBorder - LBorder) > Precision && nbiter < 1000)
            {
                ParamTest = (LBorder + HBorder) * 0.5;
                ResTest = Function(ParamTest);

                if (ResTest > Target)
                {
                    LBorder = ParamTest;
                }
                else
                {
                    HBorder = ParamTest;
                }
                nbiter++;
            }
            TheParam = ((LBorder + HBorder) * 0.5);
            return TheParam;
        }
        delegate double TargetFunction(double d);//Equivalent à un pointeur de fonction
        /// <summary>
        /// Calcule les flux versés par l'obligation théorique de la date initiale à la date N
        /// L'obligation théorique est de maturité N et verse un coupon égal au taux de swap interbancaire
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        private static double[] ComputeTheoreticalBond(int N)
        {
            double[] CashFlows = new double[N + 1];
            int Reportingdate;
            double EuriborSwapRate;
            eMDayCountBasisType dcbt = CSMPreference.GetDayCountBasisType();
            double eqyc;//equivalent year count entre reporting date et autre date

            int j;//Compteur 

            //Le coupon de l'obligation théorique est le taux de swap interbancaire valeur reportingdate maturité N
            Reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            if (N < 4)//0M, 3M, 6M, 9M 
            {
                EuriborSwapRate = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(Currency, Reportingdate, Reportingdate + (N + 1) * 90);
                eqyc = CSMDayCountBasis.GetCSRDayCountBasis(dcbt).GetEquivalentYearCount(Convert.ToInt32(Reportingdate), Convert.ToInt32(Reportingdate + (N + 1) * 90));
                EuriborSwapRate = Math.Pow((EuriborSwapRate), 1 / (eqyc)) - 1;//F3M, F6M, F9M, F12M à la date reportingdate

            }
            else//1Y, 2Y, ...
            {
                EuriborSwapRate = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(Currency, Reportingdate, Reportingdate + (N - 2) * 365);
                eqyc = CSMDayCountBasis.GetCSRDayCountBasis(dcbt).GetEquivalentYearCount(Convert.ToInt32(Reportingdate), Convert.ToInt32(Reportingdate + (N - 2) * 365));
                EuriborSwapRate = Math.Pow((EuriborSwapRate), 1 / (eqyc)) - 1;//F2Y, F3Y, ... à la date reportingdate
            }

            j = 0;
            while (j < 4 && j < N + 1)//0M, 3M, 6M, 9M
            {
                CashFlows[j] = EuriborSwapRate / 4;
                j++;
            }
            for (int i = j; i < N + 1; i++)
            {
                CashFlows[j] = EuriborSwapRate;
            }
            CashFlows[N] = CashFlows[N] + 100;

            return CashFlows;
        }
    }
    public class OptionAdjustedSpread
    {
        //Attributs
        private static OptionAdjustedSpread Instance = null;
        private static double OAS = 0;
        private static BinaryTreeNode FWBinTree;
        private static int FWBinTreeSize;
        private static double[] CallOptions;//tableau de taille la maturité de l'obligation avec la valeur des call à chanque échéance
        private static double[] BondCF;//tableau des flux de l'obligation
        private static double Redemption;
        private static int Size;//Nombre de flux de l'obligation
        private static CSMInstrument InstrumentPtr;//instrument: obligation
        private static int reportingdate;//date courante de Sophis
        private static sophis.static_data.eMDayCountBasisType DayCountBasisType;//base de calcul des flux de l'obligation

        //Constructeur
        private OptionAdjustedSpread(CSMInstrument Instrument, BinaryTreeNode BinTree, int BinTreeSize)
        {
            InstrumentPtr = Instrument;
            CSMApi.Log("Début OptionAdjustedSpread for instrument : " + Instrument.GetCode(), true);
            reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            DayCountBasisType = Instrument.GetMarketYTMDayCountBasisType();

            //Arbre binomial
            FWBinTree = BinTree;
            FWBinTreeSize = BinTreeSize;

            //Size
            Size = 0;
            GetBondCFSize();

            //Remplir BondCF
            Redemption = 0;
            GetBondCF(Size);

            //Remplir CallOptions;
            GetCallOptions(Size);

            OAS = ComputeOAS();
        }
        public static OptionAdjustedSpread GetInstance(CSMInstrument Instrument, BinaryTreeNode BinTree, int BinTreeSize)
        {
            Instance = new OptionAdjustedSpread(Instrument, BinTree, BinTreeSize);
            return Instance;
        }

        //Accesseurs
        public double GetOAS()
        {
            return OAS;
        }

        //Methods
        private double ComputeOAS()
        {
            double OASSpread = 0;
            try
            {
                double CallBondMtM = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentPtr.GetCode());//choper le prix sophis en date de reportingdate
                //double CallBondMtM = GetBondDirtyPrice(InstrumentPtr.GetCode(), reportingdate);
                TargetFunction TargetComputeValuesAtDateN = new TargetFunction(ComputeCallBondTh);
                OASSpread = TargetValue(CallBondMtM, 0, 0.5, ComputeCallBondTh, 0.0001);//Prix inclus entre 50% et 150%
                return OASSpread;
            }
            catch (Exception)
            {
                CSMApi.Log("OAS cannot be computed for instrument " + InstrumentPtr.GetCode(), true);
                return 0;
            }
        }
        private static double ComputeCallBondTh(double SpreadParameter)
        {
            double ThValueByBinTree = 0;
            double[] ValuesAtN = new double[Size + 1];//Tableau de taille le nombre de noeud en date de maturité
            BinaryTreeNode LowestNodeAtN;//Noeud le plus bas dans l'arbre de taux à la adte N

            //Initialisation
            //Le flux final est connu, c'est le pair et le dernier coupon. 
            for (int j = 0; j < Size + 1; j++)
            {
                ValuesAtN[j] = Redemption;
            }

            //initialisation de LowestNodeAtN
            LowestNodeAtN = FWBinTree;
            for (int k = 0; k < Size - 1; k++)
            {
                LowestNodeAtN = (BinaryTreeNode)LowestNodeAtN.Acces_Next[0];
            }//LowestNodeAtN est le plus bas noeud de la date précédant la date de maturité

            //itération sur les Size échéances.
            for (int i = 0; i < Size; i++)
            {
                ValuesAtN = ComputeCallBondThAtN(Size - i - 1, ValuesAtN, LowestNodeAtN, SpreadParameter);
                if (i < Size - 1)
                {
                    LowestNodeAtN = (BinaryTreeNode)LowestNodeAtN.Acces_Prev[1];
                }
            }
            ThValueByBinTree = ValuesAtN[0];
            return ThValueByBinTree;
        }
        private static double[] ComputeCallBondThAtN(int N, double[] ValueAtNplus1, BinaryTreeNode LowestNodeAtN, double SpreadParameter)
        {
            BinaryTreeNode CurrentN;
            double[] ValuesAtN = new double[N + 1];

            //Inititalisation
            CurrentN = LowestNodeAtN;
            for (int i = 0; i < N + 1; i++)
            {
                if (N < 5)
                {
                    ValuesAtN[N - i] = 0.5 * (ValueAtNplus1[N - i + 1] + ValueAtNplus1[N - i] + BondCF[N] + BondCF[N]) / Math.Pow(1 + CurrentN.Acces_FWRate + SpreadParameter, 0.25);
                }
                else
                {
                    ValuesAtN[N - i] = 0.5 * (ValueAtNplus1[N - i + 1] + ValueAtNplus1[N - i] + BondCF[N] + BondCF[N]) / (1 + CurrentN.Acces_FWRate + SpreadParameter);
                }
                if (CallOptions[N] > 0 && ValuesAtN[N - i] > CallOptions[N]) { ValuesAtN[N - i] = CallOptions[N]; }//exercice du call si prix supérieur au prix d'achat
                if (i < N)
                {
                    CurrentN = (BinaryTreeNode)CurrentN.Acces_Next[1].Acces_Prev[1];
                }
            }
            return ValuesAtN;
        }

        /// <summary>
        /// Fonction "Valeur Cible" pour une fonction monotone décroissante
        /// </summary>
        /// <param name="Target">Résultat à atteindre en sortie de Function</param>
        /// <param name="LBorder">Borne inférieure limitant la valeur cible Target</param>
        /// <param name="HBorder">Borne supérieure limitant la valeur cible Target</param>
        /// <param name="Function">Fonction dont on veut que le résultat soit égal à Target. Elle doit être monotone décroissante</param>
        /// <param name="Precision">Précision à atteindre entre </param>
        /// <returns>La  valeur qui permet d'approcher Target à "precision" près</returns>
        private static double TargetValue(double Target, double LBorder, double HBorder, TargetFunction Function, double Precision)
        {
            double TheParam;//Parametre de Function permettant d'approcher au mieux Target
            double ResTest = 0;//Valeur retournée par Function pour le paramètre testé
            double ParamTest;//
            int nbiter = 0;
            while ((HBorder - LBorder) > Precision && nbiter < 1000)
            {
                ParamTest = (LBorder + HBorder) * 0.5;
                ResTest = Function(ParamTest);

                if (ResTest > Target)
                {
                    LBorder = ParamTest;
                }
                else
                {
                    HBorder = ParamTest;
                }
                nbiter++;
            }
            TheParam = ((LBorder + HBorder) * 0.5);
            return TheParam;
        }
        //Equivalent à un pointeur de fonction
        delegate double TargetFunction(double d);

        private void GetBondCFSize()
        {
            CSMBond Bond = CSMBond.GetInstance(InstrumentPtr.GetCode());//Création de l'obligation à partir de son sicovam
            int MaturityDate = Bond.GetMaturity();
            double Maturity = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, MaturityDate);

            if (Maturity < 0.25) { Size = 1; }//[0-3M[
            if (Maturity >= 0.25 && Maturity < 0.5) { Size = 2; }//[3M-6M[
            if (Maturity >= 0.5 && Maturity < 0.75) { Size = 3; }//[6M-9M[
            if (Maturity >= 0.75 && Maturity < 1) { Size = 4; }//[9M-12M[
            if (Maturity >= 1) { Size = (int)Maturity + 4; }//[1Y-2Y[ et plus

            if (Size > FWBinTreeSize) { Size = FWBinTreeSize; }
        }
        private void GetBondCF(int Size)
        {
            BondCF = new double[Size];
            CSMBond Bond = CSMBond.GetInstance(InstrumentPtr.GetCode());//Création de l'obligation à partir de son sicovam

            //Informations sur l'obligation
            int MaturityDate = Bond.GetMaturity();
            int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
            int PariPassudate = Bond.GetPariPassuDate(reportingdate);
            double IthDate = 0;

            //Table des CF restants de l'obligation (table "Explanation" de Sophis)
            System.Collections.ArrayList explicationArray = new System.Collections.ArrayList();
            SSMExplication explication = new SSMExplication();
            SSMRedemption IthRedemption;
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            explication.transactionDate = reportingdate;
            explication.settlementDate = reportingdate + SettlementShift;
            explication.pariPassuDate = PariPassudate;
            explication.endDate = MaturityDate;
            explication.withCredit = false;
            explication.discounted = false;
            Bond.GetRedemptionExplication(Context, explicationArray, explication);

            for (int i = 0; i < explicationArray.Count; i++)
            {
                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[i];
                IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);
                if (IthDate < 0.25) { BondCF[0] += IthRedemption.coupon / Bond.GetNotional() * 100; }//[0-3M[
                if (IthDate >= 0.25 && IthDate < 0.5) { BondCF[1] += IthRedemption.coupon / Bond.GetNotional() * 100; }//[3M-6M[
                if (IthDate >= 0.5 && IthDate < 0.75) { BondCF[2] += IthRedemption.coupon / Bond.GetNotional() * 100; }//[6M-9M[
                if (IthDate >= 0.75 && IthDate < 1) { BondCF[3] += IthRedemption.coupon / Bond.GetNotional() * 100; }//[9M-12M[
                if (IthDate >= 1 && IthDate + 3 < Size) { BondCF[(int)IthDate + 3] += IthRedemption.coupon / Bond.GetNotional() * 100; }//[1Y-2Y[ et plus

                if (IthDate + 3 >= Size) { Redemption += IthRedemption.coupon / Bond.GetNotional() * 100; }//Flux versés a des dates pour lesquelles l'arbre n'est plus construit
            }
            IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[explicationArray.Count - 1];//Pour l'instant, on ne considère que les remboursement in fine
            Redemption += IthRedemption.redemption / Bond.GetNotional() * 100;


            /////////////A CHANGER :::::: QUE PREND T ON COMME TAUX ??? //////////////////
            if (IthDate + 3 >= Size)//Il y a des flux postérieurs à l'arbre de taux.
            {
                IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate) - Size;
                Redemption = Redemption * Math.Exp(-IthDate * 0.01);
            }
        }//Tous les flux sont en %
        private void GetCallOptions(int Size)
        {
            CallOptions = new double[Size];
            double IthDate;
            ArrayList Clauses = new ArrayList();
            InstrumentPtr.GetClauseInformation(Clauses);

            SSMClause IthClause = new SSMClause();
            for (int i = 0; i < Clauses.Count; i++)
            {
                IthClause = (SSMClause)Clauses[i];
                if (IthClause.type == 2)
                {
                    if (IthClause.start_date == IthClause.end_date)//option européenne
                    {
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthClause.start_date);
                        if (IthDate + 3 >= Size) { IthDate = Size - 1; }//Calls a des dates pour lesquelles l'arbre n'est plus construit (ca ne devrait pas arriver) >> on prend la dernière valeur de call

                        if (IthDate > 0 && IthDate < 0.25) { CallOptions[0] = IthClause.value.value; }//[0-3M[
                        if (IthDate >= 0.25 && IthDate < 0.5) { CallOptions[1] = IthClause.value.value; }//[3M-6M[
                        if (IthDate >= 0.5 && IthDate < 0.75) { CallOptions[2] = IthClause.value.value; }//[6M-9M[
                        if (IthDate >= 0.75 && IthDate < 1) { CallOptions[3] = IthClause.value.value; }//[9M-12M[
                        if (IthDate >= 1) { CallOptions[(int)IthDate + 3] = IthClause.value.value; }//[1Y-2Y[ et plus
                    }
                    else//option américaine
                    {
                        int USoptionStart = 0;//indice à partir duquel on rempli le tableau "CallOptions"
                        IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthClause.start_date);
                        if (IthDate + 3 >= Size) { IthDate = Size - 1; }//Calls a des dates pour lesquelles l'arbre n'est plus construit (ca ne devrait pas arriver) >> on prend la dernière valeur de call

                        if (IthDate > 0 && IthDate < 0.25) { USoptionStart = 0; CallOptions[0] = IthClause.value.value; }//[0-3M[
                        if (IthDate >= 0.25 && IthDate < 0.5) { USoptionStart = 1; CallOptions[1] = IthClause.value.value; }//[3M-6M[
                        if (IthDate >= 0.5 && IthDate < 0.75) { USoptionStart = 2; CallOptions[2] = IthClause.value.value; }//[6M-9M[
                        if (IthDate >= 0.75 && IthDate < 1) { USoptionStart = 3; CallOptions[3] = IthClause.value.value; }//[9M-12M[
                        if (IthDate >= 1) { USoptionStart = (int)IthDate + 3; CallOptions[(int)IthDate + 3] = IthClause.value.value; }//[1Y-2Y[ et plus

                        for (int j = USoptionStart + 1; j < Size; j++)//A toutes les échéances suivantes, l'option peut être exercée
                        {
                            CallOptions[j] = IthClause.value.value;
                        }
                    }
                }
            }
        }
        private double GetBondDirtyPrice(int instrumentCode, int reportingdate)
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
    public class ZSpread
    {
        //Attributs
        private static ZSpread Instance = null;
        private static CSMInstrument InstrumentPtr;//instrument: obligation
        private static int reportingdate;//date courante de Sophis
        private static int Currency;
        private static sophis.static_data.eMDayCountBasisType DayCountBasisType;//base de calcul des flux de l'obligation
        private double Zspread;

        //Constructeur
        private ZSpread(CSMInstrument Instrument)
        {
            InstrumentPtr = Instrument;
            CSMApi.Log("Début ZSpread for instrument : " + Instrument.GetCode(), true);
            reportingdate = CSMMarketData.GetCurrentMarketData().GetDate();
            DayCountBasisType = Instrument.GetMarketYTMDayCountBasisType();
            Currency = InstrumentPtr.GetCurrency();
            Zspread = ComputeZSpread();
        }
        public static ZSpread GetInstance(CSMInstrument Instrument)
        {
            Instance = new ZSpread(Instrument);
            return Instance;
        }

        //Accesseurs
        public double GetZSpread()
        {
            return Zspread;
        }

        //Methods
        private double ComputeBondTh(double SpreadParameter)
        {
            double BondThPrice = 0;
            CSMBond Bond = CSMBond.GetInstance(InstrumentPtr.GetCode());//Création de l'obligation à partir de son sicovam
            double DiscountFactor = 0;

            //Informations sur l'obligation
            int MaturityDate = Bond.GetMaturity();
            int SettlementShift = Bond.GetPaymentShift();//Gets the payment shift as specified by the bond's Market Data
            int PariPassudate = Bond.GetPariPassuDate(reportingdate);
            double IthDate = 0;
            double MtMrate;

            //Table des CF restants de l'obligation (table "Explanation" de Sophis)
            System.Collections.ArrayList explicationArray = new System.Collections.ArrayList();
            SSMExplication explication = new SSMExplication();
            SSMRedemption IthRedemption;
            CSMMarketData Context = CSMMarketData.GetCurrentMarketData();//Données de marché
            explication.transactionDate = reportingdate;
            explication.settlementDate = reportingdate + SettlementShift;
            explication.pariPassuDate = PariPassudate;
            explication.endDate = MaturityDate;
            explication.withCredit = false;
            explication.discounted = false;
            Bond.GetRedemptionExplication(Context, explicationArray, explication);

            for (int i = 0; i < explicationArray.Count; i++)
            {
                IthRedemption = (sophis.instrument.SSMRedemption)explicationArray[i];

                //Eq year count entre date de reporting et tdate de tombée du flux
                IthDate = CSMDayCountBasis.GetCSRDayCountBasis(DayCountBasisType).GetEquivalentYearCount(reportingdate, IthRedemption.endDate);

                //Taux spot annualisé (en date de reporting) pour la date de tombée du coupon
                MtMrate = CSMMarketData.GetCurrentMarketData().GetForwardCompoundFactor(Currency, reportingdate, IthRedemption.endDate);
                MtMrate = Math.Pow((MtMrate), 1 / (IthDate)) - 1;

                //Dicount factor
                //DiscountFactor = Math.Exp(-IthDate * (MtMrate + SpreadParameter));
                DiscountFactor = Math.Pow(1 / (1 + MtMrate + SpreadParameter), IthDate);

                BondThPrice += (IthRedemption.coupon + IthRedemption.redemption) * DiscountFactor;
            }
            BondThPrice = BondThPrice / Bond.GetNotional() * 100;
            return BondThPrice;
        }
        private double ComputeZSpread()
        {
            double ZSpread = 0;
            try
            {
                double BondMtM = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentPtr.GetCode());//choper le prix sophis en date de reportingdate
                //double BondMtM = GetBondDirtyPrice(InstrumentPtr.GetCode(), reportingdate);
                TargetFunction TargetComputeValuesAtDateN = new TargetFunction(ComputeBondTh);
                ZSpread = TargetValue(BondMtM, 0, 0.5, ComputeBondTh, 0.0001);//ZSpread inclus entre 0% et 50%
                return ZSpread;
            }
            catch (Exception)
            {
                CSMApi.Log("ZSpread cannot be computed for instrument " + InstrumentPtr.GetCode(), true);
                return 0;
            }
        }
        private static double TargetValue(double Target, double LBorder, double HBorder, TargetFunction Function, double Precision)
        {
            double TheParam;//Parametre de Function permettant d'approcher au mieux Target
            double ResTest = 0;//Valeur retournée par Function pour le paramètre testé
            double ParamTest;//
            int nbiter = 0;
            while ((HBorder - LBorder) > Precision && nbiter < 1000)
            {
                ParamTest = (LBorder + HBorder) * 0.5;
                ResTest = Function(ParamTest);

                if (ResTest > Target)
                {
                    LBorder = ParamTest;
                }
                else
                {
                    HBorder = ParamTest;
                }
                nbiter++;
            }
            TheParam = ((LBorder + HBorder) * 0.5);
            return TheParam;
        }
        //Equivalent à un pointeur de fonction
        delegate double TargetFunction(double d);
        private double GetBondDirtyPrice(int instrumentCode, int reportingdate)
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

    public class DataSourceOAS
    {
        // Holds the instance of the singleton class
        private static DataSourceOAS Instance = null;

        public static Hashtable DataCacheOAS;//Cace pour la base par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument

        public static BinaryTreeNode EURBinTree;//Arbre binaire de taux Euros
        public static int EURBinTreeSize;//Taille de l'arbre binaire de taux Euros

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceOAS()
        {
            DataCacheOAS = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
        }
        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceOAS GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceOAS();
            return Instance;
        }
        /// <summary>
        /// Queries the database for the maximum quote for a certain position. When this method is called
        /// for the first time, i.e. for the first position of a portfolio, it gets maximum quotes for all
        /// the positions in the portfolio. The results are then stored in the cache. When the method is
        /// called for the other positions in the portfolio, the result is taken directly from the cache.
        /// The SQL query is therefore executed for the first position of the portfolio.
        /// </summary>
        /// <param name="PortfolioCode">Code of the opened portfolio</param>
        /// <param name="PositionIdentifier">Identifier of the current position</param>
        /// <returns>Value to display in the position cell</returns>
        public double GetOAS(CSMPosition Position, CSMInstrument Instrument)
        {
            //Au 1er appel de GetOAS, l'arbre est construit
            //Il est impossible de construire l'arbre dans le constructeur (même si c'est plus "logique") 
            //car Sophis n'a pas encore ne peut pas acceder aux données (elles n'ont pas encore été chargées)
            if (EURBinTreeSize == 0)
            {
                //Contruction de l'arbre binaire de taux Euros
                EURBinTreeSize = 20;
                BinaryTree BinTree = BinaryTree.GetInstance(54875474, 0.3, EURBinTreeSize);
                EURBinTree = BinTree.GetFWBinTree();
            }

            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0 && Instrument.GetInstrumentType().Equals('O') )
            {
                //La valeur est (re)calculée:
                //si le cache est vide 
                //si la version de l'instrument change
                //si le spot change
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                if (!DataCacheOAS.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheOAS.ContainsKey(Instrumentcode))
                {
                    return (double)DataCacheOAS[Instrumentcode];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }
        }
        private void FillCache(CSMInstrument Instrument)
        {
            //mise à jour de la base
            //MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();
            
            OptionAdjustedSpread OAS = OptionAdjustedSpread.GetInstance(Instrument, EURBinTree, EURBinTreeSize);
            double CreditSpread = OAS.GetOAS() * 100;//spread en %          
            CSMApi.Log("FillCache: " + CreditSpread, true);
            if (DataCacheOAS.ContainsKey(InstrumentCode))
                DataCacheOAS[InstrumentCode] = CreditSpread;
            else
                DataCacheOAS.Add(InstrumentCode, CreditSpread);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(InstrumentCode))
                DataCacheInstrVersion[InstrumentCode] = InstrVersion;
            else
                DataCacheInstrVersion.Add(InstrumentCode, InstrVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode);
            if (DataCacheSpot.ContainsKey(InstrumentCode))
                DataCacheSpot[InstrumentCode] = Spot;
            else
                DataCacheSpot.Add(InstrumentCode, Spot);
        }
    }
    public class DataSourceZSpread
    {
        // Holds the instance of the singleton class
        private static DataSourceZSpread Instance = null;

        public static Hashtable DataCacheZSpread;//Cace pour la base par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument
        public static Hashtable DataCacheSpot;//Cache pour le spot de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSourceZSpread()
        {
            DataCacheZSpread = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
            DataCacheSpot = new Hashtable();
        }
        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSourceZSpread GetInstance()
        {
            if (null == Instance)
                Instance = new DataSourceZSpread();
            return Instance;
        }
        /// <summary>
        /// Queries the database for the maximum quote for a certain position. When this method is called
        /// for the first time, i.e. for the first position of a portfolio, it gets maximum quotes for all
        /// the positions in the portfolio. The results are then stored in the cache. When the method is
        /// called for the other positions in the portfolio, the result is taken directly from the cache.
        /// The SQL query is therefore executed for the first position of the portfolio.
        /// </summary>
        /// <param name="PortfolioCode">Code of the opened portfolio</param>
        /// <param name="PositionIdentifier">Identifier of the current position</param>
        /// <returns>Value to display in the position cell</returns>
        public double GetZSpread(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0 && Instrument.GetInstrumentType().Equals('O'))
            {
                //La valeur est (re)calculée:
                //si le cache est vide 
                //si la version de l'instrument change
                //si le spot change
                CSMApi.Log("Instrument " + Instrumentcode, true);
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                if (!DataCacheZSpread.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion() || (double)DataCacheSpot[Instrumentcode] != context.GetSpot(Instrumentcode))
                {
                    FillCache(Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCacheZSpread.ContainsKey(Instrumentcode))
                {
                    CSMApi.Log("acces cache: " + DataCacheZSpread[Instrumentcode], true);
                    return (double)DataCacheZSpread[Instrumentcode];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }
        }
        private void FillCache(CSMInstrument Instrument)
        {
            //mise à jour de la base
            //MarketIndicCompute IMarketIndic = MarketIndicCompute.GetInstance();
            int InstrumentCode = Instrument.GetCode();

            ZSpread ZS = ZSpread.GetInstance(Instrument);
            double CreditSpread = ZS.GetZSpread() * 100;//spread en %           
            CSMApi.Log("FillCache: " + CreditSpread, true);
            if (DataCacheZSpread.ContainsKey(InstrumentCode))
                DataCacheZSpread[InstrumentCode] = CreditSpread;
            else
                DataCacheZSpread.Add(InstrumentCode, CreditSpread);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(InstrumentCode))
                DataCacheInstrVersion[InstrumentCode] = InstrVersion;
            else
                DataCacheInstrVersion.Add(InstrumentCode, InstrVersion);

            //mise à jour du spot de l'instrument
            double Spot = CSMMarketData.GetCurrentMarketData().GetSpot(InstrumentCode);
            if (DataCacheSpot.ContainsKey(InstrumentCode))
                DataCacheSpot[InstrumentCode] = Spot;
            else
                DataCacheSpot.Add(InstrumentCode, Spot);
        }
    }
    public class DataSource1stCallDate
    {
        // Holds the instance of the singleton class
        private static DataSource1stCallDate Instance = null;

        public static Hashtable DataCache1stCallDate;//Cace pour la base par instrument
        public static Hashtable DataCacheInstrVersion;//Cache pour la version de l'instrument

        /// <summary>
        /// Constructor
        /// </summary>
        private DataSource1stCallDate()
        {
            DataCache1stCallDate = new Hashtable();
            DataCacheInstrVersion = new Hashtable();
        }
        /// <summary>
        /// Returns an instance of the DataSource singleton
        /// </summary>
        public static DataSource1stCallDate GetInstance()
        {
            if (null == Instance)
                Instance = new DataSource1stCallDate();
            return Instance;
        }
        /// <summary>
        /// Queries the database for the maximum quote for a certain position. When this method is called
        /// for the first time, i.e. for the first position of a portfolio, it gets maximum quotes for all
        /// the positions in the portfolio. The results are then stored in the cache. When the method is
        /// called for the other positions in the portfolio, the result is taken directly from the cache.
        /// The SQL query is therefore executed for the first position of the portfolio.
        /// </summary>
        /// <param name="PortfolioCode">Code of the opened portfolio</param>
        /// <param name="PositionIdentifier">Identifier of the current position</param>
        /// <returns>Value to display in the position cell</returns>
        public int Get1stCallDate(CSMPosition Position, CSMInstrument Instrument)
        {
            //Si la date de Sophis change, on vide tous les caches
            if (VersionClass.Get_ReportingDate() != CSMMarketData.GetCurrentMarketData().GetDate())
            {
                VersionClass.Set_ReportingDate(CSMMarketData.GetCurrentMarketData().GetDate());
                VersionClass.DeleteCache();
            }

            int PositionIdentifier = Position.GetIdentifier();//Vaut 0 si vue flat ou underlying donc ne fonctionne pas dans ces cas là
            int Instrumentcode = Position.GetInstrumentCode();
            double Nominal = Position.GetInstrumentCount() * Instrument.GetNotional();
            if (Nominal != 0 && Instrument.GetInstrumentType().Equals('O'))
            {
                //La valeur est (re)calculée:
                //si le cache est vide 
                //si la version de l'instrument change
                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                if (!DataCache1stCallDate.ContainsKey(Instrumentcode) || (int)DataCacheInstrVersion[Instrumentcode] != Instrument.GetVersion())
                {
                    FillCache(Instrument);
                }

                // At this point, the value that this method should return must be available in the cache
                if (DataCache1stCallDate.ContainsKey(Instrumentcode))
                {
                    return (int)DataCache1stCallDate[Instrumentcode];
                }
                else { return 0; }//on ne devrait jamais passer par là ms au cas où
            }
            else { return 0; }
        }
        private void FillCache(CSMInstrument Instrument)
        {
            //mise à jour de la base
            int InstrumentCode = Instrument.GetCode();

            int FirstCallDate = Get1stCallDate(Instrument);//date du 1er call si il y en a 
            if (DataCache1stCallDate.ContainsKey(InstrumentCode))
                DataCache1stCallDate[InstrumentCode] = FirstCallDate;
            else
                DataCache1stCallDate.Add(InstrumentCode, FirstCallDate);

            //mise à jour de la version de l'instrument
            int InstrVersion = Instrument.GetVersion();
            if (DataCacheInstrVersion.ContainsKey(InstrumentCode))
                DataCacheInstrVersion[InstrumentCode] = InstrVersion;
            else
                DataCacheInstrVersion.Add(InstrumentCode, InstrVersion);
        }
        private int Get1stCallDate(CSMInstrument Instrument)
        {
            ArrayList Clauses = new ArrayList();
            Instrument.GetClauseInformation(Clauses);
            int i = 0;//Numéro de la clause
            if (Clauses.Count > 0)
            {
                SSMClause IthClause = new SSMClause();
                IthClause = (SSMClause)Clauses[i];
                while (IthClause.type != 2)//on cherche la première clause de call
                {
                    i++;
                    if (i >= Clauses.Count) return 0;
                    IthClause = (SSMClause)Clauses[i];
                   
                }
                return IthClause.start_date;
            }
            else
            {
                return 0;
            }
        }
    }
}