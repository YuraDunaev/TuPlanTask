namespace CadBridge.Core.Models;

public static class WarningCodeText
{

    public static string ShortLabel(WarningCode code) => code switch
    {
        WarningCode.OpenPolyline => "OPEN",
        WarningCode.NoTag => "NO TAG",
        WarningCode.MultipleTags => "TAGS x2",
        WarningCode.OrphanTag => "ORPHAN",
        WarningCode.CalculationFailed => "CALC",
        _ => "ISSUE",
    };

    public static string Description(WarningCode code) => code switch
    {
        WarningCode.OpenPolyline => "Битый контур: полилиния границы помещения не замкнута.",
        WarningCode.NoTag => "Нет тега: внутри контура не найдена подпись с номером помещения.",
        WarningCode.MultipleTags => "Несколько тегов: внутри контура несколько подписей, взята первая.",
        WarningCode.OrphanTag => "Лишний тег: подпись лежит вне границ любого помещения.",
        WarningCode.CalculationFailed => "Не считается: геометрия контура некорректна (мало вершин или вырождена).",
        _ => "Неизвестный код расхождения.",
    };
}
