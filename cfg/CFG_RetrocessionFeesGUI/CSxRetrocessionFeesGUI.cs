using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using sophis.guicommon.basicDialogs;
using sophisTools;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using sophis.utils;
using Sophis.Windows.Forms.Input;

namespace CFG_RetrocessionFeesGUI
{
    public partial class CSxRetrocessionFeesGUI : XtraUserControl, ISMMdiEmbeddable, Sophis.Windows.IAssociatedMFCWindow
    {
        private bool _IsInitialized = false;

        public bool IsInitialized
        {
            get { return _IsInitialized; }
            set { _IsInitialized = value; }
        }
        
        public CSxRetrocessionFeesGUI()
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

        public void Initialize(DataTable dataTable)
        {
            this.gridControl1.DataSource = dataTable;

            this.gridView1.Columns.ColumnByFieldName("Fund id").Visible = false;
            this.gridView1.Columns.ColumnByFieldName("Business partner id").Visible = false;
            this.gridView1.Columns.ColumnByFieldName("Business partner type id").Visible = false;
            this.gridView1.Columns.ColumnByFieldName("Computation method id").Visible = false;

            this.gridView1.BestFitColumns();
            this.gridView1.OptionsSelection.MultiSelect = true;

            _IsInitialized = true;
        }

        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            CSMLog log = new CSMLog();

            log.Begin("CSxRetrocessionFeesGUI", "gridControl1_DoubleClick");

            try
            {
                if (this.gridView1.SelectedRowsCount == 1)
                {
                    DataRow selectedRow = this.gridView1.GetDataRow(this.gridView1.GetSelectedRows()[0]);
                    if (selectedRow != null)
                    {
                        int rowId = 0;
                        int.TryParse(selectedRow["Row"].ToString(), out rowId);

                        int fundId = 0;
                        int.TryParse(selectedRow["Fund id"].ToString(), out fundId);

                        int businessPartnerId = 0;
                        int.TryParse(selectedRow["Business partner id"].ToString(), out businessPartnerId);

                        int businessPartnerType = 0;
                        int.TryParse(selectedRow["Business partner type id"].ToString(), out businessPartnerType);

                        int computationMethod = 0;
                        int.TryParse(selectedRow["Computation method id"].ToString(), out computationMethod);

                        DateTime startDateObj;
                        DateTime.TryParse(selectedRow["Start date"].ToString(), out startDateObj);
                        CSMDay startDateDay = new CSMDay(startDateObj.Day, startDateObj.Month, startDateObj.Year);
                        int startDate = startDateDay.toLong();

                        DateTime endDateObj;
                        DateTime.TryParse(selectedRow["End date"].ToString(), out endDateObj);
                        CSMDay endDateDay = new CSMDay(endDateObj.Day, endDateObj.Month, endDateObj.Year);
                        int endDate = endDateDay.toLong();

                        CSxRetrocessionFeesResultsHandler.DisplayRetroFeesDetailedResultsGUI(rowId, fundId, businessPartnerId, businessPartnerType,
                                                                                                                            computationMethod, startDate, endDate);
                    }

                }
            }
            catch (Exception ex)
            {
                log.Write(CSMLog.eMVerbosity.M_error, ex.Message);
                MessageBox.Show(ex.Message);
            }

            log.End();                       
        }
        
    }
}
