using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace MauiApp1;

internal sealed class BarcodeScannerPage : ContentPage
{
    private readonly CameraBarcodeReaderView _camera;
    private readonly Action<string> _onScanned;
    private int _scanHandled;

    public BarcodeScannerPage(Action<string> onScanned)
    {
        _onScanned = onScanned;

        _camera = new CameraBarcodeReaderView
        {
            IsDetecting = false,
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.OneDimensional,
                AutoRotate = true,
                Multiple = false
            }
        };

        _camera.BarcodesDetected += OnBarcodesDetected;

        var cancelButton = new Button
        {
            Text = "Отмена"
        };

        cancelButton.Clicked += async (_, _) =>
        {
            _camera.IsDetecting = false;
            await Navigation.PopAsync(animated: false);
        };

        var layout = new Grid
        {
            Padding = 16,
            RowSpacing = 12,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        layout.Children.Add(new Label
        {
            Text = "Наведи камеру на штрихкод",
            FontSize = 22,
            HorizontalTextAlignment = TextAlignment.Center
        });

        layout.Children.Add(_camera);
        Grid.SetRow(_camera, 1);

        layout.Children.Add(cancelButton);
        Grid.SetRow(cancelButton, 2);

        Content = layout;

        Shell.SetNavBarIsVisible(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var permission =
            await Permissions.RequestAsync<Permissions.Camera>();

        if (permission != PermissionStatus.Granted)
        {
            await DisplayAlert(
                "Камера недоступна",
                "Для сканирования разреши приложению доступ к камере.",
                "ОК");

            await Navigation.PopAsync(animated: false);
            return;
        }

        _camera.IsDetecting = true;
    }

    private void OnBarcodesDetected(
        object? sender,
        BarcodeDetectionEventArgs e)
    {
        var code = e.Results.FirstOrDefault()?.Value;

        if (string.IsNullOrWhiteSpace(code))
            return;

        if (System.Threading.Interlocked.Exchange(ref _scanHandled, 1) != 0)
            return;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _camera.IsDetecting = false;
            await Navigation.PopAsync(animated: false);
            _onScanned(code);
        });
    }

    protected override void OnDisappearing()
    {
        _camera.IsDetecting = false;
        base.OnDisappearing();
    }
}