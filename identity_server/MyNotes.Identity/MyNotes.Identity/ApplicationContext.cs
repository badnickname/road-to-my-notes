using Microsoft.EntityFrameworkCore;

namespace MyNotes.Identity;

public sealed class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions options) : base(options)
    {
    }
}