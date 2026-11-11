using System;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
using StudentSystemProgramming.Models;

namespace StudentSystemProgramming.Services;

public sealed class RegistrySettingsService
{
    private const string RegistryPath =
        @"Software\StudentSystemProgramming";

    private const int CurrentSchemaVersion = 1;

    public AppSettings Load()
    {
        var settings = new AppSettings();

        using RegistryKey? key =
            Registry.CurrentUser.OpenSubKey(RegistryPath);

        if (key is null)
        {
            return settings;
        }

        // === ПРОВЕРКА ВЕРСИИ СХЕМЫ ===
        object? schemaValue = key.GetValue("SchemaVersion");
        int storedSchemaVersion = 0;

        if (schemaValue is int version)
        {
            storedSchemaVersion = version;
        }
        else if (schemaValue is not null)
        {
            throw new InvalidDataException(
                "Некорректный тип SchemaVersion в реестре.");
        }

        if (storedSchemaVersion < 0)
        {
            throw new InvalidDataException(
                "Версия схемы не может быть отрицательной.");
        }

        if (storedSchemaVersion > CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                "Настройки созданы более новой версией приложения.");
        }
        // ==============================

        object? themeValue = key.GetValue("Theme");
        object? launchCountValue = key.GetValue("LaunchCount");
        object? lastRunValue = key.GetValue("LastRunUtc");

        if (themeValue is string theme &&
            (theme == "Light" || theme == "Dark"))
        {
            settings.Theme = theme;
        }

        if (launchCountValue is int launchCount &&
            launchCount >= 0)
        {
            settings.LaunchCount = launchCount;
        }

        // Чтение даты последнего запуска (ДОМАШНЕЕ ЗАДАНИЕ)
        if (lastRunValue is string lastRunText &&
            DateTime.TryParse(
                lastRunText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTime lastRun))
        {
            settings.LastRunUtc = lastRun.Kind == DateTimeKind.Utc
                ? lastRun
                : lastRun.ToUniversalTime();
        }

        // Миграция старой схемы (если версия была 0)
        if (storedSchemaVersion < CurrentSchemaVersion)
        {
            Save(settings);
        }

        return settings;
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.Theme != "Light" &&
            settings.Theme != "Dark")
        {
            throw new ArgumentException(
                "Недопустимая тема оформления.");
        }

        if (settings.LaunchCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings.LaunchCount));
        }

        using RegistryKey key =
            Registry.CurrentUser.CreateSubKey(
                RegistryPath,
                writable: true);

        key.SetValue(
            "Theme",
            settings.Theme,
            RegistryValueKind.String);

        key.SetValue(
            "LaunchCount",
            settings.LaunchCount,
            RegistryValueKind.DWord);

        key.SetValue(
            "SchemaVersion",
            CurrentSchemaVersion,
            RegistryValueKind.DWord);

        // Сохранение даты (ДОМАШНЕЕ ЗАДАНИЕ)
        if (settings.LastRunUtc is DateTime lastRun)
        {
            key.SetValue(
                "LastRunUtc",
                lastRun.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                RegistryValueKind.String);
        }
        else
        {
            key.DeleteValue("LastRunUtc", throwOnMissingValue: false);
        }
    }

    // Удаляется только HKCU\Software\StudentSystemProgramming
    public void Reset()
    {
        Registry.CurrentUser.DeleteSubKeyTree(
            RegistryPath,
            throwOnMissingSubKey: false);
    }
}