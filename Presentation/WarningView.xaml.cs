namespace SkyLock.Presentation;

public partial class WarningView : ContentView
{
    public WarningView()
    {
        InitializeComponent();
    }

    public void UpdateTime(int secondsLeft)
    {
        // Hide the message outside the warning period.
        if (secondsLeft <= 0 || secondsLeft > 30)
        {
            brdWarning.IsVisible = false;
            return;
        }

        brdWarning.IsVisible = true;

        if (secondsLeft <= 10)
        {
            brdWarning.Style = (Style)Application.Current!
                .Resources["WarningCriticalBannerStyle"];

            lblWarningMessage.Text =
                $"Brace for impact!\n{secondsLeft}s until time runs out!";
        }
        else
        {
            brdWarning.Style = (Style)Application.Current!
                .Resources["WarningBannerStyle"];

            lblWarningMessage.Text =
                $"Warning: Altitude dropping!\n{secondsLeft}s remaining";
        }
    }
}