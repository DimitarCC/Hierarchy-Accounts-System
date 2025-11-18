using System;
using System.Collections.Generic;

namespace HierarchyAccountsSystem.BusinessLogic.Models;

public partial class Account {
  public Int32 AccountId { get; set; }

  public String Name { get; set; } = null!;

  public Int32? ParentAccountId { get; set; }

  public HierarchyPath AccountNodePath { get; set; } = HierarchyPath.Root;

  public virtual ICollection<Account> Children { get; set; } = new List<Account>();

  public virtual Account? ParentAccount { get; set; }
}
