using System;
//@DPH
using Oracle.DataAccess.Client;
//using System.Data.OracleClient;
using sophis.misc;

namespace ORCLFactory
{
    public class ORCLFactory : IDisposable
    {
    
        //attributes

        public static string orcl_query { get ; set; }
        //@DPH
        public static readonly OracleConnection _Connection = Sophis.DataAccess.DBContext.Connection;


        public static void Initialize()
        {
            string SetDbStyle = "ALTER SESSION SET NLS_NUMERIC_CHARACTERS = ', '";
            OracleCommand DbStyle = new OracleCommand(SetDbStyle, _Connection);
            int retstyle = (int)DbStyle.ExecuteNonQuery();
        }




        // Specific methods

        public static string getResultS(string myquery)
        {
            orcl_query = myquery;
            OracleCommand myCommand = new OracleCommand(orcl_query, _Connection);
            object o = myCommand.ExecuteScalar();
            string rquery = Convert.ToString(o);
            return rquery;
        }

        public static double getResultD(string myquery)
        {
            double rquery = 0;

            try
            {

                orcl_query = myquery;
                OracleCommand myCommand = new OracleCommand(orcl_query, _Connection);
                object o = myCommand.ExecuteScalar();
                rquery = Convert.ToDouble(o);
                return rquery;
            }

            catch (Exception)
            {
                return rquery;

            }
        }

        public static int getResultI(string myquery)
        {
            int rquery = 0;

            try
            {
                orcl_query = myquery;
                OracleCommand myCommand = new OracleCommand(orcl_query, _Connection);
                object o = myCommand.ExecuteScalar();
                rquery = Convert.ToInt32(o);
                return rquery;
            }

            catch (Exception)
            {
                return rquery;
            }
        }

        //Renvoie le résultat dans un tableau à 2 dimensions
        public static double[,] getResultTab2(string myquery, int Dim1, int Dim2)
        {
            double[,] rquery = new double[Dim1, Dim2];
            int j;
            try
            {
                orcl_query = myquery;
                OracleCommand myCommand = new OracleCommand(orcl_query, _Connection);
                OracleDataReader myReader;
                myReader = myCommand.ExecuteReader();
                j = 0;
                while (myReader.Read())
                {
                    rquery[j, 0] = myReader.GetDouble(0);
                    rquery[j, 1] = myReader.GetDouble(1);
                    j++;
                }
                myReader.Close();
                return rquery;
            }
            catch (Exception)
            {
                return rquery;
            }
        }

        public static int Insert(string myquery)
        {
            orcl_query = myquery;
            OracleCommand myCommand = new OracleCommand(orcl_query, _Connection);
            int retour = (int)myCommand.ExecuteNonQuery();
            return retour;
        }

        //public static void CloseAll()
        //{
        //    _Connection.Close();
        //    OracleConnection.ClearAllPools();
        //}

        public void Dispose()
        {
            _Connection.Close();
            OracleConnection.ClearAllPools();
        }
    }
}
