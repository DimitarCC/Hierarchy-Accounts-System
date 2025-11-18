using System;
using System.Collections.Generic;

namespace HierarchyAccountsSystem.BusinessLogic.Models;

/// <summary>
/// Represents an account within a hierarchical structure, including its identifier, name, parent account, and child
/// accounts.
/// </summary>
/// <remarks>The Account class models entities that can be organized in a parent-child hierarchy.
/// Each account may have a parent account and multiple child accounts,
/// allowing for tree-like relationships. The AccountNodePath property provides the hierarchical path from the root to
/// this account, which can be used for navigation or querying within the hierarchy.</remarks>
public partial class Account {
  /// <summary>
  /// This is account's unique identifier.
  /// </summary>
  public Int32 AccountId { get; set; }

  /// <summary>
  /// Gets or sets the name associated with the object.
  /// </summary>
  public String Name { get; set; } = null!;

  /// <summary>
  ///  Gets or sets the identifier of the parent account in the hierarchy.
  /// </summary>
  public Int32? ParentAccountId { get; set; }

  /// <summary>
  /// Gets or sets the hierarchical path to the account node within the structure.
  /// </summary>
  public HierarchyPath AccountNodePath { get; set; } = HierarchyPath.Root;

  public virtual ICollection<Account> Children { get; set; } = new List<Account>();

  public virtual Account? ParentAccount { get; set; }
}
