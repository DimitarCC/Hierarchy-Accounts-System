using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HierarchyAccountsSystem.ConsoleApp;

internal class Program {
  static async Task<Int32> Main(String[] args) {
    // Load configuration from appconfig.json (optional).
    var config = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appconfig.json", optional: true, reloadOnChange: false)
      .Build();

    Console.WriteLine("HierarchyAccountsSystem - GetAccountTree client");

    var baseUrl = config["ApiBaseUrl"] ?? Environment.GetEnvironmentVariable("API_BASEURL");

    // Prefer account id from args. Supported forms:
    //   1) --accountId=123
    //   2) 123 (first positional argument)
    // If not provided on the command line, fall back to interactive prompt.
    String? accountIdInput = null;
    if (args != null && args.Length > 0) {
      foreach (var a in args) {
        if (a.StartsWith("--accountId=", StringComparison.OrdinalIgnoreCase)) {
          accountIdInput = a.Substring("--accountId=".Length);
          break;
        }
      }

      if (accountIdInput == null) {
        // try first positional argument if it doesn't look like an option
        var first = args[0];
        if (!first.StartsWith("-", StringComparison.Ordinal)) {
          accountIdInput = first;
        }
      }
    }

    if (String.IsNullOrWhiteSpace(accountIdInput)) {
      Console.Write("AccountId (empty for root): ");
      accountIdInput = Console.ReadLine();
    } else {
      Console.WriteLine($"Using AccountId from args: {accountIdInput}");
    }

    Int32? accountId = null;
    if (!String.IsNullOrWhiteSpace(accountIdInput) && Int32.TryParse(accountIdInput, out var parsedId)) {
      accountId = parsedId;
    }

    try {
      var client = new ApiClient(baseUrl);
      var root = await client.GetAccountTreeAsync(accountId);

      if (root == null) {
        Console.WriteLine("No account returned.");
        return 1;
      }

      PrintTree(root, "");
      Console.WriteLine("Press any key to close the app...");
      Console.ReadKey();
      return 0;
    } catch (Exception ex) {
      Console.WriteLine("Error: " + ex.Message);
      return 1;
    }
  }

  private static void PrintTree(HierarhycalAccountDto node, String indent) {
    Console.WriteLine($"{indent}- [{node.AccountId}] {node.Name} (Depth: {node.Depth})");
    if (node.Children != null) {
      foreach (var child in node.Children) {
        PrintTree(child, indent + "  ");
      }
    }
  }
}
