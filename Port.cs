using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BSUIR_Lab_9_Task_1
{
    internal class Port<T> where T : Ship
    {
        public const int MaxShipCount = 10; //ship
        public const int MaxTotalCargoWeight = 100; // ton

        private Mutex weightMut = new Mutex();
        private Mutex shipsMut = new Mutex();
        private Mutex openMut = new Mutex();
        private Mutex nubmerShipComeMut = new Mutex();
        private Mutex numberShipGoneMut = new Mutex();
        private Mutex thMut = new Mutex();


        public string Name { get; set; } = "Unknown";
        private bool open = true; // индикатор открыт-ли порт
        public bool Open 
        { 
            get
            {
                bool r;
                openMut.WaitOne();
                r = open;
                openMut.ReleaseMutex();
                return r;
            }
            set
            {
                openMut.WaitOne();
                open = value;
                openMut.ReleaseMutex();
            }
        }
        private int totalCargoWeight = 0;
        public int TotalCargoWeight
        {
            get
            {
                int r = 0;
                weightMut.WaitOne();
                r= totalCargoWeight;
                weightMut.ReleaseMutex();
                return r;
            }
            set
            {
                weightMut.WaitOne();
                totalCargoWeight= value;
                weightMut.ReleaseMutex();
            }
        }


        private Queue<T> Ships = new Queue<T>(MaxShipCount);

        private int nubmerShipCome = 0; // Счетчик прибывших кораблей.
        private int numberShipGone = 0; // Счетчик убывших кораблей.
        public int NumberShipCome
        {
            get
            {
                int r;
                nubmerShipComeMut.WaitOne();
                r = nubmerShipCome;
                nubmerShipComeMut.ReleaseMutex();
                return r;
            }
            set
            {
                nubmerShipComeMut.WaitOne();
                nubmerShipCome = value;
                nubmerShipComeMut.ReleaseMutex();
            }

        }
        public int NumberShipGone
        {
            get
            {
                int r;
                numberShipGoneMut.WaitOne();
                r = numberShipGone;
                numberShipGoneMut.ReleaseMutex();
                return r;
            }
            set
            {
                numberShipGoneMut.WaitOne();
                numberShipGone = value;
                numberShipGoneMut.ReleaseMutex();
            }
        }

        private List<Thread> threads = new List<Thread>();

        public Port() { }
        public Port(string name) : this() { Name = name; }

        public override string ToString()
        {
            return $"Port name: {Name}. Total Cargo Weight: {TotalCargoWeight} ton.";
        }

        public int NumberShips()
        {
            int number = 0;
            shipsMut.WaitOne();
            number = Ships.Count;
            shipsMut.ReleaseMutex();
            return number;
        }

        private void PrintShipCome(T ship) => Console.WriteLine($"Корабль {ship.Name} прибыл на разгрузку. " +
            $"Вес груза {ship.CargoWeight} тонн.");
        private void PrintShipGone(T ship) => Console.WriteLine($"Корабль {ship.Name} убыл." +
            $"Осталось кораблей {Ships.Count} в очереди. Сумарный вес груза в порту {TotalCargoWeight} тонн.");
        private void PrintPortWorkEnd() => Console.WriteLine($"ПОРТ {Name} ОКОНЧИЛ РАБОТУ!!! ВСЕ ПРИБЫВШИЕ КОРАБЛИ РАЗГРУЖЕНЫ. " +
            $"СУММАРНЫЙ ВЕС ГРУЗА {TotalCargoWeight} тонн.");
        private void PrintFullPort(T ship) => Console.WriteLine($"Порт переполнен! Корабль {ship.Name} перенапралвен.");
        private void PrintAddShip(T ship) => Console.WriteLine($"корабль {ship.Name} прибыл в порт.");
        private void PrintNoTakeShip() => Console.WriteLine($"Порт {Name} больше  не принимает корабли!");
        public void PrintPortClosed() => Console.WriteLine($"Порт {Name} ЗАКРЫТ, Корабли больше не принимаются!");
        private void PrintAllNumbersShips() => Console.WriteLine($"Всего кораблей было принято: {NumberShipCome}.Кораблей убыло: {NumberShipGone}");


        public void AddShip(T ship)
        {
            // Добавляет корбали  в порт, если порт не закрыт и количество находящихся кораблей в порту не привышает лимит.
            if (!Open)
            {
                PrintNoTakeShip();
            }
            else
            {
                shipsMut.WaitOne();
                if(Ships.Count < MaxShipCount)
                {
                    Ships.Enqueue(ship);
                    PrintAddShip(ship);
                }
                else
                {
                    PrintFullPort(ship);
                }
                shipsMut.ReleaseMutex();
            }

        }
        private void UnloadingShip(object shipObj)
        {
            // Разгружает корабаль. Увеличивает соотвествующие счетчики.
            T ship = (T) shipObj;
            PrintShipCome(ship);

            if ((TotalCargoWeight + ship.CargoWeight) > MaxTotalCargoWeight && Open is true)
            {
                Open = false; 
                PrintPortClosed();
            }

            
            int cargoWeight =  ship.Unloading();
            TotalCargoWeight += cargoWeight;

            NumberShipCome++;
            NumberShipGone++;

            shipsMut.WaitOne();
            PrintShipGone(ship);    
            shipsMut.ReleaseMutex();
        }
        public void StartUnloading()
        {
            // Отправляет корабли из очереди на разгрузку. Каждый корабль разгружается в отдельном потоке.
            // Отслеживает работу всех созданных потоков, по их заврешению заканчивает работу всего порта.
            while (Open || NumberShips() > 0)
            {
                // Отправляет корабли на разгруку
                try
                {
                    T ship = Ships.Dequeue();
                    Thread th = new Thread(new ParameterizedThreadStart(this.UnloadingShip));

                    thMut.WaitOne();
                    threads.Add(th);
                    thMut.ReleaseMutex();

                    th.Start(ship);

                }
                catch (InvalidOperationException) { }               
            }
            bool portAwait = true;
            while (portAwait)
            {
                //Отслеживает разгрузку всех кораблей. Гарантирует ,что порт не прекратит работу пока все прибывшие корабли не будут разгружены,
                //после того как порт закрыт для приема новых кораблей
                if (!Open)
                {
                    thMut.WaitOne();

                    for (int i = 0; i < threads.Count; i++)
                    {
                        if (threads[i].ThreadState == ThreadState.Stopped) threads.RemoveAt(i);
                    }
                    if(threads.Count <1) portAwait =false;

                    thMut.ReleaseMutex();
                }
            }
            PrintPortWorkEnd();
            PrintAllNumbersShips();
        }
    }
}
