using System;

namespace VotingApp.Exceptions
{
    public class VoteQueueException : Exception
    {
        public VoteQueueException(string message)
            : base(message)
        {
        }

        public VoteQueueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
