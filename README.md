<div align="center">
<h1>Live Streaming API<br/><sub>Control one more live streaming events on Azure Media Services</sub></h1>

<strong>[:one: API](https://github.com/literal-life-church/live-streaming-api/)</strong> |
[:two: Controller](https://github.com/literal-life-church/live-streaming-controller/) |
[:three: Player](https://github.com/literal-life-church/stream-switch/)

```text
"Streamlining the process to get your events online."
```

[![Build Status](https://dev.azure.com/literal-life-church/live-streaming-api/_apis/build/status/literal-life-church.live-streaming-api?branchName=develop)](https://dev.azure.com/literal-life-church/live-streaming-api/_build/latest?definitionId=1&branchName=develop) [![Maintainability](https://api.codeclimate.com/v1/badges/094efdcf9b724ad8871f/maintainability)](https://codeclimate.com/github/literal-life-church/live-streaming-api/maintainability) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/60a578f696cb4313849fef9a0589be78)](https://www.codacy.com/gh/literal-life-church/live-streaming-api) [![Postman Documentation](https://img.shields.io/badge/Postman-Documentation%20Available-orange?logo=postman)](https://documenter.getpostman.com/view/3329098/Szmb6fCx) ![Live Streaming API Releases](https://img.shields.io/github/v/release/literal-life-church/live-streaming-api?label=Releases)

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

## Installation

As mentioned before, this application is intended to run on Azure Functions. Therefore, the instructions are geared toward the process of installating the API there. Overall, there are three major phases to the installation:

1. Setting up the resources on Azure
1. Setting up the environment variables
1. Deploying the application

Guides for using the API as a [broadcaster](https://github.com/literal-life-church/live-streaming-controller) and [viewer](https://github.com/literal-life-church/stream-switch) are available in the READMEs of their respective repositories.

### Setting up the Resources on Azure

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

The sections below show how to obtain the necessary information to populate these environment variables for the application. Please log into the [Azure Portal](https://portal.azure.com/ to find the information shown in these steps. Some of the most important information in the above table is available from a single location in Azure.

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

#### LIVE_STREAMING_API_ARCHIVE_WINDOW_LENGTH

This value is completely up you. Note that values less than `3` minutes are not allowed. The value can be as large as `1500` (25 hours).

#### All Webhooks
