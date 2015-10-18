using System;
using System.Management.Automation;
using Sitecore.Data.Clones;
using Sitecore.Data.Items;

namespace Cognifide.PowerShell.Commandlets.Data.Clone
{
    [Cmdlet("Reject", "ItemNotification")]
    [OutputType(typeof(Notification), ParameterSetName = new[] { "Item from Pipeline", "Item from Path", "Item from ID" })]
    public class RejectNotificationCommand : CloneNotificationCommand
    {
        [Parameter(Mandatory = true)]
        public Notification Notification { get; set; }

        protected override void ProcessNotification(NotificationProvider notificationProvider, Item item)
        {
            Notification.Reject(item);
        }
    }
}