using Microsoft.Maui.Dispatching;
using Plugin.Maui.Audio;

namespace SkyLock.Presentation;

public partial class LoadingPage : ContentPage
{
    private readonly IDispatcherTimer loadingTimer;
    private int loadingPercentage;

    private Stream? loadingSoundStream;
    private IAudioPlayer? loadingPlayer;

    public LoadingPage()
    {
        InitializeComponent();

        loadingTimer = Dispatcher.CreateTimer();
        loadingTimer.Interval = TimeSpan.FromMilliseconds(40);
        loadingTimer.IsRepeating = true;
        loadingTimer.Tick += OnLoadingTimerTick;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        loadingTimer.Stop();

        loadingPercentage = 0;
        lblLoadingStatus.Text = "Loading... 0%";
        bxvLoadingFill.WidthRequest = 0;

        StartLoadingSound();

        loadingTimer.Start();
    }

    private void OnLoadingTimerTick(object? sender, EventArgs e)
    {
        if (grdLoadingTrack.Width <= 0)
            return;

        if (loadingPercentage >= 100)
        {
            loadingTimer.Stop();
            StopLoadingSound();

            if (Window is not null)
                Window.Page = new WelcomePage();

            return;
        }

        loadingPercentage++;

        lblLoadingStatus.Text = $"Loading... {loadingPercentage}%";

        bxvLoadingFill.WidthRequest =
            grdLoadingTrack.Width * loadingPercentage / 100.0;
    }

    private void StartLoadingSound()
    {
        StopLoadingSound();

        loadingSoundStream = GetType().Assembly.GetManifestResourceStream(
            "SkyLock.Resources.Raw.loading_sound.wav");

        if (loadingSoundStream is null)
        {
            System.Diagnostics.Debug.WriteLine("Sound resource not found.");
            return;
        }

        loadingPlayer = AudioManager.Current.CreatePlayer(loadingSoundStream);
        loadingPlayer.Loop = true;
        loadingPlayer.Play();
    }

    private void StopLoadingSound()
    {
        loadingPlayer?.Stop();
        loadingPlayer?.Dispose();
        loadingPlayer = null;

        loadingSoundStream?.Dispose();
        loadingSoundStream = null;
    }

    protected override void OnDisappearing()
    {
        loadingTimer.Stop();
        StopLoadingSound();

        base.OnDisappearing();
    }
}