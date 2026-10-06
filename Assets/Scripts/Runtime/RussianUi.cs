using System;
namespace ZeroToMillionaire
{
    // Presentation-only text. Quest IDs, stored requirements and economy remain unchanged.
    public static class RussianUi
    {
        public const string RewardHint = "1 СИМВОЛ = 1 ФРАГМЕНТ • ПОКУПКА: ФР. + МОНЕТЫ";
        public const string Help = "8 улучшений — одна комната. Нужные символы дают фрагменты текущего задания. Когда они собраны, символы приносят монеты.\n\nДля покупки нужны и фрагменты, и монеты. Нажмите «КУПИТЬ», чтобы установить улучшение.\n\nСпин: 5 монет. Награда: 12 монет + бонусы. Первые 3 спина бесплатны. Если монет меньше 5, спин бесплатный.";
        static readonly string[] Titles = {
            "ЗАЛАТАТЬ ПОЛ", "ПОЧИНИТЬ СТЕНЫ", "ПОКЛЕИТЬ ОБОИ", "ПОЧИНИТЬ ДВЕРЬ",
            "ПОЧИНИТЬ ОКНО", "ПОЛОЖИТЬ МАТРАС", "ПОСТАВИТЬ ЛАМПУ", "ПОСТАВИТЬ ТАБУРЕТ"
        };
        public static string QuestTitle(int index) { return Titles[index]; }
        public static string Coins(int value) {
            int n = Math.Abs(value) % 100;
            if (n >= 11 && n <= 14) return "МОНЕТ";
            return n % 10 == 1 ? "МОНЕТА" : n % 10 >= 2 && n % 10 <= 4 ? "МОНЕТЫ" : "МОНЕТ";
        }
        public static string SaveNotice(string notice, bool failed) {
            if (notice == SaveStore.RecoveryNotice) return "Сохранение не прочитано. Исходные файлы защищены.";
            if (failed) return "Не удалось сохранить. Повторите действие.";
            if (notice == "Save upgraded: all progress kept.") return "Сохранение обновлено. Прогресс сохранён.";
            if (notice == "Recovered the previous local save.") return "Восстановлено предыдущее сохранение.";
            if (notice == "Local save could not be read. Starting a new room.") return "Сохранение не прочитано. Начата новая комната.";
            if (notice == "Reset saved; backup update pending.") return "Сброс сохранён. Резервная копия ожидает записи.";
            if (notice == "Saved locally · offline") return "Сохранено на устройстве • офлайн";
            return "Прогресс хранится на устройстве • офлайн";
        }
    }
}
