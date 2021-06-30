using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sophis.Windows.OMS.ViewModels;
using System.ComponentModel;
using System.Reflection;

namespace ExtraPanelRepoSample
{
    public class MyCustomRepoOrderViewModel : RepoOrderEntryViewModel
    {
        private double _cashAvailableInTheFund;
        private double _impactOnNominal;

        public MyCustomRepoOrderViewModel()
        {
            this.PropertyChanged += new PropertyChangedEventHandler(MyCustomRepoOrderViewModel_PropertyChanged);
        }

        [Browsable(true)]
        public double CashAvailableInTheFund
        {
            get { return _cashAvailableInTheFund; }
            set 
            { 
                _cashAvailableInTheFund = value;
                RaisePropertyChanged("CashAvailableInTheFund");
            }
        }

        [Browsable(true)]
        public double ImpactOnNominal
        {
            get { return _impactOnNominal; }
            set 
            { 
                _impactOnNominal = value;
                RaisePropertyChanged("ImpactOnNominal");
            }
        }

        void MyCustomRepoOrderViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CommissionType":
                {
                    CashAvailableInTheFund = 123;
                    break;
                };
            }
        }


    }
}
