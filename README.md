<a href="https://elsa-workflows.github.io/elsa-core/">
  <p align="center">
    <img src="./doc/github-social-preview-banner-for-elsa.png" alt="Elsa Workflows">
  </p>
</a>

## Elsa Workflows

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Elsa)](https://www.nuget.org/packages/Elsa/2.0.0-preview5-0)
[![MyGet (with prereleases)](https://img.shields.io/myget/elsa-2/vpre/Elsa?label=myget)](https://www.myget.org/gallery/elsa-2)
[![Build status](https://ci.appveyor.com/api/projects/status/github/elsa-workflows/elsa-core?svg=true&branch=feature/elsa-2.0)](https://ci.appveyor.com/project/sfmskywalker/elsa)
[![Discord](https://img.shields.io/discord/814605913783795763?label=chat&logo=discord)](https://discord.gg/hhChk5H472)
[![Stack Overflow questions](https://img.shields.io/badge/stackoverflow-elsa_workflows-orange.svg)]( http://stackoverflow.com/questions/tagged/elsa-workflows )
![Docker Pulls](https://img.shields.io/docker/pulls/elsaworkflows/elsa-dashboard?label=elsa%20dashboard%3Adocker%20pulls)

Elsa Core is a workflows library that enables workflow execution in any .NET Core application.
Workflows can be defined not only using code but also as JSON, YAML or XML.

<p align="center">
  <img src="./doc/elsa-2-dashboard-plus-designer.gif" alt="Elsa 2 Preview">
</p>

## Get Started

Follow the [Getting Started](https://elsa-workflows.github.io/elsa-core/docs/installing-elsa-core) instructions on the [Elsa Workflows documentation site](https://elsa-workflows.github.io/elsa-core).

## Roadmap

Version 1.0

- [x] Workflow Invoker
- [x] Long-running Workflows
- [x] Workflows as code
- [x] Workflows as data
- [x] Correlation
- [x] Persistence: CosmosDB, Entity Framework Core, MongoDB, YesSQL 
- [x] HTML5 Workflow Designer Web Component
- [x] ASP.NET Core Workflow Dashboard
- [x] JavaScript Expressions
- [x] Liquid Expressions
- [x] Primitive Activities
- [X] Control Flow Activities
- [x] Workflow Activities
- [x] Timer Activities
- [x] HTTP Activities
- [x] Email Activities

Version 2.0

- [x] Composite Activities API
- [x] Service Bus Messaging
- [x] Workflow Host REST API
- [x] Workflow Server
- [x] Distributed Hosting Support (support for multi-node environments)
- [x] Persistence: MongoDB, YesSQL, Entity Framework Core (SQL Server, SQLLite, PostgreSql)
- [ ] Lucene Indexing
- [ ] New Workflow Designer + Dashboard
- [ ] Generic Command & Event Activities
- [ ] Job Activities (simplify kicking off a background process while the workflow sleeps & gets resumed once job finishes)

Version 3.0
- [ ] Composite Activity Definitions (with designer support)
- [ ] Localization Support
- [ ] State Machines
- [ ] Sagas


## Workflow Designer

Workflows can be visually designed using the Elsa Designer, a reusable & extensible HTML5 web component built with [StencilJS](https://stenciljs.com/).
To manage workflow definitions and instances, Elsa comes with a reusable Razor Class Library that provides a dashboard application in the form of an MVC area that you can include in your own ASP.NET Core application.

## Programmatic Workflows

Workflows can be created programmatically and then executed using `IWorkflowRunner` or scheduled for execution using `IWorkflowQueue`.

### Hello World
The following code snippet demonstrates creating a workflow with two WriteLine activities from code and then invoking it:

```c#

// Define a strongly-typed workflow.
public class HelloWorldWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .WriteLine("Hello World!")
            .WriteLine("Goodbye cruel world...");
    }
}

// Setup a service collection.
var services = new ServiceCollection()
    .AddElsa()
    .AddConsoleActivities()
    .AddWorkflows<HelloWorldWorkflow>()
    .BuildServiceProvider();

// Run startup actions (not needed when registering Elsa with a Host).
var startupRunner = services.GetRequiredService<IStartupRunner>();
await startupRunner.StartupAsync();

// Get a workflow runner.
var workflowRunner = services.GetService<IWorkflowRunner>();

// Run the workflow.
await workflowRunner.RunWorkflowAsync<HelloWorld>();

// Output:
// /> Hello World!
// /> Goodbye cruel world...
```

## Declarative Workflows

Instead of writing C# code to define a workflow, Elsa also supports reading and writing declarative workflows from the database as well as from JSON formats.
The following is a small example that constructs a workflow using a generic set of workflow and activity models, describing the workflow.
This models is then serialized to JSON and deserialized back into the model

```csharp
// Create a service container with Elsa services.
var services = new ServiceCollection()
    .AddElsa()


    // For production use.
    .UseYesSqlPersistence()
    
    // Or use any of the other supported persistence providers such as EF Core or MongoDB:
    // .UseEntityFrameworkPersistence(ef => ef.UseSqlite())
    // .UseMongoDbPersistence()

    .BuildServiceProvider();

// Run startup actions (not needed when registering Elsa with a Host).
var startupRunner = services.GetRequiredService<IStartupRunner>();
await startupRunner.StartupAsync();

// Define a workflow.
var workflowDefinition = new WorkflowDefinition
{
    WorkflowDefinitionId = "SampleWorkflow",
    WorkflowDefinitionVersionId = "1", 
    Version = 1,
    IsPublished = true,
    IsLatest = true,
    IsEnabled = true,
    PersistenceBehavior = WorkflowPersistenceBehavior.Suspended,
    Activities = new[]
    {
        new ActivityDefinition
        {
            ActivityId = "activity-1",
            Type = nameof(WriteLine),
            Properties = new ActivityDefinitionProperties
            {
                [nameof(WriteLine.Text)] = new ActivityDefinitionPropertyValue
                {
                    Syntax = "Literal",
                    Expression = "Hello World!",
                    Type = typeof(string)
                }
            }
        }, 
    }
};

// Serialize workflow definition to JSON.
var serializer = services.GetRequiredService<IContentSerializer>();
var json = serializer.Serialize(workflowDefinition);

Console.WriteLine(json);

// Deserialize workflow definition from JSON.
var deserializedWorkflowDefinition = serializer.Deserialize<WorkflowDefinition>(json);

// Materialize workflow.
var materializer = services.GetRequiredService<IWorkflowBlueprintMaterializer>();
var workflowBlueprint = materializer.CreateWorkflowBlueprint(deserializedWorkflowDefinition);

// Execute workflow.
var workflowRunner = services.GetRequiredService<IWorkflowRunner>();
await workflowRunner.RunWorkflowAsync(workflowBlueprint);
```

## Persistence

Elsa abstractes away data access, which means you can use any persistence provider you prefer. 

## Long Running Workflows

Elsa has native support for long-running workflows. As soon as a workflow is halted because of some blocking activity, the workflow is persisted.
When the appropriate event occurs, the workflow is loaded from the store and resumed. 

## Why Elsa Workflows?

One of the main goals of Elsa is to **enable workflows in any .NET application** with **minimum effort** and **maximum extensibility**.
This means that it should be easy to integrate workflow capabilities into your own application.

### What about Azure Logic Apps?

As powerful and as complete Azure Logic Apps is, it's available only as a managed service in Azure. Elsa on the other hand allows you to host it not only on Azure, but on any cloud provider that supports .NET Core. And of course you can host it on-premise.

Although you can implement long-running workflows with Logic Apps, you would typically do so with splitting your workflow with multiple Logic Apps where one workflow invokes the other. This can make the logic flow a bit hard to follow.
with Elsa, you simply add triggers anywhere in the workflow, making it easier to have a complete view of your application logic. And if you want, you can still invoke other workflows form one workflow.

### What about Windows Workflow Foundation?

I've always liked Windows Workflow Foundation, but unfortunately [development appears to have halted](https://forums.dotnetfoundation.org/t/what-is-the-roadmap-of-workflow-foundation/3066).
Although there's an effort being made to [port WF to .NET Standard](https://github.com/dmetzgar/corewf), there are a few reasons I prefer Elsa:

- Elsa intrinsically supports triggering events that starts new workflows and resumes halted workflow instances in an easy to use manner. E.g. `workflowHost.TriggerWorkflowAsync("HttpRequestTrigger");"` will start and resume all workflows that either start with or are halted on the `HttpRequestTrigger`. 
- Elsa has a web-based workflow designer. I once worked on a project for a customer that was building a huge SaaS platform. One of the requirements was to provide a workflow engine and a web-based editor. Although there are commercial workflow libraries and editors out there, the business model required open-source software. We used WF and the re-hosted Workflow Designer. It worked, but it wasn't great.

### What about Orchard Workflows?

Both [Orchard](http://docs.orchardproject.net/en/latest/Documentation/Workflows/) and [Orchard Core](https://orchardcore.readthedocs.io/en/dev/docs/reference/modules/Workflows/) ship with a powerful workflows module, and both are awesome.
In fact, Elsa Workflows is taken & adapted from Orchard Core's Workflows module. Elsa uses a similar model, but there are some differences:  

- Elsa Workflows is completely decoupled from web, whereas Orchard Core Workflows is coupled to not only the web, but also the Orchard Core Framework itself.
- Elsa Workflows can execute in any .NET Core application without taking a dependency on any Orchard Core packages.

## Features

TODO

## How to use Elsa

TODO

### Setting up a Workflow Designer ASP.NET Core Application

TODO: describe all the steps to add packages and register services.

### Setting up a Workflow Host .NET Application 

TODO: describe all the steps to add packages and register services.

### Building & Running Elsa Workflows Dashboard

TODO

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct). 

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

## Sponsored by Interfirst

This project is backed by [Interfirst](http://www.interfirst.com/), a Residential Mortgage Licensee.

<a href="http://www.interfirst.com/"> <svg width="155px" height="27px" viewBox="0 0 155 27"><defs><linearGradient x1="11.4507342%" y1="104.02323%" x2="88.4575057%" y2="21.2737321%" id="linearGradient-1"><stop stop-color="#1994FF" offset="0%"></stop><stop stop-color="#33FFBB" offset="100%"></stop></linearGradient></defs><g id="Page-1" stroke="none" stroke-width="1" fill="none" fill-rule="evenodd"><g id="Retails" transform="translate(-214.000000, -39.000000)" fill-rule="nonzero"><g id="Group-7" transform="translate(214.000000, 37.000000)"><g id="121" transform="translate(0.000000, 2.000000)"><path d="M39.9715762,26.8132331 C39.6842534,26.8248283 39.4052446,26.7140593 39.202106,26.5077464 C38.9989674,26.3014336 38.8902206,26.0183878 38.9021964,25.7271429 L38.9021964,7.97210526 C38.8903212,7.68049569 38.9992288,7.39712872 39.202584,7.19052632 C39.4161986,7.00148321 39.6881188,6.89380621 39.9715762,6.88601074 C40.2521953,6.88519875 40.5209298,7.0007865 40.715347,7.20592474 C40.9097642,7.41106299 41.0129399,7.68789449 41.0009044,7.97210526 L41.0009044,25.7271429 C41.0127796,26.0187524 40.903872,26.3021194 40.7005168,26.5087218 C40.5183832,26.7209074 40.2482986,26.8337339 39.9715762,26.8132331 L39.9715762,26.8132331 Z M114.267442,26.8132331 C113.990822,26.8330887 113.721011,26.7203764 113.538501,26.5087218 C113.335669,26.3019119 113.227467,26.0184917 113.240116,25.7271429 L113.240116,12.7894737 C113.221284,12.4961171 113.33383,12.209771 113.546512,12.0099248 C113.729021,11.7982702 113.998832,11.685558 114.275452,11.7054135 C114.564382,11.6853147 114.847087,11.7969715 115.046447,12.0099248 C115.239909,12.2222676 115.347806,12.5004227 115.348837,12.7894737 L115.348837,25.7271429 C115.340131,26.0148902 115.234071,26.2908489 115.04845,26.5087218 C114.84909,26.7216751 114.566385,26.8333319 114.277455,26.8132331 L114.267442,26.8132331 Z M114.267442,9.62052632 C113.865751,9.63598958 113.476419,9.47811905 113.196059,9.18609023 C112.92555,8.89110643 112.772816,8.50475147 112.767506,8.10203008 C112.760005,7.69996254 112.934053,7.31643652 113.240116,7.0606015 C113.502935,6.76977454 113.87866,6.61088786 114.267442,6.62616541 C114.660483,6.61577624 115.041043,6.76653888 115.322989,7.04433124 C115.604935,7.32212361 115.76443,7.70345909 115.765375,8.10203008 C115.763348,8.5031286 115.614975,8.88927869 115.348837,9.18609023 C115.063897,9.47110295 114.67769,9.62770635 114.277455,9.62052632 L114.267442,9.62052632 Z M127.458463,14.3972932 C127.716985,14.3892697 127.962417,14.2801466 128.143346,14.092782 C128.315725,13.8703331 128.406353,13.5939989 128.399677,13.311203 C128.419434,13.0317668 128.309015,12.759067 128.101292,12.5742857 C127.880278,12.3670887 127.587034,12.2575301 127.28624,12.2697744 L123.785724,12.2697744 C122.870813,12.2485513 121.968529,12.4891802 121.182364,12.9640602 C120.417966,13.4184385 119.793789,14.080009 119.380039,14.8743609 C118.941978,15.7010746 118.720636,16.6279576 118.737209,17.5662406 L118.737209,25.7271429 C118.744932,26.0150924 118.851117,26.2913742 119.037597,26.5087218 C119.236957,26.7216751 119.519662,26.8333319 119.808592,26.8132331 C120.09617,26.8247734 120.37551,26.7144456 120.579587,26.5087218 C120.764625,26.2906555 120.869988,26.0146725 120.877972,25.7271429 L120.877972,17.5662406 C120.845082,16.7207923 121.1373,15.8954264 121.693023,15.2641353 C122.205685,14.6551128 122.934625,14.3972932 123.791731,14.3972932 L127.458463,14.3972932 Z M84.5090439,24.6857143 C84.7714008,24.6749641 85.0240254,24.7872854 85.1939276,24.9902256 C85.3795491,25.2080985 85.4856095,25.4840572 85.4943152,25.7718045 C85.488909,26.0556735 85.3643427,26.3237339 85.1518734,26.5087218 C84.9308593,26.7159188 84.6376158,26.8254775 84.3368217,26.8132331 L77.9605943,26.8132331 C75.2190568,26.8132331 73.4207364,25.162782 73.4207364,22.2110526 L73.4207364,16.8719549 C73.4207364,14.1354135 74.9627261,12.4869925 77.3598191,12.2697744 L81.7294574,12.2697744 C84.1325581,12.4869925 85.6685401,14.1354135 85.6685401,16.8719549 L85.6685401,19.3466165 C85.6685401,20.6478947 84.5991602,21.4294737 83.3996124,21.4294737 L75.5194444,21.4294737 L75.5194444,22.2110526 C75.5194444,23.9914286 76.6328811,24.6857143 77.9605943,24.6857143 L84.5090439,24.6857143 Z M81.1687339,14.3972932 L77.9645995,14.3972932 C76.6368863,14.3972932 75.5234496,15.0915789 75.5234496,16.8333835 L75.5234496,19.3080451 L83.5738372,19.3080451 L83.5738372,16.8272932 C83.5738372,15.0915789 82.5044574,14.391203 81.1707364,14.391203 L81.1687339,14.3972932 Z M132.693217,24.6857143 C132.416597,24.6658587 132.146786,24.778571 131.964276,24.9902256 C131.791898,25.2126744 131.70127,25.4890086 131.707946,25.7718045 C131.696629,26.050213 131.806448,26.3196225 132.008333,26.5087218 C132.228497,26.7159956 132.521167,26.8256093 132.821382,26.8132331 L139.07345,26.8132331 C143.056589,26.8132331 144.298191,22.1237594 140.529328,20.2134586 L134.405426,16.9653383 C132.821382,16.226391 133.336047,14.4033835 135.134367,14.4033835 L141.600711,14.4033835 C141.861105,14.4041056 142.109744,14.2935563 142.285594,14.0988722 C142.488949,13.8922698 142.597857,13.6089028 142.585982,13.3172932 C142.590922,13.0310728 142.464386,12.7587744 142.24354,12.5803759 C142.027344,12.3666488 141.731831,12.2559705 141.430491,12.2758647 L135.134367,12.2758647 C131.151227,12.2758647 129.695349,16.9653383 133.720543,18.873609 L139.630168,21.9999248 C141.172158,22.7815038 140.873773,24.6918045 139.07345,24.6918045 L132.693217,24.6857143 Z M51.4083333,12.2697744 L49.7361757,12.2697744 C48.82124,12.2483309 47.9188961,12.4889756 47.1328165,12.9640602 C46.3819624,13.4208944 45.7732826,14.0829588 45.3765504,14.8743609 C44.93849,15.7010746 44.7171474,16.6279576 44.7337209,17.5662406 L44.7337209,25.7271429 C44.7276137,26.0101866 44.8189584,26.286547 44.9920543,26.5087218 C45.427997,26.9148176 46.0981012,26.9148176 46.5340439,26.5087218 C46.7190827,26.2906555 46.8244454,26.0146725 46.8324289,25.7271429 L46.8324289,17.5662406 C46.7995392,16.7207923 47.0917569,15.8954264 47.6474806,15.2641353 C48.1601421,14.6551128 48.8890827,14.3972932 49.744186,14.3972932 L52.2293928,14.3972932 C53.0864987,14.3972932 53.8134367,14.6571429 54.3281008,15.2641353 C54.8832832,15.8956287 55.174798,16.7210374 55.1411499,17.5662406 L55.1411499,25.7271429 C55.1488725,26.0150924 55.2550571,26.2913742 55.4415375,26.5087218 C55.8774802,26.9148176 56.5475844,26.9148176 56.9835271,26.5087218 C57.1559058,26.2862729 57.2465338,26.0099388 57.2398579,25.7271429 L57.2398579,17.5662406 C57.2564314,16.6279576 57.0350888,15.7010746 56.5970284,14.8743609 C56.2002962,14.0829588 55.5916164,13.4208944 54.8407623,12.9640602 C54.0547676,12.4887706 53.1523644,12.2481101 52.2374031,12.2697744 L51.4083333,12.2697744 Z M97.8682817,14.3972932 C98.1414122,14.3974377 98.4034901,14.287956 98.5972222,14.092782 C98.7696009,13.8703331 98.8602289,13.5939989 98.853553,13.311203 C98.8815395,13.0242338 98.7572955,12.7437449 98.5271318,12.5742857 C98.3109357,12.3605586 98.0154232,12.2498803 97.7140827,12.2697744 L94.1915375,12.2697744 C93.2877329,12.2516541 92.3972095,12.4922932 91.6222222,12.9640602 C90.8573577,13.4178404 90.233037,14.0795634 89.8198966,14.8743609 C89.3818363,15.7010746 89.1604936,16.6279576 89.1770672,17.5662406 L89.1770672,25.7271429 C89.165192,26.0187524 89.2740996,26.3021194 89.4774548,26.5087218 C89.6587745,26.7208463 89.9282507,26.8337288 90.2043928,26.8132331 C90.4933227,26.8333319 90.7760275,26.7216751 90.9753876,26.5087218 C91.1787428,26.3021194 91.2876504,26.0187524 91.2757752,25.7271429 L91.2757752,17.5662406 C91.2421271,16.7210374 91.5336419,15.8956287 92.0888243,15.2641353 C92.6281335,14.6756887 93.3961136,14.3584852 94.1875323,14.3972932 L97.8682817,14.3972932 Z M108.62416,9.01353383 C108.886574,9.02459219 109.139327,8.91221417 109.309044,8.70902256 C109.502244,8.49585615 109.609448,8.21692257 109.609432,7.92744361 C109.595824,7.64558414 109.472708,7.38064499 109.26699,7.19052632 C109.045976,6.9833293 108.752732,6.87377067 108.451938,6.88601504 L106.82584,6.88601504 C105.924668,6.88894805 105.039566,7.1281223 104.256525,7.58030075 C103.501451,8.04653312 102.880047,8.70516553 102.454199,9.4906015 C102.022739,10.3350875 101.802673,11.2741432 101.813372,12.2251128 L101.813372,25.7271429 C101.821356,26.0146725 101.926718,26.2906555 102.111757,26.5087218 C102.306423,26.7286535 102.592622,26.8416905 102.882752,26.8132331 C103.159372,26.8330887 103.429183,26.7203764 103.611693,26.5087218 C103.815048,26.3021194 103.923955,26.0187524 103.91208,25.7271429 L103.91208,14.3972932 L108.664212,14.3972932 C109.949871,14.3972932 109.949871,12.3996992 108.664212,12.3996992 L103.914083,12.3996992 L103.914083,12.2251128 C103.894724,11.3813655 104.187629,10.5608342 104.735142,9.92503759 C105.274253,9.3313897 106.037991,8.99968557 106.83385,9.01353383 L108.62416,9.01353383 Z M153.892571,24.6857143 C154.154928,24.6749641 154.407553,24.7872854 154.577455,24.9902256 C154.763935,25.2075732 154.87012,25.4838549 154.877842,25.7718045 C154.873205,26.0558552 154.748497,26.3242189 154.535401,26.5087218 C154.314386,26.7159188 154.021143,26.8254775 153.720349,26.8132331 L150.980814,26.8132331 C150.065904,26.8344562 149.163619,26.5938273 148.377455,26.1189474 C147.645671,25.638645 147.042355,24.9824145 146.621189,24.2086466 C146.18323,23.3665415 145.962119,22.4259651 145.978359,21.4741353 L145.978359,7.97210526 C145.972252,7.68906149 146.063597,7.41270112 146.236693,7.19052632 C146.542827,6.86726722 147.013611,6.76826931 147.421324,6.94141861 C147.829037,7.11456791 148.089636,7.52417313 148.077067,7.97210526 L148.077067,12.2697744 L153.944638,12.2697744 C154.198823,12.2548906 154.447438,12.3492169 154.629522,12.5296241 C154.973847,12.8939479 154.973847,13.4686085 154.629522,13.8329323 C154.447438,14.0133395 154.198823,14.1076658 153.944638,14.092782 L148.077067,14.092782 L148.077067,21.4741353 C148.077067,22.4282707 148.335401,23.2098496 148.892119,23.7742105 C149.426225,24.3749896 150.193328,24.7081551 150.990827,24.6857143 L153.892571,24.6857143 Z M68.7527132,24.6857143 C69.028752,24.6658672 69.2979529,24.7786343 69.4796512,24.9902256 C69.652747,25.2124004 69.7440917,25.4887607 69.7379845,25.7718045 C69.7493008,26.050213 69.6394821,26.3196225 69.4375969,26.5087218 C69.2171975,26.7156057 68.924693,26.8251574 68.6245478,26.8132331 L65.8850129,26.8132331 C64.9667204,26.8363771 64.0607055,26.5956792 63.2716408,26.1189474 C62.5300602,25.6512271 61.9245946,24.9919069 61.5173773,24.2086466 C61.0808087,23.3661709 60.8610801,22.4256145 60.878553,21.4741353 L60.878553,7.97210526 C60.871877,7.68930933 60.962505,7.41297518 61.1348837,7.19052632 C61.4415839,6.86799859 61.9121642,6.76945818 62.3198251,6.94239734 C62.727486,7.11533649 62.988576,7.52426835 62.977261,7.97210526 L62.977261,12.2697744 L68.844832,12.2697744 C69.0976616,12.2634878 69.3430865,12.3566037 69.5297158,12.5296241 C69.8709901,12.8951848 69.8709901,13.4673716 69.5297158,13.8329323 C69.3430865,14.0059526 69.0976616,14.0990686 68.844832,14.092782 L62.977261,14.092782 L62.977261,21.4741353 C62.977261,22.4282707 63.2335917,23.2098496 63.7903101,23.7742105 C64.3185271,24.3808955 65.0873167,24.715432 65.8850129,24.6857143 L68.7527132,24.6857143 Z" id="Shape" fill="#FFFFFF"></path><path d="M1.68617571,8.83894737 C1.45006851,8.98740936 1.16372519,9.0297465 0.895623399,8.95583404 C0.627521612,8.88192159 0.401768342,8.69840555 0.272351421,8.44917293 C-0.0215870084,7.9436029 0.129360973,7.29304344 0.614793282,6.97330827 L12.1777132,0.203007519 C12.5140814,0.0284378889 12.9127274,0.0284378889 13.2490956,0.203007519 L24.7699612,6.97533835 C25.0861813,7.16753251 25.2805327,7.51308615 25.2826227,7.88684211 L25.3266796,25.7291729 C25.318077,26.3253544 24.8434052,26.8065425 24.2552972,26.8152632 L10.4134367,26.8152632 C9.82611084,26.8054569 9.35263142,26.3245788 9.34405685,25.7291729 C9.36655795,25.1398283 9.83211605,24.6669953 10.4134367,24.6430827 L23.1759044,24.6430827 L23.1759044,8.53646617 L12.6923773,2.37112782 L1.68617571,8.83894737 Z M4.08927649,15.8731579 L12.1777132,11.1410526 C12.4211523,10.9944173 12.7125088,10.9531062 12.9862933,11.026405 C13.2600777,11.0997039 13.4933195,11.2814628 13.6335917,11.5308271 C13.9132415,12.048124 13.7442399,12.6968262 13.2490956,13.0066917 L5.11059432,17.738797 L5.06854005,17.7834586 L2.24289406,19.4339098 L2.24289406,25.7271429 C2.24289406,26.3269739 1.7632198,26.8132331 1.17151163,26.8132331 C0.579803451,26.8132331 0.100110495,26.3269739 0.100110495,25.7271429 L0.100110495,18.8248872 C0.0978073375,18.4270692 0.31181409,18.0602924 0.656847545,17.8707519 L0.700904393,17.8707519 L4.04121447,15.9157895 L4.04121447,15.9157895 L4.08927649,15.8731579 Z" id="Shape" fill="url(#linearGradient-1)"></path></g></g></g></g></svg> </a>
