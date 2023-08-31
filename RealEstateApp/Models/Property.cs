using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RealEstateApp.Models
{
    public class Property : INotifyPropertyChanged
    {
        public Property()
        {
            Id = Guid.NewGuid().ToString();

            ImageUrls = new List<string>();
        }

        public string Id { get; set; }
        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }
        public int? Price { get; set; }
        public string Description { get; set; }
        public int? Beds { get; set; }
        public int? Baths { get; set; }
        public int? Parking { get; set; }
        public int? LandSize { get; set; }
        public string AgentId { get; set; }
        public List<string> ImageUrls { get; set; }

        private double? _lat;
        public double? Latitude { get { return _lat; } set { _lat = value; OnPropertyChanged(); } }

        private double? _long;
        public double? Longitude { get { return _long; } set { _long = value; OnPropertyChanged(); } }
        private string _aspect;
        public string Aspect { get => _aspect; set { _aspect = value; OnPropertyChanged(); } }
        public Vendor Vendor { get; set; }
        public string NeighbourhoodUrl { get; set; }


        public string MainImageUrl => ImageUrls?.FirstOrDefault() ?? GlobalSettings.Instance.NoImageUrl;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
