using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sophis.ClauseBuilder;

namespace MEDIO.ClauseBuilder.net.GUI
{
    public partial  class CSxClauseBuilderAutocallGUI : ExoticMaskControlBase
    {
        //This name will appear in the exotic mask tab header
        public override String GetTabHeader()
        {
            return "Medio Autocall";
        }

        public CSxClauseBuilderAutocallGUI()
            : base()
        {
            InitializeComponent();
        }
    }
}
