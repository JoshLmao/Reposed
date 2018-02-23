using Reposed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposed.Events
{
    public class PreferencesUpdated
    {
        public Models.Preferences Prefs { get; set; }
        public PreferencesUpdated(Models.Preferences prefs)
        {
            Prefs = prefs;
        }
    }
}
