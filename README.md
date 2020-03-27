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
  CREATE TABLE [dbo].[assessment] (
    [assessment_id] [nvarchar](max),        
    [database_name] [nvarchar](200),        
    [space_name] [nvarchar](40),    
    [connection_string] [nvarchar](max),    
    [assessment_status] [nvarchar](max),
    [assessment_parameters] [nvarchar](max),
    [email_list] [nvarchar](max),       
    [assessment_result] [nvarchar](max),        
    [errors] [nvarchar](max),
    [created_on] DATETIME,
    [modified_on] DATETIME
)
 GO

 
 Insert into [dbo].[assessment] ( assessment_id, assessment_status) values ('aaa', 'dsdsd');
 Insert into [dbo].[assessment] ( assessment_id, assessment_status) values ('bbb', 'dddd');

 select * from [dbo].[assessment];

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

Make sure Service Principal is granted access policy for KeyVault Keys including all Cryptographic Operations

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
	<RuntimeIdentifiers>ubuntu.16.04-x64</RuntimeIdentifiers>
	<RuntimeFrameworkVersion>2.1.4</RuntimeFrameworkVersion>
```

- Navigate to publish directory and push the application and specify buildpack (manifest will be located there as well)

```
cd bin/Release/netcoreapp2.1/publish
cf push -f manifest.yml 
```

- Navigate to application home - this page will retrive and show data that was encrypted

#### Note: Tested on both PCF and Ubuntu 14 VM 
To test on Ubuntu VM with dotnet runtime installed, set all required env variables and run
```
dotnet coremvc.dll
```

## Update driver Notes

On linux VM install [ODBC Driver](https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server?view=sql-server-ver15#ubuntu17)

Verify install at `/opt/microsoft/msodbcsql17` , will need all files in installed directory
and some libraries from dependencies.
List dependencoes with `ldd` command:

```
azureuser@xenial:/opt/microsoft/msodbcsql17$ ldd lib64/libmsodbcsql-17.5.so.2.1 
        linux-vdso.so.1 =>  (0x00007ffff958f000)
        libdl.so.2 => /lib/x86_64-linux-gnu/libdl.so.2 (0x00007ffa7091d000)
        librt.so.1 => /lib/x86_64-linux-gnu/librt.so.1 (0x00007ffa70715000)
        libodbcinst.so.2 => /usr/lib/x86_64-linux-gnu/libodbcinst.so.2 (0x00007ffa704fa000)
        libkrb5.so.3 => /usr/lib/x86_64-linux-gnu/libkrb5.so.3 (0x00007ffa70228000)
        libgssapi_krb5.so.2 => /usr/lib/x86_64-linux-gnu/libgssapi_krb5.so.2 (0x00007ffa6ffde000)
        libstdc++.so.6 => /usr/lib/x86_64-linux-gnu/libstdc++.so.6 (0x00007ffa6fc5c000)
        libm.so.6 => /lib/x86_64-linux-gnu/libm.so.6 (0x00007ffa6f953000)
        libgcc_s.so.1 => /lib/x86_64-linux-gnu/libgcc_s.so.1 (0x00007ffa6f73d000)
        libpthread.so.0 => /lib/x86_64-linux-gnu/libpthread.so.0 (0x00007ffa6f520000)
        libc.so.6 => /lib/x86_64-linux-gnu/libc.so.6 (0x00007ffa6f156000)
        /lib64/ld-linux-x86-64.so.2 (0x00007ffa70f38000)
        libk5crypto.so.3 => /usr/lib/x86_64-linux-gnu/libk5crypto.so.3 (0x00007ffa6ef27000)
        libcom_err.so.2 => /lib/x86_64-linux-gnu/libcom_err.so.2 (0x00007ffa6ed23000)
        libkrb5support.so.0 => /usr/lib/x86_64-linux-gnu/libkrb5support.so.0 (0x00007ffa6eb18000)
        libkeyutils.so.1 => /lib/x86_64-linux-gnu/libkeyutils.so.1 (0x00007ffa6e914000)
        libresolv.so.2 => /lib/x86_64-linux-gnu/libresolv.so.2 (0x00007ffa6e6f9000)
 ```       

 Add all `/usr/lib/x86_64-linux-gnu/libodbc*.so.2` libraries