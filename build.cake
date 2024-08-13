#addin nuget:?package=Cake.Docker&version=1.3.0

var target = Argument("target", "Default");
var tag = Argument("tag", "latest");
var image = Argument("image", "pills-bot");
var registry = Argument("registry", "andreikondratov");

string amd64RegistryReference = $"{registry}/{image}:{tag}";
string arm64RegistryReference = $"{registry}/{image}:{tag}-arm64v8";

// General
Task("Default")
    .IsDependentOn("Build");

// Build
Task("Build")
    .IsDependentOn("BuildAmd64")
    .IsDependentOn("BuildArm64");

Task("BuildAmd64")
    .Does(() => DockerBuild(new DockerImageBuildSettings
    {
        Tag = [amd64RegistryReference],
        File = "./src/docker/amd64/Dockerfile"
    }, "./src"));

Task("BuildArm64")
    .Does(() => DockerBuild(new DockerImageBuildSettings
    {
        Tag = [arm64RegistryReference],
        File = "./src/docker/arm64/Dockerfile"
    }, "./src"));

// Push
Task("Push")
    .IsDependentOn("PushAmd64")
    .IsDependentOn("PushArm64");

Task("PushAmd64")
    .IsDependentOn("BuildAmd64")
    .Does(() => DockerPush(amd64RegistryReference));

Task("PushArm64")
    .IsDependentOn("BuildArm64")
    .Does(() => DockerPush(arm64RegistryReference));

RunTarget(target);
