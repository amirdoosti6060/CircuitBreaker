using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CircuitBreaker
{
    public class TimeoutTest: IResilienceTest
    {
        private readonly string _serviceUrl;
        private readonly ResiliencePipeline _pipeline;

        public TimeoutTest(string serviceUrl)
        {
            _serviceUrl = serviceUrl;

            // Define a ResiliencePipeline which is included Timeout
            _pipeline = new ResiliencePipelineBuilder()
                .AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(1),
                    OnTimeout = (args) =>
                    {
                        Console.WriteLine($"Timeout after {args.Timeout} seconds ...");
                        return default;
                    }
                })
                
                .Build(); // Builds the resilience pipeline
        }

        public async Task Run()
        {
            Console.WriteLine("Timeout Design Pattern");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            //Test Timeout
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    // Execute the operation within the circuit breaker policy
                    await _pipeline.ExecuteAsync(async (token) =>
                    {
                        try
                        {
                            Console.WriteLine($"Try execute request {i + 1}");
                            using (var httpClient = new HttpClient())
                            {
                                var response = await httpClient.GetAsync(_serviceUrl);
                                response.EnsureSuccessStatusCode(); // Throws if the response status code is not success
                                Console.WriteLine($"Request {i + 1} succeeded.");
                            }
                        }
                        catch
                        {
                            await Task.Delay(TimeSpan.FromSeconds(4));
                        }
                    });
                }
                catch (TimeoutRejectedException ex)
                {
                    Console.WriteLine($"Timeout reject exception during operation {i + 1}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during operation {i + 1}: {ex.Message}");
                }
            }
        }
    }
}
