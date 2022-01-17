Archived
======
Tech Leads: Repository archived due to inactivity in more than 6 months.
Please remember to add a CODEOWNERS file to the root of the repository when unarchiving.

# Sentry logging using gRPC interceptors
This repository implements [gRPC](https://grpc.io/) inteceptors for logging exceptions to the [Sentry](https://sentry.io).

## Usage

### Server
```C#
var server = new Server()
{
  Services =
  {
    TestService.BindService(new ServiceImp()).Intercept(new SentryInterceptor("DSN"))
  },
  Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
};
```

## Breadcrumbs
The library sets up continuation task for each call and in case that exception is thrown in the call, the continuation tasks loggs details about the call in form of breadcrumbs.

### Example of breadcrumbs in Sentry
![example](https://github.com/michalderdak/csharp-grpc-sentry/blob/master/example.JPG)


## License
License can be found [here](https://github.com/michalderdak/csharp-grpc-sentry/blob/master/LICENSE)
