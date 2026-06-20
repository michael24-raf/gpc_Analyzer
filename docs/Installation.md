# Guide d'installation — GCP Cost Analyzer

## Prérequis

| Outil | Version | Vérification |
|---|---|---|
| Ubuntu | 24.04 LTS | `lsb_release -a` |
| .NET SDK | 8.0+ | `dotnet --version` |
| SQL Server | 2022 | `systemctl status mssql-server` |
| Nginx | 1.24+ | `nginx -v` |
| Git | 2.x | `git --version` |
| Google Cloud CLI | Latest | `gcloud --version` |

---

## 1. Installer .NET 8 SDK

```bash
# Ajouter le dépôt Microsoft
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# Installer .NET 8
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Vérifier
dotnet --version
```

---

## 2. Installer SQL Server 2022

```bash
# Importer la clé Microsoft
curl -fsSL https://packages.microsoft.com/keys/microsoft.asc \
  | sudo gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg

# Ajouter le dépôt
curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list \
  | sudo tee /etc/apt/sources.list.d/mssql-server-2022.list

# Installer
sudo apt-get update
sudo apt-get install -y mssql-server

# Configurer (choisir Developer edition)
sudo /opt/mssql/bin/mssql-conf setup

# Démarrer
sudo systemctl start mssql-server
sudo systemctl enable mssql-server
```

---

## 3. Cloner le projet

```bash
git clone https://github.com/ton-username/MonProjetWeb.git
cd MonProjetWeb
```

---

## 4. Configurer la base de données

```bash
# Créer la base de données
sqlcmd -S localhost -U SA -P 'TonMotDePasse' -C -Q "
CREATE DATABASE GcpCostAnalyzer;
GO
"
```

---

## 5. Configurer appsettings.json

Modifier `src/MonProjetWeb.Web/appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GcpCostAnalyzer;User Id=SA;Password=TonMotDePasse;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "TaCleSecrete256BitsMinimumPourHMACSHA256!",
    "Issuer": "MonProjetWeb",
    "Audience": "MonProjetWebUsers",
    "ExpiresInMinutes": "60"
  },
  "GoogleCloud": {
    "ServiceAccountPath": "secrets/gcp-service-account.json",
    "ProjectId": "TON_PROJECT_ID",
    "BillingAccountId": "XXXXXX-XXXXXX-XXXXXX"
  }
}
```

---

## 6. Configurer Google Cloud (optionnel)

```bash
# Installer Google Cloud CLI
sudo apt-get install -y google-cloud-cli

# Authentification
gcloud auth login

# Créer un Service Account
gcloud iam service-accounts create gcp-cost-analyzer \
  --display-name="GCP Cost Analyzer" \
  --project=TON_PROJECT_ID

# Attribuer les rôles
gcloud projects add-iam-policy-binding TON_PROJECT_ID \
  --member="serviceAccount:gcp-cost-analyzer@TON_PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/billing.viewer"

# Générer la clé JSON
mkdir -p src/MonProjetWeb.Web/secrets
gcloud iam service-accounts keys create \
  src/MonProjetWeb.Web/secrets/gcp-service-account.json \
  --iam-account=gcp-cost-analyzer@TON_PROJECT_ID.iam.gserviceaccount.com
```

---

## 7. Appliquer les migrations

```bash
# Installer EF Core Tools
dotnet tool install --global dotnet-ef

# Appliquer les migrations
dotnet ef database update \
  --project src/MonProjetWeb.Persistence \
  --startup-project src/MonProjetWeb.Web
```

---

## 8. Lancer en développement

```bash
dotnet run --project src/MonProjetWeb.Web
```

Ouvrir `http://localhost:5281` dans le navigateur.

---

## 9. Déploiement en production

### Publier l'application

```bash
sudo mkdir -p /var/www/gcpanalyzer

dotnet publish src/MonProjetWeb.Web/MonProjetWeb.Web.csproj \
  -c Release -r linux-x64 --self-contained false \
  -o /var/www/gcpanalyzer

sudo chown -R www-data:www-data /var/www/gcpanalyzer
```

### Créer le service systemd

```bash
sudo nano /etc/systemd/system/gcpanalyzer.service
```

```ini
[Unit]
Description=GCP Cost Analyzer — ASP.NET Core 8
After=network.target mssql-server.service

[Service]
Type=simple
User=www-data
Group=www-data
WorkingDirectory=/var/www/gcpanalyzer
ExecStart=/usr/bin/dotnet /var/www/gcpanalyzer/MonProjetWeb.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=gcpanalyzer
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable gcpanalyzer
sudo systemctl start gcpanalyzer
```

### Configurer Nginx

```bash
sudo nano /etc/nginx/sites-available/gcpanalyzer
```

```nginx
server {
    listen 80;
    server_name localhost;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Real-IP         $remote_addr;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_read_timeout 90s;
    }
}
```

```bash
sudo ln -sf /etc/nginx/sites-available/gcpanalyzer \
            /etc/nginx/sites-enabled/gcpanalyzer
sudo nginx -t
sudo systemctl reload nginx
```

---

## 10. Lancer les tests

```bash
dotnet test tests/MonProjetWeb.Tests/MonProjetWeb.Tests.csproj --verbosity normal
```

---

## Commandes de maintenance

```bash
# Statut des services
sudo systemctl status gcpanalyzer
sudo systemctl status nginx

# Logs en temps réel
sudo journalctl -u gcpanalyzer -f

# Redéployer après modification
dotnet publish src/MonProjetWeb.Web -c Release -r linux-x64 \
  --self-contained false -o /var/www/gcpanalyzer
sudo systemctl restart gcpanalyzer

# Nouvelle migration EF Core
dotnet ef migrations add NomDeLaMigration \
  --project src/MonProjetWeb.Persistence \
  --startup-project src/MonProjetWeb.Web
dotnet ef database update \
  --project src/MonProjetWeb.Persistence \
  --startup-project src/MonProjetWeb.Web
```
