---
name: Bug report
about: Create a report to help us improve
title: ''
labels: ''
assignees: ''

---

**Describe the bug**
A clear and concise description of what the bug is.

**To reproduce**
Reproduction of the issue, ideally in a unit test format.

```c#
[Fact]
public void ReproduceIssue()
{
  var builder = new ContainerBuilder();
  var container = builder.Build();
  // This assertion should pass but it doesn't
  Assert.NotNull(container.Resolve<IService>());
}
```

**Full exception with stack trace:**

```
[put exception here between fences]
```

**Assembly/dependency versions:**
```xml
<!-- Paste packages.config or PackageReferences from .csproj file here -->
```

**Additional context**
Add any other context about the problem here.
