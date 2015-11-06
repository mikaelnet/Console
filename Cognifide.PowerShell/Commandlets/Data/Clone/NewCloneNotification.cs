using System;
using System.Management.Automation;
using Sitecore.Data;
using Sitecore.Data.Clones;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Cognifide.PowerShell.Commandlets.Data.Clone
{
    [Cmdlet("New", "ChildCreatedNotification")]
    [OutputType(typeof(ChildCreatedNotification))]
    public class NewChildCreatedNotification : CloneNotificationCommand
    {
        [Parameter(Mandatory = true)]
        public ID ChildId { get; set; }

        protected override void ProcessNotification(NotificationProvider notificationProvider, Item item)
        {
            var notification = new ChildCreatedNotification();
            notification.ChildId = ChildId;
            notification.Processed = false;
            notification.Uri = new ItemUri(item.ID, item.Paths.FullPath, Language.Invariant, Sitecore.Data.Version.Latest, item.Database.Name);
            WriteNotification(notification);
        }
    }

    [Cmdlet("New", "FieldChangedNotification")]
    public class NewFieldChangedNotification
    {
        
    }

    public class NewItemMovedChildCreatedNotification
    {
        
    }

    public class NewItemMovedChildRemovedNotification
    {
        
    }

    public class NewItemMovedNotification
    {
        
    }

    public class NewItemTreeMovedNotification
    {
        
    }

    public class NewOriginalItemChangedTemplateNotification
    {
        
    }
}