using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSUIR_Lab_9_Task_1
{
    internal class Ship
    {
        public const int MaxCargoWeight = 10; //ton

        public string Name { get; set; } = "Unknown";
        private int cargoWeight = 0; //ton
        public int CargoWeight 
        {
            get {  return cargoWeight; } 
            set 
            { 
                if (value >= 0 && value <= MaxCargoWeight) cargoWeight = value;
                else throw new ArgumentOutOfRangeException("Вес груза превышает грузоподъемность или указан некорректно."); 
            } 

        }
        public int timeUnloadingMSek
        { 
            get 
            {
                if (CargoWeight == 0) return 0;
                return CargoWeight *1 * 1000; // Время необходимое на разгрузку в миллисекундах.
            }
        }

        public Ship() { }
        public Ship(string name, int cargoWeight = 0): this() { Name = name; CargoWeight = cargoWeight; }
        public override string ToString()
        {
            return $"Ship name: {Name}. Cargo weight: {CargoWeight} kg";
        }
        private void PrintStartUnloading() => Console.WriteLine($"Корабль {Name} приступил к разгрузке. Вес груза {CargoWeight} тонн");
        private void PrintEndUnloading(int mSec) => Console.WriteLine($"Корабль {Name} разгружен." +
            $"Время разгрузки: {mSec} сек.");
        private void PrintNoCargo() => Console.WriteLine($"На корабле {Name} нет груза.");

        public int Unloading()
        {
            if (CargoWeight == 0)
            {
                PrintNoCargo();
                return CargoWeight;
            }
            else
            {
                PrintStartUnloading();
                int TempCargoWeight = CargoWeight;
                int TempTimeUnloadingMSek = timeUnloadingMSek;

                Thread.Sleep(timeUnloadingMSek);
                CargoWeight = 0;

                PrintEndUnloading(TempCargoWeight);
                return TempCargoWeight;

            }
        }

    }
}
