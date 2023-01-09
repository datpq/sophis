using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MEDIO.OrderAutomation.net.Source.DataModel;
using sophis.oms.execution;
//using sophisOrderBlotters;

namespace MEDIO.OrderAutomation.NET.Source.DataModel
{
    public class CSxCustomAllocationData : IList<CSxCustomAllocation>
    {
        public CSxCustomAllocationData()
        {
        }
        
        public CSxCustomAllocationData(IList<CSxCustomAllocation> executions)
        {
            this._Executions = executions;
        }

        private IList<CSxCustomAllocation> _Executions;
        public IList<CSxCustomAllocation> Executions
        {
            get
            {
                if (_Executions == null) _Executions = new List<CSxCustomAllocation>();
                return _Executions;
            }
            set
            {
                _Executions = value;
            }
        }

        #region IList<CSxCustomAllocation> Members

        public int IndexOf(CSxCustomAllocation item)
        {
            return this._Executions.IndexOf(item);
        }

        public void Insert(int index, CSxCustomAllocation item)
        {
            this._Executions.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this._Executions.RemoveAt(index);

        }

        public CSxCustomAllocation this[int index]
        {
            get
            {
                return this._Executions[index];
            }
            set
            {
                this._Executions[index] = value;
            }
        }

        #endregion

        #region ICollection<CSxCustomAllocation> Members

        public void Add(CSxCustomAllocation item)
        {
            this.Executions.Add(item);
        }

        public void AddRange(List<CSxCustomAllocation> CSxCustomAllocationList)
        {
            foreach (CSxCustomAllocation orderExecutionAdapter in CSxCustomAllocationList)
            {
                Add(orderExecutionAdapter);
            }
        }

        public void Add(List<CSxCustomAllocation> CSxCustomAllocationList)
        {
            foreach (CSxCustomAllocation orderExecutionAdapter in CSxCustomAllocationList)
            {
                Add(orderExecutionAdapter);
            }
        }

        public void Clear()
        {
            this._Executions.Clear();
        }

        public bool Contains(CSxCustomAllocation item)
        {
            return this._Executions.Contains(item);
        }

        public void CopyTo(CSxCustomAllocation[] array, int arrayIndex)
        {
            this._Executions.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this._Executions.Count(); }
        }

        public bool IsReadOnly
        {
            get { return this._Executions.IsReadOnly; }
        }

        public bool Remove(CSxCustomAllocation item)
        {
            return this._Executions.Remove(item);
        }

        public bool Remove(List<CSxCustomAllocation> orderExecutionAdapterList)
        {
            bool result = false;
            foreach (CSxCustomAllocation orderExecutionAdapter in orderExecutionAdapterList)
            {
                result = this._Executions.Remove(orderExecutionAdapter);
            }
            return result;
        }

        public void Refresh()
        {
            if (RefreshNeeded != null)
                RefreshNeeded(this, EventArgs.Empty);
        }

        #endregion

        #region IEnumerable<CSxCustomAllocation> Members

        public IEnumerator<CSxCustomAllocation> GetEnumerator()
        {
            return this._Executions.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._Executions.GetEnumerator();
        }

        #endregion

        public void AddExecutionToCache(IList<CSxCustomAllocation> items)
        {
            if (ExecutionCreated != null)
            {
                CSxExecutionAllocationArgs e = new CSxExecutionAllocationArgs(items);
                ExecutionCreated(null, e);
            }
        }

        public void RemoveExecutionFromCache(IList<CSxCustomAllocation> items)
        {
            if (ExecutionRemoved != null)
            {
                CSxExecutionAllocationArgs e = new CSxExecutionAllocationArgs(items);
                ExecutionRemoved(null, e);
            }
        }

        public void UpdateExecutionFromCache(IList<CSxCustomAllocation> items)
        {
            if (ExecutionUpdated != null)
            {
                CSxExecutionAllocationArgs e = new CSxExecutionAllocationArgs(items);
                ExecutionUpdated(null, e);
            }
        }

        public void SortByOrderID()
        {
            this._Executions = _Executions.OrderByDescending(x => x.OrderID).ToList();
        }

        public event EventHandler<CSxExecutionAllocationArgs> ExecutionCreated;
        public event EventHandler<CSxExecutionAllocationArgs> ExecutionUpdated;
        public event EventHandler<CSxExecutionAllocationArgs> ExecutionRemoved;
        public event EventHandler<EventArgs> RefreshNeeded;

    }
}
