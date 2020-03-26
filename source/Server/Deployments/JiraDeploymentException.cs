using System;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    public class JiraDeploymentException : Exception
    {
        public JiraDeploymentException(string message)
            : base(message)
        {
        }

        public JiraDeploymentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}