# Generative Toolkit

__GenerativeToolkit__ is a [Dynamo](http://www.dynamobim.org) package that can be used with Dynamo Core, Dynamo for Revit and Refinery. It provides specific tools related to developing generative design workflows. 

### What does it do ?

With a range of different nodes it helps you create generative design workflows in Dynamo and Revit.  
![DynaWeb package screenshot](https://raw.githubusercontent.com/radumg/DynaWeb/master/samples/DynaWeb.png)

### How it came about

__DynaWeb__ was designed as a package to make other packages, so it provides building blocks enabling you to build Dynamo integrations with just about any web service out there. After making DynaSlack & DynAsana, it became clear that writing a ZeroTouch-based package for every web service I or the community would want to integrate with was simply not scalable or sustainable, no matter how much code was re-used. DynAsana is an abstracted DynaSlack and DynaWeb is an even more abstracted & modularised DynAsana.

# Getting Started

## Package manager
`DynaWeb` is now available on the Dynamo package manager, search for `DynaWeb` and install it from there.
See [Alternative installation methods](#alternative-installation-methods) at the end of this document for alternative install methods.

# Using DynaWeb
This repository has quite a few sample files provided to help you get started with __DynaWeb__. 

I highly recommed starting with the samples as they contain detailed notes and instructions on how to use each of the nodes. 
Feel free to open an issue or submit a PR if you'd like to see further some documentation added here.

## Samples
There are 8 sample Dynamo graphs included with the package, provided in both Dynamo `1.3` XML file format and Dynamo `2.0`'s new JSON format.

You can find the samples in this repository's [`samples folder`](https://github.com/radumg/DynaWeb/tree/master/samples) folder, as well as and in the `extra` folder of the package you download using the Dynamo Package Manager, typically found here : `%appdata%\Dynamo\Dynamo Revit\1.3\packages\DynaWeb` (note version and Revit flavour of Dynamo, your location may vary).

The samples start from super-simple and progressively increase in complexity :

#### Sample 1 - A first request
3 nodes, similar to out-of-the-box (OOTB) Dynamo experience today.

#### Sample 2 - A simple request
Introduces the 3 stages of performing web requests and explains quite a few things. Also show how to achieve same thing with the OOTB node.

#### Sample 3 - Requst + benchmarking
Same as sample 2 but with added nodes that provide more information about the request (timing, etc) and output the results to text files.

#### Sample 4 - REST API example
This introduces the use of the `WebClient` class and some of the basic priciples of interacting with REST services. Uses a REST API that is freely accessible and returns JSON reponses. Contrasts using a `WebClient` and a `WebRequest` to achieve same thing and also introduces `Deserialisation`.

#### Sample 5 - REST API advanced
Introduces POST-ing to a REST API service and handling JSON payloads. Once the request is submitted, the response is deserialised too.

#### Sample 6 - Complex POST request
Further expands on the above example, building a complex `WebRequest` with 6 steps before its execution.

#### Sample 7 - Autodesk Forge - Upload file
This example builds a `WebRequest` and attaches a file to it, to upload directly to the `Autodesk Forge` service. See the issue that sparked this sample [here](https://github.com/radumg/DynaWeb/issues/11).

#### Sample 8 - Autodesk Forge - Request auth token
This example builds a POST `WebRequest`, used to request an authorisation token from the `Autodesk Forge` service. See the issue that sparked this sample [here](https://github.com/radumg/DynaWeb/issues/13).


## Structure
There's 5 main components in DynaWeb :
- `WebRequest` : the web request that gets executed
- `WebClient` : the context in which a request is executed
- `WebResponse` : this contains the response from the server, as well as additional metadata about the response & server itself 
- `Execution` : this provides nodes that simply execute requests, making it easier & clearer to use standard http verbs such as GET, POST, etc.
- `Helpers` : a few helper nodes, with a particular focus on `Deserialisation.`

Simply put, use `WebRequest` nodes for one-off requests and start using a `WebClient` when you are interacting with REST APIs and/or have multiple request to similar endpoints/URLs.
When using a `WebClient`, the `WebRequest` is still what gets executed, but it allows you more control over how that occurs (custom timeouts, etc)

#### Fun facts
- when executing a `WebRequest` on its own, the DynaWeb package constructs an empty `WebClient` in the background anyway as it's needed for execution
- the strucuture of the source code shows up directly in Dynamo

## Alternative installation methods

### Manual install
If you prefer to install one of the more experimental/work-in-progress builds, you can still follow the instructions below.

- Download the latest release from the [Releases page](https://github.com/radumg/DynaWeb/releases)
- unzip the downloaded file
- once unzipped, copy the `DynaWeb` folder to the location of your Dynamo packages  :
    - `%appdata%\Dynamo\Dynamo Core\1.3\packages` for Dynamo Sandbox, replacing `1.3` with your version of Dynamo
    - `%appdata%\Dynamo\Dynamo Revit\1.3\packages` for Dynamo for Revit, replacing `1.3` with your version of Dynamo
- start Dynamo, the package should now be listed as `DynWWW` in the library.

### Still can't see the package in Dynamo ?

This issue should be fixed now the package is distributed through the package manager, I definitely recommending getting it that way. However, in case you still have issues, see instructions below :

As [reported](https://github.com/radumg/DynaWeb/issues/10) by users, Windows sometimes blocks `.dll` files for security reasons. To resolve this, you'll have to go through the steps below for each assembly (`.dll` file) in the package :
  1. Right-click on `.dll` file and select properties
  2. Tick the `Unblock` checkbox at the bottom, in the Security section.
  3. Launch Dynamo again, the package should now load.

![image](https://user-images.githubusercontent.com/15014799/29770289-3c13172a-8be6-11e7-983e-6fb3c71ad136.png)

### Updating from alpha-0.5 build ?
The changes in `1.0` are breaking, meaning graphs using the previous version will not work. However, instead of re-creating them, you can simply open the `.dyn` files using Notepad (though i recommend SublimeText) and perform the following text find/replaces :
- replace `DSCore.Web.` with `DynaWeb.`
- replace `DynWWW.dll` with `DynaWeb.dll`
- replace `WebClient.WebClient` with `WebClient.ByUrl`


## Prerequisites

This project requires the following applications or libraries be installed :

```
Dynamo : version 1.3 or later
```
```
.Net : version 4.5 or later
```

Please note the project has no dependency to Revit and its APIs, so it will happily run in Dynamo Sandbox or Dynamo Studio.


## Built with

The `DynaWeb` project relies on a few community-published NuGet packages as listed below :
* [Newtonsoft](https://www.nuget.org/packages/newtonsoft.json/) - handles serializing and deserializing to JSON
* [RestSharp](https://www.nuget.org/packages/RestSharp/) - enables easier interaction with REST API endpoints
* [DynamoServices](https://www.nuget.org/packages/DynamoVisualProgramming.DynamoServices/2.0.0-beta4066) - an official Dynamo package providing support for better mapping of C# code to Dynamo nodes

## Contributing

Please read [CONTRIBUTING.md](https://github.com/radumg/DynWWW/blob/master/docs/CONTRIBUTING.md) for details on how to contribute to this package. Please also read the [CODE OF CONDUCT.md](https://github.com/radumg/DynWWW/blob/master/docs/CODE_OF_CONDUCT.md).

## Authors

__Radu Gidei__ : [Github profile](https://github.com/radumg), [Twitter profile](https://twitter.com/radugidei)

## License

This project is licensed under the GNU AGPL 3.0 License - see the [LICENSE FILE](https://github.com/radumg/DynWWW/blob/master/LICENSE) for details.
