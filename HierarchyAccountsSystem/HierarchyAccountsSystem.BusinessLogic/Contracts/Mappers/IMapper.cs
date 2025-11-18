
using HierarchyAccountsSystem.BusinessLogic.DataContext;

namespace HierarchyAccountsSystem.BusinessLogic.Contracts.Mappers {
  public interface IMapper<T1, T2> {
    T2 ToViewModel(T1 entity);

    Task<T2> ToViewModelWithDBAsync(T1 entity, HASDbContext db);
  }
}