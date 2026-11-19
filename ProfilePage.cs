namespace MauiApp1;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;

internal sealed class ProfilePage : ContentPage
{
    public ProfilePage(ObservableCollection<StockItem> products)
    {
        var nameEntry = new Entry
        {
            Placeholder = "Как тебя зовут?"
        };

        var foodPreference = new Picker
        {
            Title = "Выбери тип питания",
            ItemsSource = new[]
            {
                "Без ограничений",
                "Вегетарианское",
                "Веганское"
            },
            SelectedIndex = 0
        };

        var dislikedProducts = new Entry
        {
            Placeholder = "Например, лук, оливки"
        };

        var allergies = new Entry
        {
            Placeholder = "Например, арахис, молоко"
        };

        var notifications = new Switch
        {
            IsToggled = true
        };

        var warningDays = new Picker
        {
            Title = "За сколько дней предупреждать?",
            ItemsSource = new[] { "За 1 день", "За 2 дня", "За 3 дня" },
            SelectedIndex = 2
        };

        var notificationTime = new TimePicker
        {
            Time = new TimeSpan(10, 0, 0)
        };

        var saveButton = new Button
        {
            Text = "Сохранить настройки"
        };
        // Загружаем ранее сохранённые настройки.
        nameEntry.Text = Preferences.Default.Get("profile_name", "");
        dislikedProducts.Text = Preferences.Default.Get("profile_disliked", "");
        allergies.Text = Preferences.Default.Get("profile_allergies", "");

        foodPreference.SelectedIndex = Math.Clamp(
            Preferences.Default.Get("profile_food_type", 0),
            0,
            foodPreference.ItemsSource.Count - 1);

        notifications.IsToggled =
            Preferences.Default.Get("profile_notifications", true);

        warningDays.SelectedIndex = Math.Clamp(
            Preferences.Default.Get("profile_warning_days", 2),
            0,
            warningDays.ItemsSource.Count - 1);

        int savedMinutes = Math.Clamp(
            Preferences.Default.Get("profile_notification_minutes", 600),
            0,
            1439);

        notificationTime.Time = TimeSpan.FromMinutes(savedMinutes);

        // Сохраняем значения при нажатии кнопки.
        saveButton.Clicked += async (_, _) =>
        {
            Preferences.Default.Set(
                "profile_name",
                (nameEntry.Text ?? "").Trim());

            Preferences.Default.Set(
                "profile_disliked",
                (dislikedProducts.Text ?? "").Trim());

            Preferences.Default.Set(
                "profile_allergies",
                (allergies.Text ?? "").Trim());

            Preferences.Default.Set(
                "profile_food_type",
                foodPreference.SelectedIndex);

            Preferences.Default.Set(
                "profile_notifications",
                notifications.IsToggled);

            Preferences.Default.Set(
                "profile_warning_days",
                warningDays.SelectedIndex);

            Preferences.Default.Set(
                "profile_notification_minutes",
                (int)notificationTime.Time.TotalMinutes);

            await DisplayAlert(
                "Настройки сохранены",
                "Они останутся после закрытия приложения.",
                "ОК");
        };

        var profileScroll = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "Мой профиль",
                        FontSize = 28
                    },
                    new Label { Text = "Имя" },
                    nameEntry,
                    new Label { Text = "Тип питания" },
                    foodPreference,
                    new Label { Text = "Не люблю" },
                    dislikedProducts,
                    new Label { Text = "Аллергии" },
                    allergies,
                    new Label { Text = "Напоминать о сроках годности" },
                    notifications,
                    warningDays,
                    new Label { Text = "Время напоминания" },
                    notificationTime,
                    saveButton
                }
            }
        };
        var fridgeButton = new Button
        {
            Text = "Холодильник",
            Padding = new Thickness(4, 12)
        };

        var recipesButton = new Button
        {
            Text = "Рецепты",
            Padding = new Thickness(4, 12)
        };

        var homeButton = new Button
        {
            Text = "Главная",
            Padding = new Thickness(4, 12)
        };

        fridgeButton.Clicked += async (_, _) =>
        {
            var navigation = Navigation;

            await navigation.PopToRootAsync(animated: false);

            await navigation.PushAsync(
                new ProductsPage(products, async (sender, args) =>
                {
                    await navigation.PushAsync(
                        new AddProductPage(products),
                        animated: false);
                }),
                animated: false);
        };

        recipesButton.Clicked += async (_, _) =>
        {
            await Navigation.PushAsync(
                new RecipesPage(products),
                animated: false);
        };

        var bottomBar = new Grid
        {
            Padding = new Thickness(16, 0, 16, 16),
            ColumnSpacing = 8,
            ColumnDefinitions =
    {
        new ColumnDefinition { Width = GridLength.Star },
        new ColumnDefinition { Width = GridLength.Star },
        new ColumnDefinition { Width = GridLength.Star }
    },
            Children = { fridgeButton, recipesButton, homeButton }
        };

        Grid.SetColumn(recipesButton, 1);
        Grid.SetColumn(homeButton, 2);

        var pageLayout = new Grid
        {
            RowDefinitions =
    {
        new RowDefinition { Height = GridLength.Star },
        new RowDefinition { Height = GridLength.Auto }
    },
            Children = { profileScroll, bottomBar }
        };

        Grid.SetRow(bottomBar, 1);
        Content = pageLayout;

        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
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