﻿using System;
using System.Management.Automation;
using Sitecore.Data.Clones;
using Sitecore.Data.Items;

namespace Cognifide.PowerShell.Commandlets.Data.Clone
{
    [Cmdlet("Add", "ItemNotification")]
    public class AddCloneNotificationCommand : CloneNotificationCommand
    {
        [Parameter(Mandatory = true)]
        public Notification Notification { get; set; }

        protected override void ProcessNotification(NotificationProvider notificationProvider, Item item)
        {
            notificationProvider.AddNotification(Notification);
        }
    }
}