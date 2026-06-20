# GCP Cost Analyzer

> Application web ASP.NET Core 8 d'analyse et d'optimisation des coûts Google Cloud Platform.

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-blue)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-red)
![Ubuntu](https://img.shields.io/badge/Ubuntu-24.04-orange)

---

## Présentation

**GCP Cost Analyzer** est une application full-stack permettant de :

- Visualiser et analyser les coûts GCP en temps réel
- Gérer des budgets avec alertes automatiques
- Détecter les anomalies de dépenses
- Générer des recommandations d'optimisation
- Synchroniser les données depuis Google Cloud Billing API

---

## Fonctionnalités

| Fonctionnalité | Description |
|---|---|
| Tableau de bord | Vue globale des coûts, budgets, alertes et recommandations |
| Analyse des coûts | Coûts par service, par mois, avec graphiques interactifs |
| Gestion des budgets | Création, suivi et alertes de dépassement |
| Alertes | Détection automatique d'anomalies et alertes budget |
| Recommandations | Suggestions d'optimisation avec économies estimées |
| API REST | Endpoints complets pour intégration externe |
| Authentification | JWT sécurisé avec rôles utilisateurs |

---

## Stack technique

- **Backend** : ASP.NET Core 8, C# 12
- **Architecture** : Clean Architecture (Domain, Application, Infrastructure, Persistence, Web)
- **Base de données** : SQL Server 2022 + Entity Framework Core 8
- **Frontend** : Razor Pages, Bootstrap 5, Chart.js
- **Auth** : JWT Bearer Token, BCrypt
- **Cloud** : Google Cloud Billing API, Budgets API
- **Tests** : xUnit, Moq, FluentAssertions, EF Core InMemory
- **Déploiement** : Ubuntu 24.04, systemd, Nginx

---

## Structure du projet

```
MonProjetWeb/
├── src/
│   ├── MonProjetWeb.Domain          # Entités métier, enums
│   ├── MonProjetWeb.Application     # Use cases, interfaces, DTOs, services
│   ├── MonProjetWeb.Infrastructure  # GCP, JWT, Auth
│   ├── MonProjetWeb.Persistence     # EF Core, DbContext, Repositories
│   └── MonProjetWeb.Web             # Razor Pages, Controllers, Program.cs
├── tests/
│   └── MonProjetWeb.Tests           # Tests unitaires xUnit
└── docs/
    ├── README.md
    ├── Architecture.md
    ├── Installation.md
    └── UserGuide.md
```

---

## Démarrage rapide

```bash
# Cloner le projet
git clone https://github.com/ton-username/MonProjetWeb.git
cd MonProjetWeb

# Restaurer les dépendances
dotnet restore

# Appliquer les migrations
dotnet ef database update \
  --project src/MonProjetWeb.Persistence \
  --startup-project src/MonProjetWeb.Web

# Lancer en développement
dotnet run --project src/MonProjetWeb.Web
```

Ouvrir `http://localhost:5281` dans le navigateur.

---

## Auteur

**Michaël** — Étudiant en 3ème année Licence Informatique  
École Nationale de l'Informatique (ENI), Université de Fianarantsoa, Madagascar

---

## Licence

Projet académique — ENI Fianarantsoa 2026
