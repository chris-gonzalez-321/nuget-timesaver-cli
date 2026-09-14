# Nuget Timesaver CLI - Rough Specs

## Why I want to build this

I am looking to extend the nuget cli to make it easier to update more than one project at once, but also being able to specify what I want updated.

## What I am after

### 1. Easily add or remove feeds

Pretty self explanatory. I should be able to "view", "add", "remove" feeds

### 2. View what packages can be updated

optional parameters: feed, fldr, pkg-wc, pre-r

feed = feed to use (specify by name; only use nuget.org feed if not specified)

fldr = folder to search inside (otherwise look in root directory)

pkg-wc = wildcard for package name; example: "IntegrationBroker*" should pull newest versions (if available) for IntegrationBroker.Services.Models, IntegrationBroker.Logging, etc. and "IntegrationBroker.Domain" shoud pull newest version (if available) of that one single package; Otherwise look for updates for every package

pre-r = flag to allow pre-release version of packages

### 3. Update packages

optional parameters: feed, fldr, pkg-wc, pre-r

feed = feed to use (specify by name; only use nuget.org feed if not specified)

folder = folder containing projects to update (otherwise update every project in root directory)

pkg-wc = wildcard for package name; example: specifying "IntegrationBroker*" should update to newest versions (if available) for IntegrationBroker.Services.Models, IntegrationBroker.Logging, etc. and "IntegrationBroker.Domain" shoud update to newest version (if available) of that one single package; Otherwise update every single package

pre-r = flag to allow pre-release version of packages

### command name

`nuget-t` (extra `t` stands for timesaver)

## Questions

1. Which language makes the most sense to use here? Python? 