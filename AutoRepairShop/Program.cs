using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoRepairShop
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AutoRepairShop autoRepairShop = new AutoRepairShop();

            autoRepairShop.Work();
        }
    }

    class AutoRepairShop
    {
        private int _money;
        private PartsBase _partsBase;
        private PartsStorage _partsStorage;
        private List<Part> _partsForReplace;

        public AutoRepairShop()
        {
            _partsBase = new PartsBase();
            _partsStorage = new PartsStorage();
            _partsForReplace = new List<Part>();
        }

        public void Work()
        {
            const string CommandGetNewClient = "1";
            const string CommandExit = "2";

            bool isWorking = true;

            Console.ForegroundColor = ConsoleColor.Yellow;

            while (isWorking)
            {
                ShowMoney();
                Console.WriteLine();
                Console.WriteLine($"{CommandGetNewClient}) - взять нового клиента");
                Console.WriteLine($"{CommandExit}) - закрыть автосервис");

                string userInput = Console.ReadLine();

                switch (userInput)
                {
                    case CommandGetNewClient:
                        ServeCar();
                        break;

                    case CommandExit:
                        isWorking = false;
                        break;

                    default:
                        ShowErrorMessage();
                        break;
                }
            }
        }

        private void ServeCar()
        {
            const string CommandGoToStorage = "1";
            const string CommandCompleteRepair = "2";

            string message = "===== Новый клиент приехал в автосервис =====";
            int stringLenght = message.Length;

            bool isRepairing = true;

            Car newClient = new Car(_partsBase);

            while (isRepairing)
            {
                ShowMoney();

                Console.WriteLine(new string('=', stringLenght));
                Console.WriteLine(message);
                Console.WriteLine(new string('=', stringLenght));
                Console.WriteLine("Следующие детали неисправны и требуют замены:\n");

                newClient.ShowBrokenParts();

                int totalPrice = GetTotalPrice(newClient);

                Console.WriteLine(new string('=', stringLenght));
                Console.WriteLine($"Итоговая стоимость: {totalPrice} руб.");
                Console.WriteLine(new string('=', stringLenght));

                ShowPartsForReplace();

                Console.WriteLine($"{CommandGoToStorage}) - идти на склад за деталями");
                Console.WriteLine($"{CommandCompleteRepair}) - завершить ремонт");

                string userInput = Console.ReadLine();

                switch (userInput)
                {
                    case CommandGoToStorage:
                        GoToStorage();
                        break;

                    case CommandCompleteRepair:
                        CompleteRepair(newClient);
                        isRepairing = false;
                        break;

                    default:
                        ShowErrorMessage();
                        break;
                }
            }
        }

        private int GetTotalPrice(Car newClient)
        {
            int totalPrice = 0;

            foreach (var carParts in newClient.GetBrokenParts())
                totalPrice += carParts.Price;

            return totalPrice;
        }

        private void ShowMoney()
        {
            Console.Clear();
            Console.WriteLine($"Всего заработано: {_money} руб.");
        }

        private void GoToStorage()
        {
            List<Box> boxes = _partsStorage.GetBoxes(); ///

            string exitCommand = "ВЫХОД";
            string returnPartCommand = "ВЕРНУТЬ";

            bool isSearching = true;

            while (isSearching)
            {
                Console.Clear();
                Console.WriteLine("Что нужно взять для ремонта?\n");
                _partsStorage.Show();

                ShowPartsForReplace();

                if (_partsForReplace.Count > 0)
                    Console.WriteLine($"Если хотите вернуть деталь на склад введите \"{returnPartCommand}\"");

                Console.Write($"Введите номер детали или \"{exitCommand}\" для завершения: ");

                string userInput = Console.ReadLine();

                if (userInput.ToLower() == exitCommand.ToLower())
                {
                    isSearching = false;
                }
                else if (userInput.ToLower() == returnPartCommand.ToLower())
                {
                    if (_partsForReplace.Count > 0)
                        ReturnPart(boxes);
                }
                else if (int.TryParse(userInput, out int index) == false || index < 0 || index > boxes.Count)
                {
                    ShowErrorMessage();
                }
                else
                {
                    _partsForReplace.Add(boxes[index - 1].GetPart());

                    boxes[index - 1].RemovePart();
                }
            }
        }

        private void CompleteRepair(Car newClient)
        {
            List<Part> brokenParts = newClient.GetBrokenParts();
            List<Part> wrightParts = GetRightParts(brokenParts);
            List<Part> wrongParts = GetWrongParts(brokenParts);
            List<Part> missingParts = GetMissingParts(brokenParts);

            brokenParts.Sort();
            _partsForReplace.Sort();

            if (brokenParts.SequenceEqual(_partsForReplace))
                _money += GetTotalPrice(newClient);
            else if (brokenParts.Count <= _partsForReplace.Count && brokenParts.SequenceEqual(_partsForReplace) == false)
                EarnIncomeWithPenalty(newClient, wrongParts, 2);
            else if (brokenParts.Count > _partsForReplace.Count)
                EarnIncomeWithPenalty(newClient, missingParts);
            else
                ShowErrorMessage();

            Console.ReadKey();

            _partsForReplace.Clear();
        }

        private void ShowErrorMessage()
        {
            Console.WriteLine("Некорректная команда");
            Console.ReadKey();
        }

        private void ReturnPart(List<Box> boxes)
        {
            Console.Write("Введите номер коробки с деталями в которую хотите вернуть деталь: ");

            string userInput = Console.ReadLine();

            if (int.TryParse(userInput, out int index) == false && index < 0 && index > boxes.Count)
            {
                ShowErrorMessage();
            }

            for (int i = 0; i < _partsForReplace.Count; i++)
            {
                if (boxes[index - 1].GetPart().Title == _partsForReplace[i].Title)
                {
                    boxes[index - 1].AddPart();

                    _partsForReplace.Remove(_partsForReplace[i]);
                }
            }
        }

        private void ShowPartsForReplace()
        {
            if (_partsForReplace.Count > 0)
            {
                Console.WriteLine("Вы взяли:\n");

                foreach (Part part in _partsForReplace)
                {
                    Console.WriteLine("    " + part.Title);
                }

                Console.WriteLine();
            }
        }

        private List<Part> GetWrongParts(List<Part> brokenParts) =>
            brokenParts.Except(_partsForReplace).ToList();

        private List<Part> GetRightParts(List<Part> brokenParts)
        {
            List<Part> rightParts = new List<Part>();

            for (int i = 0; i < brokenParts.Count; i++)
            {
                for (int j = 0; j < _partsForReplace.Count; j++)
                {
                    if (brokenParts[i] == _partsForReplace[j])                    
                        rightParts.Add(brokenParts[i]);                    
                }
            }

            return rightParts;
        }

        private List<Part> GetMissingParts(List<Part> brokenParts) =>
            _partsForReplace.Except(brokenParts).ToList();

        private void EarnIncomeWithPenalty(Car newClient, List<Part> parts, int index)
        {
            int penalty = 0;

            foreach (Part part in parts)
                penalty += part.Price;

            _money += GetTotalPrice(newClient);
            _money -= penalty * index;
        }

        private void EarnIncomeWithPenalty(Car newClient, List<Part> parts)
        {
            int penalty = 0;

            foreach (Part part in parts)
                penalty += part.Price;

            _money += GetTotalPrice(newClient);
            _money -= penalty;
        }
    }

    class PartsStorage
    {
        private PartsBase _partsBase = new PartsBase();
        private List<Part> _parts;
        private List<Box> _boxes = new List<Box>();

        public PartsStorage()
        {
            _parts = _partsBase.CreateNewParts();

            FillWithParts();
        }

        private void FillWithParts()
        {
            for (int i = 0; i < _parts.Count; i++)
            {
                int count = HolyRandom.GetNumber(1, 50);

                _boxes.Add(new Box(_parts[i], count));
            }
        }

        public void Show()
        {
            for (int i = 0; i < _boxes.Count; i++)
            {
                int partsCount = _boxes[i].Count;

                if (partsCount == 0)
                    Console.WriteLine($"{i + 1}) {_partsBase.GetTitle(i)} - закончились");
                else
                    Console.WriteLine($"{i + 1}) {_partsBase.GetTitle(i)} {partsCount} шт.");
            }

            Console.WriteLine();
        }

        public List<Box> GetBoxes() =>
            new List<Box>(_boxes);
    }

    class Box
    {
        private List<Part> _parts = new List<Part>();

        public Box(Part part, int count)
        {
            for (int i = 0; i < count; i++)
                _parts.Add(part);
        }

        public int Count =>
            _parts.Count;

        public List<Part> GetParts() =>
            new List<Part>(_parts);

        public void RemovePart() =>
            _parts.RemoveAt(0);

        public Part GetPart() =>
            _parts[0];

        public void AddPart() =>
            _parts.Add(_parts[0]);
    }

    class Part : IComparable<Part>
    {
        public Part(string title, int price)
        {
            Title = title;
            Price = price;
            IsBroken = false;
        }

        public readonly string Title;
        public readonly int Price;

        public bool IsBroken { get; private set; }

        public void Break() =>
            IsBroken = true;

        public int CompareTo(Part other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;

            int titleComparison = string.Compare(Title, other.Title, StringComparison.Ordinal);
            if (titleComparison != 0)
                return titleComparison;

            int priceComparison = Price.CompareTo(other.Price);
            if (priceComparison != 0)
                return priceComparison;

            return IsBroken.CompareTo(other.IsBroken);
        }
    }

    class Car
    {
        private List<Part> _parts = new List<Part>();

        public Car(PartsBase partsBase)
        {
            _parts = partsBase.CreateNewParts();

            BreakRandomParts();
        }

        public void ShowBrokenParts()
        {
            foreach (Part part in GetBrokenParts())
                Console.WriteLine($"{part.Title} - {part.Price} руб.");
        }

        public List<Part> GetBrokenParts()
        {
            List<Part> brokenParts = new List<Part>();

            foreach (Part part in _parts)
            {
                if (part.IsBroken)
                    brokenParts.Add(part);
            }

            return brokenParts;
        }

        private void BreakRandomParts()
        {
            float coeficient = 0.5f;
            int indexOfDamage = (int)(_parts.Count * coeficient);

            int amount = HolyRandom.GetNumber(1, indexOfDamage);

            for (int i = 0; i < amount; i++)
            {
                int index = HolyRandom.GetNumber(_parts.Count);

                _parts[index].Break();
            }
        }
    }

    class PartsBase
    {
        private List<Part> _parts;

        public PartsBase()
        {
            _parts = new List<Part>()
            {
                new Part("Колесо", 2000),
                new Part("Мотор", 12000),
                new Part("Топливный фильтр", 1500),
                new Part("Воздушный фильтр", 800),
                new Part("Аккумулятор", 3000),
                new Part("Решетка радиатора", 2500),
                new Part("Капот", 4000),
                new Part("Правое переднее крыло", 3000),
                new Part("Левое переднее крыло", 3000),
                new Part("Правое заднее крыло", 3000),
                new Part("Левое заднее крыло", 3000),
                new Part("Передний бампер", 2000),
                new Part("Задний бампер", 2000),
                new Part("Левая фара", 4000),
                new Part("Правая фара", 4000),
                new Part("Левый стоп-сигнал", 4000),
                new Part("Правый стоп-сигнал", 4000)
            };
        }

        public List<Part> CreateNewParts()
        {
            List<Part> parts = new List<Part>(_parts);

            return parts;
        }

        public void ShowInfo()
        {
            foreach (var item in _parts)
                Console.WriteLine(item.Title);
        }

        public string GetTitle(int index) =>
            _parts[index].Title;
    }

    class HolyRandom
    {
        private static Random _random = new Random();

        public static int GetNumber(int minValue, int maxValue) =>
                    _random.Next(minValue, maxValue);

        public static int GetNumber(int maxValue) =>
                   _random.Next(maxValue);
    }
}
