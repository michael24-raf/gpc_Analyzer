# Guide utilisateur — GCP Cost Analyzer

## Introduction

GCP Cost Analyzer vous permet de surveiller, analyser et optimiser vos dépenses Google Cloud Platform depuis une interface web intuitive.

---

## Accès à l'application

Ouvrez votre navigateur et accédez à :

- **Développement** : `http://localhost:5281`
- **Production** : `http://localhost` ou votre domaine

---

## 1. Inscription et connexion

### Créer un compte

1. Aller sur `/Auth/Register`
2. Remplir le formulaire :
   - Prénom / Nom
   - Email
   - Mot de passe (minimum 8 caractères)
3. Cliquer sur **Créer mon compte**
4. Vous êtes automatiquement redirigé vers le tableau de bord

### Se connecter

1. Aller sur `/Auth/Login`
2. Saisir votre email et mot de passe
3. Cliquer sur **Se connecter**

### Rôles utilisateurs

| Rôle | Accès |
|---|---|
| Admin | Accès complet |
| Analyst | Lecture + recommandations |
| Viewer | Lecture seule |

---

## 2. Tableau de bord

Le tableau de bord (`/Dashboard`) affiche une vue globale de vos coûts GCP.

### Indicateurs clés (KPI)

| Indicateur | Description |
|---|---|
| Coût total (mois) | Total des dépenses GCP du mois en cours |
| Économies potentielles | Estimation des économies possibles (15%) |
| Alertes actives | Nombre d'alertes non résolues |
| Anomalies détectées | Pics de dépenses détectés ce mois |

### Graphiques

- **Coûts mensuels** : Évolution des dépenses sur 6 mois (barres)
- **Répartition par service** : Part de chaque service GCP (donut)

### Sections

- **Top services GCP** : Les 5 services les plus coûteux avec barres de progression
- **Alertes actives** : Dernières alertes à traiter
- **Budgets** : État de consommation des budgets
- **Recommandations** : Suggestions d'optimisation en attente

---

## 3. Analyse des coûts

La page `/Costs` permet d'analyser vos dépenses en détail.

### Filtrer par période

1. Sélectionner une **date de début** et une **date de fin**
2. Cliquer sur **Filtrer**
3. Les graphiques et le tableau se mettent à jour

### Graphiques disponibles

- **Évolution mensuelle** : Courbe des coûts sur 6 mois
- **Répartition** : Donut par service pour la période sélectionnée

### Tableau des coûts

Le tableau affiche pour chaque service :
- Nom et identifiant du service
- Région
- Période de facturation
- Montant en USD
- Pourcentage du total

---

## 4. Gestion des budgets

La page `/Budgets` permet de créer et suivre vos budgets GCP.

### Créer un budget

1. Cliquer sur **Nouveau budget**
2. Remplir le formulaire :
   - **Nom** : ex. "Budget Production"
   - **Montant** : limite en USD
   - **Période** : dates de début et fin
   - **Seuil d'alerte** : pourcentage déclenchant une alerte (défaut 80%)
3. Cliquer sur **Créer**

### Indicateurs budget

Chaque budget affiche :
- Barre de progression colorée :
  - 🟢 Vert : moins de 80% consommé
  - 🟡 Jaune : entre 80% et 100%
  - 🔴 Rouge : budget dépassé
- Montant dépensé / total alloué
- Seuil d'alerte configuré

### Supprimer un budget

Cliquer sur l'icône 🗑️ à droite du budget, confirmer la suppression.

---

## 5. Gestion des alertes

La page `/Alerts` centralise toutes les alertes de l'application.

### Types d'alertes

| Type | Description |
|---|---|
| Budget dépassé | Le seuil d'alerte du budget est atteint |
| Anomalie | Un service dépense 2x plus que sa moyenne historique |

### Niveaux de sévérité

| Sévérité | Condition |
|---|---|
| 🔴 Critical | Budget à 100%+ ou anomalie 3x la moyenne |
| 🟠 High | Budget à 90%+ ou anomalie 2x la moyenne |
| 🟡 Medium | Budget entre 80% et 90% |
| ⚫ Low | Informationnel |

### Actions disponibles

- **👁 Acquitter** : Marquer l'alerte comme vue (status → Acknowledged)
- **✅ Résoudre** : Marquer l'alerte comme traitée (status → Resolved)
- **🔍 Détecter les anomalies** : Lancer une analyse des coûts anormaux

---

## 6. Recommandations

La page `/Recommendations` liste les suggestions d'optimisation générées automatiquement.

### Générer des recommandations

Cliquer sur **Générer les recommandations** pour analyser vos coûts et produire des suggestions.

### Types de recommandations

| Type | Description | Économie estimée |
|---|---|---|
| 🗑️ Ressource inutilisée | Service avec moins de 5$ sur 3 mois | 100% du coût |
| ↕️ Réduction de capacité | Compute Engine > 500$ sur 3 mois | ~20% |
| ⚙️ Optimisation VM | Kubernetes/Cloud Run coûteux | ~30% |

### Actions

- **✅ Appliquer** : Marquer la recommandation comme appliquée
- **✖️ Ignorer** : Rejeter la recommandation

---

## 7. API REST

L'application expose une API REST complète pour intégration externe.

### Authentification

```bash
# Obtenir un token JWT
curl -X POST http://localhost/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@exemple.com","password":"MonMotDePasse1!"}'
```

### Endpoints principaux

```
GET  /api/billing/summary/{gcpAccountId}         Résumé dashboard
GET  /api/billing/costs/{id}/monthly?months=6    Coûts mensuels
POST /api/billing/sync/{gcpAccountId}            Synchroniser GCP

GET  /api/budget/{gcpAccountId}                  Liste budgets
POST /api/budget                                 Créer budget
PUT  /api/budget/{budgetId}                      Modifier budget
DELETE /api/budget/{budgetId}                    Supprimer budget

GET  /api/alert/{gcpAccountId}/active            Alertes actives
POST /api/alert/{gcpAccountId}/detect-anomalies  Détecter anomalies

GET  /api/recommendation/{gcpAccountId}/pending  Recommandations
POST /api/recommendation/{gcpAccountId}/generate Générer
```

---

## 8. Synchronisation des données GCP

Pour synchroniser les données réelles depuis GCP :

```bash
# Via l'API
curl -X POST http://localhost/api/billing/sync/1

# Via curl avec token JWT
curl -X POST http://localhost/api/billing/sync/1 \
  -H "Authorization: Bearer VOTRE_JWT_TOKEN"
```

---

## 9. Dépannage

| Problème | Solution |
|---|---|
| Page blanche | Vérifier `sudo systemctl status gcpanalyzer` |
| Erreur 502 | L'application ne tourne pas, vérifier les logs |
| Données vides | Lancer une synchronisation via `/api/billing/sync/1` |
| Erreur SQL | Vérifier `sudo systemctl status mssql-server` |
| Token expiré | Se reconnecter via `/Auth/Login` |

### Voir les logs

```bash
# Logs application
sudo journalctl -u gcpanalyzer -f

# Logs Nginx
sudo tail -f /var/log/nginx/gcpanalyzer.error.log
```
