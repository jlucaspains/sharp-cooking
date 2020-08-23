using System;
using System.Collections.Generic;
using System.Text;

namespace SharpCooking.Models
{
    public class ReleaseNotesItem
    {
        public string Version { get; set; }
        public IEnumerable<string> New { get; set; }
        public IEnumerable<string> KnownIssues { get; set; }
    }
}
