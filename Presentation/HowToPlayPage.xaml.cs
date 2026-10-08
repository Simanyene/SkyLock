namespace SkyLock.Presentation;

public partial class HowToPlayPage : ContentPage
{
    public HowToPlayPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}