using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using sophis;
using sophis.instrument;
using sophis.oms;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
using Sophis.Activities;
using Sophis.Logging;
using Sophis.OMS.Activities;
using Sophis.WF.Core;

namespace MEDIO.OMS.WF4Activities.Activity
{
    public sealed class GetGroupId : CodeActivity
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();

        [Category("Medio order routing")]
        [Description(@"Out_GroupId")]
        public OutArgument<int> Out_GroupId
        {
            get;
            set;
        }


        //To access creator userId in the workflow use CreationInfo.UserID
        //To access to the event raiser userUd create a variable Sophis.OMS.WFEvents.OrderEventArgs OrderEventArgs.
        //Map it with the Result field of the activity WaitForUserAction before this activity
        [Category("Medio order routing")]
        [Description(@"In_UserId")]
        public InArgument<int> In_UserId
        {
            get;
            set;
        }

      
        protected override void Execute(CodeActivityContext context)
        {
            int groupId = GetSphGroupId(this.In_UserId.Get(context));
            this.Out_GroupId.Set(context, groupId);
            _logger.LogDebug("GetGroupId::Execute CreatorId " + this.In_UserId.Get(context) + " groupId " + this.In_UserId.Get(context));
        }

        private static int GetSphGroupId(int userId)
        {
            int groupId = 0;
            
            SophisWcfConfig.SynchronizationContext.Send(delegate(object _)
            {
                try
                {
                    CSMUserRights rights = new CSMUserRights(Convert.ToUInt32(userId));
                    groupId = rights.GetParentID();
                    _logger.LogDebug("GetGroupId::GetSphGroupId. UserId : " + userId + "; GroupId : " + groupId);
                }
                catch (Exception ex)
                {
                    _logger.LogError("GetGroupId::GetSphGroupId. Error while getting groupId for userId : " + userId + " : " + ex.ToString());
                    groupId = -1;
                }
            }, null);
            
            return groupId;
        }

    }

}
