using System.ComponentModel;
using DevExpress.XtraEditors;
using Sophis.Windows.OMS.Builder;
using Sophis.Windows.OMS.Views;
using System.Reflection;

namespace ExtraPanelRepoSample
{
    public class MyCustomRepoOrderEntry : RepoOrderEntry
    {

        protected override void PopulateToolkitControls(Sophis.Windows.OMS.IViewModel viewModel, Sophis.Windows.OMS.Builder.LayoutControlItemCollection collection)
        {
            base.PopulateToolkitControls(viewModel, collection);

            var myViewModel = (MyCustomRepoOrderViewModel)viewModel;

            TextEdit control1 = new TextEdit();
            control1.Enabled = false;
            control1.Name = "CashAvailableInTheFund";
            control1.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
            control1.Properties.Mask.EditMask = "n";
            control1.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            control1.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", myViewModel, "CashAvailableInTheFund", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N2"));
            collection.AddControl(control1);


            TextEdit control2 = new TextEdit();
            control2.Enabled = false;
            control2.Name = "ImpactOnNominal";
            control2.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
            control2.Properties.Mask.EditMask = "n";
            control2.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            control2.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", myViewModel, "ImpactOnNominal", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N2"));
            collection.AddControl(control2);
        }
    }


}