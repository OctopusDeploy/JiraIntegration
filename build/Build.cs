// ReSharper disable RedundantUsingDirective

using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.Tools.OctoVersion;
using Octopus.NukeBuildComponents;
using Octopus.NukeBuildComponents.Build;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild, IExtensionBuild
{
    public string TargetPackageDescription => "JiraIntegration";

    public string TestFilter => "FullyQualifiedName~.Tests";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public Enumeration Config => Configuration.Release;

    public string NuspecFilePath => "../../build/Octopus.Server.Extensibility.JiraIntegration.nuspec";

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => ((IExtensionBuild)x).Default);
}
