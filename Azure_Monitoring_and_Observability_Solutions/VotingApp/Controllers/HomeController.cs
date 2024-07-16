using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VotingApp.Models;
using VotingApp.Interfaces;

namespace VotingApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;

    private readonly IVoteDataClient client;
   public HomeController(ILogger<HomeController> logger,IVoteDataClient client)
    {
        this.logger = logger;                
        this.client = client;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Votes()
    {
        var result = await this.client.GetCountsAsync();
        return View(result);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
