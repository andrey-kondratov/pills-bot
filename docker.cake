#addin nuget:?package=Cake.Docker&version=0.11.0

var target = Argument("target", "Default");
var tag = Argument("tag", "latest");
var image = Argument("image", "pills-bot");
var registry = Argument("registry", "andreikondratov");

// General
Task("Default")
    .IsDependentOn("Build");

// Build
var dockerComposeBuildSettings = new DockerComposeBuildSettings
{
    ProjectDirectory = "./src"
};

Task("Build")
    .IsDependentOn("BuildAmd64")
    .IsDependentOn("BuildArm64");

Task("BuildAmd64")
    .Does(() => 
    {
        var settings = new DockerComposeBuildSettings
        {
            Files = new [] {"./src/docker-compose.yml"}
        };

        DockerComposeBuild(settings);
    });

Task("BuildArm64")
    .Does(() => 
    {
        var settings = new DockerComposeBuildSettings
        {
            Files = new [] {"./src/docker-compose.arm64.yml"}
        };

        DockerComposeBuild(settings);
    });

// Tag
string amd64ImageReference = $"{image}:{tag}";
string amd64RegistryReference = $"{registry}/{image}:{tag}";
string arm64ImageReference = $"{image}:{tag}-arm64v8";
string arm64RegistryReference = $"{registry}/{image}:{tag}-arm64v8";

Task("TagAmd64")
    .IsDependentOn("BuildAmd64")
    .Does(() => DockerTag(amd64ImageReference, amd64RegistryReference));

Task("TagArm64")
    .IsDependentOn("BuildArm64")
    .Does(() => DockerTag(arm64ImageReference, arm64RegistryReference));

// Push
Task("Push")
    .IsDependentOn("PushAmd64")
    .IsDependentOn("PushArm64");

Task("PushAmd64")
    .IsDependentOn("TagAmd64")
    .Does(() => DockerPush(amd64RegistryReference));

Task("PushArm64")
    .IsDependentOn("TagArm64")
    .Does(() => DockerPush(arm64RegistryReference));

RunTarget(target);
