## PCF ASP.NET Core Application using SQLServer AlwaysEncrypted on Linux (with ODBC)

This project demonstrates very simple .NET Core application connecting to SQL Server using ODBC driver.
Driver details are here: https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server?view=sql-server-2017

## Application setup

Applications uses Nuget package `System.Data.ODBC` and corresponding OdbcConnection and DataReader - rfor details refer to `HomeController.cs`

###  MS ODBC driver files for Linux 14
For the Core application to be able to use it ODBC driver WITHOUT installing it, all the required libraries should be included along with the application.
In this example all Ubuntu 14 related libraries and resources are included with the application in `msodbcsql17` directory. 
During `publish` this directory is copied alond with other application artifacts.

For the driver to load required libraries and find system ini files, following environment variables have to be set:
- `LD_LIBRARY_PATH` - points to directory where all driver *.so files are located. For application published to PCF: `/home/vcap/app/msodbcsql17`
- `ODBCSYSINI` - points to directory where `odbcinst.ini` is located with information about Microsoft ODBC Driver 17. For application published to PCF: `/home/vcap/app/msodbcsql17`

Example could be found in this project `manifest.yml` 

### SQL Setup

- Create SQL Database and follow AlwaysEncrypted documentation to grant sql user required permissions. 
- Create Key Vault and service principal that has required persmissions to access and use keys (created and used by AlwaysEncrpted)
- Create SQL Table and data 
This example uses very simple SQL table that could be created
 
 ```
    CREATE TABLE dbo.person  
        ( First_Name varchar(25) NOT NULL,  
            Last_Name varchar(50) NULL)  
    GO 

    INSERT person Values ('Test', 'Blah blah');
    INSERT person Values ('Test2', 'mine blah');
    INSERT person Values ('Test3', 'ohhhh blah');
  ```
  - Use AlwaysEncrypted SSMS wizard or powershell to encrypt one of the columns

### SQL Connection 

ODBC driver uses connection string in the following format

```    
Driver={ODBC Driver 17 for SQL Server}; 
Server=tcp:<server>.database.windows.net,1433;Database=<database>;
Uid=<db user>;
Pwd=<db user password>;
Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;
```
And following settings are added to enable AlwaysEncrypted
```
ColumnEncryption=Enabled; 
KeyStoreAuthentication=KeyVaultClientSecret;
KeyStorePrincipalId=<spn app id>;
KeyStoreSecret=<spn secret>;
```

In this example full connection string is combined from settings in `appsettings.json` or environment variables that override json file.

Refer to `manifest.yml` for example  of setting connection string variables:
```
     ODBC__Server: "tcp:<server>,1433"
     ODBC__Database: <db name>
     ODBC__Uid: <db user>
     ODBC__Pwd: <db user password>
     ODBC__ColumnEncryption: Enabled
     ODBC__KeyStoreAuthentication: KeyVaultClientSecret
     ODBC__KeyStorePrincipalId: <spn id>
     ODBC__KeyStoreSecret: <spn secret>
```

### Build and Run

Set all the ODBC connectivity vaules in `manifest.yml` with your SQL server/db and service principal credentials.

- Build and publish the app:

```
dotnet restore
dotnet build
dotnet publish -c Release
```

Note: added in project file to point to linux runtime (used by PCF cells) and .net runtime that is used by dotnetcore2.1 buildpack 
```
    <TargetFramework>netcoreapp2.1</TargetFramework>
	<RuntimeIdentifiers>ubuntu.14.04-x64</RuntimeIdentifiers>
	<RuntimeFrameworkVersion>2.1.0</RuntimeFrameworkVersion>
```

- Navigate to publish directory and push the application and specify buildpack (manifest will be located there as well)

```
cd bin/Release/netcoreapp2.1/publish
cf push -f manifest.yml -b https://github.com/cloudfoundry/dotnet-core-buildpack.git#v2.1.4
```

- Navigate to application home - this page will retrive and show data that was encrypted

#### Note: Tested on both PCF and Ubuntu 14 VM 
To test on Ubuntu VM with dotnet runtime installed, set all required env variables and run
```
dotnet coremvc.dll
```

