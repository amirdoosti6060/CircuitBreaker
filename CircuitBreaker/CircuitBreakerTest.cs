using Polly;
using Polly.CircuitBreaker;

namespace CircuitBreaker
{
    public class CircuitBreakerTest: IResilienceTest
    {
        private readonly string _serviceUrl;
        private  ResiliencePipeline _pipeline;

        public CircuitBreakerTest(string serviceUrl)
        {
            _serviceUrl = serviceUrl;

            // Define a ResiliencePipeline which is included Circuit Breaker
            _pipeline = new ResiliencePipelineBuilder()
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.1,
                    SamplingDuration = TimeSpan.FromSeconds(5),
                    MinimumThroughput = 2,
                    BreakDuration = TimeSpan.FromSeconds(10),
                    ShouldHandle = new PredicateBuilder()
                                        .Handle<Exception>()
                                        .Handle<HttpRequestException>()
                })
                .Build(); // Builds the resilience pipeline
        }

        public async Task Run()
        {
            Console.WriteLine("Circuit Breaker Design Pattern");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            //Test Circuit Breaker
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    // Execute the operation within the circuit breaker policy
                    await _pipeline.ExecuteAsync(async (token) =>
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var response = await httpClient.GetAsync(_serviceUrl);
                            response.EnsureSuccessStatusCode(); // Throws if the response status code is not success
                            Console.WriteLine($"Request {i + 1} succeeded.");
                        }
                    });
                }
                catch (BrokenCircuitException ex)
                {
                    Console.WriteLine($"Circuit is open. Operation {i + 1} not executed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during operation {i + 1}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
