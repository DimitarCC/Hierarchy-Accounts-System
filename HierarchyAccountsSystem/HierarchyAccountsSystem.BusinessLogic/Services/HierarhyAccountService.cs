using HierarchyAccountsSystem.BusinessLogic.Contracts;
using HierarchyAccountsSystem.BusinessLogic.Contracts.Mappers;
using HierarchyAccountsSystem.BusinessLogic.DataContext;
using HierarchyAccountsSystem.BusinessLogic.Models;
using HierarchyAccountsSystem.BusinessLogic.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace HierarchyAccountsSystem.BusinessLogic.Services;

public class HierarhyAccountService : IHierarhyAccountService {
  private readonly HASDbContext _Db;
  readonly IMapper<Account, HierarhycalAccount> _Mapper;
  public HierarhyAccountService(HASDbContext db, IMapper<Account, HierarhycalAccount> mapper) {
    this._Mapper = mapper;
    this._Db = db;
  }

  public async Task<HierarhycalAccount> GetAccountByIdAsync(Int32 accountId) {
    var account = await this._Db.Accounts.FindAsync(accountId);
    if (account == null) {
      throw new InvalidOperationException("Account not found");
    }
    return this._Mapper.ToViewModel(account);
  }

  public async Task<HierarhycalAccount> GetAccountTreeAsync(Int32? accountId) {
    var account = await this._Db.Accounts
      .AsNoTracking()
      .FirstOrDefaultAsync(f => f.AccountId == (accountId ?? 0));
    if (account == null) {
      throw new InvalidOperationException("Account not found");
    }
    return await this._Mapper.ToViewModelWithDBAsync(account, this._Db)
        ?? throw new InvalidOperationException("Account mapping failed");
  }

  public async Task<HierarhycalAccount> AddAccountAsync(String name, Int32? parentId = null) {
    if (parentId == null) { // Create root node
      var existingRoot = await this._Db.Accounts
          .Where(n => n.ParentAccountId == null)
          .FirstOrDefaultAsync();
      if (existingRoot != null) {
        throw new InvalidOperationException("Root account already exists");
      }

      var root = new Account {
        AccountId = 0,
        Name = name,
      };

      this._Db.Accounts.Add(root);
      await this._Db.SaveChangesAsync();
      return this._Mapper.ToViewModel(root);
    }

    using var transaction = await this._Db.Database.BeginTransactionAsync();

    try {
      // Load parent
      var parent = await this._Db.Accounts
          .FirstOrDefaultAsync(a => a.AccountId == parentId);

      if (parent == null) {
        throw new InvalidOperationException("Parent not found.");
      }

      SanityConstraints.ThrowIfDepthExceeded(parent);

      // Get next child ID
      var nextId = await this._Db.GetNextAccountIdAsync(transaction);

      // Generate new child path
      var newChildPath = HierarchyPath.BuildPathForChild(parent.AccountNodePath, nextId);

      SanityConstraints.ThrowIfCycleDetected(parent, newChildPath);

      var child = new Account {
        AccountId = nextId,
        Name = name,
        ParentAccountId = parent.AccountId,
        AccountNodePath = newChildPath
      };

      this._Db.Accounts.Add(child);
      await this._Db.SaveChangesAsync();

      await transaction.CommitAsync();

      return this._Mapper.ToViewModel(child);
    } catch {
      await transaction.RollbackAsync();
      throw;
    }
  }

  public async Task<HierarhycalAccount> UpdateAccountAsync(Int32 accountId, Int32? newParentId) {
    using var transaction = await this._Db.Database.BeginTransactionAsync();

    try {
      // Load node (tracked)
      var node = await this._Db.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
      if (node == null) {
        throw new InvalidOperationException("Account not found");
      }

      // Prevent setting self as parent
      if (newParentId == node.AccountId) {
        throw new InvalidOperationException("Cannot set account as its own parent.");
      }

      // Gather all accounts (safe fallback when EF cannot translate custom owned type path operations)
      var allAccounts = await this._Db.Accounts.AsNoTracking().ToListAsync();

      // Determine subtree (node + its descendants)
      var nodePathValue = node.AccountNodePath.Value;
      var subtree = allAccounts.Where(a => a.AccountNodePath.Value.StartsWith(nodePathValue)).ToList();

      // Load new parent or handle root case
      HierarchyPath? newNodePath;
      HierarchyPath parentPath;
      if (newParentId == null) {
        // Move to root: ensure no other root exists (except this node)
        var existingRoot = allAccounts.FirstOrDefault(a => a.ParentAccountId == null && a.AccountId != node.AccountId);
        if (existingRoot != null) {
          throw new InvalidOperationException("Root account already exists");
        }

        parentPath = HierarchyPath.Root;
        newNodePath = HierarchyPath.Root;
      } else {
        var parent = allAccounts.FirstOrDefault(a => a.AccountId == newParentId);
        if (parent == null) {
          throw new InvalidOperationException("Parent not found.");
        }

        // parentPath for depth calculations / path building
        parentPath = parent.AccountNodePath;

        // check basic parent depth limit (same as AddAccountAsync)
        SanityConstraints.ThrowIfDepthExceeded(parent);

        // Build new node path (child of parent)
        newNodePath = HierarchyPath.BuildPathForChild(parent.AccountNodePath, node.AccountId);

        // Cycle detection: parent cannot be a descendant of the node (would create a cycle)
        SanityConstraints.ThrowIfCycleDetected(parent, newNodePath);
      }

      // Depth validation for the moved subtree:
      // current node level and subtree max level
      var nodeLevel = node.AccountNodePath.GetLevel();
      var subtreeMaxLevel = subtree.Max(a => a.AccountNodePath.GetLevel());
      // how many levels below node the deepest descendant is
      var subtreeDepth = subtreeMaxLevel - nodeLevel;
      // new node level: if root => 0 otherwise parent level + 1
      var newNodeLevel = (parentPath == HierarchyPath.Root) ? 0 : parentPath.GetLevel() + 1;
      var newMaxLevel = newNodeLevel + subtreeDepth;
      if (newMaxLevel > 4) { // SanityConstraints expects max level index 4 (max depth of 5)
        throw new InvalidOperationException("Max depth of 5 exceeded.");
      }

      // Apply updates:
      // 1) update node (tracked)
      node.ParentAccountId = newParentId;
      node.AccountNodePath = newNodePath;

      // 2) update descendants' paths (excluding node itself)
      // Update each descendant by loading tracked entity and changing path
      foreach (var desc in subtree.Where(a => a.AccountId != node.AccountId)) {
        var suffix = desc.AccountNodePath.Value.Substring(nodePathValue.Length);
        var newDescPathValue = newNodePath.Value + suffix;
        var trackedDesc = await this._Db.Accounts.FindAsync(desc.AccountId);
        if (trackedDesc == null) {
          // this should not happen but guard it
          throw new InvalidOperationException($"Descendant account {desc.AccountId} not found for update.");
        }
        trackedDesc.AccountNodePath = new HierarchyPath(newDescPathValue);
      }

      await this._Db.SaveChangesAsync();
      await transaction.CommitAsync();
      return this._Mapper.ToViewModel(node);
    } catch {
      await transaction.RollbackAsync();
      throw;
    }
  }

  public async Task RemoveAccountAsync(Int32 accountId) {
    using var transaction = await this._Db.Database.BeginTransactionAsync();

    try {
      var node = await this._Db.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
      if (node == null) {
        throw new InvalidOperationException("Account not found");
      }

      // Snapshot of all accounts for path-based operations
      var allAccounts = await this._Db.Accounts.AsNoTracking().ToListAsync();

      var nodePathValue = node.AccountNodePath.Value;

      var subtree = allAccounts.Where(a => a.AccountNodePath.Value.StartsWith(nodePathValue)).ToList();

      var children = subtree.Where(a => a.ParentAccountId == node.AccountId).ToList();

      var parentId = node.ParentAccountId;

      if (parentId == null) {
        throw new InvalidOperationException("Master Root can not be removed!");
      }


      var parent = allAccounts.FirstOrDefault(a => a.AccountId == parentId);
      if (parent == null) {
        throw new InvalidOperationException("Parent not found");
      }

      // For each immediate child, move it under parent and adjust its subtree paths
      foreach (var child in children) {
        var newChildPath = HierarchyPath.BuildPathForChild(parent.AccountNodePath, child.AccountId);

        // Ensure moving child under parent won't create a cycle
        SanityConstraints.ThrowIfCycleDetected(parent, newChildPath);

        // gather child's subtree (descendants of child)
        var childPathValue = child.AccountNodePath.Value;
        var childSubtree = subtree.Where(a => a.AccountNodePath.Value.StartsWith(childPathValue)).ToList();

        var childLevel = child.AccountNodePath.GetLevel();
        var childSubtreeMax = childSubtree.Max(a => a.AccountNodePath.GetLevel());
        var childSubDepth = childSubtreeMax - childLevel;
        var newChildLevel = parent.AccountNodePath.GetLevel() + 1;
        var newMaxLevel = newChildLevel + childSubDepth;
        if (newMaxLevel > 4) {
          throw new InvalidOperationException("Max depth of 5 exceeded when reparenting child " + child.AccountId);
        }

        // Apply path updates for child subtree
        foreach (var desc in childSubtree) {
          var suffix = desc.AccountNodePath.Value.Substring(childPathValue.Length);
          var newDescPathValue = newChildPath.Value + suffix;
          var trackedDesc = await this._Db.Accounts.FindAsync(desc.AccountId);
          if (trackedDesc == null) {
            throw new InvalidOperationException($"Descendant account {desc.AccountId} not found for update.");
          }

          trackedDesc.AccountNodePath = new HierarchyPath(newDescPathValue);

          // update immediate child's ParentAccountId to the new parent
          if (desc.AccountId == child.AccountId) {
            trackedDesc.ParentAccountId = parentId;
          }
        }
      }

      this._Db.Accounts.Remove(node);

      await this._Db.SaveChangesAsync();
      await transaction.CommitAsync();
    } catch {
      await transaction.RollbackAsync();
      throw;
    }
  }
}
