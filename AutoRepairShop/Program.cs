using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoRepairShop
{
    enum Errors
    {
        IncorrectCommand
    }

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

                Console.Clear();

                switch (userInput)
                {
                    case CommandGetNewClient:
                        ServeCar();
                        break;

                    case CommandExit:
                        isWorking = false;
                        break;

                    default:
                        StorageOfErrors.ShowError(Errors.IncorrectCommand);
                        break;
                }
            }
        }

        private void ServeCar()
        {
            const string CommandGoToStorage = "1";
            const string CommandCompleteRepair = "2";

            bool isRepairing = true;

            Car client = new Car(_partsBase);

            while (isRepairing)
            {
                ShowRepairUi(CommandGoToStorage, CommandCompleteRepair, client);

                string userInput = Console.ReadLine();

                Console.Clear();

                switch (userInput)
                {
                    case CommandGoToStorage:
                        GoToStorage();
                        break;

                    case CommandCompleteRepair:
                        isRepairing = false;
                        break;

                    default:
                        StorageOfErrors.ShowError(Errors.IncorrectCommand);
                        break;
                }
            }

            CompleteRepair(client);
        }

        private void ShowRepairUi(string CommandGoToStorage, string CommandCompleteRepair, Car client)
        {
            string message = "===== Новый клиент приехал в автосервис =====";
            int stringLenght = message.Length;

            ShowMoney();

            Console.WriteLine(new string('=', stringLenght));
            Console.WriteLine(message);
            Console.WriteLine(new string('=', stringLenght));
            Console.WriteLine("Следующие детали неисправны и требуют замены:\n");

            client.ShowBrokenParts();

            int totalPrice = GetTotalPrice(client);

            Console.WriteLine(new string('=', stringLenght));
            Console.WriteLine($"Итоговая стоимость: {totalPrice} руб.");
            Console.WriteLine(new string('=', stringLenght));

            _partsStorage.ShowPartsForReplace();

            Console.WriteLine($"{CommandGoToStorage}) - идти на склад за деталями");
            Console.WriteLine($"{CommandCompleteRepair}) - завершить ремонт");
        }

        private void ShowMoney()
        {
            Console.WriteLine($"Всего заработано: {_money} руб.");
        }

        private void GoToStorage()
        {
            _partsStorage.ShowMenu();
        }        

        private void CompleteRepair(Car client)
        {
            _partsForReplace = _partsStorage.GetPartsForReplace();
            List<Part> carBrokenParts = client.GetBrokenParts();
            List<Part> changedParts = GetChangedParts(carBrokenParts);
            List<Part> restBrokenParts = GetParts(carBrokenParts, changedParts);
            //List<Part> restBrokenParts = null;
            List<Part> wrongParts = new List<Part>(_partsForReplace);

            restBrokenParts.Sort();
            wrongParts.Sort();

            List<Part> penaltyParts = GetPenaltyParts(restBrokenParts, wrongParts);

            CalculateReward(changedParts);
            CalculatePenalty(penaltyParts);

            ShowResult(changedParts, penaltyParts);

            Console.ReadKey();
            Console.Clear();

            _partsForReplace.Clear();
        }

        private void CalculateReward(List<Part> changedParts)
        {
            _money += CalculatePrice(changedParts);
        }

        private void CalculatePenalty(List<Part> penaltyParts)
        {
            _money -= CalculatePrice(penaltyParts);
        }

        private void ShowResult(List<Part> changedParts, List<Part> penaltyParts)
        {
            if (changedParts.Count > 0)
            {
                Console.WriteLine("Заменены:");

                ShowPartsInfo(changedParts);
            }
            else
            {
                Console.WriteLine("Вы не помогли клиенту");
            }

            Console.WriteLine();

            if (penaltyParts.Count > 0)
            {
                Console.WriteLine("Штраф за: ");

                ShowPartsInfo(penaltyParts);
            }
        }

        private void ShowPartsInfo(List<Part> parts)
        {
            foreach (var part in parts)
                Console.WriteLine($"{part.Title} - {part.Price}руб.");
        }        

        private int GetTotalPrice(Car client)
        {
            int totalPrice = 0;

            foreach (var carParts in client.GetBrokenParts())
                totalPrice += carParts.Price;

            return totalPrice;
        }

        private int CalculatePrice(List<Part> parts)
        {
            int money = 0;

            foreach (var part in parts)
                money += part.Price;

            return money;
        }

        private List<Part> GetPenaltyParts(List<Part> restBrokenParts, List<Part> wrongParts)
        {
            List<Part> penaltyParts = new List<Part>();

            int amountOfPartForPenalty = Math.Min(restBrokenParts.Count, wrongParts.Count);

            for (int i = 0; i < amountOfPartForPenalty; i++)
                penaltyParts.Add(restBrokenParts[i]);

            return penaltyParts;
        }

        private List<Part> GetParts(List<Part> carBrokenParts, List<Part> changedParts) =>
            carBrokenParts.Except(changedParts).ToList();

        private List<Part> GetChangedParts(List<Part> carBrokenParts)
        {
            List<Part> changedParts = new List<Part>();

            for (int i = 0; i < carBrokenParts.Count; i++)
            {
                for (int j = 0; j < _partsForReplace.Count; j++)
                {
                    if (carBrokenParts[i].Title == _partsForReplace[j].Title)
                    {
                        changedParts.Add(carBrokenParts[i]);
                        _partsForReplace.RemoveAt(j);
                        break;
                    }
                }
            }

            return changedParts;
        }
    }

    class PartsStorage
    {
        private PartsBase _partsBase;
        private List<Box> _boxes;
        private List<Part> _partsForReplace;

        private string _exitCommand;
        private string _returnPartCommand;

        public PartsStorage()
        {
            _partsBase = new PartsBase();            
            _boxes = new List<Box>();
            _partsForReplace = new List<Part>();

            FillWithParts();

            _exitCommand = "ВЫХОД";
            _returnPartCommand = "ВЕРНУТЬ";
        }

        public void ShowMenu()
        {
            bool isSearching = true;

            while (isSearching)
            {
                Console.Clear();

                ShowParts();

                string userInput = Console.ReadLine().ToLower();

                if (userInput == _exitCommand.ToLower())
                    isSearching = false;
                else if (userInput == _returnPartCommand.ToLower())
                    ReturnPart();
                else
                    AddPartForReplace(userInput);

                Console.Clear();
            }
        }

        public void ShowPartsForReplace()
        {
            if (_partsForReplace.Count > 0)
            {
                Console.WriteLine("Вы взяли:\n");

                foreach (Part part in _partsForReplace)
                    Console.WriteLine("    " + part.Title);

                Console.WriteLine();
            }
        }

        private void FillWithParts()
        {
            List<Part> parts = _partsBase.CreateNewParts();

            for (int i = 0; i < parts.Count; i++)
            {
                int count = HolyRandom.GetNumber(1, 50);

                _boxes.Add(new Box(parts[i], count));
            }
        }

        public List<Part> GetPartsForReplace() =>
            new List<Part>(_partsForReplace);

        private void ShowParts()
        {
            Console.WriteLine("Что нужно взять для ремонта?\n");

            for (int i = 0; i < _boxes.Count; i++)
            {
                int partsCount = _boxes[i].Count;

                if (partsCount == 0)
                    Console.WriteLine($"{i + 1}) {_partsBase.GetTitle(i)} - закончились");
                else
                    Console.WriteLine($"{i + 1}) {_partsBase.GetTitle(i)} {partsCount} шт.");
            }

            Console.WriteLine();

            ShowPartsForReplace();

            if (_partsForReplace.Count > 0)
                Console.WriteLine($"Если хотите вернуть деталь на склад введите \"{_returnPartCommand}\"");

            Console.Write($"Введите номер детали или \"{_exitCommand}\" для завершения: ");
        }

        private void ReturnPart()
        {
            if (_partsForReplace.Count > 0)
            {
                Console.Write("Введите номер коробки с деталями в которую хотите вернуть деталь: ");

                string userInput = Console.ReadLine();

                if (int.TryParse(userInput, out int number) == false && number < 0 && number > _boxes.Count)
                    StorageOfErrors.ShowError(Errors.IncorrectCommand);

                int index = number - 1;

                for (int i = 0; i < _partsForReplace.Count; i++)
                {
                    if (_boxes[index].GetPart().Title == _partsForReplace[i].Title)
                    {
                        _boxes[index].AddPart();

                        _partsForReplace.RemoveAt(i);

                        break;
                    }
                }
            }
        }

        private void AddPartForReplace(string userInput)
        {
            if (int.TryParse(userInput, out int number) == false)
            {
                StorageOfErrors.ShowError(Errors.IncorrectCommand);
                return;
            }

            int index = number - 1;

            if (index < 0 || index >= _boxes.Count)
            {
                StorageOfErrors.ShowError(Errors.IncorrectCommand);
                return;
            }

            _partsForReplace.Add(_boxes[index].GetPart());

            _boxes[index].RemovePart();
        }
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

        public void RemovePart() =>
            _parts.RemoveAt(0);

        public Part GetPart() =>
            _parts[0];

        public void AddPart() =>
            _parts.Add(_parts[0]);
    }

    class Part : IComparable<Part>
    {
        public readonly string Title;
        public readonly int Price;

        public Part(string title, int price)
        {
            Title = title;
            Price = price;
            IsBroken = false;
        }

        public bool IsBroken { get; private set; }

        public void Break() =>
            IsBroken = true;

        private int CompareTo(Part other)
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
        private List<Part> _parts;

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

    class StorageOfErrors
    {
        public static void ShowError(Errors error)
        {
            Dictionary<Errors, string> errors = new Dictionary<Errors, string>
            {                
                { Errors.IncorrectCommand, "Некорректная команда" }
            };

            if (errors.ContainsKey(error))
                Console.WriteLine(errors[error]);

            Console.ReadKey();
        }
    }
}
