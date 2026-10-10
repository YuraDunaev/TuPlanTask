using CadBridge.Core.Models;

namespace CadBridge.Core.Tests.Models;

public sealed class WarningCodeTextTests
{
    [Fact]
    public void ShortLabel_MatchesDrawingContract()
    {
        // Метки видны на чертеже — контракт с DwgIssueReporter фиксируем тестом.
        Assert.Equal("OPEN", WarningCodeText.ShortLabel(WarningCode.OpenPolyline));
        Assert.Equal("NO TAG", WarningCodeText.ShortLabel(WarningCode.NoTag));
        Assert.Equal("TAGS x2", WarningCodeText.ShortLabel(WarningCode.MultipleTags));
        Assert.Equal("ORPHAN", WarningCodeText.ShortLabel(WarningCode.OrphanTag));
        Assert.Equal("CALC", WarningCodeText.ShortLabel(WarningCode.CalculationFailed));
        Assert.Equal("ISSUE", WarningCodeText.ShortLabel(WarningCode.UnknownCode));
    }

    [Fact]
    public void ShortLabel_IsUniquePerCode()
    {
        var labels = Enum.GetValues<WarningCode>().Select(WarningCodeText.ShortLabel).ToList();
        Assert.Equal(labels.Count, labels.Distinct().Count());
    }

    [Fact]
    public void Description_ExistsForEveryCode()
    {
        foreach (var code in Enum.GetValues<WarningCode>())
        {
            var description = WarningCodeText.Description(code);
            Assert.False(string.IsNullOrWhiteSpace(description), $"Нет пояснения для {code}.");
        }
    }

    [Fact]
    public void Description_IsUniquePerCode()
    {
        var descriptions = Enum.GetValues<WarningCode>()
            .Select(WarningCodeText.Description)
            .ToList();
        Assert.Equal(descriptions.Count, descriptions.Distinct().Count());
    }

    [Fact]
    public void Description_CoversKnownCases()
    {
        // Пояснения из легенды: битый контур / нет тега — по ним пользователь сверяет чертёж.
        Assert.Contains("Битый контур", WarningCodeText.Description(WarningCode.OpenPolyline));
        Assert.Contains("Нет тега", WarningCodeText.Description(WarningCode.NoTag));
    }
}
