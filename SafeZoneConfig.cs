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

        private string _CustomDataTag = "Safezone";

        public String CustomDataTag { get => _CustomDataTag; set => SetValue(ref _CustomDataTag, value); }

        public ObservableCollection<long> ProtectedEntityIds { get; } = new ObservableCollection<long>();
        
    }
}
