# Architecture — GCP Cost Analyzer

## Vue d'ensemble

GCP Cost Analyzer suit les principes de la **Clean Architecture** (Robert C. Martin).  
Les dépendances pointent toujours vers l'intérieur — le Domain ne dépend de rien.

```
┌─────────────────────────────────────────────────────┐
│                    MonProjetWeb.Web                  │
│              (Razor Pages + Controllers)             │
├─────────────────────────────────────────────────────┤
│   MonProjetWeb.Infrastructure  │  MonProjetWeb.      │
│   (GCP, JWT, Auth)             │  Persistence        │
│                                │  (EF Core, Repos)   │
├─────────────────────────────────────────────────────┤
│              MonProjetWeb.Application                │
│         (Services, Interfaces, DTOs)                 │
├─────────────────────────────────────────────────────┤
│                MonProjetWeb.Domain                   │
│            (Entités, Enums, BaseEntity)              │
└─────────────────────────────────────────────────────┘
```

---

## Couches

### Domain
Couche centrale — zéro dépendance externe.

```
MonProjetWeb.Domain/
├── Common/
│   └── BaseEntity.cs          # Id, CreatedAt, UpdatedAt
├── Enums/
│   └── UserRole.cs            # Admin, Analyst, Viewer
└── Entities/
    ├── User.cs
    ├── GcpAccount.cs
    ├── CostRecord.cs
    ├── Budget.cs
    ├── Alert.cs
    └── Recommendation.cs
```

### Application
Contient la logique métier, les interfaces et les DTOs.

```
MonProjetWeb.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IGenericRepository.cs
│   │   ├── IUserRepository.cs
│   │   ├── ICostRepository.cs
│   │   ├── IBudgetRepository.cs
│   │   ├── IAlertRepository.cs
│   │   ├── IRecommendationRepository.cs
│   │   ├── IGcpAccountRepository.cs
│   │   ├── IAuthService.cs
│   │   ├── IJwtService.cs
│   │   ├── IGoogleCloudService.cs
│   │   ├── IBillingService.cs
│   │   └── Services/
│   │       ├── ICostAnalysisService.cs
│   │       ├── IBudgetService.cs
│   │       ├── IAlertService.cs
│   │       ├── IRecommendationService.cs
│   │       └── IAnomalyDetectionService.cs
│   └── DTOs/
│       ├── Auth/
│       │   ├── LoginDto.cs
│       │   ├── RegisterDto.cs
│       │   ├── AuthResponseDto.cs
│       │   └── UserDto.cs
│       └── GCP/
│           ├── GcpCostDto.cs
│           ├── GcpBudgetDto.cs
│           ├── GcpProjectDto.cs
│           └── GcpSummaryDto.cs
└── Services/
    ├── CostAnalysisService.cs
    ├── BudgetService.cs
    ├── AlertService.cs
    ├── RecommendationService.cs
    └── AnomalyDetectionService.cs
```

### Infrastructure
Implémentations des services externes.

```
MonProjetWeb.Infrastructure/
├── GoogleCloud/
│   ├── GoogleCloudService.cs   # GCP Billing + Budgets API
│   └── BillingService.cs       # Synchronisation DB
├── Services/
│   ├── JwtService.cs           # Génération tokens JWT
│   └── AuthService.cs          # Inscription / Connexion
└── DependencyInjection.cs
```

### Persistence
Accès aux données via Entity Framework Core.

```
MonProjetWeb.Persistence/
├── Context/
│   └── ApplicationDbContext.cs
├── Repositories/
│   ├── GenericRepository.cs
│   ├── UserRepository.cs
│   ├── CostRepository.cs
│   ├── BudgetRepository.cs
│   ├── AlertRepository.cs
│   ├── RecommendationRepository.cs
│   └── GcpAccountRepository.cs
├── Migrations/
└── DependencyInjection.cs
```

### Web
Point d'entrée de l'application.

```
MonProjetWeb.Web/
├── Controllers/
│   ├── AuthController.cs
│   ├── BillingController.cs
│   ├── BudgetController.cs
│   ├── AlertController.cs
│   ├── RecommendationController.cs
│   └── GcpAccountController.cs
├── Pages/
│   ├── Dashboard/Index.cshtml
│   ├── Costs/Index.cshtml
│   ├── Budgets/Index.cshtml
│   ├── Alerts/Index.cshtml
│   ├── Recommendations/Index.cshtml
│   └── Auth/Login.cshtml + Register.cshtml
├── Program.cs
└── appsettings.json
```

---

## Modèle de données

```
User (1) ──────────────── (*) GcpAccount
                               │
                ┌──────────────┼──────────────┐
                │              │              │
           CostRecord      Budget (1)──(*) Alert
                               │
                         Recommendation
```

### Entités principales

| Entité | Description |
|---|---|
| User | Utilisateur avec rôle (Admin/Analyst/Viewer) |
| GcpAccount | Compte GCP lié à un projet et billing account |
| CostRecord | Enregistrement de coût par service GCP |
| Budget | Budget avec seuil d'alerte |
| Alert | Alerte générée (budget ou anomalie) |
| Recommendation | Suggestion d'optimisation avec économie estimée |

---

## Flux de données

```
Google Cloud Billing API
        ↓
  GoogleCloudService
  (données simulées en dev)
        ↓
   BillingService
  (synchronisation DB)
        ↓
  CostRepository
  (persistance SQL Server)
        ↓
  CostAnalysisService
  (logique métier)
        ↓
  BillingController / Dashboard
  (API REST + Razor Pages)
        ↓
  Navigateur (Bootstrap 5 + Chart.js)
```

---

## Sécurité

- **Authentification** : JWT Bearer Token (HS256)
- **Mots de passe** : BCrypt (cost factor 10)
- **HTTPS** : Nginx termination
- **Secrets GCP** : fichier JSON hors du dépôt Git (.gitignore)
- **SQL Server** : TrustServerCertificate en dev, certificat en prod

---

## Déploiement

```
Internet
   ↓
Nginx :80 (Reverse Proxy)
   ↓
Kestrel :5000 (ASP.NET Core)
   ↓
SQL Server :1433
   ↓
Google Cloud APIs (HTTPS)
```
