using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;

namespace SafeZonePlugin
{
    public class SafeZoneConfig : ViewModel
    {

        public SafeZoneConfig()
        {
            ProtectedEntityIds.CollectionChanged += (sender, args) => OnPropertyChanged();
        }

        private string _SafeZoneCustomDataTag = "Safezone";
        private long _SafeZoneRadius = 50;
        private bool _GridEditable = false;

        public string SafeZoneCustomDataTag { get => _SafeZoneCustomDataTag; set => SetValue(ref _SafeZoneCustomDataTag, value); }
        public long SafeZoneRadius { get => _SafeZoneRadius; set => SetValue(ref _SafeZoneRadius, value); }
        public bool GridEditable { get => _GridEditable; set => SetValue(ref _GridEditable, value); }

        public ObservableCollection<long> ProtectedEntityIds { get; } = new ObservableCollection<long>();
        
    }
}
