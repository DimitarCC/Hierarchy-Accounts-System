using global::HierarchyAccountsSystem.BusinessLogic;
using global::HierarchyAccountsSystem.BusinessLogic.DataContext;
using global::HierarchyAccountsSystem.BusinessLogic.Models;
using global::HierarchyAccountsSystem.BusinessLogic.Services;
using global::HierarchyAccountsSystem.BusinessLogic.Services.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace HierarchyAccountsSystem.Tests;

public class HierarhyAccountServiceTests {
  private static HASDbContext CreateContext(String dbName) {
    var options = new DbContextOptionsBuilder<HASDbContext>()
      .UseInMemoryDatabase(dbName)
      .Options;
    return new HASDbContext(options);
  }

  private HierarhyAccountService CreateService(HASDbContext db) =>
      new HierarhyAccountService(db, new HierarhycalAccountMapper());

  private static Account MakeAccount(Int32 id, String name, String path, Int32? parentId) =>
      new Account {
        AccountId = id,
        Name = name,
        AccountNodePath = new HierarchyPath(path),
        ParentAccountId = parentId
      };

  [Fact]
  public async Task AddRoot_FirstTime_Succeeds() {
    await using var db = CreateContext(nameof(AddRoot_FirstTime_Succeeds));
    var svc = this.CreateService(db);

    var vm = await svc.AddAccountAsync("Global Account", null);

    Assert.Equal(0, vm.AccountId);
    Assert.Equal("Global Account", vm.Name);

    // persisted
    var saved = await db.Accounts.FindAsync(0);
    Assert.NotNull(saved);
    Assert.Equal("/", saved!.AccountNodePath.Value);
  }

  [Fact]
  public async Task AddRoot_WhenAlreadyExists_Throws() {
    await using var db = CreateContext(nameof(AddRoot_WhenAlreadyExists_Throws));
    // seed existing Global Account
    db.Accounts.Add(MakeAccount(0, "Global Account", "/", null));
    await db.SaveChangesAsync();

    var svc = this.CreateService(db);
    await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AddAccountAsync("Global Account", null));
  }

  [Fact]
  public async Task GetAccountById_NotFound_Throws() {
    await using var db = CreateContext(nameof(GetAccountById_NotFound_Throws));
    var svc = this.CreateService(db);
    await Assert.ThrowsAsync<InvalidOperationException>(() => svc.GetAccountByIdAsync(1000));
  }

  [Fact]
  public async Task Update_MoveUnderOwnDescendant_ThrowsCycle() {
    await using var db = CreateContext(nameof(Update_MoveUnderOwnDescendant_ThrowsCycle));

    // build: Global Account(0) -> acc a(1) -> acc b(2) -> acc c(3)
    var root = MakeAccount(0, "Global Account", "/", null);
    var a = MakeAccount(1, "A", "/1/", 0);
    var b = MakeAccount(2, "B", "/1/2/", 1);
    var c = MakeAccount(3, "C", "/1/2/3/", 2);

    db.Accounts.AddRange(root, a, b, c);
    await db.SaveChangesAsync();

    var svc = this.CreateService(db);

    // try to move 'a' under 'c' -> creates cycle
    await Assert.ThrowsAsync<InvalidOperationException>(() => svc.UpdateAccountAsync(1, 3));
  }

  [Fact]
  public async Task Update_MoveThatExceedsDepth_Throws() {
    await using var db = CreateContext(nameof(Update_MoveThatExceedsDepth_Throws));

    // Construct tree so that moving node 2 (with subtree depth 1) under a deep parent (level 4) exceeds max depth
    // master root(0) level 0
    // pDeep(10) at level 4 -> path like /10/11/12/13/  (but we must match level counts)
    var root = MakeAccount(0, "Master Root", "/", null);
    var p1 = MakeAccount(10, "L1", "/10/", 0);           // level 1
    var p2 = MakeAccount(11, "L2", "/10/11/", 10);      // level 2
    var p3 = MakeAccount(12, "L3", "/10/11/12/", 11);   // level 3
    var p4 = MakeAccount(13, "L4", "/10/11/12/13/", 12); // level 4

    var node = MakeAccount(2, "Node", "/2/", 0);        // level 1
    var child = MakeAccount(3, "Child", "/2/3/", 2);    // level 2 (subtreeDepth = 1)

    db.Accounts.AddRange(root, p1, p2, p3, p4, node, child);
    await db.SaveChangesAsync();

    var svc = this.CreateService(db);

    // moving node under p4 (level 4) would set new node level = 5 (exceeds 4)
    await Assert.ThrowsAsync<InvalidOperationException>(() => svc.UpdateAccountAsync(2, 13));
  }

  [Fact]
  public async Task Remove_ReparentsChildren_Succeeds() {
    await using var db = CreateContext(nameof(Remove_ReparentsChildren_Succeeds));

    // master root(0) -> parent(1) -> node(2) -> child(3)
    var root = MakeAccount(0, "Master Root", "/", null);
    var parent = MakeAccount(1, "Parent", "/1/", 0);
    var node = MakeAccount(2, "Node", "/1/2/", 1);
    var child = MakeAccount(3, "Child", "/1/2/3/", 2);

    db.Accounts.AddRange(root, parent, node, child);
    await db.SaveChangesAsync();

    var svc = this.CreateService(db);

    // Remove node (2) -> child 3 should become direct child of parent (1) and its path must change to /1/3/
    await svc.RemoveAccountAsync(2);

    var movedChild = await db.Accounts.FindAsync(3);
    Assert.NotNull(movedChild);
    Assert.Equal(1, movedChild!.ParentAccountId);
    Assert.Equal("/1/3/", movedChild.AccountNodePath.Value);

    // node (2) should be removed
    var removed = await db.Accounts.FindAsync(2);
    Assert.Null(removed);
  }

  [Fact]
  public async Task Remove_RootWithChildren_Throws() {
    await using var db = CreateContext(nameof(Remove_RootWithChildren_Throws));

    // mastr root(0) -> child(1)
    var root = MakeAccount(0, "Master Root", "/", null);
    var child = MakeAccount(1, "Child", "/1/", 0);
    db.Accounts.AddRange(root, child);
    await db.SaveChangesAsync();

    var svc = this.CreateService(db);
    await Assert.ThrowsAsync<InvalidOperationException>(() => svc.RemoveAccountAsync(0));
  }

  [Fact]
  public async Task Update_MoveToRoot_WhenAnotherRootExists_Throws() {
    await using var db = CreateContext(nameof(Update_MoveToRoot_WhenAnotherRootExists_Throws));

    // have existing root and another top-level node
    var root = MakeAccount(0, "Master Root", "/", null);
    var other = MakeAccount(5, "Other", "/5/", 0);
    var node = MakeAccount(2, "Node", "/2/", 5);

    db.Accounts.AddRange(root, other, node);
    await db.SaveChangesAsync();

    var svc = this.CreateService(db);

    // Try to move node to root (parentId == null) while existing root exists -> should throw
    await Assert.ThrowsAsync<InvalidOperationException>(() => svc.UpdateAccountAsync(2, null));
  }

  [Fact]
  public async Task GetAccountTree_ReturnsTree() {
    await using var db = CreateContext(nameof(GetAccountTree_ReturnsTree));

    // master root(0) -> a(1) -> b(2)
    var root = MakeAccount(0, "Master Root", "/", null);
    var a = MakeAccount(1, "A", "/1/", 0);
    var b = MakeAccount(2, "B", "/1/2/", 1);

    db.Accounts.AddRange(root, a, b);
    await db.SaveChangesAsync();

    var svc = this.CreateService(db);

    var tree = await svc.GetAccountTreeAsync(1);
    Assert.Equal(1, tree.AccountId);
    Assert.Single(tree.Children);
    Assert.Equal(2, tree.Children[0].AccountId);
  }
}