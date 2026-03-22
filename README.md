# TRIM CRM

## Docker
```
docker run --name "Trim" `
    -v Trim-data:/var/opt/mssql `
    -e ACCEPT_EULA=Y `
    -e SA_PASSWORD="TRIM@Passw0rd" `
    -e MSSQL_COLLATION=Polish_100_CI_AS `
    -p 1439:1433 `
    -d mcr.microsoft.com/mssql/server:latest
```

## Seeded admin credentials

Login admin@test.pl
Password Admin1!
