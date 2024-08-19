using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PillsBot.Server.Persistence;

public class PillsBotDbContextFactory : IDesignTimeDbContextFactory<PillsBotDbContext>
{
    public PillsBotDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PillsBotDbContext>();
        string connectionString = args.FirstOrDefault() ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__PILLSBOTDBCONTEXT") ?? "Host=db";

        optionsBuilder.UseNpgsql(connectionString);

        return new PillsBotDbContext(optionsBuilder.Options);
    }
}
