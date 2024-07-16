using System.Threading.Tasks;

namespace VotingApp.Interfaces
{
    public interface IVoteQueueClient
    {
        Task SendVoteAsync(int id);
    }
}