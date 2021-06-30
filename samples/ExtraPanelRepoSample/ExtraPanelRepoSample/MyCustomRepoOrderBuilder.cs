using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sophis.Windows.OMS.Builder;
using sophis.oms;
using Sophis.Windows.OMS.Views;
using Sophis.Windows.OMS.ViewModels;

namespace ExtraPanelRepoSample
{
    public class MyCustomRepoOrderBuilder : IOrderEntryBuilder
    {
        public sophis.oms.IOrder CreateOrder()
        {
            return OrderFactoryRepository.Instance.CreateOrder(OrderKind.Repo);
        }

        public Sophis.Windows.OMS.IView CreateView()
        {
            return new MyCustomRepoOrderEntry();
        }

        public Sophis.Windows.OMS.IViewModel CreateViewModel()
        {
            var viewModel = new MyCustomRepoOrderViewModel();
            viewModel.CashAvailableInTheFund = 42.0;
            viewModel.ImpactOnNominal = -500.0;
            return viewModel;
        }

        public Sophis.Windows.OMS.IViewModelValidator CreateViewModelValidator()
        {
            return new RepoOrderViewModelValidator();
        }

    }
}
