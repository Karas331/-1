using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1;

internal sealed record StockItem(
    string Name, decimal Amount, string Unit, DateTime ExpiresOn)
{
    public override string ToString() =>
        $"{Name} · {Amount} {Unit} · до {ExpiresOn:dd.MM.yyyy}";
}
