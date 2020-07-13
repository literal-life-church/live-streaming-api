<div align="center">
<h1>Live Streaming API<br/><sub>Control one or more live streaming events on Azure Media Services</sub></h1>

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

1.  **[Live Streaming API](https://github.com/literal-life-church/live-streaming-api/):** Turn on or off one more live events on Azure Media Services
2.  **[Live Streaming Controller](https://github.com/literal-life-church/live-streaming-controller):** A simple tool for the event broadcaster to interface with the Live Streaming API
3.  **[Stream Switch](https://github.com/literal-life-church/stream-switch):** A front-end, viewer application for viewing one or more live streams on a website

In production, an event broadcaster would use the Live Streaming Controller as a front-end application to make a `POST` call to the `/broadcaster` endpoint, a `DELETE` call to the `/broadcaster`, and a `GET` call to the `/broadcaster` endpoint on the Live Streaming API to respectively start the streaming services at the beginning of an event, stop the services at the end, and read the status of these resources at any point before, during, or after. All of these calls are authenticated, since they can reveal sensitive information about the state of your resources, or result in a state change, and thus a billing change, on the broadcaster's Azure account.

A viewer would then go to a website which has the Stream Switch installed to view the event. That application calls the `/locators` endpoint to fetch the streaming URLs from Azure to play inside of an HTML5 video player on the web. Since this endpoint intended for public consumption, it is the only endpoint in the application which is not authenticated.

This portion of the application trio focuses on what is necessary to directly manipulate the state of these services on Azure. The collection you are viewing is to document all of the available calls one can make to this service, once it is deployed online. It is intended to be published on Azure Functions, although, it could be changed slightly to work on other serverless platforms, such as AWS Lambda.

Please note that this application is designed to create and destroy resources on Azure in a completely self-contained manner. It does not require an administrator to change or manage these resources in any way beyond what is required for the [installation](#Installation). Manually changing these resources in Azure after the application is running may cause interference, resulting in an application malfunction. If you want to manually manage your own Media Services resources, it is recommended that you create a separate Media Services instance and leave the one used by this application to run on its own.

The impetus of this application was to create an easy way to spin up Media Services resources on Azure before a broadcast and spin them down again after the show ends. This helps minimize unnecessary billing on our Azure account and puts the power of service management into the hands of the broadcaster. To reduce the costs associated with streaming on Azure, note that ALL resources created for a live streaming session will be DESTROYED when the application stops them. That includes the [video buffer stored in the archive window](#LIVE_STREAMING_API_ARCHIVE_WINDOW_LENGTH). If you want to retain a copy of your live streaming session, it is recommended that the broadcaster records it locally before shutting down the stream and stopping the media resources.

## Basic Definitions

This project controls two major resource types on Azure Media Services: Streaming Endpoints and Live Events. Here are their definitions:

-   **Live Events:** A channel that a broadcaster will reuse to publish similar events over and over again.
-   **Streaming Endpoint:** The ingest mechanism on Azure Media Services which allows a broadcaster to connect their streaming software of choice, such as [Wirecast](https://www.telestream.net/wirecast/) or [OBS Studio](https://obsproject.com/), and begin sending a live stream.

## Installation

As mentioned before, this application is intended to run on Azure Functions. Therefore, the instructions are geared toward the process of installing the API there. Overall, there are several significant phases to the installation:

1.  [Setting up the Media Services resources on Azure](#Setting-up-the-Media-Services-Resources-on-Azure)
2.  [Creating the Azure Functions application](#Creating-the-Azure-Functions-Application)
3.  [Setting up the environment variables](#Setting-up-the-Environment-Variables)
4.  [Deploying the application](#Deploying-the-Application)
5.  [Authenticating with the application](#Authenticating-with-the-Application)
6.  [Set up the streaming software](#Set-up-the-Streaming-Software)

Guides for using the API as a [broadcaster](https://github.com/literal-life-church/live-streaming-controller) and [viewer](https://github.com/literal-life-church/stream-switch) are available in the READMEs of their respective repositories.

### Setting up the Media Services Resources on Azure

Obviously, to use this application, you'll need an active Azure account. Please [sign up for an account](https://signup.azure.com/) if you don't already have one. Once you have an account set up, [sign in to the Azure Portal](https://portal.azure.com/).

#### Create the Media Service

First, begin by creating the Media Service resource.

1.  From the search bar at the top, search for "Media Services," and then select the "Media Services" service
2.  Click the Add button to create a new Media Service
3.  Enter an Account Name, choose your active Subscription, and a Location
4.  For the resource group, create a new one
5.  Click on Storage Account &gt; Create New
6.  Enter a Storage Account Name, and leave all other settings as the default
7.  Back on the Media Service creation screen, click the Create button

#### Create the Streaming Endpoint

Now that the Media Service resource is available, create a streaming endpoint.

1.  Click on the newly created Media Service resource
2.  On the left panel, select Streaming Endpoints &gt; Endpoint button to create a new Endpoint
3.  Enter a name for the Streaming Endpoint, and configure the rest of the options on the page as desired
4.  Click the Add Button

#### Create the Live Events

Last, the Live Event(s) should be created.

1.  Inside of the Media Service resource, go to the left panel and select Live Streaming &gt; Live Events tab &gt; Add Live Event button
2.  Enter a Live Event Name and configure the rest of the options as desired
3.  Do not start the Live Event
4.  Click the Review + Create button and confirm your entry to create the Live Event
5.  Repeat these steps for each necessary Live Event

### Creating the Azure Functions Application

Since this application needs a platform to host it, the next major step is to create a Function App on Azure for it.

#### Create the Function App

Follow these steps to create a resource to which the application can be deployed.

1.  From the search bar at the top, search for "Function App", and then select the "Function App" service
2.  Click the Add button to create a new Function App
3.  Specify a billing Subscription, Resource Gropu, and Function App Name
4.  For the Publish option, select Code
5.  For the Runtime Stack, select .NET Core
6.  For the Version, select the lastest available version
7.  Specify the Region as desired
8.  Click the Review + Create button and confirm your entry to create the Function App

#### Setup Azure Application Insights

Although it is not strictly necessary, adding Azure Application Insights can help provide better into how the application is performing and how often it is used.

1.  In the Function Apps list, select the newly created application
2.  From the panel on the left, select Application Insights &gt; Turn on Application Insights
3.  Select the Apply button to confirm your action

An environment variable called `APPINSIGHTS_INSTRUMENTATIONKEY` is automatically applied to the Function App for the application to use and send its telemetry. 

### Setting up the Environment Variables

All of the application's configuration resides inside of the environment variables. Please refer to this table for a complete reference:

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
| `LIVE_STREAMING_API_WEBHOOK_START_FAILURE` | Webhook to call when a POST request to the `/broadcaster` endpoint fails                               | :x:                |
| `LIVE_STREAMING_API_WEBHOOK_START_SUCCESS` | Webhook to call when a POST request to the `/broadcaster` endpoint succeeds                            | :x:                |
| `LIVE_STREAMING_API_WEBHOOK_STOP_FAILURE`  | Webhook to call when a DELETE request to the `/broadcaster` endpoint fails                             | :x:                |
| `LIVE_STREAMING_API_WEBHOOK_STOP_SUCCESS`  | Webhook to call when a DELETE request to the `/broadcaster` endpoint succeeds                          | :x:                |
| `SENTRY_DSN`                               | URL to send crashes and telemetry to Sentry                                                            | :x:                |
| `SENTRY_ENVIRONMENT`                       | Application environment type. Set by you. Examples include `production` or `development`.              | :x:                |
| `SENTRY_RELEASE`                           | String identifying the application and version number. Set by you, such as `live-streaming-api@1.0.0`. | :x:                |

The sections below show how to obtain the necessary information to populate these environment variables for the application. Please log in to the [Azure Portal](https://portal.azure.com/) to find the information shown in these steps. Some of the most important information in the above table is available from a single location in Azure.

1.  From the search bar at the top, search for "Media Services", and then select the "Media Services" service
2.  In the table of available Media Services, select the service to use
3.  In the panel on the left, select API Access
4.  Under the Manage Your AAD App and Secret section, for the AAD App, click Create New &gt; Enter a Name &gt; click the Create button
5.  Once the application is created, for the Secret, click Create New &gt; Enter a Description and Expiration &gt; click the Create button
6.  Under the Connect to Media Services API section, ensure `V3` is selected
7.  Here is the mapping of environment variables to the values shown in Azure:

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

This value is completely up to you. Note that values less than `3` minutes are not allowed. The value can be as large as `1500` (25 hours).

#### All Webhooks

Webhooks can provide an excellent amount of extensibility and automation to your processes. The application will perform just fine if these variables are omitted. For more information on how the webhooks work, see the section titled [Webhooks](#Webhooks) in this document.

#### Sentry Configuration

These environment variables are also optional. It is recommended to include them if you have a Sentry account and wish to monitor the health of this application. For more information on these environment variables, please see the [Sentry Docs](https://docs.sentry.io/).

-   `SENTRY_DSN`: [https://docs.sentry.io/error-reporting/configuration/?platform=csharp#dsn](https://docs.sentry.io/error-reporting/configuration/?platform=csharp#dsn)
-   `SENTRY_ENVIRONMENT`: [https://docs.sentry.io/error-reporting/configuration/?platform=csharp#environment](https://docs.sentry.io/error-reporting/configuration/?platform=csharp#environment)
-   `SENTRY_RELEASE`: [https://docs.sentry.io/error-reporting/configuration/?platform=csharp#release](https://docs.sentry.io/error-reporting/configuration/?platform=csharp#release)

### Deploying the Application

The easiest way to deploy this application is to clone this repository and publish from Visual Studio. These steps assume you are using Visual Studio for Windows.

1.  Clone this repository
2.  Open the solution in Visual Studio
3.  In the Solution pane, right-click on the `api` project &gt; Publish
4.  In the Publish window, select Azure as the publish target
5.  If necessary, log into your Azure account
6.  Select the Function App which was created above and click Finish

### Authenticating with the Application

For security purposes, all variants of the `/broadcaster` endpoint requires an API key to access. This key is generated and managed by Azure.

1.  In the Azure Portal, select the newly created Function App
2.  In the panel on the left, select App Keys &gt; New Host Key
3.  Give the key a Name, and let it automatically generate the Value
4.  Press the OK button to generate the key

When making calls to any of the above three endpoints, make sure to append `code=your-key-here` as a query parameter to the call to authenticate your request.

### Set up the Streaming Software

Now that all of the resources are in place on Azure, the streaming software can be configured. Common examples of streaming software include [Wirecast](https://www.telestream.net/wirecast/) or [OBS Studio](https://obsproject.com/). Both pieces of software require a streaming URL to which the stream is sent.

1.  In the Azure Portal, go to Media Services and select the appropriate resource
2.  In the panel on the left select Live Streaming &gt; Live Events tab
3.  Select the appropriate Live Event from the list
4.  Copy the Input URL from the Live Event details screen. This URL begins with `rtmp://` or `rtmps://`. If you do not see this URL, make sure that the Essentials panel is expanded by clicking the toggle arrows near the top of the screen.
5.  Paste the URL into your streaming software, according to the manufacture's instructions
6.  Repeat these steps to extract the Input URL for each relevant Live Event

## Webhooks

This application is able to send out events for four different cases. Those cases are:

1.  **Azure Media Services Start Success:** Whenever all of the steps to start one or more Live Event(s) and a Streaming Endpoint succeed. This does not indicate that all services are running (since they must have been in the proper state beforehand to start), but rather that the process finished without an error.
2.  **Azure Media Services Start Failure:** Similar to the above condition, except the application stopped partway through because of an error. If Sentry is configured, the error will be reported there.
3.  **Azure Media Services Stop Success:** Whenever all of the steps to stop one or more Live Event(s) and a Streaming Endpoint succeed. This does not indicate that all services are stopped (since they must have been in the proper state beforehand to stop), but rather that the process finished without an error.
4.  **Azure Media Services Stop Failure:** Similar to the above condition, except the application stopped partway through because of an error. If Sentry is configured, the error will be reported there.

The application is designed to be flexible enough to send the webhook in a variety of ways so that it can accommodate whatever the expectations of the receiving application may be.

For example, a simple setup may set all four webhook environment variables to different URLs, one for each case. Therefore, the consuming application can easily determine what event occurred based on the incoming webhook. A more sophisticated setup may point all of the variables to the same URL and have the receiving application determine what happened based on the data sent in the request.

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

Aside from turning on and off the resources themselves, status reports are perhaps the most important aspect of this application. Since the state of each service decides whether or not your Azure account is billed, it is very important to have a reliable way of knowing their current state. This section summarizes what the possible statuses are for each resource.

Each status has a `type` associated with it. Here are the types:

-   **Stable:** The status will not change at all unless explicitly instructed to do so by the user.
-   **Transient:** The current status is only temporary and is expected to change shortly

For example, since it may take a moment to start any given resource, the API may show a resource as `starting` (transient state) until it starts completely and shows as `running` (stable state).

### Streaming Endpoint

Here are the seven possible states for a Streaming Endpoint.

| Name     | Type      | Description                                                         |
|----------|-----------|---------------------------------------------------------------------|
| Deleting | Transient | Resource is being deleted on Azure                                  |
| Error    | Stable    | Whenever none of the other conditions in this table are satisfied   |
| Running  | Stable    | Endpoint is on and ready to ingest a stream                         |
| Scaling  | Transient | Endpoint is on and Azure is scaling up the capacity of the endpoint |
| Starting | Transient | Endpoint is turning on                                              |
| Stopped  | Stable    | Endpoint is off                                                     |
| Stopping | Transient | Endpoint is turning off                                             |

Note that this application does not delete your streaming endpoints. Thus, the `Deleting` status can only appear if the resource is deleted from the Azure Portal.

### Live Events

Here are the six possible states for a Live Event.

| Name     | Type      | Description                                                       |
|----------|-----------|-------------------------------------------------------------------|
| Deleting | Transient | Resource is being deleted on Azure                                |
| Error    | Stable    | Whenever none of the other conditions in this table are satisfied |
| Running  | Stable    | Live Event is on and ready to broadcast a stream                  |
| Starting | Transient | Live Event is turning on                                          |
| Stopped  | Stable    | Live Event is off                                                 |
| Stopping | Transient | Live Event is turning off                                         |

Note that this application does not delete your live events. Thus, the `Deleting` status can only appear if the resource is deleted from the Azure Portal.

### Summary

Since there are several steps to determine how to collectively summarize the state of a Streaming Endpoint and all requested Live Events, the application does this on your behalf. Here are the rules it follows for each state.

| Name     | Type      | Description                                                                                                                 |
|----------|-----------|-----------------------------------------------------------------------------------------------------------------------------|
| Error    | Stable    | Whenever none of the other conditions in this table are satisfied, or ANY Streaming Endpoint or Live Event reports an error |
| Running  | Stable    | Streaming Endpoint is either running or scaling and ALL Live Events are either running or scaling                           |
| Starting | Transient | Either the Streaming Endpoint or ANY Live Event is in the starting state                                                    |
| Stopped  | Stable    | Streaming Endpoint is either stopped or deleting and ALL Live Events are either stopped or deleting                         |
| Stopping | Transient | Either the Streaming Endpoint or ANY Live Event is in the stopping state                                                    |

Notice how the `scaling` and `deleting` statuses are omitted from the summary, since they effectively map to `running` and `stopped`, respectively.
