using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents;

namespace TryToCrash
{
    class Program
    {
        private static DocumentStore _store;

        static void Main(string[] args)
        {
            _store = new DocumentStore
            {
                Urls = new[] {"http://localhost:8080/"},
                Database = "trytocrash"
            };
            _store.Initialize();

            var tasks = new List<Task>();
            for (int i = 0; i < 100000; i++)
            {
                tasks.Add(Store(new SomeObject {Idke = i, FieldProperty = $"Field{i}"}));
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