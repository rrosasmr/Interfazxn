using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Interfazxn.Services;

namespace Interfazxn.Converters
{
    /// <summary>
    /// Convierte booleano a color para indicador de conexión
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool isConnected)
            {
                return isConnected ? new SolidColorBrush(Colors.LimeGreen) : new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convierte tipo de mensaje ASTM a color
    /// </summary>
    public class MessageTypeColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string messageType)
            {
                return messageType switch
                {
                    "RESULTS" => new SolidColorBrush(Color.Parse("#27AE60")), // Verde
                    "HOST_QUERY" => new SolidColorBrush(Color.Parse("#3498DB")), // Azul
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convierte nivel de log a color
    /// </summary>
    public class LogLevelColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is LogLevel level)
            {
                return level switch
                {
                    LogLevel.Info => new SolidColorBrush(Color.Parse("#3498DB")), // Azul
                    LogLevel.Warning => new SolidColorBrush(Color.Parse("#F39C12")), // Naranja
                    LogLevel.Error => new SolidColorBrush(Color.Parse("#E74C3C")), // Rojo
                    LogLevel.Success => new SolidColorBrush(Color.Parse("#27AE60")), // Verde
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}