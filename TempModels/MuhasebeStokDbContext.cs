using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MuhasebeStokWebApp.TempModels;

public partial class MuhasebeStokDbContext : DbContext
{
    public MuhasebeStokDbContext()
    {
    }

    public MuhasebeStokDbContext(DbContextOptions<MuhasebeStokDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
