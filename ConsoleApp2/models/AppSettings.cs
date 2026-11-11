namespace StudentSystemProgramming.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Light";

    public int LaunchCount { get; set; }

    public int SchemaVersion { get; set; } = 1;

    // ДОБАВЛЕНО ПО ДОМАШНЕМУ ЗАДАНИЮ:
    // null означает, что предыдущий запуск неизвестен
    // (первый запуск, значение удалено или повреждено).
    public DateTime? LastRunUtc { get; set; }
}