namespace StatsBot
{
    public class Consts
    {
        private const string RulesUrl = "";
        internal const string Rules = "Правила: " + RulesUrl;
        internal const string Usage = @"Бот собирает все сообщения в чатике и выводит статистику.
Команды:
/rules -- правила
/stats или /stats сегодня -- статистика за сегодня
/stats вчера -- статистика за вчера
/stats неделя -- статистика за последние 7 дней
/stats месяц -- статистика за последние 30 дней
/stats 11.02.17 12.03.17";
    }
}
