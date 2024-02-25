using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSUIR_Lab_9_Task_1
{
    internal class LogisticsManagement
    {
        private Port<Ship> port;
        private Random random = new Random();

        public LogisticsManagement(string name)
        {
            port = new Port<Ship>(name);
            port.AddShip(createShip(7, 10, no_sleep: true));
            port.AddShip(createShip(7, 10, no_sleep: true));
        }
        public override string ToString()
        {
            return $"LogisticsManagement: Port name '{port.Name}'.";
        }


        private void PrintCreateShip(Ship ship) => Console.WriteLine($"Корабль {ship.Name} создан. Груз {ship.CargoWeight} тонн");

        private Ship createShip(int minWeight = 1, int maxWeight = 10, bool no_sleep = false)
        {
            // Создает корабаль с задержкой времени.
            int sleep_sec;
            if (no_sleep) { sleep_sec = 0; }
            else { sleep_sec = random.Next(2,5);}

            Thread.Sleep(sleep_sec * 1000);

            int weight = random.Next(minWeight, maxWeight);
            string name = $"{(char)random.Next((int)'A', (int)'Z')}_{weight}";
            Ship ship = new Ship(name, weight);
            PrintCreateShip(ship);
            return ship;
        }

        private void addShips()
        {
            // Создает и добвавляет корабли в порт пока порт открыт.
            while (port.Open)
            {
                Ship ship = createShip(7, 10);
                port.AddShip(ship);
            }
        }

        public void StartTask()
        {
            // Добавляет корабли в порт в отдельном потоке. Разгружает корабли в основном потоке.
            Thread th = new Thread(addShips);
            th.Start();

            port.StartUnloading();
            Console.WriteLine($"{this} ОКОНЧИЛ РАБОТУ!");

        }

    }
}
