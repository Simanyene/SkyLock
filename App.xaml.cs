using SkyLock.Services;

namespace SkyLock
{
    public partial class App : Application
    {
        public static PlayerAccountService AccountService { get; }
            = new PlayerAccountService();

        public static FlightRecordService FlightRecordsService { get; }
            = new FlightRecordService();

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(
            IActivationState? activationState)
        {
            return new Window(
                new SkyLock.Presentation.LoadingPage());
        }
    }
}