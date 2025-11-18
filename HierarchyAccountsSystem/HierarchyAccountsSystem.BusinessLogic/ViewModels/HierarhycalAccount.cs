using System;
using System.Linq;

namespace HierarchyAccountsSystem.BusinessLogic.ViewModels;

public class HierarhycalAccount {
  public Int32 AccountId { get; set; }

  public String Name { get; set; }

  public HierarhycalAccount? ParentAccount { get; set; }

  public Int32 Depth { get; set; }

  public List<HierarhycalAccount> Children { get; set; } = [];

}
