using System;
using System.Collections.Generic;

namespace HierarchyAccountsSystem.BusinessLogic.Models;

public partial class Account {
  public int AccountId { get; set; }

  public string Name { get; set; } = null!;

  public int? ParentAccountId { get; set; }

  // Store hierarchyid as string in EF Core
  // (SQL Server column type will be hierarchyid)
  public string AccountNodePath { get; set; } = string.Empty;

  public virtual ICollection<Account> Children { get; set; } = new List<Account>();

  public virtual Account? ParentAccount { get; set; }
}
