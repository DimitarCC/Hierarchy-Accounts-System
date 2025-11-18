using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierarchyAccountsSystem.BusinessLogic.DTO;

public class HierarchyAccountCreateRequest {
  public Int32? ParentId { get; set; }
  public String Name { get; set; } = String.Empty;
}
