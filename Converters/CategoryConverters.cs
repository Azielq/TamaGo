using System.Globalization;

namespace TamaGo.Converters;

public class CategoryBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = value?.ToString();
        var selectedCategory = parameter?.ToString();
        
        return category == selectedCategory ? Color.FromHex("#5865F2") : Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CategoryBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = value?.ToString();
        var selectedCategory = parameter?.ToString();
        
        return category == selectedCategory ? Color.FromHex("#5865F2") : Color.FromHex("#E0E0E0");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CategoryTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = value?.ToString();
        var selectedCategory = parameter?.ToString();
        
        return category == selectedCategory ? Colors.White : Color.FromHex("#333333");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}