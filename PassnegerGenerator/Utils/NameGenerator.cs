using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassnegerGenerator
{
    internal static class NameGenerator
    {
        static Random rand = new Random(Environment.TickCount);
        static string[] MaleNames = { "Arkhip", "Efim", "Timofey", "Oleg", "Sergey", "Alexander", "David", "Nikita", "Evgeniy"};
        static string[] FemaleNames = { "Anastasia", "Alina", "Svetlana", "Evdokia" };
        static string[] MaleSurnames = {"Vasilev", "Kazakov", "Bolonin", "Zhmelev", "Bodrov", "Bortsov", "Tolmachev", "Bubnov", "Gaifullin",
                                    "Ovechkin", "Kovalev", "Gerasimov", "Afanasyev"};
        static string[] PatronymicBases = { "Igorev", "Leonidov", "Dmitriev", "Sergeyev", "Anatolyev", "Olegov", "Alexandrov", "Evgenyev", "Grigoriev"};

        public static string GetRandomName() 
        {
            if (rand.NextDouble() < 0.5)
                return GetMaleName();
            else return GetFemaleName();
        }
        static string GetMaleName() 
        {
            string name = MaleNames[rand.Next() % MaleNames.Length];
            string surname = MaleSurnames[rand.Next() % MaleSurnames.Length];
            string patronymic = PatronymicBases[rand.Next() % PatronymicBases.Length] + "ich";
            return $"{surname} {name} {patronymic}";
        }
        static string GetFemaleName()
        {
            string name = FemaleNames[rand.Next() % FemaleNames.Length];
            string surname = MaleSurnames[rand.Next() % MaleSurnames.Length] + "a";
            string patronymic = PatronymicBases[rand.Next() % PatronymicBases.Length] + "na";
            return $"{surname} {name} {patronymic}";
        }
    }
}
