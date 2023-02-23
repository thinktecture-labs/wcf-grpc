# Architektur-Modernisierung: Migration von WCF zu gRPC mit ASP.NET Core - ein pragmatischer Ansatz

Viele Projekte mit verteilten Anwendungen in der .NET-Welt basieren noch auf der Windows Communication Foundation (WCF). Doch wie kommt man weg von der "Altlast" und wie stellt man seinen Code auf sowohl moderne als auch zukunftssichere Beine? Eine mögliche Lösung ist gRPC.

Der Wechsel von WCF zu gRPC ist ein Schritt, den Projekt-Teams anstreben können, um ihre verteilten Anwendungen zu modernisieren und zu optimieren. WCF-Dienste bieten umfassende Unterstützung für verschiedene Transportprotokolle und Sicherheitsmechanismen. gRPC, ein von Google entwickeltes Open-Source-Framework, verwendet HTTP/2 als Transportprotokoll und verwendet Protocol Buffers (Protobuf) als Standard-Nachrichtenformat, welches höhere Geschwindigkeit und bessere Skalierbarkeit verspricht. Dieser Artikel stellt einen praktischen und pragmatischen Ansatz zum Migrieren vorhandener WCF-Dienste zu gRPC-Diensten mit ASP.NET Core vor. Er enthält die notwendigen konkreten Schritte, die zum Planen und Umsetzen der Migration erforderlich sind.

## Migration mit gRPC Code-First

Im Standardvorgehen verwendet gRPC einen Contract-First-Ansatz mit Protocol Buffers als Nachrichten- und Contract-Format. Protobuf ist eine schnelle und effiziente Möglichkeit, Daten zwischen verschiedenen Systemen auszutauschen. Zudem erlaubt es die Definition eines Schnittstellen-, Nachrichten- und Datenkontrakts für Kommunikation zwischen verschiedenen Systemen, indem es ermöglicht wird, Service-Methoden und zugehörige Parameter in einer interoperablen Art und Weise zu definieren. Allerdings müsste man dann alle bestehenden Service-Schnittstellen und Modelle in Protobuf beschreiben, was insbesondere bei komplexen Anwendungen einen hohen Aufwand bedeuten kann. Und man hat ja bei existierenden WCF-Lösungen oft .NET sowohl auf der Client- als auch auf der Services-Seite. Um daher den Aufwand bei der Migration zu verringern, gibt es ein Community-Projekt namens `protobuf-net.Grpc`, das es ermöglicht bestehende WCF-Dienste mittels Code-First-Ansatz in gRPC Services zu refactoren. Dies erlaubt das Schreiben von .NET-Code für die Nutzung von gRPC und die Wiederverwendung existierenden Codes als ersten Schritt zur Modernisierung bestehender Applikationen.

## Voraussetzungen für WCF-Dienste

Damit ein bestehender WCF Service als gRPC Code-First Dienst genutzt werden kann, muss der Contract bestimmte Anforderungen erfüllen:

1. Auf dem Contract muss das `ServiceContractAttribute` gesetzt sein
2. Alle Service-Methoden müssen das `OperationContractAttribute` haben
3. Alle Service-Methoden dürfen nur ein Argument haben und dieses muss eine Klasse/DTO sein. Gleiches gilt auch für den Rückgabetypen, diese müssen vom Typ `void`, `Task` oder eine eigene Klasse/DTO sein
4. Alle DTOs müssen das `DataContractAttribute` haben
5. Alle Properties auf dem DTO müssen das `DataMemberAttribute` haben und dies muss eine eindeutige `Order` haben, damit der richtige Datenfluss garantiert ist.

Im Folgenden sieht man ein Beispiel, wie ein existierender Contract refactored werden muss, damit dieser mit gRPC Code-First nutzbar ist.

```csharp
[ServiceContract]
public interface IGreeter
{
    [OperationContract]
    public string Greet(string firstName, string lastName);
}
```

Dieser Service-Contract erfüllt nicht alle Anforderungen, um als gRPC Code-First Dienst verwendet zu werden, da er mehrere Parameter entgegennimmt. Die Methode kann jedoch leicht angepasst werden, indem zwei DTO-Klassen erstellt werden, welche zum einen die Parameter und zum anderen die Rückgabe enthalten. Zum Beispiel:

```csharp
[DataContract]
public class GreetRequest
{
    [DataMember(Order = 1)]
    public string FirstName { get; set; }

    [DataMember(Order = 2)]
    public string LastName { get; set; }
}

[DataContract]
public class GreeterResponse
{
    [DataMember(Order = 1)]   
    public string Response { get; set; }
}

[ServiceContract]
public interface IGreeter
{
    [OperationContract]
    public GreeterResponse Greet(GreetRequest request);
}
```

## gRPC-Dienst in ASP.NET Core hosten

Wenn der Contract alle Anforderungen erfüllt, kann der Dienst mittels des Pakets [protobuf-net.Grpc.AspNetCore](https://github.com/protobuf-net/protobuf-net.Grpc) in ASP.NET Core gehostet werden. Folgende Schritte sind notwendig:

1. Das Nuget-Paket hinzufügen. Dafür kann die Command Line oder der Nuget-Paket-Manager der IDE genutzt werden bzw. die `csproj`-Datei angepasst werden:

```xml
<ItemGroup>
    <PackageReference Include="protobuf-net.Grpc.AspNetCore" Version="1.1.1" />
</ItemGroup>
```

2. Die gRPC Code-First Dienste müssen in der Dependency Injection (DI) registriert werden. Hierfür muss die Methode `AddCodeFirstGrpc` aufgerufen werden. 

```csharp
builder.Services.AddCodeFirstGrpc();
```

3. Die Service-Implementierung (`GreeterService`) muss in die ASP.NET Core Pipeline hinzugefügt werden, um ihn erreichbar zu machen.

```csharp
app.MapGrpcService<GreeterService>();
```

Das vollständige minimale Beispiel sieht dann so aus:

```csharp
using ProtoBuf.Grpc.Server;
using Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCodeFirstGrpc();

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.Run();
```

Damit ist der Dienst in der ASP.NET-Core-Anwendung registriert und erreichbar.

## Client

Um einen Client für einen gRPC Code-First Dienst in C# zu erstellen, müssen zuerst die benötigten NuGet-Pakete installiert werden. Die Pakete `Grpc.Net.Client` und `protobuf-net.Grpc` enthalten die erforderlichen Klassen, um einen gRPC-Client zu erstellen.

```xml
<ItemGroup>
    <PackageReference Include="Grpc.Net.Client" Version="2.51.0" />
    <PackageReference Include="protobuf-net.Grpc" Version="1.1.1" />
</ItemGroup>
```

Anschließend muss eine Verbindung zum Dienst hergestellt und eine Client-Instanz erstellt werden. Dazu kann die `GrpcChannel`-Klasse und die Methode `CreateGrpcService` mit dem Service Contract verwendet werden. Dies ist die Power des Code-First-Ansatzes für gRPC. WCF-Entwickler werden hier sicherlich die `ChannelFactory<T>` aus den alten Zeiten wiedererkennen.

Sobald die Client-Instanz erstellt ist, kann die gRPC-Methode über die entsprechende Client-Methode aufgerufen werden.

Hier ist ein Beispiel für die Erstellung eines Clients für den `GreeterService`:

```csharp
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Service;

using var channel = GrpcChannel.ForAddress("https://localhost:7199");
var client = channel.CreateGrpcService<IGreeter>();
var reply = client.Greet(new GreetRequest() { FirstName = "John", LastName = "Doe"});
Console.WriteLine($"Greeting: {reply.Response}");
```

## WCF Behaviors in gRPC nachstellen

[WCF Behaviors](https://learn.microsoft.com/en-us/dotnet/framework/wcf/extending/configuring-and-extending-the-runtime-with-behaviors) ermöglichen zusätzliche Verarbeitungsfunktionen für eingehende und ausgehende Nachrichten in einem WCF-Dienst. Diese Funktionalität kann in gRPC mit Hilfe von [ASP.NET Core Middlewares](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/) und [gRPC Interceptors](https://learn.microsoft.com/en-us/aspnet/core/grpc/interceptors) nachgebildet werden. Eine Middleware kann zum Beispiel genutzt werden, um Authentifizierung und Autorisierung zu implementieren, während Interceptors dazu dienen können, eingehende und ausgehende Nachrichten zu überwachen, zu ändern oder zu blockieren.

Zur Registrierung von ASP.NET Core Middlewares kann die `Use`-Methode der `IApplicationBuilder`-Schnittstelle aufgerufen werden. So kann mit folgendem Beispiel Authentifizierung für einen gRPC-Dienst hinzugefügt werden.

```csharp
..
builder.Services
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        // TODO: JWT Bearer Authentifizierung konfigurieren
    });
....
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<GreeterService>();
....
```

Mit dem `AddInterceptor`-Methodenaufruf auf den `GrpcServiceOptions` können gRPC-Interceptors konfiguriert werden, um benutzerdefinierten Code für die Verarbeitung von eingehenden und ausgehenden Nachrichten zu registrieren. Interceptors können global für alle Dienste oder aber pro Dienst festgelegt werden. Ein Beispiel für einen globalen Interceptor, der eingehende Nachrichten logged, kann so aussehen:

```csharp
public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }
    
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogDebug("Aufruf der gRPC-Methode {method} mit Argument {argument}", context.Method, request);
        var result = await base.UnaryServerHandler(request, context, continuation);
        _logger.LogDebug("gRPC-Methode {method} mit Ergebnis {result} wurde aufgerufen", context.Method, result);
        return result;
    }
}
```

Um den Interceptor zu registrieren, muss die entsprechende `AddInterceptor`-Method aufgerufen werden. 

```csharp
// global
builder.Services
    .AddCodeFirstGrpc(options => options.Interceptors.Add<LoggingInterceptor>())

// per Service
builder.Services
    .AddCodeFirstGrpc(_ => { })
    .AddServiceOptions<GreeterService>(options => options.Interceptors.Add<LoggingInterceptor>());
```

Durch die Verwendung von ASP.NET Core Middlewares und gRPC Interceptors kann also die Funktionalität von WCF Behaviors in einem gRPC Service nachgebildet werden.

## Fazit

Insgesamt bietet der Wechsel von WCF zu gRPC mit ASP.NET Core ein leistungsfähigeres und skalierbareres Framework für die Entwicklung verteilter Anwendungen. Das Code-First-Design mit dem Community-Projekt `protobuf-net.Grpc` und ASP.NET Core Middlewares sowie die Verwendung von gRPC Interceptors sorgen für einen einfacheren Übergang und verringern den Aufwand für die Portierung vorhandener WCF-Dienste. Die Migration von WCF zu gRPC kann ein komplexes Unterfangen sein, aber sie hat viele Vorteile, einschließlich verbesserter Performance, Skalierbarkeit, Interoperabilität und Zukunftssicherheit. 

Wesentlich umfassendere Informationen zu dem Thema gRPC für WCF Entwickler gibt es direkt von Microsoft als [eBook](https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/).
