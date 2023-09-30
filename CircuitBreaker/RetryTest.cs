using Polly;
using Polly.Retry;

namespace CircuitBreaker
{
    public class RetryTest: IResilienceTest
    {
        private readonly string _serviceUrl;
        private readonly ResiliencePipeline _pipeline;

        public RetryTest(string serviceUrl)
        {
            _serviceUrl = serviceUrl;

            // Define a ResiliencePipeline which is included Retry
            _pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 4,
                    Delay = TimeSpan.FromSeconds(1),
                    OnRetry = (args) =>
                    {
                        Console.WriteLine($"Retrying ..., Attempt {args.AttemptNumber + 1}");
                        return default;
                    }
                })
                .Build(); // Builds the resilience pipeline
        }

        public async Task Run()
        {
            Console.WriteLine("Retry Design Pattern");

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            //Test Retry
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
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during operation {i + 1}: {ex.Message}");
                }
            }
        }
    }
}
