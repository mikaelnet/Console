﻿using System;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;

namespace Cognifide.PowerShell.Client.Commands.MenuItems
{
    [Serializable]
    public class EditPowerShellScript : Command
    {
        public override CommandState QueryState(CommandContext context)
        {
            if (context.Items.Length != 1)
                return CommandState.Hidden;

            return context.Items[0].Template.Name.Equals("PowerShell Script", StringComparison.OrdinalIgnoreCase)
                ? CommandState.Enabled
                : CommandState.Hidden;
        }

        public override void Execute(CommandContext context)
        {
            var itemId = context.Items[0].ID.ToString();
            var itemDb = context.Items[0].Database.Name;
            var item = Factory.GetDatabase(itemDb).GetItem(new ID(itemId));

            var urlString = new UrlString();
            urlString.Append("id", item.ID.ToString());
            urlString.Append("db", itemDb);
            if (!string.IsNullOrEmpty(context.Parameters["frameName"]))
            {
                urlString.Add("pfn", context.Parameters["frameName"]);
            }
            Windows.RunApplication("PowerShell/PowerShellIse", urlString.ToString());
        }
    }
}