﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management.Automation;
using Cognifide.PowerShell.Core.Utility;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;

namespace Cognifide.PowerShell.Commandlets.Data
{
    [Cmdlet(VerbsCommon.Add, "ItemLanguage", SupportsShouldProcess = true)]
    [OutputType(typeof (Item), ParameterSetName = new[] {"Item from Pipeline", "Item from Path", "Item from ID"}
        )]
    public class AddItemLanguageCommand : BaseItemCommand
    {
        public enum ActionIfExists
        {
            Skip,
            Append,
            OverwriteLatest
        }

        private static readonly HashSet<string> configIgnoredFields =
            new HashSet<string>(Factory.GetStringSet("powershell/translation/ignoredFields/field"));

        private HashSet<string> ignoredFields;

        [Parameter]
        public SwitchParameter Recurse { get; set; }

        [Parameter]
        public ActionIfExists IfExist { get; set; }

        [Parameter(Mandatory = true)]
        public string[] TargetLanguage { get; set; }

        [Parameter]
        public SwitchParameter DoNotCopyFields { get; set; }

        [Parameter]
        public string[] IgnoredFields { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            ignoredFields = new HashSet<string>(configIgnoredFields);
            if (IgnoredFields != null)
            {
                foreach (var ignoredField in IgnoredFields)
                {
                    ignoredFields.Add(ignoredField);
                }
            }
        }

        protected override void ProcessItem(Item item)
        {
            if (!ShouldProcess(item.GetProviderPath(),
                $"Add language '{TargetLanguage.Aggregate((seed, curr) => seed + ", " + curr)}' version(s){(Recurse ? " recursively" : "")}"))
                return;

            foreach (var targetLanguage in TargetLanguage)
            {
                var lang = LanguageManager.GetLanguage(targetLanguage);
                if (lang == null)
                {
                    WriteError(typeof(ObjectNotFoundException), $"Cannot find target language '{targetLanguage}' or it is not enabled.", 
                        ErrorIds.LanguageNotFound, ErrorCategory.ObjectNotFound, item);
                }
                else
                {
                    var latestVersion = item.Versions.GetLatestVersion(lang);
                    if (IfExist != ActionIfExists.Skip || (latestVersion.Versions.Count == 0))
                    {
                        CopyFields(item, latestVersion, false);
                    }
                    if (Recurse)
                    {
                        foreach (Item childItem in item.Children)
                        {
                            ProcessItem(childItem);
                        }
                    }
                }
            }
        }

        public void CopyFields(Item sourceItem, Item targetItem, bool itemWasCreated)
        {
            try
            {
                if (sourceItem.Versions.Count > 0)
                {
                    if (targetItem.Versions.Count == 0 || IfExist == ActionIfExists.Append)
                    {
                        targetItem = targetItem.Versions.AddVersion();
                    }

                    if (!DoNotCopyFields)
                    {
                        targetItem.Editing.BeginEdit();
                        sourceItem.Fields.ReadAll();
                        foreach (Field field in sourceItem.Fields)
                        {
                            if (ShouldProcessField(field, itemWasCreated))
                            {
                                targetItem.Fields[field.Name].SetValue(field.Value, true);
                            }
                        }
                        targetItem.Editing.EndEdit();
                        targetItem.Editing.AcceptChanges();
                    }
                    WriteItem(targetItem);
                }
            }
            catch (Exception exception)
            {
                targetItem.Editing.CancelEdit();
                WriteError(new ErrorRecord(exception, "sitecore_item_translation_field_copy_error",
                    ErrorCategory.NotSpecified, null));
            }
        }

        public bool ShouldProcessField(Field field, bool allowCopyShared)
        {
            if (field.ContainsStandardValue)
            {
                return false;
            }
            return !ignoredFields.Contains(field.Name, StringComparer.OrdinalIgnoreCase) &&
                   (allowCopyShared || !field.Shared);
        }
    }
}