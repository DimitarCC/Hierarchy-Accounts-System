using HierarchyAccountsSystem.BusinessLogic.ViewModels;
using System;
using System.Linq;

namespace HierarchyAccountsSystem.BusinessLogic.Contracts;

public interface IHierarhyAccountService {
  Task<HierarhycalAccount> GetAccountByIdAsync(Int32 accountId);

  Task<HierarhycalAccount> GetAccountTreeAsync(Int32? accountId);

  Task<HierarhycalAccount> AddAccountAsync(String name, Int32? parentId = null);

  Task<HierarhycalAccount> UpdateAccountAsync(Int32 accountId, Int32? newParentId);

  Task RemoveAccountAsync(Int32 accountId);

}
