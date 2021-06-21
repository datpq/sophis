using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.portfolio;
using sophis.instrument;

namespace TkoPortfolioColumn
{
    public static class Helper
    {
        //Fonction de calcul pour l'arbre de décisions YTM/YTC/YTW
        //fonction qui retourne 1 si la notation est haute, -1 si la notation est basse, 0 en cas d'erreur
        public static int TestRatingNb(double notationNum)
        {
            if (notationNum >= 12 && notationNum < 22) return 1;
            else if ((notationNum >= 0 && notationNum < 12) || notationNum == -1) return -1;
            else return 0;
        }

        //Definir la notation numérique suivant la notation de l'agence 
        //quelque soit sa provenance à partir d'une chaine de caractère
        public static int DefineNumerating(String notation)
        {
            try
            {
                string tmp = "    ";
                notation += tmp; //augmentation de la taille de la chaine pour passer les tests
                //liés à la fonction Substring

                if (notation.Substring(0, 3) == "AAA" || notation.Substring(0, 3) == "Aaa" || notation.Substring(0, 5) == "(P)Aaa") return 21;
                else if (notation.Substring(0, 3) == "AA+" || notation.Substring(0, 3) == "Aa1") return 20;
                else if (notation.Trim() == "AA    " || notation == "Aa2    ") return 19;
                else if (notation == "AA-    " || notation == "Aa3    ") return 18;
                else if (notation == "A+    " || notation == "A1    ") return 17;

                else if (notation == "A    " || notation.Substring(0, 2) == "A2") return 16;

                else if (notation == "A-    " || notation == "A3    ") return 15;
                else if (notation == "BBB+    " || notation == "Baa1    ") return 14;
                else if (notation == "BBB    " || notation.Substring(0, 4) == "Baa2") return 13;
                else if (notation.Substring(0, 4) == "BBB-" || notation.Substring(0, 4) == "Baa3") return 12;


                else if (notation.Substring(0, 3) == "BB+" || notation.Substring(0, 3) == "Ba1") return 11;
                else if (notation == "BB    " || notation.Substring(0, 3) == "Ba2") return 10;
                else if (notation.Substring(0, 3) == "BB-" || notation.Substring(0, 3) == "Ba3") return 9;
                else if (notation.Substring(0, 2) == "B+" || notation.Substring(0, 2) == "B1") return 8;
                else if (notation == "B    " || notation.Substring(0, 2) == "B2") return 7;
                else if (notation.Substring(0, 2) == "B-" || notation.Substring(0, 2) == "B3") return 6;
                else if (notation.Substring(0, 4) == "CCC+" || notation.Substring(0, 4) == "Caa1") return 5;
                else if (notation == "CCC    " || notation.Substring(0, 4) == "Caa2") return 4;
                else if (notation.Substring(0, 4) == "CCC-" || notation.Substring(0, 4) == "Caa3") return 3;
                else if (notation.Substring(0, 2) == "CC" || notation.Substring(0, 3) == "Ca1" || notation.Substring(0, 3) == "Ca2") return 2;
                else if (notation == "C    " || notation == "C1    " || notation == "C2    " || notation == "C3    ") return 1;
                else if (notation == "R    " || notation == "D    " || notation == "SD    ") return 0;
                else if (notation == "    ") return -1;
                else return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        //Trouve la plus grande notation
        public static int FindBestRatingNum(int[] tabrating)
        {
            int bestrating = -1;
            try
            {
                //mise à jour
                if (tabrating[0] > -1) bestrating = tabrating[0];

                // Parcourir l'ensemble des notations d'un instrument puis conserver la valeur la plus élévée
                for (int i = 0; i < 2; i++)
                    if (tabrating[i + 1] > bestrating) bestrating = tabrating[i + 1];

                return bestrating;
            }
            catch (Exception ex)
            {
                return bestrating;
            }
        }

        //trouve la plus petite notation
        public static int FindWorstRatingNum(int[] tabrating)
        {
            int worstrating = -1;
            //mise à jour
            if (tabrating[0] > -1) worstrating = tabrating[0];

            for (int i = 0; i < 2; i++)
                if (tabrating[i + 1] < worstrating && tabrating[i + 1] != -1) worstrating = tabrating[i + 1];

            return worstrating;
        }

        //Trouve la seconde best notation
        public static int FindSecondRatingNum(int[] tabrating)
        {
            int temp = -1;
            int cpt = 0;
            //Si une seule notation on sort direct
            for (int i = 0; i < 3; i++)
            {
                if (tabrating[i] < 0) cpt++;
                else temp = tabrating[i];
            }

            if (cpt == 1) return temp;
            int bestrating = -1;

            //mise à jour
            if (tabrating[0] > -1) bestrating = tabrating[0];

            // Parcourir l'ensemble des notations d'un instrument puis conserver la valeur la plus élévée
            for (int i = 0; i < 2; i++)
                if (tabrating[i + 1] > bestrating) bestrating = tabrating[i + 1];

            for (int i = 0; i < 2; i++)
            {
                if (tabrating[i + 1] == bestrating)
                {
                    tabrating[i + 1] = -1;
                    i = 2;
                }
            }

            bestrating = -1;
            if (tabrating[0] > -1) bestrating = tabrating[0];

            //deuxième boucle
            for (int i = 0; i < 2; i++)
                if (tabrating[i + 1] > bestrating) bestrating = tabrating[i + 1];

            return bestrating;
        }

        //Fonction qui retourne une valeur de type double provenant de Sophis d'après le nom de la colonne
        //et des infos de l'instrument.
        public static double TkoGetValuefromSophisDouble(InputProvider input)
        { // position = position // activePortfolioCode = 0 // portFolioCode = 18649 // extraction = // underlyingCode = 0 // instrumentCode = //style = à définir selon le cas//onlyThe value =false //
            //Var
            double Svalue = 1;
            int activePortfolioCode = 0;
            int portfolioCode = input.Position.GetPortfolio().GetCode();
            int underlyingCode = 0;
            int instrumentCode = input.Instrument.GetCode();
            bool onlyTheValue = true;

            //paramètre
            SSMCellValue cvalue = new SSMCellValue();
            SSMCellStyle cstyle = new SSMCellStyle();

            cstyle.kind = NSREnums.eMDataType.M_dDouble;
            cstyle.@decimal = 3;

            //Création de la colonne pour récupérer les données
            CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn(input.TmpPortfolioColName);
            if (col != null)
            {
                col.GetPositionCell(input.Position, activePortfolioCode, portfolioCode, null, underlyingCode, instrumentCode, ref cvalue, cstyle, onlyTheValue);
                Svalue = cvalue.doubleValue;
                return Svalue;
            }
            else return 0;

        }

        public static string TkoGetValuefromSophisString(InputProvider input)
        {
            //Var
            string Svalue = "";
            int activePortfolioCode = 0;
            int portfolioCode = input.PortFolioCode;
            int underlyingCode = 0;
            int instrumentCode = input.InstrumentCode;
            bool onlyTheValue = false;

            //paramètre
            SSMCellValue cvalue = new SSMCellValue();
            SSMCellStyle cstyle = new SSMCellStyle();

            cstyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;

            //Création de la colonne pour récupérer les données
            CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn(input.TmpPortfolioColName);

            if (col != null)
            {
                col.GetPositionCell(input.Position, activePortfolioCode, portfolioCode, null, underlyingCode, instrumentCode, ref cvalue, cstyle, onlyTheValue);
                Svalue = cvalue.GetString();
                return Svalue;
            }
            else return "";
        }

        //Focntion qui récupère une date dans les colonnes sophis
        public static int TkoGetValuefromSophisDate(InputProvider input)
        { // position = position // activePortfolioCode = 0 // portFolioCode = 18649 // extraction = // underlyingCode = 0 // instrumentCode = //style = à définir selon le cas//onlyThe value =false //
            //Var
            int activePortfolioCode = 1;
            int portfolioCode = input.Position.GetPortfolio().GetCode();//C'est la ou ca va pas :)
            int underlyingCode = 0;
            int instrumentCode = input.Instrument.GetCode();
            bool onlyTheValue = true;

            //paramètre
            SSMCellValue cvalue = new SSMCellValue();
            SSMCellStyle cstyle = new SSMCellStyle();

            cstyle.kind = NSREnums.eMDataType.M_dDate;
            //cstyle.@decimal = 3;

            //Création de la colonne pour récupérer les données
            CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn(input.TmpPortfolioColName);
            if (col != null)
            {
                col.GetPositionCell(input.Position, activePortfolioCode, portfolioCode, null, underlyingCode, instrumentCode, ref cvalue, cstyle, onlyTheValue);
                return cvalue.integerValue;
            }
            else return 0;

        }

        public static string TkoGetStringValueFromSophis(InputProvider input)
        {
            string Svalue = "";
            bool onlyTheValue = true;

            SSMCellValue cvalue = new SSMCellValue();
            SSMCellStyle cstyle = new SSMCellStyle();

            cstyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;

            //Create the column to retrieve information
            CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn(input.TmpPortfolioColName);

            if (col != null)
            {
                col.GetPortfolioCell(input.ActivePortfolioCode, input.PortFolioCode, input.Extraction, ref cvalue, cstyle, onlyTheValue);
                Svalue = cvalue.GetString();
                return Svalue;
            }
            else return "";
        }

        public static DateTime mydate(int i)
        {
            DateTime d = new DateTime(1904, 01, 01, 0, 0, 0);
            System.TimeSpan spa = new TimeSpan(i, 0, 0, 0);
            DateTime nd = d.Add(spa);
            return nd;
        }

        public static int stendate(DateTime d)
        {
            DateTime refdate = new DateTime(1904, 01, 01);
            TimeSpan diff = d - refdate;
            return diff.Days;
        }

    }
}
