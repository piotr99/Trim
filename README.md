# TRIM CRM

System CRM i wsparcia sprzedaży dedykowany dla branży samochodów ciężarowych. Aplikacja automatyzuje procesy handlowe, ułatwiając zarządzanie zgłoszeniami klientów (Sales Cases), zaawansowaną konfiguracją pojazdów, generowaniem ofert oraz procesowaniem końcowych zamówień.

## 🛠 Technologie
* **Backend:** ASP.NET Core, Entity Framework Core
* **Baza danych:** SQL Server
* **Frontend:** Razor Pages, Bootstrap 5, jQuery (AJAX)
* **Infrastruktura:** Docker

---

## 🚀 Wymagania wstępne
Aby uruchomić projekt lokalnie, upewnij się, że posiadasz zainstalowane:
* [Zestaw SDK dla platformy .NET](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/products/docker-desktop)
* [Ollama](https://ollama.com/download/windows)

---

## 🐳 Baza Danych (Docker)

Aplikacja wykorzystuje bazę SQL Server uruchamianą w kontenerze. Poniższe polecenie pobiera obraz, konfiguruje polską kolację (`Polish_100_CI_AS`), ustawia hasło i wystawia bazę na porcie `1439`.

Uruchom w terminalu (PowerShell):

```powershell
docker run --name "Trim" `
    -v Trim-data:/var/opt/mssql `
    -e ACCEPT_EULA=Y `
    -e SA_PASSWORD="TRIM@Passw0rd" `
    -e MSSQL_COLLATION=Polish_100_CI_AS `
    -p 1439:1433 `
    -d [mcr.microsoft.com/mssql/server:latest](https://mcr.microsoft.com/mssql/server:latest)
