# Contoso Moments

![Microsoft Azure](https://img.shields.io/badge/platform-Azure-00abec.svg?style=flat-square)
![Visual Studio 2015](https://img.shields.io/badge/Visual%20Studio-2015-373277.svg?style=flat-square)
![ASP.NET 4.6](https://img.shields.io/badge/ASP.NET-4.6-blue.svg?style=flat-square)
![Xamarin.Forms](https://img.shields.io/badge/Xamarin.Forms-2.0-1faece.svg?style=flat-square)
![iOS](https://img.shields.io/badge/platform-iOS-lightgrey.svg?style=flat-square)
![Android](https://img.shields.io/badge/platform-Andriod-green.svg?style=flat-square)
![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)

## What is Contoso Moments

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

    [![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net)

1. Fill out the values in the Deployment page

1. If deploying from the main repo, use `true` for ManualIntegration, otherwise use `false`. This parameter controls whether or not a webhook is created when you deploy. If you don't have permissions to the repo and it tries to create a webhook (i.e., `ManualIntegration` is `false`, then deployment will fail). 

1. When the deployment steps complete, it will provide a link to the Web App

#### NOTE: If Deploy to Azure shows all the steps succeeded but shows an error at the end because it can't find the site in your resource group, that's because the ARM template is dynamically generating the site name. If you go to your resource group in the Azure Portal, you'll see that you have a site with a unique string appended to it.  

## How: Customize the service

### Authentication 
The web and mobile client support Facebook and Azure Active Directory Authentication. Follow these tutorials:

- [How to configure your App Service application to use Facebook login](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-facebook-authentication/)
- [How to configure your App Service application to use Azure Active Directory login](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-how-to-configure-active-directory-authentication/)

The mobile app uses [client-directed login](https://azure.microsoft.com/en-us/documentation/articles/app-service-authentication-overview/#mobile-authentication-with-a-provider-sdk) for Facebook, which provides a better user experience on a mobile device. This requires that you set your Facebook App ID in the following locations:

- (iOS) The **FacebookAppID** key in [Info.plist](src/Mobile/ContosoMoments.iOS/Info.plist#L123).
- (iOS) The value for **CFBundleURLSchemes** in [Info.plist](src/Mobile/ContosoMoments.iOS/Info.plist#L118). This string should have "fb" followed by your app ID.
- (Android) The value for **facebook_app_id** in [strings.xml](src/Mobile/ContosoMoments.Droid/Resources/values/strings.xml#L4).
- (Android) The value for **fb_login_protocol_scheme** in [strings.xml](src/Mobile/ContosoMoments.Droid/Resources/values/strings.xml#L5). This string should have "fb" followed by your app ID.

To learn more, see:

- [Facebook Login for iOS](https://developers.facebook.com/docs/facebook-login/ios)
- [Facebook Login for Android](https://developers.facebook.com/docs/facebook-login/android)

### Other customizations

By default, the service restricts uploads to the public album to users who are logged in with Azure Active Directory, by setting `Image.IsVisible = false` for other users. To change this behavior, change the app setting `PublicAlbumRequiresAuth` in **web.config** or in the App Settings section of the Azure Portal.

## How To: Run the Mobile Client App

In Visual Studio or Xamarin Studio, open the project [src/ContosoMoments-Mobile.sln](src/ContosoMoments-Mobile.sln). 

| Platform  | Build Status |
|-----------|----------------------------------------------------------------------------------------------------------------------|
| Android   | ![VSTS Build](https://img.shields.io/vso/build/cfowler/6a1734d8-b06d-4591-8240-ef2ce88d8250/2.svg?style=flat-square) |
| iOS       | ![VSTS Build](https://img.shields.io/vso/build/cfowler/6a1734d8-b06d-4591-8240-ef2ce88d8250/3.svg?style=flat-square) |

## Change Log

we are working on a v1.0 release.
