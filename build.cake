#addin nuget:?package=Cake.Docker&version=1.3.0

var target = Argument("target", "build-and-push");
var registry = Argument("registry", "andrey-kondratov");
var image = Argument("image", "pills-bot");
var tag = Argument("tag", "latest");
var username = Argument("username", "andrey-kondratov");
var server = Argument("server", "ghcr.io");

Task("build-and-push")
    .IsDependentOn("login")
    .Does(() => DockerBuildXBuild(new() { 
        Tag = [$"{server}/{registry}/{image}:{tag}"],
        Platform = ["linux/amd64", "linux/arm64"],
        Progress = "plain",
        Push = true
    }, "./src/PillsBot"));

Task("login")
    .Does(() => DockerLogin(new() { Username = username, PasswordStdin = true }, server));

RunTarget(target);
