using System.Text;
using sophis.portfolio;
using System.Collections;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using TkoPortfolioColumn.DataCache;
using sophis.static_data;
using TkoPortfolioColumn.DbRequester;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using sophis.DAL;

namespace TkoPortfolioColumn
{
    namespace Sectors
    {
        public static class SophisStaticData
        {

            public static double TkoHandleInstrumentSectorBySectorType(this CSMInstrument instrument, InputProvider input)
            {
                try
                {
                    var listofconfig = DbrTikehauPortFolioColumn.GetTikehauPortFolioColumn(input.Column);
                    int level = 0;
                    int sectortypeid = 0;

                    string[] array = null;
                    string[] config = null;
                    foreach (var conf in listofconfig)
                    {
                        array = conf.DESCRIPTION.Split('/');
                        foreach (var elt in array)
                        {
                            config = elt.Split('-');
                            if (config.Length > 0)
                            {
                                if (config[0] == "LEVEL")
                                    level = int.Parse(config[1]);

                                if (config[0] == "SECTORTYPE")
                                    sectortypeid = int.Parse(config[1]);
                            }
                        }
                    }

                    // Sectors
                    ArrayList sectors = new ArrayList();
                    instrument.GetAvailableSectors(sectors);
                    string[] arrayOfsectorTree;

                    //@BK
                    CSMSectorReferential refSectors = CSMSectorReferential.GetInstance();

                    foreach (Int32 sectorCode in sectors)
                    {
                        //@BK
                        // CSMSector sector = CSMSector.GetSectorByIdent(sectorCode);
                        CSMSectorData sector = refSectors.Get(sectorCode);

                        int dpt = 0;
                        if (sector != null)
                        {
                            //@BK
                            //CSMSector sectortype = sector.GetSectorType();
                            //int ident = sectortype.GetIdent();
                            int ident = sector.GetSectorTypeIdent();

                            if (ident == sectortypeid)
                            {
                                input.StringIndicatorValue = " " + sector.GetName().StringValue + " ";
                                string path = sector.GetPath().StringValue;
                                arrayOfsectorTree = path.Split(':');
                                if (level == 0)
                                {
                                    //@BK
                                    // input.StringIndicatorValue = " " + sectortype.GetName().StringValue + " ";
                                    input.StringIndicatorValue = " " + sector.GetName().StringValue + " ";
                                    break;
                                }
                                else
                                {
                                    foreach (var elt in arrayOfsectorTree)
                                    {
                                        dpt++;
                                        if (dpt == level)
                                        {
                                            input.StringIndicatorValue = " " + elt + " ";
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    return -1;
                }
            }
        }
    }
}






