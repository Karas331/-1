using System.Collections.ObjectModel;

namespace MauiApp1;

internal sealed class ProductsPage : ContentPage
{
    public ProductsPage(
        ObservableCollection<StockItem> products,
        EventHandler openAddProductPage)
    {
        var addButton = new Button
        {
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Padding = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        SemanticProperties.SetDescription(addButton, "Добавить продукт");
        addButton.Clicked += openAddProductPage;

        var productList = new CollectionView
        {
            ItemsSource = products
        };

        var emptyMessage = new Label
        {
            Text = "Твой холодильник пуст!\nНажми на полку, чтобы\nдобавить продукты",
            FontFamily = "IosevkaCharon",
            FontSize = 24,
            TextColor = Color.FromArgb("#295B61"),
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true,
            IsVisible = false
        };

        emptyMessage.Triggers.Add(new DataTrigger(typeof(Label))
        {
            Binding = new Binding("Count", source: products),
            Value = 0,
            Setters =
                            {
                                new Setter { Property = VisualElement.IsVisibleProperty, Value = true }
                            }
        });

        var content = new Grid
        {
            Padding = 16,
            RowSpacing = 12,
            RowDefinitions =
                        {
                            new RowDefinition { Height = new GridLength(4, GridUnitType.Star) },
                            new RowDefinition { Height = GridLength.Star }
                        }
        };

        content.Children.Add(addButton);
        content.Children.Add(productList);
        Grid.SetRow(productList, 1);
        Grid.SetRowSpan(emptyMessage, 2);
        content.Children.Add(emptyMessage);

        Title = "Мои продукты";
        Content = new Grid
        {
            Children =
            {
                new Image
                {
                    Source = "add_product.png",
                    Aspect = Aspect.AspectFill,
                    InputTransparent = true
                },
                content
            }
        };

        Shell.SetNavBarIsVisible(this, false);
    }
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Navigation.PopAsync(animated: false);
        });

        return true;
    }
}