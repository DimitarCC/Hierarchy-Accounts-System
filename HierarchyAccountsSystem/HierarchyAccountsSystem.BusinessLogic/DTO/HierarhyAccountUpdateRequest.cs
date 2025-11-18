using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierarchyAccountsSystem.BusinessLogic.DTO;

public class HierarhyAccountUpdateRequest : HierarchyAccountCreateRequest {
  public Int32 AccountId { get; set; }

}
