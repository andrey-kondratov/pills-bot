#addin nuget:?package=Cake.Docker&version=1.3.0

var target = Argument("target", "build-and-push");
var registry = Argument("registry", "andrey-kondratov");
var image = Argument("image", "pills-bot");
var tag = Argument("tag", "latest");
var username = Argument("username", "andrey-kondratov");
var server = Argument("server", "ghcr.io");
var push = Argument("push", false);
var progress = Argument("progress", "auto");
var platform = Argument("platform", "linux/amd64,linux/arm64").Split(',');

Task("build-and-push")
    .IsDependentOn("login")
    .Does(() => DockerBuildXBuild(new() { 
        Tag = [$"{server}/{registry}/{image}:{tag}"],
        Platform = platform,
        Progress = progress,
        Push = push
    }, "./src/PillsBot"));

Task("login")
    .Does(() => DockerLogin(new() { Username = username, PasswordStdin = true }, server));

RunTarget(target);
