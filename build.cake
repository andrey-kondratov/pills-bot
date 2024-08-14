#addin nuget:?package=Cake.Docker&version=1.3.0

var target = Argument("target", "Build");
var registry = Argument("registry", "andrey-kondratov");
var image = Argument("image", "pills-bot");
var tag = Argument("tag", "latest");
var username = Argument("username", "andrey-kondratov");
var server = Argument("server", "ghcr.io");

string amd64RegistryReference = $"{server}/{registry}/{image}:{tag}";
string arm64RegistryReference = $"{server}/{registry}/{image}:{tag}-arm64v8";

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
    .IsDependentOn("DockerLogin")
    .Does(() => DockerPush(amd64RegistryReference));

Task("PushArm64")
    .IsDependentOn("BuildArm64")
    .IsDependentOn("DockerLogin")
    .Does(() => DockerPush(arm64RegistryReference));

Task("DockerLogin")
    .Does(() => DockerLogin(new() { Username = username, PasswordStdin = true }, server));

RunTarget(target);
