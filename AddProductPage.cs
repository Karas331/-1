using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Channels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MauiApp1;

internal sealed class AddProductPage : ContentPage
{
    public AddProductPage(ObservableCollection<StockItem> products)
    {
        string[] catalog = { "Молоко", "Яйца", "Сыр", "Яблоко", "Помидор" };

        var search = new SearchBar { Placeholder = "Найти продукт" };
        var barcodeLabel = new Label
        {
            Text = "Штрихкод ещё не считан"
        };

        var scanButton = new Button
        {
            Text = "Сканировать штрихкод"
        };

        scanButton.Clicked += async (_, _) =>
        {
            await Navigation.PushAsync(
                new BarcodeScannerPage(code =>
                {
                    barcodeLabel.Text = $"Штрихкод: {code}";
                }),
                animated: false);
        };
        var productPicker = new Picker
        {
            Title = "Выбери продукт из списка",
            ItemsSource = catalog
        };

        search.TextChanged += (_, args) =>
        {
            string query = (args.NewTextValue ?? "").Trim();

            productPicker.ItemsSource = catalog
                .Where(name => name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        };

        var expiry = new DatePicker { Date = DateTime.Today };
        var quantity = new Entry
        {
            Placeholder = "Например, 500",
            Keyboard = Keyboard.Numeric
        };

        var unit = new Picker
        {
            ItemsSource = new[] { "г", "мл", "шт." },
            SelectedIndex = 0
        };

        var error = new Label();
        var save = new Button { Text = "Сохранить" };
        var cancel = new Button { Text = "Отмена" };

        save.Clicked += async (_, _) =>
        {
            if (productPicker.SelectedItem is not string product)
            {
                error.Text = "Выбери продукт из списка.";
                return;
            }

            string amountText = (quantity.Text ?? "").Trim().Replace(',', '.');

            if (!decimal.TryParse(amountText,
                    System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal amount) || amount <= 0)
            {
                error.Text = "Введи количество больше нуля.";
                return;
            }



            products.Add(new StockItem(product, amount, (string)unit.SelectedItem, expiry.Date.Date));
            await Navigation.PopAsync(animated: false);
        };

        cancel.Clicked += async (_, _) => await Navigation.PopAsync(animated: false);

        Title = "Добавить продукт";
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 12,
                Children =
                {
                    scanButton,
                    barcodeLabel,
                    search,
                    productPicker,
                    new Label { Text = "Срок годности" },
                    expiry,
                    new Label { Text = "Количество" },
                    quantity,
                    unit,
                    error,
                    save,
                    cancel
                }
            }
        };

        Shell.SetNavBarIsVisible(this, false);
    }
}