using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;

namespace TryToCrash
{
    class Program
    {
        private static DocumentStore _store;

        static void Main(string[] args)
        {
            Console.WriteLine("Async session? y or n (default n)");
            var async = Console.ReadLine()?.ToLower();

            
            Task.Run(() =>
            {
                decimal previousWorkingSet = 0M;

                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"                                             ");
                    Console.WriteLine($"                                             ");

                    var currentProcess = Process.GetCurrentProcess();
                    var workingSet = (decimal)currentProcess.WorkingSet64;

                    Console.SetCursorPosition(0, 0);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(CreateSizeFormatString(workingSet));

                    if (previousWorkingSet > workingSet)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("- ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("+ ");
                    }

                    Console.WriteLine(CreateSizeFormatString(workingSet - previousWorkingSet));

                    previousWorkingSet = workingSet;
                }
            });

            _store = new DocumentStore
            {
                Urls = new[] {"http://localhost:8080/"},
                Database = "trytocrash"
            };
            _store.Initialize();

            var tasks = new List<Task>();
            for (int y = 0; y < 10000; y++)
            {
                Console.WriteLine("Starting new batch");
                for (int i = 0; i < 8000; i++)
                {
                    if (async == "y")
                    {
                        tasks.Add(StoreAsyncSession(new SomeObject { Idke = i, FieldProperty = $"Field{i}" }));
                    }
                    else
                    {
                        tasks.Add(Store(new SomeObject { Idke = i, FieldProperty = $"Field{i}" }));
                    }
                }
                Task.WhenAll(tasks).Wait();
                
            }
           

            try
            {
                var t = Task.WhenAll(tasks);
           

                t.Wait();
                Console.WriteLine(t.Exception);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _store.Dispose();
            }
        }
        private static string CreateSizeFormatString(decimal workingSet)
        {
            var sizeFormat = "bytes";
            if (workingSet > 2048)
            {
                workingSet /= 1024;
                sizeFormat = "KB";
            }
            if (workingSet > 2048)
            {
                workingSet /= 1024;
                sizeFormat = "MB";
            }
            if (workingSet > 2048)
            {
                workingSet /= 1024;
                sizeFormat = "GB";
            }

            return $"{workingSet:# ###.00} {sizeFormat}";
        }
        private static async Task Store(ISomeObject o)
        {
            await Task.Yield();

            using (var session = _store.OpenSession())
            {
                await Task.Delay(1000);
                session.Store(o);
                session.SaveChanges();
            }
        }

        private static async Task StoreAsyncSession(ISomeObject o)
        {
            await Task.Yield();

            using (var session = _store.OpenAsyncSession())
            {
                await session.StoreAsync(o);
                await session.SaveChangesAsync();
            }
        }
    }

    internal interface ISomeObject
    {
        int Idke { get; set; }
    }

    class SomeObject : ISomeObject
    {
        public int Idke { get; set; }
        public string FieldProperty { get; set; }
    }
}