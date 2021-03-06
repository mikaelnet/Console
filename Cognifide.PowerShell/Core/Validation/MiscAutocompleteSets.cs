﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Data.Managers;

namespace Cognifide.PowerShell.Core.Validation
{
    public class MiscAutocompleteSets
    {
        private static readonly string[] standardItemTypeCompleters =
        {
            "File",
            "Directory",
            "SymbolicLink",
            "Junction",
            "HardLink"
        };

        private static readonly Dictionary<string, string> completers = new Dictionary<string, string>()
        {
            {"New-Item:ItemType", "[Cognifide.PowerShell.Core.Validation.MiscAutocompleteSets]::Templates"}
        };

        public static Dictionary<string, string> Completers
        {
            get { return completers; }
        }

        public static string[] Templates
        {
            get
            {
                var result = new List<string>(standardItemTypeCompleters);
                result.AddRange(TemplateManager.GetTemplates(Factory.GetDatabase("master"))
                        .Select(template => "\""+template.Value.FullName+ "\"").OrderBy(a => a));
                return result.ToArray();
            }
        }
    }
}