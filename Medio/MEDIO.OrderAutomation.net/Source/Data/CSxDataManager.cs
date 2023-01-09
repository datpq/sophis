using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MEDIO.OrderAutomation.net.Source.Data;
using MEDIO.OrderAutomation.NET.Source.DataModel;
using sophis.oms;

namespace MEDIO.OrderAutomation.NET.Source.Data
{
    class CSxGUIDataManager
    {
        private static CSxGUIDataManager _Instance = null;

        public static CSxGUIDataManager Instance
        {
            [MethodImpl(MethodImplOptions.Synchronized)] 
            get { return _Instance ?? 
                ( _Instance = new CSxGUIDataManager() ); }
        }
        private readonly CSxCustomAllocationData _allocations;

        private CSxGUIDataManager()
        {
            _allocations = CSxDataFacade.GetAllAllocatedExecutions();
            _allocations.SortByOrderID();
            _allocations.ExecutionCreated += new EventHandler<CSxExecutionAllocationArgs>(AddOrUpdateExecutionToCache);
            _allocations.ExecutionUpdated += new EventHandler<CSxExecutionAllocationArgs>(AddOrUpdateExecutionToCache);
            _allocations.ExecutionRemoved += new EventHandler<CSxExecutionAllocationArgs>(RemoveExecutionFromCache);
            _allocations.RefreshNeeded += AllocationsOnRefreshNeeded;
        }

        public CSxCustomAllocationData GetAllocations()
        {
            return _allocations;
        }

        private void AddOrUpdateExecutionToCache(object sender, CSxExecutionAllocationArgs e)
        {
            foreach (var exec in e.ExecList)
            {
                var order = OrderManagerConnector.Instance.GetOrderManager().GetOrderById(exec.Execution.SophisOrderID.GetValueOrDefault()) as SingleOrder;
                if (order != null)
                {
                    var found = _allocations.FirstOrDefault(x => x.ExecutionID == exec.ExecutionID && x.FolioPath == exec.FolioPath);
                    if (_allocations.Contains(found))
                        _allocations.Remove(found);
                    _allocations.Add(exec);
                }
            }
            _allocations.SortByOrderID();
        }

        private void RemoveExecutionFromCache(object sender, CSxExecutionAllocationArgs e)
        {
            foreach (var exec in e.ExecList)
            {
                if (_allocations.Contains(exec))
                    _allocations.Remove(exec);
            }
            _allocations.SortByOrderID();
        }

        private void AllocationsOnRefreshNeeded(object sender, EventArgs eventArgs)
        {
            _allocations.AddExecutionToCache(CSxDataFacade.GetAllAllocatedExecutions());
        }

    }

}
