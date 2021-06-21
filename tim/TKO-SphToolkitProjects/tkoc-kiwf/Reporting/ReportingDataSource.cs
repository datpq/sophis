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
using sophis.utils;

namespace dnPortfolioColumn
{
    //Fonctions de calcul
    public class ReportingDataCompute
    {
        // Holds the instance of the singleton class
        private static ReportingDataCompute Instance = null;
        //private ORCLFactory.ORCLFactory CS_ORCL;

        //Constructeur
        private ReportingDataCompute()
        {
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
        public static ReportingDataCompute GetInstance()
        {
            Instance = new ReportingDataCompute();
            return Instance;
        }

        //rl mettre ici les fonctions de calcul pour avoir un cache des colonnes reporting


    }
}
