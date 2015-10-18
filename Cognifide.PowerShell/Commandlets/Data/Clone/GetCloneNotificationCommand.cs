using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Web;
using Cognifide.PowerShell.Core.Extensions;
using Sitecore.Data.Clones;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;

namespace Cognifide.PowerShell.Commandlets.Data.Clone
{
    [Cmdlet("Get", "ItemNotification")]
    [OutputType(typeof(Notification), ParameterSetName = new []{ "Item from Pipeline", "Item from Path", "Item from ID"})]
    public class GetCloneNotificationCommand : CloneNotificationCommand
    {
        protected override void ProcessNotification(NotificationProvider notificationProvider, Item item)
        {
            notificationProvider.GetNotifications(item).ToList().ForEach(WriteNotification);
        }

    }


    
}