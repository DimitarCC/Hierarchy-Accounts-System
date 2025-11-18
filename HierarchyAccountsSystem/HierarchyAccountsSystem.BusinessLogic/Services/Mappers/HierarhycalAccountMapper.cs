using HierarchyAccountsSystem.BusinessLogic.Contracts.Mappers;
using HierarchyAccountsSystem.BusinessLogic.DataContext;
using HierarchyAccountsSystem.BusinessLogic.Models;
using HierarchyAccountsSystem.BusinessLogic.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace HierarchyAccountsSystem.BusinessLogic.Services.Mappers;

public class HierarhycalAccountMapper : IMapper<Account, HierarhycalAccount> {
  public HierarhycalAccount ToViewModel(Account entity) {
    return new HierarhycalAccount {
      AccountId = entity.AccountId,
      Name = entity.Name,
      Depth = entity.AccountNodePath.GetLevel(),
      ParentAccount = entity.ParentAccount != null
          ? new HierarhycalAccount {
            AccountId = entity.ParentAccount.AccountId,
            Name = entity.ParentAccount.Name
          }
          : null,
    };
  }

  public async Task<HierarhycalAccount?> ToViewModelWithDBAsync(Account entity, HASDbContext db) {
    // Get the path of the node
    var accountNodePath = await db.Accounts
        .Where(n => n.AccountId == entity.AccountId)
        .Include(i => i.ParentAccount)
        .Select(n => n.AccountNodePath)
        .FirstOrDefaultAsync();

    if (accountNodePath == null) {
      return null;
    }

    // Load root
    var root = await db.Accounts.FirstOrDefaultAsync(a => a.AccountId == entity.AccountId);
    if (root == null) {
      throw new InvalidOperationException("Root not found.");
    }

    // Load all descendants (including root)
    var descendantAccounts = await db.Accounts
      .Include(a => a.ParentAccount)
      .ToListAsync();

    descendantAccounts = [.. descendantAccounts.Where(
        a => a.AccountNodePath.ToString().StartsWith(entity.AccountNodePath.ToString()))];

    var descendants = descendantAccounts
        .OrderBy(a => a.AccountNodePath.Value)
        .Select(s => new HierarhycalAccount() {
          AccountId = s.AccountId,
          Name = s.Name,
          Depth = s.AccountNodePath.GetLevel(),
          ParentAccount = s.ParentAccount == null ? null : this.ToViewModel(s.ParentAccount)
        })
        .ToList();

    // Build dictionary for fast lookup
    var lookup = descendants.ToDictionary(a => a.AccountId);

    // Initialize children lists
    foreach (var acc in descendants) {
      acc.Children = new List<HierarhycalAccount>();
    }

    // Build hierarchy
    foreach (var acc in descendants) {
      if (acc.ParentAccount?.AccountId != null && lookup.ContainsKey(acc.ParentAccount.AccountId)) {
        lookup[acc.ParentAccount.AccountId].Children.Add(acc);
      }
    }

    return lookup[entity.AccountId];
  }
}
