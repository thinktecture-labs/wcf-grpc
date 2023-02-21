# Wie man bestehende WCF-Dienste mit ASP.NET Core zu gRPC migriert: Ein praktischer Ansatz

Der Wechsel von Windows Communication Foundation (WCF) zu gRPC ist ein Schritt, den viele Organisationen anstreben, um ihre verteilten Anwendungen zu modernisieren und zu optimieren. WCF-Dienste bieten umfassende Unterstützung für verschiedene Transportprotokolle und Sicherheitsmechanismen, aber gRPC, ein von Google entwickeltes Open-Source-Framework, verwendet HTTP/2 als Transportprotokoll und verwendet Protokollpuffer als Standard-Nachrichtenformat, das bei Verwendung eine bessere Leistung und Skalierbarkeit bietet. Dieser Artikel stellt einen praktischen Ansatz zum Migrieren vorhandener WCF-Dienste zu gRPC-Diensten mit ASP.NET Core vor. Es enthält auch die Schritte, die zum Planen und Ausführen der Migration erforderlich sind.

## Migration mit gRPC Code-First

gRPC verwendet einen Contract-First-Ansatz mit Protocol Buffers (Protobuf) als Nachrichten- und Contract-Format. Protobuf ist eine schnelle und effiziente Möglichkeit, Daten zwischen verschiedenen Systemen auszutauschen. Die Definition eines Vertragsmodells vereinfacht zunächst die Interoperabilität und Integration zwischen verschiedenen Systemen, indem es ermöglicht wird, Servicemethoden und zugehörige Parameter klar und konsistent zu definieren. Allerdings müsste man dann alle bestehenden Services und Models in Protobuf beschreiben was insbesondere bei komplexen Anwendungen eine hohen Aufwand bedeuten kann. Um den Aufwand bei der Migration zu verringern, gibt es ein Community-Projekt, dass es ermoeglicht bestehende WCF-Dienste mittels einem Code-First-Ansatz als gRPC zu betreiben. Dies ermoeglicht die Wiederverwendung existierenden Codes und ermoeglicht einen ersten Schritt zu der Modernisierung bestehender Applikationen.

## Voraussetzungen

Damit ein bestehender WCF-Service als gRPC Code-First Dienst genutzt werden kann, muss der Kontrakt bestimmte Anforderungen erfüllen:

1. Auf dem Kontrakt muss das `ServiceContractAttribute` gesetzt sein
2. Alle Service-Methoden müssen das `OperationContractAttribute` haben
3. Alle Service-Methoden duerfen nur ein Argument haben und dieses muss eine Klasse/DTO sein. Gleiches gilt auch fuer den Rückgabe Typen, dieser muss vom Typ `void`, `Task` oder eine eigene Klasse/DTO sein. 
4. Alle DTOs müssen dass `DataContractAttribute` haben
5. Alle Properties auf dem DTO müssen das `DataMemberAttribute` haben und dies muss eine eindeutige `Order` haben, damit der richtige Datenfluss garantiert ist.

Im folgenden ein Beispiel, wie ein existierender Kontrakt refaktoriert werden muss, damit diese mit gRPC Code-First nutzbar ist.

```csharp
[ServiceContract]
public interface IGreeter
{
    [OperationContract]
    public string Greet(string firstName, string lastName);
}
```

Dieser Service-Kontrakt erfüllt nicht alle Anforderungen, um als gRPC Code-First-Dienst verwendet zu werden, da er mehrere Parameter entgegennimmt. Die Methode kann jedoch leicht angepasst werden, indem zwei DTO-Klasse erstellt werden, die die Parameter enthält, die die Methode benötigt und die Rueckgabe enthaelt. Zum Beispiel:

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

## Dienst in ASP.NET Core hosten

Wenn der Kontrakt alle Anforderungen erfuellt, kann der Dienst mittels des Pakets [protobuf-net.Grpc](https://github.com/protobuf-net/protobuf-net.Grpc) in ASP.NET Core gehostet werden. Folgende Schritte sind dafuer notwendig:

1. Das Nuget Paket hinzufuegen. Dafuer kann der Nuget-Paket-Manager der IDE genutzt werden oder die csproj Datei angepasst werden

```xml
<ItemGroup>
    <PackageReference Include="protobuf-net.Grpc.AspNetCore" Version="1.1.1" />
</ItemGroup>
```

2. Die gRPC-Code-First Dienste müssen in der DI registriert werden. Hierfuer muss die Methode `AddCodeFirstGrpc` aufgerufen werden. 

```csharp
builder.Services.AddCodeFirstGrpc();
```

3. Die Service-Implementierung (GreeterService) muss in die ASP.NET Core Pipeline hinzugefuegt werden um ihn erreichbar zu machen.. 

```csharp
app.MapGrpcService<GreeterService>();
```

Das vollstaendige, minimale, Beispiel sieht dann so aus:

```csharp
using ProtoBuf.Grpc.Server;
using Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCodeFirstGrpc();

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.Run();
```

Damit ist der Dienst in der ASP.NET Core Anwendung registriert und erreichbar.

## Client

Um einen Client für einen gRPC-Code-First-Dienst in C# zu erstellen, müssen zuerst die benötigten NuGet-Paket installiert werden. Die Pakete `Grpc.Net.Client` und `protobuf-net.Grpc` enthalten die erforderlichen Klassen, um einen gRPC-Client zu erstellen. Dafuer kann der Nuget-Paket-Manager der IDE genutzt werden oder die csproj Datei angepasst werden:

```xml
<ItemGroup>
    <PackageReference Include="Grpc.Net.Client" Version="2.51.0" />
    <PackageReference Include="protobuf-net.Grpc" Version="1.1.1" />
</ItemGroup>
```

Anschließend muss eine Verbindung zum Dienst hergestellt und eine Client-Instanz erstellt werden. Dazu kann die `GrpcChannel`-Klasse und die Methode `CreateGrpcService` mit dem Servicekontrakt verwendet werden. Sobald die Client-Instanz erstellt ist, kann die gRPC-Methode über die entsprechende Client-Methode aufgerufen werden.

Hier ist ein Beispiel für die Erstellung eines Clients fuer den GreeterService:

```csharp
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Service;

using var channel = GrpcChannel.ForAddress("https://localhost:7199");
var client = channel.CreateGrpcService<IGreeter>();
var reply = client.Greet(new GreetRequest() { FirstName = "John", LastName = "Doe"});
Console.WriteLine($"Greeting: {reply.Response}");
```

## WCF-Behaviors in gRPC nachstellen

[WCF-Behaviors](https://learn.microsoft.com/en-us/dotnet/framework/wcf/extending/configuring-and-extending-the-runtime-with-behaviors) ermöglichen zusätzliche Verarbeitungsfunktionen für eingehende und ausgehende Nachrichten in einem WCF-Dienst. Diese Funktionalität kann in gRPC mit Hilfe von [ASP.NET Core Middlewares](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/) und [gRPC-Interceptoren](https://learn.microsoft.com/en-us/aspnet/core/grpc/interceptors) nachgebildet werden. Eine Middleware kann zum Beispiel genutzt werden, um Authentifizierung und Autorisierung zu implementieren, während Interceptor dazu dienen können, eingehende und ausgehende Nachrichten zu überwachen, zu ändern oder zu blockieren.

Zur Registrierung von ASP.NET Core Middlewares kann die `Use`-Methode der `IApplicationBuilder`-Schnittstelle aufgerufen werden. So kann mit folgendem Beispiel Authentifizierung fuer einen gRPC Dienst hinzugefuegt werden.

```csharp
..
builder.Services
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        // TODO: Configure JWT Bearer authentication
    });
....
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<GreeterService>();
....
```

Mit dem `AddInterceptor`-Methodenaufruf auf den `GrpcServiceOptions` können gRPC-Interceptoren konfiguriert werden, um benutzerdefinierte Interceptor für die Verarbeitung von eingehenden und ausgehenden Nachrichten zu registrieren. Interceptoren koennen global fuer alle Dienste oder aber pro Dienst konfiguriert werden. Ein Beispiel fuer einen global Interceptor, der eingehende Nachrichten logged, kann so aussehen.

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
        _logger.LogDebug("Calling gRPC Method {method} with argument {argument}", context.Method, request);
        var result = await base.UnaryServerHandler(request, context, continuation);
        _logger.LogDebug("Called gRPC Method {method} with result {result}", context.Method, result);
        return result;
    }
}
```

Um den Interceptor zu registrieren muss die entsprechende `AddInterceptor`-Method aufgerufen werden. 

```csharp
// global
builder.Services
    .AddCodeFirstGrpc(options => options.Interceptors.Add<LoggingInterceptor>())

// per Service
builder.Services
    .AddCodeFirstGrpc(_ => { })
    .AddServiceOptions<GreeterService>(options => options.Interceptors.Add<LoggingInterceptor>());
```

Durch die Verwendung von ASP.NET Core-Middleware und gRPC-Interceptoren kann die Funktionalität von WCF-Behaviors in einem gRPC-Service nachgebildet werden, um ähnliche Verarbeitungsfunktionen für eingehende und ausgehende Nachrichten zu implementieren.

## Fazit

Insgesamt bietet der Wechsel von WCF zu gRPC mit ASP.NET Core ein leistungsfähigeres und skalierbareres Framework für die Entwicklung verteilter Anwendungen. Das Code-First-Design mit dem Community-Projekt protobuf-net.Grpc und ASP.NET Core Middlewares sowie die Verwendung von gRPC-Interceptoren sorgen für einen einfacheren Übergang und verringern den Aufwand für die Portierung vorhandener WCF-Dienste. Die Migration von WCF zu gRPC kann ein komplexes Unterfangen sein, aber die Migration hat viele Vorteile, einschließlich verbesserter Leistung, Skalierbarkeit und Interoperabilität. 

Wesentlich umfassendere Informationen zu dem Thema gRPC fuer WCF Entwickler gibt es direkt von Microsoft als [eBook](https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/).
