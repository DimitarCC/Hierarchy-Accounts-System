using System.Collections.Generic;

namespace HierarchyAccountsSystem.ConsoleApp;

internal sealed class HierarhycalAccountDto {
  public int AccountId { get; set; }
  public string? Name { get; set; }
  public int Depth { get; set; }
  public HierarhycalAccountDto? ParentAccount { get; set; }
  public List<HierarhycalAccountDto> Children { get; set; } = new();
}