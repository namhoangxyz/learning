using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MSA.BankService.Domain;


namespace MSA.BankService.Controllers;

[ApiController]
[Route("v1/bank")]
[Authorize]
public class BankController : ControllerBase
{
    public BankController()
    {
    }

    [HttpGet]
    [Authorize("read_access")]
    public IEnumerable<Bank> Get()
    {
        return new List<Bank>();
    }
}