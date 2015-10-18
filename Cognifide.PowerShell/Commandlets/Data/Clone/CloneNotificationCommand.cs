using System;
using System.Management.Automation;
using Sitecore.Data.Clones;
using Sitecore.Data.Items;

namespace Cognifide.PowerShell.Commandlets.Data.Clone
{
    public abstract class CloneNotificationCommand : BaseLanguageAgnosticItemCommand
    {
        protected override void ProcessItem(Item item)
        {
            var notificationProvider = item.Database.NotificationProvider;
            if (notificationProvider == null)
            {
                // No notification provider exists for db
                WriteError(new ErrorRecord(new NotSupportedException(
                    $"The database {item.Database.Name} does not have a configured notification provider"),
                    "sitecore_no_item_notification_provider", ErrorCategory.InvalidOperation, item));
                return;
            }

            ProcessNotification(notificationProvider, item);
        }

        protected abstract void ProcessNotification(NotificationProvider notificationProvider, Item item);

        protected void WriteNotification(Notification notification)
        {
            if (notification != null)
            {
                var psobj = GetPsObject(SessionState, notification);
                WriteObject(psobj);
            }
        }

        internal static PSObject GetPsObject(SessionState provider, Notification notification)
        {
            var psobj = PSObject.AsPSObject(notification);

            psobj.Properties.Add(new PSNoteProperty("Type", notification.GetType().Name));

            return psobj;
        }
    }
}