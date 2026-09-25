# LexisNexisOrderIntakeTracking

Take Home Assignment: Order Intake & Tracking (Senior .NET + Angular)

Customers browse products, place orders and view their order history. 
Admins view all orders and move them through a simple status flow (Pending → Confirmed → Shipped → Delivered, or Cancelled).

Folders
`OrderIntakeTracking/OrderIntakeTracking.Api` | ASP.NET Core Web API (.NET 10) |
`OrderIntakeTracking/OrderIntakeTracking.Tests` | xUnit unit tests |
`order-intake-tracking-ui` | Angular frontend |

## Prerequisites

- .NET 10 SDK
- Node.js 22+
- Visual Studio 2022+
- Angular CLI

## Run the API

Open `OrderIntakeTracking/OrderIntakeTracking.slnx` in Visual Studio and run the **IIS Express** profile. 
The API runs at `https://localhost:44368`, which is the URL the frontend expects. Swagger is at `/swagger`.

Data is stored in JSON files in `OrderIntakeTracking.Api/Data/`. There is no database to set up.

## Run the frontend

```bash
cd order-intake-tracking-ui
npm install
ng serve
```

Open http://localhost:4200.

## Run the tests

```bash
cd OrderIntakeTracking
dotnet test
```

## Logins for demo

Admin: admin.admin@example.com
Customer: Test.test@example.com
Any other email takes you to a "Create Account" form to create a customer only.

## Link to Loom Video
https://www.loom.com/share/2a23088a2379423fb76a1ecf9b685669

