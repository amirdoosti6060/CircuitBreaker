using CircuitBreaker;

if (Environment.GetCommandLineArgs().Length != 3)
{
    Console.WriteLine(
        "Usage: CircuitBreaker {url} {testtype} \n" +
        "       {url}: the web service url like: http://localhost:5000/weatherforecast\n" +
        "       {testtype}: circuitbreaker / retry / timeout");
}

string url = Environment.GetCommandLineArgs()[1];
string testType = Environment.GetCommandLineArgs()[2].ToLower();

IResilienceTest test = testType switch
{
    "circuitbreaker" => new CircuitBreakerTest(url),
    "retry" => new RetryTest(url),
    "timeout" => new TimeoutTest(url),
    _ => new CircuitBreakerTest(url)
};

await test.Run();
