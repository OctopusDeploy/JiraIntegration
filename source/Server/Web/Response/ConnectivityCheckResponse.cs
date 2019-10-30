using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Web.Response
{
    class ConnectivityCheckResponse
    {
        public bool WasSuccessful => ErrorMessages == null || ErrorMessages.Length == 0;
        public string[] ErrorMessages { get; set; }
        
        public static ConnectivityCheckResponse Success => new ConnectivityCheckResponse();

        public static ConnectivityCheckResponse Failure(params string[] errors) => new ConnectivityCheckResponse
            {ErrorMessages = errors.Where(e => e != null).ToArray()};
    }
}