---
services: app-service\mobile, app-service\web, app-service
platforms: dotnet, xamarin
author: lindydonna, syntaxc4
---

# Contoso Moments

## What is Contoso Moments?

Contoso Moments is a photo sharing application that demonstrates the following features of Azure App Service:

- App Service authentication/authorization
- Continuous Integration and deployment
- Mobile app server SDK
- Mobile offline sync client SDK
- Mobile file sync SDK
- Mobile push notifications

## How To: Configure your Development Environment

Download and install the following tools to build and/or develop this application locally.

1. [Visual Studio 2015 Community](https://go.microsoft.com/fwlink/?LinkId=691978&clcid=0x409)
1. [Xamarin Platform for Visual Studio](https://xamarin.com/platform)

## How To: Deploy the Demo

1. Check to ensure that the bulid is passing 
    ![VSTS Build](https://img.shields.io/vso/build/cfowler/6a1734d8-b06d-4591-8240-ef2ce88d8250/1.svg?style=flat-square)
1. Fork this repository to your GitHub account 
1. Click on the **Deploy to Azure** Button

    [![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fazure-appservice-samples%2FContosoMoments%2Fmaster%2Fazuredeploy.json)

1. Fill out the values in the Deployment page

1. If deploying from the main repo, use `true` for ManualIntegration, otherwise use `false`. This parameter controls whether or not a webhook is created when you deploy. If you don't have permissions to the repo and it tries to create a webhook (i.e., `ManualIntegration` is `false`, then deployment will fail). 

1. When the deployment steps complete, it will provide a link to the Web App

## Download the iOS client app

The iOS client app is available on the Apple App Store. You can have it connect to your own service by setting the URL in the app's settings page.

[![Contoso Moments Icon](appicon-small.png)](https://itunes.apple.com/us/app/contoso-moments/id1118186646?ls=1&mt=8)

## How: Customize the service

### Authentication 
The web and mobile client support Facebook and Azure Active Directory Authentication. Follow these tutorials:

- [How to configure your App Service application to use Facebook login](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-facebook-authentication/)
- [How to configure your App Service application to use Azure Active Directory login](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-active-directory-authentication/)

> **Note**: Ensure that the *Action to take when request is not authenticated* is set to **Allow request (no action)**

The mobile app uses [client-directed login](https://azure.microsoft.com/en-us/documentation/articles/app-service-authentication-overview/#mobile-authentication-with-a-provider-sdk) for Facebook, which provides a better user experience on a mobile device. This requires that you set your Facebook App ID in the following locations:

- (iOS) The **FacebookAppID** key in [Info.plist](src/Mobile/ContosoMoments.iOS/Info.plist#L123).
- (iOS) The value for **CFBundleURLSchemes** in [Info.plist](src/Mobile/ContosoMoments.iOS/Info.plist#L118). This string should have "fb" followed by your app ID.
- (Android) The value for **facebook_app_id** in [strings.xml](src/Mobile/ContosoMoments.Droid/Resources/values/strings.xml#L4).
- (Android) The value for **fb_login_protocol_scheme** in [strings.xml](src/Mobile/ContosoMoments.Droid/Resources/values/strings.xml#L5). This string should have "fb" followed by your app ID.

To learn more, see:

- [Facebook Login for iOS](https://developers.facebook.com/docs/facebook-login/ios)
- [Facebook Login for Android](https://developers.facebook.com/docs/facebook-login/android)

### Other customizations

Our hosted version of the service restricts uploads to the public album to users who are logged in with Azure Active Directory, by setting `Image.IsVisible = false` for other users. On the web client, anyone can upload images to the public album, but the image will be marked as not visible and cannot be viewed unless it is changed in the database. On the mobile client, if you're using the default service url (https://contosomoments.azurewebsites.net), the image upload icon is hidden for the public album unless you're logged in with AAD. 

The provided ARM template opts out of this behavior by using `false` for the app setting `PublicAlbumRequiresAuth`.

## How To: Run the Mobile Client Apps

In Visual Studio or Xamarin Studio, open the project [src/ContosoMoments-Mobile.sln](src/ContosoMoments-Mobile.sln). 

| Platform  | Build Status |
|-----------|----------------------------------------------------------------------------------------------------------------------|
| Android   | ![VSTS Build](https://img.shields.io/vso/build/cfowler/6a1734d8-b06d-4591-8240-ef2ce88d8250/2.svg?style=flat-square) |
| iOS       | ![VSTS Build](https://img.shields.io/vso/build/cfowler/6a1734d8-b06d-4591-8240-ef2ce88d8250/3.svg?style=flat-square) |


