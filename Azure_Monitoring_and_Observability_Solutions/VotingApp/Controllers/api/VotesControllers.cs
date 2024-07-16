using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using VotingApp.Exceptions;
using VotingApp.Interfaces;
using VotingApp.Clients;

namespace VotingApp.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class VotesController : Controller
{
    private readonly ILogger<VotesController> logger;

    private readonly IVoteDataClient client;
    private readonly IVoteQueueClient queueClient;

    public VotesController(IVoteDataClient client,
                           IVoteQueueClient queueClient,
                           ILogger<VotesController> logger)
    {
        this.client = client;
        this.queueClient = queueClient;
        this.logger = logger;
    }

    [HttpPut("{name}")]
    [Route("[action]/{name}")]
    public async Task<IActionResult> Add(string name)
    {
        try
        {
            var response = await this.client.AddVoteAsync(name);
            if (response.IsSuccessStatusCode)
            {
                logger.LogDebug("New candidate {name} has been created, Activity.Current.Id:{id}", name, Activity.Current?.Id);
                return this.Ok();
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return BadRequest(errorMessage);
        }
        catch (Exception ex) when (ex is VoteDataException)
        {
            logger.LogError(ex, "Exception creating vote in database");
            return BadRequest("Bad Request");
        }
    }

    [HttpPut("{id}")]
    [Route("[action]/{id}")]
    public async Task<IActionResult> Vote(int id)
    {
        try
        {
            await queueClient.SendVoteAsync(id); //Send Enqueue message.
            logger.LogInformation("Enqueue message has been sent, Activity.Currrent.Id:{Id}", Activity.Current?.Id);
            return this.Ok();
        }
        catch (Exception ex) when (ex is VoteQueueException)
        {
            logger.LogError(ex, "Exception sending vote to the queue");
            return BadRequest("Bad Request");
        }
    }
}