using CommunityToolkit.Mvvm.Input;
using Network_Traffic_Analyzer.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Network_Traffic_Analyzer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<DeviceData> NetworkDevices { get; } = new(); // Collection for DeviceData
        public ObservableCollection<PacketData> CapturedPackets { get; } = new(); // Collection for PacketData

        private DeviceData _selectedDevice;
        public DeviceData SelectedDevice
        {
            get => _selectedDevice; // Returns the current value
            set // Updates the _selectedDevice property
            {
                _selectedDevice = value;
                OnPropertyChanged(); // Notifies the UI that the property has changed and should be updated
            }
        }

        public ICommand StartCaptureCommand => new RelayCommand(() =>
        {
            if (SelectedDevice != null) // Can only start analyzing the traffic if SelectedDevice is valid
            {
                NetworkFunctions.AnalyzeTraffic(SelectedDevice, this);
            }
        });

        public MainViewModel()
        {
            NetworkFunctions.LoadDevices(this);
        }
    }
}