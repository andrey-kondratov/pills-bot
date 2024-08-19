using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PillsBot.Server.Persistence;

internal sealed class MessagesRepository(PillsBotDbContext dbContext) : IMessagesRepository
{
    private readonly PillsBotDbContext _dbContext = dbContext;

    public async Task<(string reminder, string acknowledgement, string appreciation)[]> GetMessages(string language,
        CancellationToken cancellationToken = default)
    {
        var messages = await _dbContext.Messages
            .Where(m => m.Language == language)
            .Select(m => new
            {
                m.Reminder,
                m.Acknowledgement,
                m.Appreciation
            })
            .ToListAsync(cancellationToken);

        return messages.Select(m => (m.Reminder, m.Acknowledgement, m.Appreciation)).ToArray();
    }

    public async Task AddMessages(string language, IEnumerable<(string reminder, string acknowledgement, string appreciation)> messages,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AddRangeAsync(messages.Select(m => new Message
        {
            Reminder = m.reminder,
            Acknowledgement = m.acknowledgement,
            Appreciation = m.appreciation,
            Language = language
        }), cancellationToken);

        await _dbContext.SaveChangesAsync();
    }
}
