# ADR_019namespace設計規約 抜粋

## 重要ポイント

### Bounded Context別サブnamespace使用

**基本テンプレート**:
```
<ProjectName>.<Layer>.<BoundedContext>[.<Feature>]

例:
- UbiquitousLanguageManager.Domain.Authentication
- UbiquitousLanguageManager.Application.ProjectManagement.UseCases
```

### 階層制限

- **3階層推奨**: `<ProjectName>.<Layer>.<BoundedContext>`
- **4階層許容**: `<ProjectName>.<Layer>.<BoundedContext>.<Feature>`
- **5階層以上禁止**: 過度な階層化を避ける

### F# Compilation Order制約対応

**依存順序**:
```
Common → Authentication → ProjectManagement → UbiquitousLanguageManagement
```

**ファイル配置順序**:
```
ValueObjects → Errors → Entities → DomainServices
```

**前方参照禁止**: F#制約により、定義順序が重要

---

**詳細**: `/Doc/07_Decisions/backup/ADR_019_namespace設計規約.md`
