using System;

namespace Octopus.Server.Extensibility.JiraIntegration
{
    class JiraDeploymentException : Exception
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
