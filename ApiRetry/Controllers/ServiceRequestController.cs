using Microsoft.AspNetCore.Mvc;
using Polly;
using RestSharp;
using System.Runtime.ExceptionServices;

namespace ApiRetry.Controllers;

[ApiController]
[Route("[controller]")]
public class ServiceRequestController : ControllerBase
{
    private readonly ILogger<ServiceRequestController> _logger;

    public ServiceRequestController(ILogger<ServiceRequestController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetData")]
    public IActionResult Get()
    {
        /*var retryPolicy = Policy
            .Handle<Exception>()
            .RetryAsync(5, onRetry: (exception, retryCount) =>
            {
                Console.WriteLine("Error: " + exception.Message + "... Retry Count" + retryCount);
            });

        await retryPolicy.ExecuteAsync(async () =>
        {
            await ConnectToApi();
        });
        //await ConnectToApi();
        */

        var amountToPause = TimeSpan.FromSeconds(15);
        /*
        var retrywaitPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(5, i => amountToPause, onRetry: (exception, retryCount) =>
            {
                Console.WriteLine("Error: " + exception.Message + "...Retry Count" + retryCount);
            });

        await retrywaitPolicy.ExecuteAsync(async () =>
        {
            await ConnectToApi();
        });*/

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetry(5, i => amountToPause, (exception, retryCount) =>
            {
                Console.WriteLine("Error: " + exception.Message + "...Retry Count" + retryCount);
            });

        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreaker(3, TimeSpan.FromSeconds(30));

        var finalPolicy = retryPolicy.Wrap(circuitBreakerPolicy);

        finalPolicy.Execute(() =>
        {
            Console.WriteLine("Executing");
            ConnectToApiSync();
        });
        return Ok();
    }


    private void ConnectToApiSync()
    {
        var url = "https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/random";
        var client = new RestClient();
        var request = new RestRequest(url, Method.Get);

        request.AddHeader("accept", "application/json");
        request.AddHeader("X-RapidAPI-Key", "444ded4021msh0516c573ba2a066p19b2adjsnc20affcdbd9d");
        request.AddHeader("X-RapidAPI-Host", "matchilling-chuck-norris-jokes-v1.p.rapidapi.com");

        var response = client.Execute(request);

        //Console.WriteLine("Error: " + response.ErrorMessage);
        //Console.WriteLine("Error Status: " + response.IsSuccessful);

        if(response.IsSuccessful)
        {
            Console.WriteLine(response.Content);
        }
        else{
            Console.WriteLine(response.ErrorMessage);
            throw new Exception("Not able to connect to the service");
        }
    }
}
