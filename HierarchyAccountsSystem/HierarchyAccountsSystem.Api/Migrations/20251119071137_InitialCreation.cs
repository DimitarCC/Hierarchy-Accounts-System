using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HierarchyAccountsSystem.Api.Migrations {
  /// <inheritdoc />
  public partial class InitialCreation : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
      var sqlTrigger1 = @"CREATE OR ALTER TRIGGER [dbo].[TR_Accounts_PreventCycles]
                ON [dbo].[Accounts]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- Prevent cycles: a node cannot be its own ancestor/descendant
                    IF EXISTS (
                        SELECT 1
                        FROM inserted AS i
                        JOIN dbo.Accounts AS a ON a.AccountID = i.ParentAccountID
                        WHERE i.AccountNodePath LIKE a.AccountNodePath + '%'
                          AND i.AccountID = a.AccountID -- same node as parent (self-parent)
                    )
                    BEGIN
                        RAISERROR('Cycle detected: a node cannot be its own ancestor.', 16, 1);
                        ROLLBACK TRANSACTION;
                        RETURN;
                    END

                    -- Prevent cycles: parent must not be descendant of child
                    IF EXISTS (
                        SELECT 1
                        FROM inserted AS i
                        JOIN dbo.Accounts AS a ON a.AccountID = i.ParentAccountID
                        WHERE a.AccountNodePath LIKE i.AccountNodePath + '%'
                    )
                    BEGIN
                        RAISERROR('Cycle detected: parent cannot be descendant of child.', 16, 1);
                        ROLLBACK TRANSACTION;
                        RETURN;
                    END
                END;";
      migrationBuilder.Sql(sqlTrigger1);
      var sqlTrigger2 = @"CREATE OR ALTER TRIGGER [dbo].[TR_Accounts_OneRoot]
                ON [dbo].[Accounts]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    IF (SELECT COUNT(*) FROM dbo.Accounts WHERE [ParentAccountID] IS NULL) > 1
                    BEGIN
                        RAISERROR('Only one root node is allowed.', 16, 1);
                        ROLLBACK TRANSACTION;
                    END
                END;";
      migrationBuilder.Sql(sqlTrigger2);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
      migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[trg_Accounts_CycleProtection];");
      migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_Accounts_OneRoot];");
    }
  }
}
