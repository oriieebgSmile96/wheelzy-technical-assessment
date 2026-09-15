# Wheelzy Technical Assessment

Layered .NET solution: Domain, Application, Persistence, and Infrastructure class libraries.

```text
Wheelzy.Domain          entities + BaseEntity
Wheelzy.Application     IApplicationDbContext, MediatR queries/commands
Wheelzy.Persistence     ApplicationDbContext, EF configurations, SQL
Wheelzy.Infrastructure  cache + Roslyn file processor
```

## How to run

```bash
dotnet test Wheelzy.Assessment.slnx
```

Written answers: [ANSWERS.md](ANSWERS.md) and `Wheelzy-Technical-Assessment-Answers.pdf`.
