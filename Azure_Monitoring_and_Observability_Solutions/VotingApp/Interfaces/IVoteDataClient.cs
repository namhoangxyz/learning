using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VotingApp.Models;

namespace VotingApp.Interfaces
{
    public interface IVoteDataClient
    {
        Task<IList<Counts>> GetCountsAsync();

        Task<HttpResponseMessage> AddVoteAsync(string candidate);

        Task DeleteCandidateAsync(string candidate);
    }
}