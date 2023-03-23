using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyNotes.Identity;

public sealed class ApplicationContext : IdentityDbContext<IdentityUser>
{
    public ApplicationContext(DbContextOptions options) : base(options)
    {
#if RELEASE
        Database.EnsureCreated();
#endif
    }
}