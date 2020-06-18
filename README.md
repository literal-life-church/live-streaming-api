<div align="center">
<h1>Live Streaming API<br/><sub>Control one more live streaming events on Azure Media Services</sub></h1>

<strong>[:one: API](https://github.com/literal-life-church/live-streaming-api/)</strong> |
[:two: Controller](https://github.com/literal-life-church/live-streaming-controller/) |
[:three: Player](https://github.com/literal-life-church/stream-switch/)

```text
"Streamlining the process to get your events online."
```

[![Build Status](https://dev.azure.com/literal-life-church/live-streaming-api/_apis/build/status/literal-life-church.live-streaming-api?branchName=develop)](https://dev.azure.com/literal-life-church/live-streaming-api/_build/latest?definitionId=1&branchName=develop) [![Maintainability](https://api.codeclimate.com/v1/badges/094efdcf9b724ad8871f/maintainability)](https://codeclimate.com/github/literal-life-church/live-streaming-api/maintainability) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/60a578f696cb4313849fef9a0589be78)](https://www.codacy.com/gh/literal-life-church/live-streaming-api) [![Postman Documentation](https://img.shields.io/badge/Postman-Documentation%20Available-orange?logo=postman)](https://documenter.getpostman.com/view/3329098/SzzkddRA) ![Live Streaming API Releases](https://img.shields.io/github/v/release/literal-life-church/live-streaming-api?label=Releases)

<hr />
</div>

## Project Introduction

The Live Streaming API is a subset of a three-part application designed to control the state of one or more live events on [Azure Media Services](https://azure.microsoft.com/en-us/services/media-services/). These parts are:

1. **[Live Streaming API](https://github.com/literal-life-church/live-streaming-api/):** Turn on or off one more live events on Azure Meida Services
1. **[Live Streaming Controller](https://github.com/literal-life-church/live-streaming-controller):** A simple tool for the event broadcaster to interface with the Live Streaming API
1. **[Stream Switch](https://github.com/literal-life-church/stream-switch):** A front-end, viewer application for viewing one or more live streams on a website

In production, an event broadcaster would use the Live Streaming Controller as a front-end application to call the `/start`, `/stop`, and `/status` endpoints on the Live Streaming API to respectively start the streaming services at the beginning of an event, stop the services at the end, and read the status of these resources at any point before, during, or after. All of these calls are authenticated, since they can reveal sensitive information about the state of your resources, or result in a state change, and thus a billing change, on the broadcaster's Azure account.

A viewer would then go to a website which has the Stream Switch installed to view the event. That application calls the `/locators` endpoint to fetch the streaming URLs from Azure to play inside of an HTML5 video player on the web. Since this endpoint intended for public consumption, it is the only endpoint in the application which is not authenticated.

This portion of the application trio focuses on what is necessary to directly manipulate the state of these services on Azure. The collection you are viewing is to document all of the available calls one can make to this service, once it is deployed online. It is intended to be published on Azure Functions, although, it could be changed slightly to work on other serverless platforms, such as AWS Lambda.

Please note that this application is designed to create and destroy resources on Azure in a completely self-contained manner. It does not require an administrator to change or manage these resources in any way beyond what is required for the [installation](#Installation). Manually changing these resoures in Azure after the application is running may cause interference, resulting in an application malfunction. If you want to manually manage your own Media Services resources, it is recommended that you create a seperate Media Services instance and leave the one used by this application to run on its own.

The impetus of this application was to create an easy way to spin up Media Services resources on Azure before a broadcast, and spin them down again after the broadcast ends. This helps minimize unnecessary billing on our Azure account and puts the power of service management into the hands of the broadcaster. In an effort to minimize the costs associated with streaming on Azure, note that ALL resources created for live streaming session will be DESTROYED when the application stops them. That includes the [video buffer stored in the archive window](#LIVE_STREAMING_API_ARCHIVE_WINDOW_LENGTH). If you want to retain a copy of your live streaming session, it is recommended that the broadcaster records it locally before shutting down the stream and stopping the media resources.

## Basic Definitions

This project controls two major resource types on Azure Media Services: Streaming Endpoints and Live Events. Here are their definitions:

- **Live Events:** A &quot;channel&quot; to which a broadcaster 

## Installation

As mentioned before, this application is intended to run on Azure Functions. Therefore, the instructions are geared toward the process of installating the API there. Overall, there are several major phases to the installation:

1. [Setting up the Media Services resources on Azure](#Setting-up-the-Media-Services-Resources-on-Azure)
1. [Creating the Azure Functions application](#Creating-the-Azure-Functions-Application)
1. [Setting up the environment variables](#Setting-up-the-Environment-Variables)
1. [Deploying the application](#Deploying-the-Application)

Guides for using the API as a [broadcaster](https://github.com/literal-life-church/live-streaming-controller) and [viewer](https://github.com/literal-life-church/stream-switch) are available in the READMEs of their respective repositories.

### Setting up the Media Services Resources on Azure

Obviously, to use this application, you'll need an active Azure account. Please [sign up for an account](https://signup.azure.com/), if you don't already have one. Once you have an account set up, [sign into the Azure Portal](https://portal.azure.com/).

#### Create the Media Service

First, begin by creating the Media Service resource.

1. From the search bar at the top, search for "Media Services", and then select the "Media Services" service
1. Click the Add button to create a new Media Service
1. Enter an Account Name, select your active Subscription, and a Location
1. For the resource group, create a new one
1. Click on Storage Account &gt; Create New
1. Enter a Storage Account Name, and leave all other settings as the default
1. Back on the Media Service creation screen, click the Create button

#### Create the Streaming Endpoint

Now that the Media Service resource is available, create a streaming endpoint.

1. Click on the newly created Media Service resource
1. On the left panel, select Streaming Endpoints &gt; Endpoint button to create a new Endpoint
1. Enter a name for the Streaming Endpoint, and configure the rest of the options on the page as desired
1. Click the Add Button


#### Create the Live Events

Last, the Live Event(s) should be created.

1. Inside of the Media Service resource, go to the left panel and select Live Streaming &gt; Live Events tab &gt; Add Live Event button
1. Enter a Live Event Name and configure the rest of the options as desired
1. Do not start the Live Event
1. Click the Review + Create button and confirm your entry to create the Live Event
1. Repeat these steps for each necessary Live Event

### Creating the Azure Functions Application

Since this application needs a platform to host it, the next major step is to create a Function App on Azure for it.

#### Create the Function App

Follow these steps to create a resource to which the application can be deployed.

1. From the search bar at the top, search for "Function App", and then select the "Function App" service
1. Click the Add button to create a new Function App
1. Specify a billing Subscription, Resource Gropu, and Function App Name
1. For the Publish option, select Code
1. For the Runtime Stack, select .NET Core
1. For the Version, select the lastest available version
1. Specify the Region as desired
1. Click the Review + Create button and confirm your entry to create the Function App

#### Setup Azure Application Insights

Although it is not strictly necessary, adding Azure Application Insights can help provide better into how the application is performing and how often it is used.

1. In the Function Apps list, select the newly created application
1. From the panel on the left, select Application Insights &gt; Turn on Application Insights
1. Select the Apply button to confirm your action

An environment variable called `APPINSIGHTS_INSTRUMENTATIONKEY` is automatically applied to the Function App for the application to use and send its telemetry. 

### Setting up the Environment Variables

All of the application's configuration resides inside of environment variables. Please refer to this table for a complete reference:

| Variable                                   | Description                                                                                            |      Required      |
|--------------------------------------------|--------------------------------------------------------------------------------------------------------|:------------------:|
| `APPINSIGHTS_INSTRUMENTATIONKEY`           | Key to authentication with Azure Application Insights and begin sending telemetry                      | :x:                |
| `LIVE_STREAMING_API_ACCOUNT_NAME`          | Name of the Media Service resource in Azure                                                            | :heavy_check_mark: |
| `LIVE_STREAMING_API_ARCHIVE_WINDOW_LENGTH` | Length of the window, in minutes, a viewer can rewind to see behind the real-time stream               | :heavy_check_mark: |
| `LIVE_STREAMING_API_CLIENT_ID`             | Client ID of the application to authenticate with the Azure API                                        | :heavy_check_mark: |
| `LIVE_STREAMING_API_CLIENT_SECRET`         | Client secret of the application to authenticate with the Azure API                                    | :heavy_check_mark: |
| `LIVE_STREAMING_API_RESOURCE_GROUP`        | Resource group under which all of the streaming endpoints and live events reside on Azure              | :heavy_check_mark: |
| `LIVE_STREAMING_API_SUBSCRIPTION_ID`       | ID of the billing account which is paying for the Azure subscription                                   | :heavy_check_mark: |
| `LIVE_STREAMING_API_TENANT_ID`             | Domain hooked up to Azure                                                                              | :heavy_check_mark: |
| `LIVE_STREAMING_API_WEBHOOK_START_FAILURE` | Webhook to call when a request to the `/start` endpoint fails                                          | :x:                |
| `LIVE_STREAMING_API_WEBHOOK_START_SUCCESS` | Webhook to call when a request to the `/start` endpoint succeeds                                       | :x:                |
| `LIVE_STREAMING_API_WEBHOOK_STOP_FAILURE`  | Webhook to call when a request to the `/stop` endpoint fails                                           | :x:                |
| `LIVE_STREAMING_API_WEBHOOK_STOP_SUCCESS`  | Webhook to call when a request to the `/stop` endpoint succeeds                                        | :x:                |
| `SENTRY_DSN`                               | URL to send crashes and telemetry to Sentry                                                            | :x:                |
| `SENTRY_ENVIRONMENT`                       | Application environment type. Set by you. Examples include `production` or `development`.              | :x:                |
| `SENTRY_RELEASE`                           | String identifying the application and version number. Set by you, such as `live-streaming-api@1.0.0`. | :x:                |

The sections below show how to obtain the necessary information to populate these environment variables for the application. Please log into the [Azure Portal](https://portal.azure.com/) to find the information shown in these steps. Some of the most important information in the above table is available from a single location in Azure.

1. From the search bar at the top, search for "Media Services", and then select the "Media Services" service
1. In table of available Media Services, select the service to use
1. In the panel on the left, select API Access
1. Under the Manage Your AAD App and Secret section, for the AAD App, click Create New &gt; Enter a Name &gt; click the Create button
1. Once the application is created, for the Secret, click Create New &gt; Enter a Description and Expiration &gt; click the Create button
1. Under the Connect to Media Services API section, ensure `V3` is selected
1. Here is the mapping of environment variables to the values shown in Azure:

| Environment Variable                       | Azure Property                    |
|--------------------------------------------|-----------------------------------|
| `APPINSIGHTS_INSTRUMENTATIONKEY`           | &lt;set elsewhere&gt;             |
| `LIVE_STREAMING_API_ACCOUNT_NAME`          | Azure Media Services Account Name |
| `LIVE_STREAMING_API_ARCHIVE_WINDOW_LENGTH` | &lt;set by you, see below&gt;     |
| `LIVE_STREAMING_API_CLIENT_ID`             | AAD Client ID                     |
| `LIVE_STREAMING_API_CLIENT_SECRET`         | AAD Client Secret                 |
| `LIVE_STREAMING_API_RESOURCE_GROUP`        | Resource Group                    |
| `LIVE_STREAMING_API_SUBSCRIPTION_ID`       | Subscription ID                   |
| `LIVE_STREAMING_API_TENANT_ID`             | AAD Tenant Domain                 |
| `LIVE_STREAMING_API_WEBHOOK_START_FAILURE` | &lt;set by you, see below&gt;     |
| `LIVE_STREAMING_API_WEBHOOK_START_SUCCESS` | &lt;set by you, see below&gt;     |
| `LIVE_STREAMING_API_WEBHOOK_STOP_FAILURE`  | &lt;set by you, see below&gt;     |
| `LIVE_STREAMING_API_WEBHOOK_STOP_SUCCESS`  | &lt;set by you, see below&gt;     |
| `SENTRY_DSN`                               | &lt;set elsewhere&gt;             |
| `SENTRY_ENVIRONMENT`                       | &lt;set elsewhere&gt;             |
| `SENTRY_RELEASE`                           | &lt;set elsewhere&gt;             |

As for the rest of the properties, read on.

#### APPINSIGHTS_INSTRUMENTATIONKEY

This environment variable is set automatically by the Azure Portal when following the steps in the [Setup Azure Application Insights](#Setup-Azure-Application-Insights) section of this document.

#### LIVE_STREAMING_API_ARCHIVE_WINDOW_LENGTH

This value is completely up you. Note that values less than `3` minutes are not allowed. The value can be as large as `1500` (25 hours).

#### All Webhooks

Webhooks can provide an excellent amount of extensibility and automation to your processes. The application will perform just fine if these variables are omitted. For more information on how the webhooks work, see the section titled [Webhooks](#Webhooks) in this document.

#### Sentry Configuration

These environment variables are also optional. It is recommended to include them if you have a Sentry account and wish to monitor the health of this application. For more information on these environment variables, please see the [Sentry Docs](https://docs.sentry.io/).

- `SENTRY_DSN`: https://docs.sentry.io/error-reporting/configuration/?platform=csharp#dsn
- `SENTRY_ENVIRONMENT`: https://docs.sentry.io/error-reporting/configuration/?platform=csharp#environment
- `SENTRY_RELEASE`: https://docs.sentry.io/error-reporting/configuration/?platform=csharp#release

### Deploying the Application

The easiest way to deploy this application is to clone this repository and publish from Visual Studio. These steps assume you are using Visual Studio for Windows.

1. Clone this repository
1. Open the solution in Visual Studio
1. In the Solution pane, right click on the `api` project &gt; Publish
1. In the Publish window, select Azure as the publish target
1. If necessary, log into your Azure account
1. Select the Function App which was created above and click Finish

## Webhooks

This application is able to send out events for four different cases. Those cases are:

1. **Azure Media Services Start Success:** Whenever all of the steps to start one or more Live Event(s) and a Streaming Endpoint succeed. This does not indicate that all services are running (since they must have been in the proper state beforehand to start), but rather that the process finished without an error.
1. **Azure Media Services Start Failure:** Similar to the above condition, except the application stopped partway through because of an error. If Sentry is configured, the error will be reported there.
1. **Azure Media Services Stop Success:** Whenever all of the steps to stop one or more Live Event(s) and a Streaming Endpoint succeed. This does not indicate that all services are stopped (since they must have been in the proper state beforehand to stop), but rather that the process finished without an error.
1. **Azure Media Services Stop Failure:** Similar to the above condition, except the application stopped partway through because of an error. If Sentry is configured, the error will be reported there.

The application is designed to be flexible enough to send the webhook in a variety of ways, so that it can accomodate whatever the expectations of the receiving application may be.

For example, a simplistic setup may set all four webhook environment variables to different URLs, one for each case. Therefore, the consuming application can easily determine what event occured based on the incoming webook. A more complex setup may point all of the variables to the same URL and have the receiving application determine what happened based on the data sent in the request.

This application sends the relevant data in two ways, regardless of how the webhooks are configured. All requests are made as `POST` requests, and thus include a body like this:

```json
{
    "action": "start|stop",
    "status": "error|running|starting|stopping|stopped"
}

```

This data is also sent along as query parameters. Consider:

```text
https://www.wwbhook.com/?action=start|stop&status=error|running|starting|stopping|stopped
```

## Statuses
