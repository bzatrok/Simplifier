# Simplifier
Simplify text on news sites 

## Purpose
Simplifier is a Blazor Server application that uses OpenAI's completions API to simplify the text contents of a url. The main purpose if the application is to help learning languages. As a language learner, I find news sites to be a good way to learn, however their language level is too high for my skills. This application aims to simplify the language of a news article, but preserve the content.

## Running the application
### Prerequisites:
- Node.js >=18
- dotnet 8 SDK

You can run this application in several ways:

### Visual Studio / Rider
Open Simplifier.sln solution, and run the Simplifier:http configuration.

### dotnet CLI
In the root of the project, run dotnet run --project Simplifier

### docker compose
Create a copy of the .env.template file found in the project root and add a valid OpenAI API key to it.

Run `docker compose --env-file .env up -d`. 

In each of the hosting scenarios, the application will be running under `http://localhost:5124`. 

## Demo
The application is currently reachable via https://simplifythissite.com/