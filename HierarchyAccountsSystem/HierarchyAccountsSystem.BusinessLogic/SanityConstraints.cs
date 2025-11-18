using HierarchyAccountsSystem.BusinessLogic.Models;
using System;
using System.Linq;

namespace HierarchyAccountsSystem.BusinessLogic;

public static class SanityConstraints {
  public static void ThrowIfDepthExceeded(Account parent) {
    if (parent.AccountNodePath.GetLevel() >= 4) {
      throw new InvalidOperationException("Max depth of 5 exceeded.");
    }
  }

  public static void ThrowIfCycleDetected(Account parent, HierarchyPath newChildPath) {
    if (parent.AccountNodePath.IsDescendantOf(newChildPath)) {
      throw new InvalidOperationException("Cycle detected.");
    }
  }
}
