using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using sophis.guicommon.basicDialogs;
using sophis.utils;
using DevExpress.XtraGrid.Columns;
using Sophis.Windows.Forms.Input;

namespace CFG_RetrocessionFeesGUI
{
    public partial class CSxRetroFeesDetailedResultsGUI : XtraUserControl, ISMMdiEmbeddable, Sophis.Windows.IAssociatedMFCWindow
    {
        private bool _IsInitialized = false;

        public bool IsInitialized
        {
            get { return _IsInitialized; }
            set { _IsInitialized = value; }
        }
        
        public CSxRetroFeesDetailedResultsGUI()
        {
            InitializeComponent();
        }

        #region ISMMdiEmbeddable Members

        public bool OnCloseDialog()
        {
            _IsInitialized = false;

            return true;
        }

        public void OnDelete()
        {

        }

        public void OnEditCopy()
        {
            //throw new NotImplementedException();
        }

        public void OnEditCut()
        {
            //throw new NotImplementedException();
        }

        public void OnEditDelete()
        {
            //throw new NotImplementedException();
        }

        public void OnEditPaste()
        {
            //throw new NotImplementedException();
        }

        public void OnEditSelectAll()
        {
            //throw new NotImplementedException();
        }

        public void OnEditUndo()
        {
            //throw new NotImplementedException();
        }

        public void OnEditXMLCopy()
        {

        }

        public void OnFileNew()
        {
            //throw new NotImplementedException();
        }

        public void OnFileSave()
        {
            //throw new NotImplementedException();
        }

        public void OnLoaded()
        {
        }

        public void OnUnloaded()
        {
            _IsInitialized = false;
        }

        //DPH
        public bool CanFileNew() { return false; }

        //DPH
        public bool CanFileSave() { return false; }

        //DPH
        public bool CanDelete() { return false; }

        #endregion

        #region ICommandBindings Members

        //DPH
        //public System.Collections.IList CommandBindings
        public FormCommandBindingCollection CommandBindings
        {
            get { return null; }
        }

        #endregion

        //DPH
        #region IEmbeddableComponent

        public void OnActivated()
        {
        }

        public void OnDeactivated()
        {
        }
        #endregion

        #region ITitled Members

        public string Title
        {
            get { return ""; }
        }

        public event Action<Sophis.Data.Utils.ITitled> TitleChanged;

        public Sophis.Data.Utils.IImageDesc Icon
        {
            get { return null; }
        }

        public event Action<Sophis.Data.Utils.ITitled> IconChanged;

        #endregion            

        #region IAssociatedMFCWindow Members

        public IntPtr AssociatedMFCWindow { get; set; }

        #endregion

        public void Initialize(int computationMethod, DataTable dataTable)
        {
            this.gridControl1.DataSource = dataTable;

            this.gridView1.Columns.ColumnByFieldName("COMPUTATION_METHOD").Visible = false;

            if (computationMethod != 0)
            {
                this.gridView1.Columns.ColumnByFieldName("NB_SHARES_TOTAL").Visible = false;
                this.gridView1.Columns.ColumnByFieldName("FDG").Visible = false;
                this.gridView1.Columns.ColumnByFieldName("CDVM").Visible = false;
                this.gridView1.Columns.ColumnByFieldName("DDG").Visible = false;
                this.gridView1.Columns.ColumnByFieldName("MCL").Visible = false;
                this.gridView1.Columns.ColumnByFieldName("FUND_PROMOTER_RETROCESSION").Visible = false;
                this.gridView1.Columns.ColumnByFieldName("PNB").Visible = false;
            }
            else
            {
                this.gridView1.Columns.ColumnByFieldName("NB_DAYS").Visible = false;
                this.gridView1.Columns.ColumnByFieldName("NAV").Visible = false;
            }            

            this.gridView1.BestFitColumns();
            this.gridView1.OptionsSelection.MultiSelect = true;

            _IsInitialized = true;
        }
    }
}
