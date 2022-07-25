using Confluent.Kafka;
using Facade.ProtosServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facade.Services
{
    public class SubmitEventNewProduct
    {
        ProducerConfig _producerConfig;

        public SubmitEventNewProduct()
        {
            _producerConfig = BuilldConfig();
        }

        public void Submit(NewProductRequest newProductRequest,Action callback)
        {
            var producer = new ProducerBuilder<Null, NewProductRequest>(_producerConfig);
            producer.SetValueSerializer(new ProducerSerializer<NewProductRequest>());
            var producerBuild = producer.Build();

            Task task = new Task(async() => 
            {
                await producerBuild.ProduceAsync("NewProductRequest", new Message<Null, NewProductRequest>
                {
                    Value = newProductRequest
                });
            });

            task.ContinueWith((Task,objects) =>
                {
                    callback?.Invoke();
                    //To do log
                },
                TaskContinuationOptions.NotOnFaulted);

            task.ContinueWith((Task, objects) =>
                {
                    //To do log
                },
                TaskContinuationOptions.OnlyOnFaulted);

            task.Start();
        }

        private ProducerConfig BuilldConfig()
        {
            ProducerConfig result = new ProducerConfig();
            try
            {
                var config = new ConfigurationBuilder();
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json");
                IConfigurationRoot root = config.Build();

                result = new ProducerConfig()
                {
                    BootstrapServers = root["ConnectionBootstrapServersKafka"]
                };
            }
            catch
            {
                //To DO log
            }
            return result;
        }

    }
}
