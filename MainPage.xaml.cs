namespace MauiApp1
{

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private readonly System.Collections.ObjectModel.ObservableCollection<StockItem>
            _products = new();

        private async void OnHelloClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(
                new ProductsPage(_products, OpenAddProductPage),
                animated: false);
        }
        private async void OpenAddProductPage(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddProductPage(_products), animated: false);
        }


        private async void OnRecipesClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RecipesPage(_products), animated: false);
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(
                new ProfilePage(_products),
                animated: false);
        }
    }
}