using HierarchyAccountsSystem.BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace HierarchyAccountsSystem.BusinessLogic.DataContext;

public partial class HASDbContext : DbContext {
  public HASDbContext() {
  }

  public HASDbContext(DbContextOptions<HASDbContext> options)
      : base(options) {
  }

  public virtual DbSet<Account> Accounts { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.HasSequence<Int32>("AccountIdSequence").StartsAt(1).IncrementsBy(1);

    var converter = new ValueConverter<HierarchyPath, String>(
       v => v.Value,               // convert to string for storage
       v => new HierarchyPath(v)   // convert back to HierarchyPath
   );
    modelBuilder.Entity<Account>(entity => {
      entity.HasKey(e => e.AccountId);

      entity.ToTable(tb => {
        tb.HasTrigger("TR_Accounts_PreventCycles");
        tb.HasTrigger("TR_Accounts_OneRoot");
        // Depth constraint (max 5 levels. Master root is at 0)
        tb.HasCheckConstraint("CK_AccountNodes_MaxDepth", "(LEN(AccountNodePath) - LEN(REPLACE(AccountNodePath, '/', ''))) <= 5");
        tb.HasCheckConstraint("CK_AccountNodes_RootPath", "(ParentAccountID IS NOT NULL) OR (AccountNodePath = '/')");
      });

      entity.Property(e => e.AccountId).HasColumnName("AccountID").ValueGeneratedNever();
      entity.Property(e => e.Name).HasMaxLength(200);
      entity.Property(e => e.ParentAccountId).HasColumnName("ParentAccountID");

      entity.Property(a => a.AccountNodePath)
            .HasColumnName("AccountNodePath")
            .HasConversion(converter)
            .HasColumnType("nvarchar(400)")
            .IsRequired();

      entity.HasOne(d => d.ParentAccount).WithMany(p => p.Children)
            .HasForeignKey(d => d.ParentAccountId)
            .HasConstraintName("FK_AccountNodes_Parent");

    });
  }

}
