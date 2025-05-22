using CommunityToolkit.Mvvm.Input;
using Network_Traffic_Analyzer.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Network_Traffic_Analyzer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<DeviceData> NetworkDevices { get; } = new();
        public ObservableCollection<PacketData> CapturedPackets { get; } = new();

        private DeviceData _selectedDevice;
        public DeviceData SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartCaptureCommand => new RelayCommand(() =>
        {
            if (SelectedDevice != null)
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