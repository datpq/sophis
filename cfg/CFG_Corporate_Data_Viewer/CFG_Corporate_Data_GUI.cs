using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using sophis.tools;
using sophis.utils;
using sophis.misc;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;


namespace CFG_Corporate_Data_Viewer
{
    public partial class CFG_Corporate_Data_GUI : Form
    {
        public CFG_Corporate_Data_GUI()
        {
            InitializeComponent();
            CSMUserRights myUserRights = new CSMUserRights();
            CSMUserRights myGroupRights = new CSMUserRights(Convert.ToUInt32(myUserRights.GetParentID()));
            myUserRights.LoadDetails();
            myGroupRights.LoadDetails();
            eMRightStatusType ExpectedData = myUserRights.GetUserDefRight("Expected Data Access");
            eMRightStatusType ExpectedDataGroup = myGroupRights.GetUserDefRight("Expected Data Access");
            eMRightStatusType OfficialData = myUserRights.GetUserDefRight("Official Data Access");
            eMRightStatusType OfficialDataGroup = myGroupRights.GetUserDefRight("Official Data Access");
            int UserIdent = myUserRights.GetIdent();

            System.Data.DataTable MyTable = new System.Data.DataTable();

            bool hasExpectedRight = false;
            bool hasofficialRight = false;

            string SQLQuery =           "select EMETTEUR as \"Emetteur\", " + 
                                        "ANNEE as \"Année\", " + 
                                        "NATURE_DE_DONNEES as \"Nature de données\", " + 
                                        "SOURCES as \"Source\", " + 
                                        "SECTEUR as \"Secteur\", " + 
                                        "CA as \"CA\", " + 
                                        "DAP as \"DAP\", " + 
                                        "CHARGES_DU_PERSONNEL as \"Charges du personnel\", " + 
                                        "AUTRES_CHARGES_D_EXPLOITATION as \"Autres charges exploitation\", " + 
                                        "REX as \"REX \", " + 
                                        "RESULTAT_FINANCIER as \"Résultat financier\", " + 
                                        "AUTRES_CHARGES as \"Autres charges\", " + 
                                        "AUTRES_PRODUITS1 as \"Autres produits\", " + 
                                        "RESULTAT_AVANT_IMPOTS as \"Résultat avant impôts\", " + 
                                        "IMPOTS as \"Impôts\", " + 
                                        "RESULTAT_NET as \"Résultat Net\", " + 
                                        "RESULTAT_MIS_EN_EQUIVALENCE as \"Résulltat mis en équivalence\", " + 
                                        "RNPG as \"RNPG\", " + 
                                        "DIVIDENDES as \"Dividendes\", " + 
                                        "INVESTISSEMENTS as \"Investissements \", " + 
                                        "ACHATS as \"Achats\", " + 
                                        "MARGE_D_INTERETS as \"Marge d'intérêts\", " + 
                                        "MARGE_SUR_COMMISSIONS as \"Marge sur commissions\", " + 
                                        "RESULTATS_ACTIVITES_MARCHE as \"Résultats activités marché\", " + 
                                        "AUTRES_PRODUITS2 as \"Autres produits\", " + 
                                        "PNB as \"PNB\", " + 
                                        "CHARGES_D_EXPLOITATION as \"Charges générales exploitation\", " + 
                                        "RBE as \"RBE\", " + 
                                        "COUT_DU_RISQUE as \"Coût du risque\", " + 
                                        "PRIMES_EMISES_NETTES as \"Primes émises nettes\", " + 
                                        "PRODUIT_DES_PLACEMENTS as \"Produit des placements\", " + 
                                        "PRESTATIONS_ET_FRAIS_PAYES as \"Préstations et frais payés\", " + 
                                        "VARIATION_PROV_TECHNIQUES as \"Variation provision technique\", " + 
                                        "SOLDE_DE_REASSURANCE as \"Solde de réassurance\", " + 
                                        "VALEUR_AJOUTEE as \"Valeur ajoutée\", " + 
                                        "RESULTAT_TECHNIQUE as \"Résultat technique\", " + 
                                        "RESULTAT_NON_TECHNIQUE as \"Résultat non technique\", " + 
                                        "TOTAL_ACTIF as \"Total Actif\", " + 
                                        "FONDS_PROPRES as \"Fonds propres\", " + 
                                        "DETTE_NETTE as \"Dette Nette\", " + 
                                        "ACTIF_CIRCULANT as \"Actif circulant\", " + 
                                        "PASSIF_CIRCULANT as \"Passif circulant\", " + 
                                        "IMMOBILISATIONS_CORPORELLES as \"Immobilisations corporelles\", " + 
                                        "TOTAL_PROVISIONS as \"Total Provisions\", " + 
                                        "PRETS_ET_CREANCES_SUR_CLIENTS as \"Prêts et créances sur clients \", " + 
                                        "DETTE_ENVERS_LA_CLIENTELE as \"Dette envers la clientèle\", " + 
                                        "CREANCES_EN_SOUFFRANCE as \"Créances en souffrance\", " + 
                                        "PLACEMENTS_ASSURANCE as \"Placements assurance\", " + 
                                        "PROVISIONS_TECHNIQUES as \"Provisions techniques\", " + 
                                        "VALEUR_GLOBALE as \"Valeur globale \", " + 
                                        "VALEUR_PAR_TITRE as \"Valeur par titre\", " + 
                                        "VALEUR_AVEC_DECOTE_15 as \"Valeur avec décote 15%\", " + 
                                        "TAUX_DE_CROISSANCE_PERPETUEL as \"Taux de croissance pérpetuel\", " + 
                                        "TAUX_SANS_RISQUE as \"Taux sans risque\", " + 
                                        "PRIME_DE_RISQUE_MARCHE_ACTIONS as \"Prime de risque marché actions\", " + 
                                        "BETA as \"Bêta\", " + 
                                        "OCE as \"OCE\", " + 
                                        "CASH_FLOW as \"Cash flow\", " + 
                                        "FREE_CASH_FLOW as \"Free Cash flow\", " + 
                                        "VALEUR_TERMINALE as \"Valeur Terminale\", " + 
                                        "DISCOUNT_FACTOR as \"Discount factor\", " + 
                                        "DECOTE_SURCOTE as \"Décote /Surcote\", " + 
                                        "PE as \"PE\", " + 
                                        "PB as \"PB\", " + 
                                        "DY as \"DY\", " + 
                                        "COMMENTAIRE1 as \"Commentaire 1\", " + 
                                        "COMMENTAIRE2 as \"Commentaire 2\", " + 
                                        "DATE_DE_SAUVEGARDE as \"Date de Sauvegarde\" " + 
                                        " from CFG_DONNEES_CORPORATES where Nature_de_donnees in (";
            if ((ExpectedData == eMRightStatusType.M_rsReadOnly) || (ExpectedData == eMRightStatusType.M_rsReadWrite) || (UserIdent == 1) || ((ExpectedData == eMRightStatusType.M_rsSameAsParent) && (ExpectedDataGroup == eMRightStatusType.M_rsReadOnly)) || ((ExpectedData == eMRightStatusType.M_rsSameAsParent) && (ExpectedDataGroup == eMRightStatusType.M_rsReadWrite)))
            {
                SQLQuery += "'Expected'";
                hasExpectedRight = true;
            }
            if ((OfficialData == eMRightStatusType.M_rsReadOnly) || (OfficialData == eMRightStatusType.M_rsReadWrite) || (UserIdent == 1) || ((OfficialData == eMRightStatusType.M_rsSameAsParent) && (OfficialDataGroup == eMRightStatusType.M_rsReadOnly)) || ((OfficialData == eMRightStatusType.M_rsSameAsParent) && (OfficialDataGroup == eMRightStatusType.M_rsReadWrite)))
            {
                if (hasExpectedRight)
                    SQLQuery += ",";
                SQLQuery += "'Published'";
                hasofficialRight = true;
            }
            SQLQuery += ")";

            if (hasExpectedRight || hasofficialRight)
            {
                OracleCommand cmd = new OracleCommand(SQLQuery, DBContext.Connection);

                try
                {
                    OracleDataAdapter ad = new OracleDataAdapter(cmd);
                    ad.Fill(MyTable);
                    dataGridView1.DataSource = MyTable;
                }
                catch (System.Exception e)
                {
                    CSMLog.Write("CFG_CORPORATE_DATA", "select in Datatable", CSMLog.eMVerbosity.M_debug, "Error while using the datatable2 : " + e.Message);
                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
            }
        }
    }
}
