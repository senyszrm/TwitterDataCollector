# TwitterDataCollector
Captures Twitter Data using the API v2 Sampled Stream Endpoint

The Visual Studio solution contains the following projects:

SampledStreamServer - The RestApi server which collects data from Twitter and stores to a local database

SampledStreamClient - A small client app which connects to SampledStreamServer, retrieves data from the restApi and prints it to a console

SampledStreamTests - Contains unit tests for functionality of SampledStreamServer

# Instructions
Modify TwitterConnectionInfo.xml to add your Twitter API Bearer Token under the <bearer_token> element where it says "PLACE YOUR BEARER TOKEN HERE"
Then compile and run the solution in Visual Studio Code

The default debug/start up includes just the server. This opens a Swagger page that can be used to query the Twitter data via RestApi.
This can be configured to run both SampledStreamClient and SampledStreamServer in parallel by setting multiple startup projects.