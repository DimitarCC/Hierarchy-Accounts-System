using System;
using System.Linq;

namespace HierarchyAccountsSystem.BusinessLogic;

public sealed class HierarchyPath {
  public String Value { get; }

  public HierarchyPath(String value) {
    if (String.IsNullOrWhiteSpace(value)) {
      throw new ArgumentException("Path cannot be null or empty.", nameof(value));
    }

    if (!value.StartsWith("/") || !value.EndsWith("/")) {
      throw new ArgumentException("Path must start and end with '/'.", nameof(value));
    }

        this.Value = value;
  }

  public static HierarchyPath Root => new HierarchyPath("/");

  public Int32 GetLevel() {
    // Count segments between slashes
    return this.Value.Count(c => c == '/') - 1;
  }

  public HierarchyPath GetAncestor(Int32 n) {
    var segments = this.Value.Trim('/').Split('/');
    if (segments.Length <= n)
      return Root;
    var ancestorSegments = segments.Take(segments.Length - n);
    return new HierarchyPath("/" + String.Join("/", ancestorSegments) + "/");
  }

  public Boolean IsDescendantOf(HierarchyPath ancestor) {
    return this.Value.StartsWith(ancestor.Value) && this.Value != ancestor.Value;
  }

  public override String ToString() => this.Value;

  public override Boolean Equals(Object? obj) => obj is HierarchyPath other && this.Value.Equals(other.Value, StringComparison.Ordinal);

  public override Int32 GetHashCode() => this.Value.GetHashCode();

  public static HierarchyPath BuildPathForChild(HierarchyPath parentPath, Int32 childAccountId) {
    return new HierarchyPath($"{parentPath.Value}{childAccountId}/");
  }
}

