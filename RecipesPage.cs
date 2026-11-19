using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace MauiApp1;

internal sealed class RecipesPage : ContentPage
{
    public RecipesPage(ObservableCollection<StockItem> products)
    {
        var recipes = new (
                string Name,
                (string Product, decimal Amount, string Unit)[] Ingredients,
                string Steps)[]
            {
        (
            "Омлет с молоком",
            new[] { ("Яйца", 2m, "шт."), ("Молоко", 100m, "мл") },
            "1. Взбей яйца с молоком.\n" +
            "2. Вылей смесь на разогретую сковороду.\n" +
            "3. Готовь под крышкой на слабом огне, пока омлет не схватится."
        ),
        (
            "Яичница с помидором",
            new[] { ("Яйца", 2m, "шт."), ("Помидор", 1m, "шт.") },
            "1. Нарежь помидор и слегка обжарь его.\n" +
            "2. Разбей яйца в сковороду.\n" +
            "3. Готовь на среднем огне, пока белок и желток не схватятся."
        ),
        (
            "Омлет с сыром",
            new[] { ("Яйца", 2m, "шт."), ("Сыр", 50m, "г") },
            "1. Натри сыр и взбей яйца.\n" +
            "2. Вылей яйца на разогретую сковороду и посыпь сыром.\n" +
            "3. Готовь под крышкой на слабом огне, пока омлет не схватится."
        )
            };

        var today = DateTime.Today;
        var soon = today.AddDays(3);
        var freshStock = products.Where(item => item.ExpiresOn >= today).ToList();

        var recommended = new List<(DateTime Expiry, View Card)>();
        var otherRecipes = new List<View>();



        foreach (var recipe in recipes)
        {
            var missing = new List<string>();
            StockItem? urgentProduct = null;

            foreach (var ingredient in recipe.Ingredients)
            {
                var available = freshStock
                    .Where(item => item.Name == ingredient.Product &&
                                   item.Unit == ingredient.Unit)
                    .ToList();

                var shortage = ingredient.Amount - available.Sum(item => item.Amount);
                if (shortage > 0)
                    missing.Add($"{ingredient.Product} — {shortage} {ingredient.Unit}");

                var earliest = available.OrderBy(item => item.ExpiresOn).FirstOrDefault();
                if (earliest is not null && earliest.ExpiresOn <= soon &&
                    (urgentProduct is null || earliest.ExpiresOn < urgentProduct.ExpiresOn))
                {
                    urgentProduct = earliest;
                }
            }

            var ingredientsText = string.Join("\n", recipe.Ingredients.Select(
                item => $"• {item.Product} — {item.Amount} {item.Unit}"));

            var details = new VerticalStackLayout
            {
                Padding = new Thickness(12, 0, 12, 8),
                Spacing = 8,
                IsVisible = false,
                Children =
            {
                new Label { Text = $"Ингредиенты\n{ingredientsText}" },
                new Label { Text = $"Приготовление\n{recipe.Steps}" }
            }
            };

            var cookedButton = new Button
            {
                Text = "Приготовлено"
            };

            cookedButton.Clicked += async (_, _) =>
            {
                cookedButton.IsEnabled = false;

                try
                {
                    var requiredIngredients = recipe.Ingredients
                        .GroupBy(item => (item.Product, item.Unit))
                        .Select(group => new
                        {
                            Product = group.Key.Product,
                            Unit = group.Key.Unit,
                            Amount = group.Sum(item => item.Amount)
                        })
                        .ToList();

                    var deductions = new List<(int Index, decimal Amount)>();

                    foreach (var ingredient in requiredIngredients)
                    {
                        decimal remaining = ingredient.Amount;

                        var packages = products
                            .Select((item, index) => new { Item = item, Index = index })
                            .Where(package =>
                                package.Item.Name == ingredient.Product &&
                                package.Item.Unit == ingredient.Unit &&
                                package.Item.Amount > 0 &&
                                package.Item.ExpiresOn.Date >= DateTime.Today)
                            .OrderBy(package => package.Item.ExpiresOn)
                            .ToList();

                        foreach (var package in packages)
                        {
                            if (remaining <= 0)
                                break;

                            decimal used = Math.Min(remaining, package.Item.Amount);

                            deductions.Add((package.Index, used));
                            remaining -= used;
                        }

                        if (remaining > 0)
                        {
                            await DisplayAlert(
                                "Не хватает продуктов",
                                $"Нужно ещё: {ingredient.Product} — " +
                                $"{remaining} {ingredient.Unit}.",
                                "ОК");

                            return;
                        }
                    }

                    bool confirmed = await DisplayAlert(
                        "Блюдо приготовлено?",
                        $"Списать продукты для блюда «{recipe.Name}»?",
                        "Да, списать",
                        "Отмена");

                    if (!confirmed)
                        return;

                    foreach (var deduction in deductions.OrderByDescending(item => item.Index))
                    {
                        var package = products[deduction.Index];
                        decimal remainder = package.Amount - deduction.Amount;

                        if (remainder == 0)
                        {
                            products.RemoveAt(deduction.Index);
                        }
                        else
                        {
                            products[deduction.Index] = package with
                            {
                                Amount = remainder
                            };
                        }
                    }

                    await DisplayAlert(
                        "Готово",
                        "Использованные продукты списаны из холодильника.",
                        "ОК");

                    await Navigation.PopAsync(animated: false);
                }
                finally
                {
                    cookedButton.IsEnabled = true;
                }
            };

            details.Children.Add(cookedButton);

            var titleButton = new Button { Text = $"▸ {recipe.Name}" };
            titleButton.Clicked += (_, _) =>
            {
                details.IsVisible = !details.IsVisible;
                titleButton.Text = $"{(details.IsVisible ? "▾" : "▸")} {recipe.Name}";
            };

            var urgentIngredients = freshStock
    .Where(item =>
        item.Amount > 0 &&
        item.ExpiresOn <= soon &&
        recipe.Ingredients.Any(ingredient =>
            ingredient.Product == item.Name &&
            ingredient.Unit == item.Unit))
            .GroupBy(item => item.Name)
            .Select(group => group.OrderBy(item => item.ExpiresOn).First())
            .OrderBy(item => item.ExpiresOn)
            .Select(item => $"{item.Name} (до {item.ExpiresOn:dd.MM})")
            .ToList();

            string status = missing.Count > 0
                ? $"Не хватает: {string.Join(", ", missing)}"
                : "Всё есть";

            if (urgentIngredients.Count > 0)
            {
                status += "\nИспользуй в первую очередь: " +
                string.Join(", ", urgentIngredients);
            }
            ;

            var card = new VerticalStackLayout
            {
                Spacing = 6,
                Children = { titleButton, new Label { Text = status }, details }
            };

            if (missing.Count == 0 && urgentProduct is not null)
                recommended.Add((urgentProduct.ExpiresOn, card));
            else
                otherRecipes.Add(card);
        }


        var layout = new VerticalStackLayout
        {
            Padding = 16,
            Spacing = 12,
            Children = { new Label { Text = "Что приготовить", FontSize = 26 } }
        };

        if (recommended.Count > 0)
        {
            layout.Children.Add(new Label { Text = "Приготовь в первую очередь", FontSize = 21 });

            foreach (var recipe in recommended.OrderBy(item => item.Expiry))
                layout.Children.Add(recipe.Card);
        }

        if (otherRecipes.Count > 0)
        {
            layout.Children.Add(new Label { Text = "Остальные рецепты", FontSize = 21 });

            foreach (var card in otherRecipes)
                layout.Children.Add(card);
        }

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

            var fridgePage = new ProductsPage(
                products,
                async (sender, args) =>
                {
                    await navigation.PushAsync(
                        new AddProductPage(products),
                        animated: false);
                });

            await navigation.PushAsync(fridgePage, animated: false);
        };

        homeButton.Clicked += async (_, _) =>
        {
            await Navigation.PushAsync(
                new ProfilePage(products),
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

        var recipeScroll = new ScrollView { Content = layout };

        var pageLayout = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            },
            Children = { recipeScroll, bottomBar }
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