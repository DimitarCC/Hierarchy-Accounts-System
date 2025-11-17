using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HierarchyAccountsSystem.BusinessLogic.Models;

public partial class HASDbContext : DbContext {
  public HASDbContext() {
  }

  public HASDbContext(DbContextOptions<HASDbContext> options)
      : base(options) {
  }

  public virtual DbSet<Account> Accounts { get; set; }

  // Map hierarchyid functions
  [DbFunction("GetDescendant", IsBuiltIn = true)]
  public static string GetDescendant(string parentPath, string child1, string child2)
        => throw new NotSupportedException();

  [DbFunction("IsDescendantOf", IsBuiltIn = true)]
  public static bool IsDescendantOf(string childPath, string parentPath)
      => throw new NotSupportedException();

  [DbFunction("GetLevel", IsBuiltIn = true)]
  public static int GetLevel(string path)
      => throw new NotSupportedException();

  [DbFunction("GetAncestor", IsBuiltIn = true)]
  public static string GetAncestor(string path, int n)
      => throw new NotSupportedException();

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Account>(entity => {
      entity.HasKey(e => e.AccountId);

      entity.ToTable(tb => {
        tb.HasTrigger("trg_Accounts_CycleProtection");
        // Depth constraint (max 5 levels. root is at 0)
        tb.HasCheckConstraint("CK_AccountNodes_MaxDepth", "AccountNodePath.GetLevel() <= 4");
      });

      entity.Property(e => e.AccountId).HasColumnName("AccountID");
      entity.Property(e => e.Name).HasMaxLength(200);
      entity.Property(e => e.ParentAccountId)
            .HasColumnName("ParentAccountID");

      entity.Property(e => e.AccountNodePath)
            .HasColumnType("hierarchyid"); // map to SQL Server hierarchyid

      entity.HasOne(d => d.ParentAccount).WithMany(p => p.Children)
            .HasForeignKey(d => d.ParentAccountId)
            .HasConstraintName("FK_AccountNodes_Parent");

    });
  }

}
