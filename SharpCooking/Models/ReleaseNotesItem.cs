using System;
using System.Collections.Generic;
using System.Text;

namespace SharpCooking.Models
{
    public class ReleaseNotesItem
    {
        public string Version { get; set; }
        public string[] New { get; set; }
        public string[] KnownIssues { get; set; }
    }
}
