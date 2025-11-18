using System;
using System.Linq;

namespace HierarchyAccountsSystem.BusinessLogic.ViewModels;

/// <summary>
/// Represents an account within a hierarchical structure, supporting parent-child relationships and depth tracking.
/// </summary>
/// <remarks>Use this class to model accounts that are organized in a tree-like hierarchy.
/// Each account can reference its parent and maintain a list of child accounts, enabling
/// traversal and management of hierarchical data. The Depth property indicates the level of the account within the
/// hierarchy, with root accounts typically having a depth of zero.</remarks>
public class HierarhycalAccount {
  /// <summary>
  /// Gets or sets the unique identifier for the account.
  /// </summary>
  public Int32 AccountId { get; set; }

  /// <summary>
  /// Gets or sets the name associated with the object.
  /// </summary>
  public String Name { get; set; }

  /// <summary>
  /// Gets or sets the parent account in the account hierarchy.
  /// </summary>
  public HierarhycalAccount? ParentAccount { get; set; }

  /// <summary>
  /// Gets or sets the depth level for the current element in the tree.
  /// </summary>
  public Int32 Depth { get; set; }

  /// <summary>
  /// Gets or sets the collection of child accounts in the hierarchy.
  /// </summary>
  /// <remarks>The list contains immediate child accounts associated with this account. Modifying the collection
  /// affects the hierarchical structure of accounts.</remarks>
  public List<HierarhycalAccount> Children { get; set; } = [];

}
