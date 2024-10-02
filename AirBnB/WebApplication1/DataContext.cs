using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WebApplication1.entities;

namespace WebApplication1;


using Microsoft.EntityFrameworkCore;
// using WebApi.Entities;

public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sqlite database
        options.UseSqlite(Configuration.GetConnectionString("WebApiDatabase"));
    }

    public DbSet<RentalProperty> RentalProperties { get; set; }
    
    public DbSet<User> Users { get; set; }
}