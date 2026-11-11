using System;
using System.Globalization;
using System.IO;
using StudentSystemProgramming.Models;
using StudentSystemProgramming.Services;

var service = new RegistrySettingsService();

try
{
    // Режим сброса настроек
    if (args.Length > 0 && args[0] == "--reset")
    {
        service.Reset();
        Console.WriteLine("Учебные настройки сброшены.");
        return;
    }

    // 1. Загрузка настроек
    AppSettings settings = service.Load();

    Console.WriteLine("Настройки приложения");
    Console.WriteLine("--------------------");
    Console.WriteLine($"Тема: {settings.Theme}");
    Console.WriteLine($"Запусков: {settings.LaunchCount}");
    Console.WriteLine($"Версия схемы: {settings.SchemaVersion}");

    // 2. Вывод даты последнего запуска (ДОМАШНЕЕ ЗАДАНИЕ)
    if (settings.LastRunUtc is DateTime previousRun)
    {
        Console.WriteLine(
            $"Предыдущий запуск (UTC): " +
            $"{previousRun.ToString("O", CultureInfo.InvariantCulture)}");
    }
    else
    {
        Console.WriteLine("Предыдущий запуск: неизвестен");
    }

    // 3. Меню выбора темы
    Console.WriteLine();
    Console.WriteLine("Выберите тему:");
    Console.WriteLine("1 - Light");
    Console.WriteLine("2 - Dark");
    Console.WriteLine("Enter - оставить текущую");

    string? choice = Console.ReadLine();

    if (choice == "1")
        settings.Theme = "Light";
    else if (choice == "2")
        settings.Theme = "Dark";

    // 4. Увеличение счетчика и сохранение
    settings.LaunchCount = checked(settings.LaunchCount + 1);

    // Фиксируем текущий момент как «последний запуск»
    settings.LastRunUtc = DateTime.UtcNow;

    service.Save(settings);

    Console.WriteLine();
    Console.WriteLine("Настройки сохранены.");
    Console.WriteLine($"Тема: {settings.Theme}");
    Console.WriteLine($"Запуск № {settings.LaunchCount}");
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Нет доступа к реестру: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"Ошибка работы с реестром: {ex.Message}");
}
catch (OverflowException ex)
{
    Console.WriteLine($"Счётчик запусков переполнен: {ex.Message}");
}
catch (InvalidDataException ex)
{
    Console.WriteLine($"Некорректные настройки: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Некорректные данные: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Непредвиденная ошибка: {ex.Message}");
}