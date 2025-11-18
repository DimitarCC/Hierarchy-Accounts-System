using System;
using HierarchyAccountsSystem.BusinessLogic.DataContext;
using HierarchyAccountsSystem.BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HierarchyAccountsSystem.BusinessLogic;

public static class Extensions {

  public static IQueryable<Account> GetDescendants(
        this IQueryable<Account> accounts,
        HierarchyPath parentPath) {
    var parentpathText = parentPath.Value;
    return accounts.Where(a => a.AccountNodePath.ToString().StartsWith(parentPath.ToString()));
  }

  public static async Task<Int32> GetNextAccountIdAsync(this HASDbContext db, IDbContextTransaction transaction) {
    await db.Database.OpenConnectionAsync();

    using var command = db.Database.GetDbConnection().CreateCommand();
    command.Transaction = transaction.GetDbTransaction();
    command.CommandText = "SELECT NEXT VALUE FOR AccountIdSequence";
    var result = await command.ExecuteScalarAsync();
    return Convert.ToInt32(result);
  }

}
